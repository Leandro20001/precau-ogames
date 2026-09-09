using System.Windows.Forms;

namespace TuxSpace.Entities;

public class Enemy : Entity
{
    public float Speed { get; set; } = 220f;
    public float TargetOffset { get; set; }
    public float WaveOffset { get; set; }
    public float WaveSpeed { get; set; } = 2f;
    public float FollowStrength { get; set; } = 0.5f;
    public float AttackDistance { get; set; } = 300f;
    public float ShootCooldown { get; set; } = 1f;

    public void Update(float delta, Player player)
    {
        float targetY = player.Y + TargetOffset;

        Y += (targetY - Y) * FollowStrength * delta;
        Y += MathF.Sin(
            (Environment.TickCount / 1000f) * WaveSpeed + WaveOffset)
            * 50f * delta;

        X -= Speed * delta;

        if (Y < 0)
            Y = 0;

        int screenHeight =
            Screen.PrimaryScreen?.Bounds.Height ?? 1080;

        if (Y + Height > screenHeight)
            Y = screenHeight - Height;

        Animation?.Update(delta);
    }
}
