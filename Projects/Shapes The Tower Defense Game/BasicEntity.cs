public class BasicEntity : Entity, IAgent
{
    private void Awake()
    {
        OnAwake();
    }

    private void Update()
    {
        Death();
        RunState();
        traitsController.Heal();

        if (!delayTimer.Update()) return; // Updates after less often

        LocateTarget();
        UpdateState();
        UpdateVisuals();
    }
}