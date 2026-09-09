using TuxSpace.Core;
using System.Drawing;

namespace TuxSpace.Entities;

public abstract class Entity
{
    public float X { get; set; }
    public float Y { get; set; }

    public int Width { get; set; } = 50;
    public int Height { get; set; } = 50;

    public int HP { get; set; } = 100;

    public Image? Sprite { get; set; }
    public Animation? Animation { get; set; }

    public RectangleF Bounds =>
        new(X, Y, Width, Height);

    public virtual void Update(float delta)
    {
        Animation?.Update(delta);
    }

    public virtual void Draw(Graphics graphics)
    {
        if (Animation?.CurrentImage != null)
        {
            graphics.DrawImage(
                Animation.CurrentImage,
                X, Y, Width, Height);
        }
        else if (Sprite != null)
        {
            graphics.DrawImage(
                Sprite,
                X, Y, Width, Height);
        }
    }

    public virtual void TakeDamage(int damage)
    {
        if (damage > 0)
            HP -= damage;
    }

    public bool IsAlive => HP > 0;
}
