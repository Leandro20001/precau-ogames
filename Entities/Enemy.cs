namespace TuxSpace.Entities;

public class Enemy : Entity
{
    public float Speed { get; set; } = 220f;
    public float VerticalSpeed { get; set; } = 180f;
    public int VerticalDirection { get; set; } = 1;
    public float ShootCooldown { get; set; } = 1f;

    public void Update(float delta, int screenHeight)
    {
        Y += VerticalDirection * VerticalSpeed * delta;
        X -= Speed * delta;

        if (Y <= 0)
        {
            Y = 0;
            VerticalDirection = 1;
        }
        else if (Y + Height >= screenHeight)
        {
            Y = screenHeight - Height;
            VerticalDirection = -1;
        }

        Animation?.Update(delta);
    }
}
