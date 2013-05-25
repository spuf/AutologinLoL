using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutologinLoL
{
    static class WinAPI
    {
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
       
        [DllImport("User32.dll")]
        public static extern Int32 SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("User32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, out Rectangle lpRect);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern Int32 ReleaseDC(IntPtr hWnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        public static Color GetPixelColor(IntPtr hWnd, int x, int y)
        {
            IntPtr hdc = GetDC(hWnd);
            uint pixel = GetPixel(hdc, x, y);
            ReleaseDC(hWnd, hdc);
            Color color = Color.FromArgb((int)(pixel & 0x000000FF), (int)(pixel & 0x0000FF00) >> 8, (int)(pixel & 0x00FF0000) >> 16);
            return color;
        }
        
        public static void KillProcess(string name)
        {
            Process[] tempProcesses = Process.GetProcessesByName(name);
            if (tempProcesses.Length != 0)
            {
                foreach (Process process in tempProcesses)
                {
                    try
                    {
                        process.Kill();
                        process.Dispose();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error!");
                    }
                }
            }
        }

        public static Process FindProcess(string name) 
        {
            Process[] tempProcesses = Process.GetProcessesByName(name);
            if (tempProcesses.Length != 0)
            {
                return tempProcesses[0];
            }
            return null;
        }

        public static void MouseClick(IntPtr hWnd, int x, int y)
        {
            int wParam = y << 16 | x;
            SendMessage(hWnd, WM_LBUTTONDOWN, 1, wParam);
            SendMessage(hWnd, WM_LBUTTONUP, 0, wParam);
        }

    }
}
