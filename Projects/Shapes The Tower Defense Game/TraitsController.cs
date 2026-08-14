using UnityEngine;

public class TraitsController
{
    private Traits origin; // Default Values
    private Traits current; // Real time values

    private Timer healTimer;

    public TraitsController(Traits traits)
    {
        origin = traits;
        this.current = traits;

        ApplyLevels();
        SetUpTimers();
    }

    public TraitsController(Traits current, Traits origin)
    {
        this.origin = origin;
        this.current = current;

        ApplyLevels();
        SetUpTimers();
    }

    public TraitsController(Traits current, Traits origin, Traits levelUp)
    {
        this.origin = origin;
        this.current = current;

        ApplyLevels();
        SetUpTimers();
    }

    private void SetUpTimers()
    {
        healTimer = new Timer(current.healRate.amount);
    }

    // ---------------------------- Health Functions ------------------------------ //
    public void RemoveHealth(int value)
    {
        if (current.health.amount <= 0) return; // Don't Go Lower than 0
        current.health.amount -= value; // Remove Health
    }

    public void AddHealth(int value)
    {
        if (current.health.amount >= GetMaxHealth()) return; // Don't Go Above Max Health
        current.health.amount += value; // Add Health
    }

    public void MaxHealth()
    {
        current.health.amount = GetMaxHealth();
    }

    // ----------------------------- Recovery Functions ------------------------------ //

    public void Heal()
    {
        if (healTimer.Update())
        {
            AddHealth(1);
        }
    }

    // ---------------------------- Level Up Attributes ----------------------------- //

    public void SetAllAttributeLevels(int level)
    {
        current.health.level = level;
        current.healRate.level = level;

        current.fireRate.level = level;
        current.speed.level = level;

        current.damage.level = level;
        current.knockback.level = level;
        current.bulletSpeed.level = level;
        current.explosiveRange.level = level;

        ApplyLevels();
    }

    public void SetHealth(int level)
    {
        current.health.level = level;
        ApplyLevels();
    }

    public void SetHealRate(int level)
    {
        current.healRate.level = level;
        ApplyLevels();
    }

    public void SetFireRate(int level)
    {
        current.fireRate.level = level;
        ApplyLevels();
    }

    public void SetSpeed(int level)
    {
        current.speed.level = level;
        ApplyLevels();
    }

    public void SetDamage(int level)
    {
        current.damage.level = level;
        ApplyLevels();
    }

    public void SetKnockback(int level)
    {
        current.knockback.level = level;
        ApplyLevels();
    }

    public void SetBulletSpeed(int level)
    {
        current.bulletSpeed.level = level;
        ApplyLevels();
    }

    public void SetExplosiveRange(int level)
    {
        current.explosiveRange.level = level;
        ApplyLevels();
    }

    private void ApplyLevels()
    {
        current.healRate.amount = origin.healRate.amount - current.healRate.upgradeAmount * (current.healRate.level - 1);
        current.fireRate.amount = origin.fireRate.amount - current.fireRate.upgradeAmount * (current.fireRate.level - 1);

        current.speed.amount = GetMaxSpeed();

        current.damage.amount = origin.damage.amount + current.damage.upgradeAmount * (current.damage.level - 1);
        current.knockback.amount = origin.knockback.amount + current.knockback.upgradeAmount * (current.knockback.level - 1);
        current.bulletSpeed.amount = origin.bulletSpeed.amount + current.bulletSpeed.upgradeAmount * (current.bulletSpeed.level - 1);
        current.explosiveRange.amount = origin.explosiveRange.amount + current.explosiveRange.upgradeAmount * (current.explosiveRange.level - 1);

        SetUpTimers();
    }

    public int GetMaxHealth()
    {
        return origin.health.amount + current.health.upgradeAmount * (current.health.level - 1);
    }

    public float GetMaxSpeed()
    {
        return origin.speed.amount + current.speed.upgradeAmount * (current.speed.level - 1);
    }

    public UpgradeAttribute<int> GetHealth()
    {
        return current.health;
    }

    public UpgradeAttribute<float> GetSpeed()
    {
        return current.speed;
    }

    public UpgradeAttribute<float> GetFireRate()
    {
        return current.fireRate;
    }

    public UpgradeAttribute<float> GetHealRate()
    {
        return current.healRate;
    }

    public UpgradeAttribute<int> GetDamage()
    {
        return current.damage;
    }

    public UpgradeAttribute<float> GetKnockback()
    {
        return current.knockback;
    }

    public UpgradeAttribute<float> GetBulletSpeed()
    {
        return current.bulletSpeed;
    }

    public UpgradeAttribute<float> GetExplosiveRange()
    {
        return current.explosiveRange;
    }

    public Traits GetCurrentTraits()
    {
        return current;
    }

    public Traits GetOriginTraits()
    {
        return origin;
    }

}