using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BgService2
{
    internal class Program
    {
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_SHOWWINDOW = 0x0040;
        public struct RECT
        {
            public readonly int left;
            public readonly int top;
            public readonly int right;
            public readonly int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWINFO
        {
            public uint cbSize;
            public RECT rcWindow;
            public RECT rcClient;
            public uint dwStyle;
            public uint dwExStyle;
            public uint dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            //public ushort atomWindowType;
            public ushort wCreatorVersion;

            public WINDOWINFO(Boolean? filler) : this()   // Allows automatic initialization of "cbSize" with "new WINDOWINFO(null/true/false)".
            {
                cbSize = (UInt32)(Marshal.SizeOf(typeof(WINDOWINFO)));
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        private static void Main()
        {
            IntPtr notePadHandle;
            IntPtr skypeHandle;
            try
            {
                var proc = GetProcess("Notepad");
                notePadHandle = proc.MainWindowHandle;
                proc = GetProcess("Skype");
                skypeHandle = proc.MainWindowHandle;
                if (notePadHandle != IntPtr.Zero)
                {
                    Console.WriteLine("SETFORGROUND");
                    WINDOWINFO info = new WINDOWINFO();
                    info.cbSize = (uint)Marshal.SizeOf(info);
                    //SetForegroundWindow(skypeHandle);
                    //SetWindowPos(notePadHandle, IntPtr.Zero, 100, 100, 100, 100, SWP_NOZORDER | SWP_NOSIZE | SWP_SHOWWINDOW);
                    while (true)
                    {
                        GetWindowInfo(skypeHandle, ref info);
                        //Console.WriteLine(info.dwWindowStatus);
                        if (info.dwWindowStatus == 1)
                        {
                            RECT skypeInfo = info.rcWindow;
                            Console.WriteLine($"top: {skypeInfo.top} \tbottom: {skypeInfo.bottom} \tleft: {skypeInfo.left} \tright: {skypeInfo.right}");
                            int lf = skypeInfo.left;
                            int tp = skypeInfo.top;
                            if (lf > 300)
                            {
                                SetWindowPos(notePadHandle, IntPtr.Zero, lf - 300, tp, skypeInfo.bottom, skypeInfo.bottom, SWP_NOZORDER | SWP_NOSIZE | SWP_SHOWWINDOW);
                            }
                            else
                            {
                                SetWindowPos(notePadHandle, IntPtr.Zero, skypeInfo.right, tp, skypeInfo.bottom, skypeInfo.bottom, SWP_NOZORDER | SWP_NOSIZE | SWP_SHOWWINDOW);
                            }

                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            Console.ReadKey();

        }

        private static Process GetProcess(string program)
        {
            Process[] currentProcesses = Process.GetProcessesByName(program);
            foreach (var item in currentProcesses)
            {
                if (item.MainWindowTitle.Contains(program))
                {
                    Console.WriteLine("MainWindowTitle: {0}", item.MainWindowTitle);
                    Console.WriteLine("MainWindowTitle: {0}", item.Container);
                    return item;
                }
            }
            return new Process();
        }

    }
}
