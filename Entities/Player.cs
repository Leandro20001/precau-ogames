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
    public float Speed { get; set; } = 350f;

    public float Acceleration { get; set; } = 900f;
    public float AutomaticDeceleration { get; set; } = 600f;
    public float DeadZone { get; set; } = 50f;

    private float velocityX;
    private float velocityY;

    public void UpdateMovement(float horizontalInput, float verticalInput, float delta)
    {
        float inputLength = MathF.Sqrt(
            horizontalInput * horizontalInput + verticalInput * verticalInput
        );

        if (inputLength > 1f)
        {
            horizontalInput /= inputLength;
            verticalInput /= inputLength;
        }

        float targetVelocityX = horizontalInput * Speed;
        float targetVelocityY = verticalInput * Speed;

        velocityX = MoveTowards(
            velocityX,
            targetVelocityX,
            (horizontalInput == 0 ? AutomaticDeceleration : Acceleration) * delta
        );
        velocityY = MoveTowards(
            velocityY,
            targetVelocityY,
            (verticalInput == 0 ? AutomaticDeceleration : Acceleration) * delta
        );

        if (MathF.Abs(velocityX) <= DeadZone && horizontalInput == 0)
            velocityX = 0;
        if (MathF.Abs(velocityY) <= DeadZone && verticalInput == 0)
            velocityY = 0;

        X += velocityX * delta;
        Y += velocityY * delta;
    }

    public void ClampToBounds(float screenWidth, float screenHeight)
    {
        if (X < 0)
        {
            X = 0;
            velocityX = 0;
        }
        else if (X + Width > screenWidth)
        {
            X = Math.Max(0, screenWidth - Width);
            velocityX = 0;
        }

        if (Y < 0)
        {
            Y = 0;
            velocityY = 0;
        }
        else if (Y + Height > screenHeight)
        {
            Y = Math.Max(0, screenHeight - Height);
            velocityY = 0;
        }
    }

    private static float MoveTowards(float current, float target, float maxDelta)
    {
        if (MathF.Abs(target - current) <= maxDelta)
            return target;

        return current + MathF.Sign(target - current) * maxDelta;
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

    public bool TrySpendAllRAM(int minimumAmount)
    {
        if (minimumAmount <= 0 || RAM < minimumAmount)
            return false;

        RAM = 0;
        return true;
    }

    public override void TakeDamage(int damage)
    {
        if (damage <= 0)
            return;

        HP = Math.Max(0, HP - damage);
    }
}
