using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TuxSpace.Core;

public static class CustomCursorFactory
{
    private static readonly List<IntPtr> CursorHandles = [];

    static CustomCursorFactory()
    {
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            foreach (IntPtr handle in CursorHandles)
                DestroyCursor(handle);
        };
    }

    public static Cursor LoadFromPng(string path)
    {
        using Image source = Image.FromFile(path);
        using Bitmap cursorImage = new(40, 58, PixelFormat.Format32bppArgb);

        using (Graphics graphics = Graphics.FromImage(cursorImage))
        {
            graphics.Clear(Color.Transparent);
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(source, new Rectangle(0, 0, cursorImage.Width, cursorImage.Height));
        }

        IntPtr iconHandle = cursorImage.GetHicon();
        try
        {
            if (!GetIconInfo(iconHandle, out IconInfo iconInfo))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            try
            {
                iconInfo.IsIcon = false;
                iconInfo.HotspotX = 3;
                iconInfo.HotspotY = 3;

                IntPtr cursorHandle = CreateIconIndirect(ref iconInfo);
                if (cursorHandle == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                try
                {
                    CursorHandles.Add(cursorHandle);
                    return new Cursor(cursorHandle);
                }
                catch
                {
                    CursorHandles.Remove(cursorHandle);
                    DestroyCursor(cursorHandle);
                    throw;
                }
            }
            finally
            {
                if (iconInfo.ColorBitmap != IntPtr.Zero)
                    DeleteObject(iconInfo.ColorBitmap);
                if (iconInfo.MaskBitmap != IntPtr.Zero)
                    DeleteObject(iconInfo.MaskBitmap);
            }
        }
        finally
        {
            DestroyIcon(iconHandle);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IconInfo
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool IsIcon;

        public uint HotspotX;
        public uint HotspotY;
        public IntPtr MaskBitmap;
        public IntPtr ColorBitmap;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetIconInfo(IntPtr icon, out IconInfo iconInfo);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CreateIconIndirect(ref IconInfo iconInfo);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(IntPtr icon);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyCursor(IntPtr cursor);

    [DllImport("gdi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject(IntPtr handle);
}
