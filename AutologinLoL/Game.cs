using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutologinLoL
{
    public class Game
    {
        public string BaseDir;
        private string launcherName = "LoLLauncher";
        private string kernelName = "rads_user_kernel";
        private string clientName = "LolClient";
        private Point loginInput = new Point(350, 320);
        private Point playButton = new Point(700, 550);

        public Game(string baseDir)
        {
            BaseDir = baseDir;
        }

        public string GetSystemCfg()
        {
            return Path.Combine(BaseDir, @"RADS\system\system.cfg");
        }

        public string GetLauncherCfg()
        {
            return Path.Combine(BaseDir, @"RADS\system\launcher.cfg");
        }

        public string GetLocaleCfg()
        {
            return Path.Combine(BaseDir, @"RADS\system\locale.cfg");
        }

        public string GetExePath()
        {
            return Path.Combine(BaseDir, @"lol.launcher.exe");
        }

        public bool SetConfig(string server, string locale)
        {
            string systemCfg = GetSystemCfg();
            string launcherCfg = GetLauncherCfg();
            string localeCfg = GetLocaleCfg();
            if (File.Exists(systemCfg) && File.Exists(launcherCfg) && File.Exists(localeCfg))
            {
                string content;

                content = File.ReadAllText(systemCfg);
                content = Regex.Replace(content, "(Region = ).+$", String.Format("$1{0}", server.ToUpper()), RegexOptions.Multiline);
                File.WriteAllText(systemCfg, content);

                content = File.ReadAllText(launcherCfg);
                content = Regex.Replace(content, "(airConfigProject = lol_air_client_config_).+$", String.Format("$1{0}", server), RegexOptions.Multiline);
                File.WriteAllText(launcherCfg, content);

                content = File.ReadAllText(localeCfg);
                content = Regex.Replace(content, "(locale = ).+$", String.Format("$1{0}", locale), RegexOptions.Multiline);
                File.WriteAllText(localeCfg, content);

                return true;
            }
            else
            {
                MessageBox.Show(String.Format("Not found LoL at path '{0}'!", BaseDir));
            }
            return false;
        }

        public bool Start()
        {
            string exePath = GetExePath();
            if (File.Exists(exePath))
            {
                KillProcesses();
                Process process = Process.Start(exePath);
                return true;
            }
            else
            {
                MessageBox.Show(String.Format("Not found launcher at path '{0}'!", exePath));
            }
            return false;
        }

        public bool Login(string login, string password)
        {
            Process process = WinAPI.FindProcess(clientName);
            if (process != null && process.Responding && process.MainWindowHandle != IntPtr.Zero && !String.IsNullOrEmpty(process.MainWindowTitle))
            {
                IntPtr handle = process.MainWindowHandle;
                if (WinAPI.GetForegroundWindow() != handle)
                {
                    WinAPI.SetForegroundWindow(handle);
                    WinAPI.ShowWindowAsync(handle, 1);
                    return false;
                }
                if (WinAPI.GetPixelColor(handle, loginInput.X, loginInput.Y) != Color.FromArgb(242, 242, 243))
                {
                    return false;
                }
                Thread.Sleep(300);
                for (int i = 0; i < 2; i++)
                    WinAPI.MouseClick(handle, loginInput.X, loginInput.Y);
                string keys = String.Format("{0}{{TAB}}{1}{{ENTER}}", login, password);
                if (Console.CapsLock)
                    keys = String.Format("{{CAPSLOCK}}{0}{{CAPSLOCK}}", keys);
                SendKeys.SendWait(keys);
                return true;
            }
            return false;
        }

        private void KillProcesses()
        {
            WinAPI.KillProcess(launcherName);
            WinAPI.KillProcess(kernelName);
            WinAPI.KillProcess(clientName);
        }

        public bool IsProcessesExist()
        {
            return !(WinAPI.FindProcess(launcherName) == null && WinAPI.FindProcess(kernelName) == null && WinAPI.FindProcess(clientName) == null);
        }

        public bool ClickPlay()
        {
            Process process = WinAPI.FindProcess(launcherName);
            if (process != null && process.Responding && process.MainWindowHandle != IntPtr.Zero && !String.IsNullOrEmpty(process.MainWindowTitle))
            {
                IntPtr handle = process.MainWindowHandle;
                WinAPI.MouseClick(handle, playButton.X, playButton.Y);
                return true;
            }
            return false;

        }
    }
}
