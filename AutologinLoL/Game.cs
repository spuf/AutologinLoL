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
using Microsoft.Win32;

namespace AutologinLoL
{
    public class Game
    {
        public readonly string BaseDir;
        private readonly string[] registryPaths = new string[] 
            {
                @"HKEY_CURRENT_USER\Software\Classes\VirtualStore\MACHINE\SOFTWARE\Wow6432Node\Riot Games\RADS", 
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Riot Games\RADS",
                @"HKEY_CLASSES_ROOT\VirtualStore\MACHINE\SOFTWARE\Wow6432Node\Riot Games\RADS", 
            };
        private readonly string launcherName = "LoLLauncher";
        private readonly string kernelName = "rads_user_kernel";
        private readonly string clientName = "LolClient";
        private readonly Point loginInput = new Point(350, 300);
        private readonly Point playButton = new Point(700, 550);
        private readonly Color loginColor = Color.FromArgb(242, 242, 243);

        public Game(string baseDir = "")
        {
            BaseDir = baseDir;
            if (CheckBaseDir())
                return;

            foreach (var path in registryPaths)
            {
                var radsFolder = Registry.GetValue(path, "LocalRootFolder", "") as string;
                baseDir = Path.GetDirectoryName(radsFolder);

                BaseDir = baseDir;
                if (CheckBaseDir())
                    return;
            }

            throw new Exception("Couldn't find League of Legends install");
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

        public void SetConfig(string server, string locale)
        {
            string systemCfg = GetSystemCfg();
            string launcherCfg = GetLauncherCfg();
            string localeCfg = GetLocaleCfg();

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
        }

        public void Start()
        {
            string exePath = GetExePath();

            KillProcesses();
            Process process = Process.Start(exePath);
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
                if (WinAPI.GetPixelColor(handle, loginInput.X, loginInput.Y) != loginColor)
                {
                    return false;
                }
                Thread.Sleep(500);
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

        private bool CheckBaseDir()
        { 
            if (!File.Exists(GetExePath()))
                return false;

            if (!File.Exists(GetSystemCfg()))
                return false;

            if (!File.Exists(GetLauncherCfg()))
                return false;

            if (!File.Exists(GetLocaleCfg()))
                return false;

            return true;
        }
    }
}
