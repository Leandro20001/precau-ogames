using System.Drawing;

namespace TuxSpace.Core;

public class Animation
{
    public List<Image> Frames { get; } = new();

    public int CurrentFrame { get; private set; }

    /// <summary>
    /// Tempo entre cada frame, em segundos.
    /// </summary>
    public float FrameTime { get; set; } = 0.1f;

    /// <summary>
    /// Indica se a animação deve reiniciar ao chegar ao último frame.
    /// </summary>
    public bool Loop { get; set; } = true;

    /// <summary>
    /// Indica se uma animação que não está em Loop terminou.
    /// </summary>
    public bool IsFinished { get; private set; }

    private float timer;

    /// <summary>
    /// Atualiza a animação.
    /// </summary>
    public void Update(float delta)
    {
        if (Frames.Count == 0 || IsFinished)
            return;

        timer += delta;

        while (timer >= FrameTime)
        {
            timer -= FrameTime;
            CurrentFrame++;

            if (CurrentFrame >= Frames.Count)
            {
                if (Loop)
                {
                    CurrentFrame = 0;
                }
                else
                {
                    CurrentFrame = Frames.Count - 1;
                    IsFinished = true;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Imagem atualmente exibida.
    /// </summary>
    public Image? CurrentImage
    {
        get
        {
            if (Frames.Count == 0)
                return null;

            return Frames[CurrentFrame];
        }
    }

    /// <summary>
    /// Cria uma cópia da animação.
    /// </summary>
    public Animation Clone()
    {
        Animation copy = new()
        {
            FrameTime = FrameTime,
            Loop = Loop
        };

        foreach (var frame in Frames)
            copy.Frames.Add(frame);

        return copy;
    }

    /// <summary>
    /// Reinicia a animação.
    /// </summary>
    public void Reset()
    {
        CurrentFrame = 0;
        timer = 0f;
        IsFinished = false;
    }

    /// <summary>
    /// Configura a animação para durar exatamente
    /// a quantidade de segundos informada.
    /// </summary>
    public void SetDuration(float duration)
    {
        if (Frames.Count == 0 || duration <= 0)
            return;

        FrameTime = duration / Frames.Count;
    }
}