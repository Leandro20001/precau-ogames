namespace TuxSpace.Entities;

public class Explosion : Entity
{
    /// <summary>
    /// Indica se a animação da explosão terminou.
    /// </summary>
    public bool Finished => Animation?.IsFinished ?? true;

    public override void Update(float delta)
    {
        Animation?.Update(delta);
    }
}