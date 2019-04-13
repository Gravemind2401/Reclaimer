using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Utils
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
