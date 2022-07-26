using System;
using System.Runtime.InteropServices;

namespace ShiftScreen.Utils;

public enum ExtendedWindowStyles
{
    ToolWindow = 0x00000080
}

public enum GetWindowLongFields
{
    GWL_EXSTYLE = (-20)
}

public class Window32
{
    public static void SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        int error = 0;
        IntPtr result = IntPtr.Zero;

        if (IntPtr.Size == 4)
        {
            // use SetWindowLong
            Int32 tempResult = User32.IntSetWindowLong(hWnd, nIndex, unchecked((int)dwNewLong.ToInt64()));
            error = Marshal.GetLastWin32Error();
            result = new IntPtr(tempResult);
        }
        else
        {
            // use SetWindowLongPtr
            result = User32.IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
            error = Marshal.GetLastWin32Error();
        }

        if ((result == IntPtr.Zero) && (error != 0))
        {
            throw new System.ComponentModel.Win32Exception(error);
        }
    }
}