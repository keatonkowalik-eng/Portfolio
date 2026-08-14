[System.Serializable]
public struct Traits
{
    public UpgradeAttribute<int> health;
    public UpgradeAttribute<float> healRate;

    public UpgradeAttribute<float> speed;
    public UpgradeAttribute<float> fireRate;

    public UpgradeAttribute<int> damage;
    public UpgradeAttribute<float> knockback;
    public UpgradeAttribute<float> bulletSpeed;
    public UpgradeAttribute<float> explosiveRange;
}

[System.Serializable]
public struct UpgradeAttribute<T>
{
    public T amount;
    public T upgradeAmount;
    public int level;
}

[System.Serializable]
public struct Inventory
{
    public int money;
    public int levelPoints;
    public LevelData levels;

    public Slot activeSlot;
    public Slot[] slots;
}

[System.Serializable]

public struct Slot
{
    public BulletSO bulletSO;
    public int amount;
}

[System.Serializable]
public struct LevelData
{
    public int level;
    public int maxLevel;
    public int experience;
    public int experienceCap;
    public int capMulitplier;
}

[System.Serializable]
public struct BulletInfo
{
    public float speed;
    public int damage;
    public float knockback;
    public float explosiveRange;
}

[System.Serializable]
public struct EntityBehavior
{
    public float stopRange;
    public float startRange;
    public float sight;
}