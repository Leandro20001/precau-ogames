using System.Drawing;

namespace TuxSpace.Entities;

public class LootPickup : Entity
{
    public const int PickupSize = 32;

    private float timeRemaining = 10f;

    public bool IsExpired => timeRemaining <= 0;

    public LootPickup(float x, float y, Image image)
    {
        X = x;
        Y = y;
        Width = PickupSize;
        Height = PickupSize;
        Sprite = image;
    }

    public override void Update(float delta)
    {
        timeRemaining -= delta;
    }

}
