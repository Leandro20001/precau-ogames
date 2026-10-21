namespace TuxSpace.Entities;

public class Projectile : Entity
{
    public float Speed { get; set; } = 500f;
    public float DirectionX { get; set; } = 1f;
    public int Damage { get; set; }
    public bool IsSpecial { get; set; }

    public override void Update(float delta)
    {
        X += Speed * DirectionX * delta;
    }
}
