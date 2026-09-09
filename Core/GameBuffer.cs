using System.Windows.Forms;

namespace TuxSpace.Core;

public class GameBuffer : PictureBox
{
    public GameBuffer()
    {
        DoubleBuffered = true;

        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.UserPaint,
            true
        );
    }
}