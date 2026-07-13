using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpcUaViewer
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using var mutex = new System.Threading.Mutex(true, "OpcUaViewer_SingleInstance", out bool isNew);
            if (!isNew)
            {
                // Bring the existing window to the foreground
                var hwnd = NativeMethods.FindWindow(null, "OPC UA Viewer");
                if (hwnd != IntPtr.Zero)
                {
                    if (NativeMethods.IsIconic(hwnd))
                        NativeMethods.ShowWindow(hwnd, 9); // SW_RESTORE
                    NativeMethods.SetForegroundWindow(hwnd);
                }
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
            internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            internal static extern bool SetForegroundWindow(IntPtr hWnd);

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            internal static extern bool IsIconic(IntPtr hWnd);

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        }
    }
}
