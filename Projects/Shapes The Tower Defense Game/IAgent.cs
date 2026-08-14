using UnityEngine;

public interface IAgent
{
    public abstract void InitAgentData();
    public abstract void Damage(int value);
    public abstract void Knockback(Vector2 direction, float value);

    public abstract void UpdateVisuals();
    public abstract Vector3 GetVelocity();
    public abstract TraitsController GetTraitsController();
    public abstract InventoryController GetInventoryController();
}