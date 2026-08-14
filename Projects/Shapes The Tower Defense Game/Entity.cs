using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public abstract class Entity : MonoBehaviour, ITarget, IAgent, ICollector
{
    // Event
    public event EventHandler OnVisualChange;
    public event EventHandler OnHit;
    public event EventHandler OnDeath;

    // Controllers
    protected TraitsController traitsController;
    protected InventoryController inventoryController;

    // Attachments
    [SerializeField] private Weapon weapon;
    [SerializeField] private BulletSO bulletSO;

    // Entity Components
    protected NavMeshAgent agent;
    protected CircleCollider2D myCollider;
    protected Rigidbody2D rb;

    // Entity SO
    [SerializeField] protected EntitySO entitySO;

    // Layer
    [SerializeField] protected LayerMask ignoreLayer;

    // Timers
    protected Timer romeTimer;
    protected Timer moveTimer;
    protected Timer fireTimer;
    protected Timer delayTimer;
    protected Timer stuckTimer;

    [SerializeField] private float updateDelay;
    private float romeDelayTime = 0f;
    private float moveDelayTime = 0.2f;
    private float stuckTime = 3f;

    // Rotation
    private float currentAngle = 0;
    private float rotationVelocity;
    [SerializeField] private float rotationSmoothTime = 0.1f;

    // See Target Values
    protected Stack<Transform> targetQueue;
    protected Transform target; // Agent Target
    private ITarget cachedTarget;
    protected Vector3 lastTargetPos;
    protected Vector2 castSize;

    // Flags
    protected bool calculateSeekPath = true;

    // Path Calculations
    protected NavMeshPath path;

    // Enemy States
    public enum enemyState
    {
        idle,
        follow,
        seek,
        pursue,
        attack,
        flee,
    }

    protected enemyState state;

    // ------------------------ Entity Initialization ----------------------- //

    protected void OnAwake()
    {
        agent = GetComponent<NavMeshAgent>();
        myCollider = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        // Timer Init
        romeTimer = new Timer(romeDelayTime, true);
        moveTimer = new Timer(moveDelayTime, true);
        delayTimer = new Timer(updateDelay);
        stuckTimer = new Timer(stuckTime);

        targetQueue = new Stack<Transform>();

        castSize = bulletSO.obj.GetComponent<BoxCollider2D>().size;

        InitAgentData();
        ApplyAttributes();
    }

    // Initialize Traits
    public void InitAgentData()
    {
        traitsController = new TraitsController(entitySO.traits, entitySO.traits); // WILL LOAD FROM SAVE FILE
        inventoryController = new InventoryController(entitySO.inventory);

        weapon.Init(gameObject, bulletSO, traitsController);
    }

    // --------------------------------------- Locate Targets -------------------------------------- //

    protected void LocateTarget(Func<GameObject, bool> newCondition = null)
    {
        newCondition ??= (obj) => false;

        if (target != null && !target.GetComponent<Collider2D>().isActiveAndEnabled) { target = null; }

        LocateClosestTarget(newCondition);
        LocatePrimaryTarget(newCondition);

        UpdateTargetQueue();
    }

    protected void LocateClosestTarget(Func<GameObject, bool> newCondition = null)
    {
        if (targetQueue.Count > 1) { return; }

        float bestDistance = entitySO.behaviour.sight;
        //Transform tempTarget = null;

        // Find Objects Around Entity
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, entitySO.behaviour.sight, entitySO.targetLayers);

        // Check Primary Target
        foreach (Collider2D collider in colliders) // Top Target Priority
        {
            if (newCondition(collider.gameObject)) { continue; }

            targetQueue.Push(collider.transform);
            /*
            float distance = Vector2.Distance(collider.gameObject.transform.position, transform.position);

            if (SeeTarget(collider.transform) && distance < bestDistance)
            {
                bestDistance = distance;
                tempTarget = collider.transform;
            }*/
        }

        /*
        if (tempTarget != null)
        {
            targetQueue.Push(tempTarget);
        }*/
        //target = tempTarget;
    }

    private void LocatePrimaryTarget(Func<GameObject, bool> newCondition = null)
    {
        if (targetQueue.Count != 0) { return; }

        // Check Primary Target
        foreach (RegistrySO registrySO in entitySO.targetingSO.targetList) // Top Target Priority
        {
            foreach (GameObject obj in Registry.objects)
            {
                if (newCondition(obj)) { continue; }

                if (obj.TryGetComponent(out ITarget itarget))
                {
                    if (registrySO.ID == itarget.GetTarget().ID) // Is the right target
                    {
                        targetQueue.Push(obj.transform); // Store Target
                        return;
                    }
                }
            }
        }
        target = null;
    }

    private void UpdateTargetQueue()
    {
        if (targetQueue.Count > 0 && targetQueue.Peek() != null && targetQueue.Peek().TryGetComponent(out Player player))
        {
            if (player.IsDead()) { targetQueue.Pop(); }
        }
        while (targetQueue.Count > 0 && (targetQueue.Peek() == null))
        {
            targetQueue.Pop();
        }

        if (targetQueue.Count > 0)
        {
            target = targetQueue.Peek();
        }
        else
        {
            target = null;
        }

    }

    // --------------------------------------- Entity Actions -------------------------------------- //

    protected virtual void Move()
    {
        if (target == null || moveTimer.Update()) { return; } // Check Target
        if (agent.hasPath && agent.remainingDistance >= (entitySO.behaviour.stopRange / 2)) { return; }

        if (Vector3.Distance(target.position, transform.position) > entitySO.behaviour.stopRange)
        {
            Vector3 position = ((target.position - transform.position).normalized * entitySO.behaviour.stopRange) + transform.position;
            agent.SetDestination(position);
        }
        else
        {
            agent.SetDestination(target.position);
        }
    }

    protected virtual void Attack()
    {
        if (fireTimer.Update())
        {
            weapon.Fire();
            fireTimer.ChangeTime(traitsController.GetFireRate().amount + UnityEngine.Random.Range(-0.1f, 0.1f)); // Randomize Fire Rate
        }
    }

    protected void Look(Vector2 direction)
    {
        // Calculate Where to Look
        float targetAngle = (Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x)) - 90f;

        if (float.IsNaN(targetAngle) || float.IsInfinity(targetAngle)) { return; }

        currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref rotationVelocity, rotationSmoothTime);

        // Apply Rotation
        weapon.transform.rotation = Quaternion.Euler(0, 0, currentAngle); // Weapon Looks At Target
    }

    protected void PredictAim()
    {
        if (target == null) { return; }

        Vector3 targetVelocity = Vector3.zero;

        if (target.TryGetComponent(out IAgent iagent))
        {
            targetVelocity = iagent.GetVelocity();
        }

        float timeAhead = (target.position - transform.position).magnitude / weapon.GetInfo().speed;

        // Predict just the target's future position, no agent scaling
        Vector2 predictedPos = (Vector2)target.position + (Vector2)(targetVelocity * timeAhead);

        // Get the Normalized Pos of the target
        Vector2 normalizedPos = (predictedPos - (Vector2)transform.position).normalized;

        Look(normalizedPos);
    }

    protected void Rome()
    {
        if (agent.hasPath && agent.remainingDistance >= (entitySO.behaviour.stopRange / 2)) { return; }

        if (romeTimer.Update())
        {
            agent.SetDestination(GetRandomPoint(transform.position));
        }
    }

    protected virtual void Seek()
    {
        if (target == null) { return; }

        float distance = Vector2.Distance(target.transform.position, transform.position);
        if (!agent.hasPath)
        {
            agent.SetPath(path);
            lastTargetPos = target.position;
        }

        if (agent.remainingDistance <= entitySO.behaviour.startRange)
        {
            // Target Direction
            Vector2 direction = (target.position - transform.position).normalized;
            //Vector2 rayOriginOffset = (Vector2)transform.position + direction * (myCollider.radius + 1f);

            // Raycast Check
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, entitySO.behaviour.sight, ~ignoreLayer);
            if (hit.collider == null) { return; }
            targetQueue.Push(hit.collider.transform);
        }

        Look((lastTargetPos - transform.position).normalized);
    }

    protected void Flee()
    {
        if (target == null || moveTimer.Update()) { return; } // Check Target

        Vector2 destination = (transform.position - target.position).normalized + transform.position; // Get Flee Position
        FindValidPath(destination);

        if (Physics2D.Raycast(transform.position, (target.position - transform.position).normalized, 1, ~ignoreLayer).collider != null)
        {
            agent.SetPath(path);
        }
    }


    private bool SeeTarget(Transform tempTarget) // Adjusts itself to get visual on target
    {
        // ITarget Info
        cachedTarget = tempTarget.GetComponent<ITarget>();

        // Target Direction
        Vector2 direction = (tempTarget.position - transform.position).normalized;

        // Raycast Check
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, castSize, 0, direction, entitySO.behaviour.sight, ~ignoreLayer);

        // If Correct Target Is Hit Then It Is Visible
        if (hit.collider == null) { return false; }
        if (hit.collider.TryGetComponent(out ITarget itarget))
        {
            return itarget.GetTarget().ID == cachedTarget.GetTarget().ID;
        }

        return false;
    }

    protected virtual void Death()
    {
        if (traitsController.GetHealth().amount > 0) { return; }

        DropCollectibles();

        OnDeath?.Invoke(this, EventArgs.Empty);
        GameManager.instance.EnemyDeathCounter();

        Destroy(gameObject);
    }

    private void ResetEntity()
    {
        agent.enabled = false;
        SetLevel(1);
        traitsController.MaxHealth();
        EnemyPoolManager.Instance.ReturnEnemy(gameObject);
    }

    protected void DropCollectibles()
    {
        foreach (DroppedItem drop in entitySO.drops.itemDrops)
        {
            int newMax = drop.maxAmount + (inventoryController.GetLevelInfo().level - 1); // Enemy level increase drops
            SpawnCollectible(drop.item.obj, UnityEngine.Random.Range(drop.minAmount, newMax + 1)); // higher levels drop more // Max is exclusive on random
        }
    }

    private void SpawnCollectible(GameObject obj, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Instantiate(obj, transform.position, obj.transform.rotation);
        }
    }

    // ----------------------------- State Control --------------------------------- //
    protected virtual void UpdateState()
    {
        //Debug.Log(target + " " + state);
        // Update State
        if (target == null)
        {
            state = enemyState.idle; // No Target
        }
        else if (!agent.isActiveAndEnabled)
        {
            state = enemyState.attack;
        }
        else // Track Target
        {
            float distance = Vector2.Distance(target.transform.position, transform.position);

            if (SeeTarget(target))
            {
                // State Depending on distance

                if (distance <= entitySO.behaviour.startRange)
                {
                    state = enemyState.flee;
                }
                else if (distance <= entitySO.behaviour.stopRange)
                {
                    state = enemyState.attack;

                    if (agent.hasPath)
                    {
                        agent.ResetPath();
                    }
                }
                else
                {
                    state = enemyState.pursue;
                }
            }
            else
            {
                FindValidPath(target.position);
                state = enemyState.follow;

                if (agent.hasPath && agent.remainingDistance <= entitySO.behaviour.startRange)
                {
                    state = enemyState.seek;
                }
                /*
                if (path.status == NavMeshPathStatus.PathPartial)
                {
                    state = enemyState.seek;
                }
                else
                {
                    state = enemyState.follow;

                    if(agent.remainingDistance <= entitySO.behaviour.startRange)
                    {
                        state = enemyState.seek;
                    }
                }*/
            }
        }
    }

    protected void FindValidPath(Vector3 destination)
    {
        path = new NavMeshPath();
        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, path);
        }
        else
        {
            NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path);
            Debug.Log("Could Not Sample Position - May Cause Issues");
        }
    }

    protected virtual void RunState()
    {
        if (!agent.isActiveAndEnabled)
        {
            PredictAim();
            Attack();
            return;
        }

        if (agent == null || !agent.isOnNavMesh) return; // Check agent

        // Run State Action
        switch (state)
        {
            default:
            case enemyState.idle:
                Look(agent.desiredVelocity);
                //Rome();
                break;

            case enemyState.follow:
                PredictAim();
                Move();
                break;

            case enemyState.seek:
                Seek();
                break;

            case enemyState.pursue:
                PredictAim();
                Move();
                Attack();
                break;

            case enemyState.attack:
                PredictAim();
                Attack();
                break;

            case enemyState.flee:
                PredictAim();
                Flee();
                Attack();
                break;
        }
    }

    // ------------------------------ Outside Interactions ----------------------- //

    public void Damage(int value)
    {
        traitsController.RemoveHealth(value);

        OnHit?.Invoke(this, EventArgs.Empty);
    }

    public virtual void Knockback(Vector2 direction, float value)
    {
        StartCoroutine(PhysicsActivated(direction * value));
    }

    public void AddExperience(int value)
    {
        int level = inventoryController.GetLevelInfo().level;
        inventoryController.AddExperienceValue(value);

        // If the Level Changed
        int currentLevel = inventoryController.GetLevelInfo().level;
        if (level != currentLevel)
        {
            traitsController.MaxHealth();
            traitsController.SetAllAttributeLevels(currentLevel);
            ApplyAttributes();
        }
    }

    public void SetLevel(int level)
    {
        inventoryController.SetLevel(level);

        int currentLevel = inventoryController.GetLevelInfo().level;
        traitsController.SetAllAttributeLevels(currentLevel);
        ApplyAttributes();
    }

    public void AddMoney(int value)
    {
        inventoryController.ChangeMoneyValue(value);
    }

    public void UpdateVisuals()
    {
        OnVisualChange?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyAttributes()
    {
        fireTimer = new Timer(traitsController.GetFireRate().amount); // Fire Rate
        agent.speed = traitsController.GetSpeed().amount; // Speed
    }

    // ------------------------------- Get Functions ----------------------------- //

    private Vector2 GetRandomPoint(Vector2 center)
    {
        Vector2 randomPoint = center + UnityEngine.Random.insideUnitCircle * entitySO.behaviour.sight;
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, entitySO.behaviour.sight, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return transform.position;
    }

    public Vector3 GetVelocity()
    {
        return agent.velocity;
    }

    public float GetSpeed()
    {
        return agent.speed;
    }

    public RegistrySO GetTarget()
    {
        return entitySO;
    }

    public Weapon GetWeapon()
    {
        return weapon;
    }

    public EntitySO GetEntitySO()
    {
        return entitySO;
    }

    public TraitsController GetTraitsController()
    {
        return traitsController;
    }

    public InventoryController GetInventoryController()
    {
        return inventoryController;
    }

    // ----------------------------------- Register Entity ---------------------------------- //

    private void OnEnable()
    {
        Registry.Register(gameObject);
    }

    private void OnDisable()
    {
        Registry.Unregister(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (entitySO == null) { return; }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, entitySO.behaviour.sight);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, entitySO.behaviour.stopRange);

        if (target == null) { return; }

        // Target Direction
        Vector2 direction = (target.position - transform.position).normalized;
        Vector2 rayOriginOffset = (Vector2)transform.position + direction * (myCollider.radius + 1f);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(rayOriginOffset, (direction * entitySO.behaviour.sight) + (Vector2)transform.position);
    }

    private IEnumerator PhysicsActivated(Vector2 force)
    {
        agent.enabled = false;
        rb.bodyType = RigidbodyType2D.Dynamic;

        rb.AddForce(force, ForceMode2D.Impulse);
        yield return new WaitUntil(() => rb.linearVelocity.sqrMagnitude < 25f);

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        agent.enabled = true;
    }
}