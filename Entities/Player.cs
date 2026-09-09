namespace TuxSpace.Entities;

public class Player : Entity
{
    public int XP { get; set; }
    public int Level { get; private set; } = 1;
    public int XPToNextLevel { get; private set; } = 100;

    public int MaxHP { get; set; } = 5;
    public int RAM { get; set; } = 100;
    public int MaxRAM { get; set; } = 100;
    public int Damage { get; set; } = 10;

    public void Move(float dx, float dy, float delta)
    {
        X += dx * delta;
        Y += dy * delta;
    }

    public void GainExperience(int amount)
    {
        if (amount <= 0)
            return;

        XP += amount;
    }

    public bool CanLevelUp => XP >= XPToNextLevel;

    public void LevelUp()
    {
        if (!CanLevelUp)
            return;

        XP -= XPToNextLevel;
        Level++;
        XPToNextLevel += 50;
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
            return;

        HP = Math.Min(MaxHP, HP + amount);
    }

    public void RestoreRAM(int amount)
    {
        if (amount <= 0)
            return;

        RAM = Math.Min(MaxRAM, RAM + amount);
    }

    public override void TakeDamage(int damage)
    {
        if (damage <= 0)
            return;

        HP = Math.Max(0, HP - damage);
    }
}
