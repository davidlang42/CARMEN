using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Carmen.Desktop.Windows
{
    /// <summary>
    /// Static calls to external windows functions (Winuser.h)
    /// </summary>
    public static class WinUser
    {
        // In Winuser.h terms, "long" means 32-bit integer, which is called "int" in C#

        /// <summary>Reference: https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-enablewindow </summary>
        [DllImport("user32.dll")]
        public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

        /// <summary>Reference: https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowlongptra </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        /// <summary>Reference: https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowlongptra </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        #region Window styles (https://docs.microsoft.com/en-us/windows/win32/winmsg/window-styles)
        public const int GWL_STYLE = -16;
        public const int WS_DLGFRAME = 0x00400000;
        public const int WS_MAXIMIZEBOX = 0x10000;
        public const int WS_MINIMIZEBOX = 0x20000;
        #endregion

        #region Extended window styles (https://docs.microsoft.com/en-us/windows/win32/winmsg/extended-window-styles)
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_DLGMODALFRAME = 0x00000001;
        public const int WS_EX_STATICEDGE = 0x00020000;
        public const int WS_EX_NOACTIVATE = 0x08000000;
        public const int WS_EX_TOPMOST = 0x00000008;
        #endregion

        #region Helper functions
        public static bool AddWindowFlag(IntPtr hWnd, int nIndex, int flag)
        {
            var new_value = new IntPtr(GetWindowLongPtr(hWnd, nIndex).ToInt32() | flag);
            var result = SetWindowLongPtr(hWnd, nIndex, new_value);
            return result.ToInt32() != 0;
        }

        public static bool RemoveWindowFlag(IntPtr hWnd, int nIndex, int flag)
        {
            var new_value = new IntPtr(GetWindowLongPtr(hWnd, nIndex).ToInt32() & ~flag);
            var result = SetWindowLongPtr(hWnd, nIndex, new_value);
            return result.ToInt32() != 0;
        }
        #endregion
    }
}
