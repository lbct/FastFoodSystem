﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FastFoodSystem.Scripts
{
    class UnsafeNative
    {
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential)]
        private struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpData;
        }

        private const int WM_COPYDATA = 0x004A;

        public static string GetMessage(int message, IntPtr lParam)
        {
            if (message == UnsafeNative.WM_COPYDATA)
            {
                try
                {
                    var data = Marshal.PtrToStructure<UnsafeNative.COPYDATASTRUCT>(lParam);
                    var result = string.Copy(data.lpData);
                    return result;
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

        public static void SendMessage(IntPtr hwnd, string message)
        {
            var messageBytes = Encoding.Unicode.GetBytes(message);
            var data = new UnsafeNative.COPYDATASTRUCT
            {
                dwData = IntPtr.Zero,
                lpData = message,
                cbData = messageBytes.Length + 1 /* +1 because of \0 string termination */
            };

            if (UnsafeNative.SendMessage(hwnd, WM_COPYDATA, IntPtr.Zero, ref data) != 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}
