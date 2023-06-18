using System.Drawing;
using System.Runtime.InteropServices;

namespace Reclaimer.Utilities
{
    internal static class NativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern short GetAsyncKeyState(int KeyID);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out Point Point);

        [DllImport("user32.dll")]
        public static extern int SetCursorPos(int X, int Y);
    }
}
