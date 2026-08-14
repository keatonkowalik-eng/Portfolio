using UnityEngine;

[CreateAssetMenu(fileName = "EntitySO", menuName = "Scriptable Objects/EntitySO")]
public class EntitySO : RegistrySO
{
    public Traits traits = new Traits
    {
        health = new UpgradeAttribute<int>
        {
            amount = 5,
            upgradeAmount = 5,
            level = 1
        },

        healRate = new UpgradeAttribute<float>
        {
            amount = 3f,
            upgradeAmount = 0.1f,
            level = 1
        },

        speed = new UpgradeAttribute<float>
        {
            amount = 15f,
            upgradeAmount = 1f,
            level = 1
        },

        fireRate = new UpgradeAttribute<float>
        {
            amount = 1.5f,
            upgradeAmount = 0.1f,
            level = 1
        }
    };

    public Inventory inventory = new Inventory
    {
        money = 0,
        levels = new LevelData
        {
            level = 1,
            maxLevel = 10,
            experience = 0,
            experienceCap = 1,
            capMulitplier = 1
        }
    };

    public EntityBehavior behaviour = new EntityBehavior
    {
        stopRange = 30,
        startRange = 20,
        sight = 50
    };

    public int startSpawningWave = 0;

    public EntityDropsSO drops;

    public TargetingSO targetingSO;

    public LayerMask targetLayers;
}