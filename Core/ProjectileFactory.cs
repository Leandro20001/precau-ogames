using TuxSpace.Entities;

namespace TuxSpace.Core;

public static class ProjectileFactory
{
    public static Projectile CreatePlayerProjectile(Player player)
    {
        return new Projectile
        {
            X = player.X + player.Width,
            Y = player.Y + player.Height / 2f,
            Width = 30,
            Height = 15,
            Speed = 800f,
            DirectionX = 1f
        };
    }

    public static Projectile CreateEnemyProjectile(Enemy enemy)
    {
        return new Projectile
        {
            X = enemy.X,
            Y = enemy.Y + enemy.Height / 2f,
            Width = 20,
            Height = 10,
            Speed = 500f,
            DirectionX = -1f
        };
    }
}
