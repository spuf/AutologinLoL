using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace AutologinLoL
{
    public partial class MainForm : Form
    {
        public List<Account> accounts = new List<Account>();
        public Game game = new Game(@"C:\Games\League of Legends");//Application.StartupPath);
        private string settingsPath = Path.Combine(Application.StartupPath, "AutologinLoL.xml");
        XmlSerializer serializer = new XmlSerializer(typeof(List<Account>));

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Load accounts
            using (FileStream file = File.OpenRead(settingsPath))
            {
                accounts = (List<Account>)serializer.Deserialize(file);
            }

            // Populate form
            Size size = new Size(190, 29);
            for (int i = 0; i < accounts.Count; i++)
            {
                Button button = new Button();
                button.Location = new Point(12, 12 + i * (size.Height + 6));
                button.Size = size;
                button.Text = accounts[i].ToString();
                button.Tag = accounts[i];
                button.Click += button_Click;
                this.Controls.Add(button);
            }
            if (accounts.Count == 1)
            {
                Button button = new Button();
                button.Tag = accounts[0];
                button_Click(button, null);
            }
        }

        private void button_Click(object sender, EventArgs e)
        {
            ToggleButtons(false);
            Account account = (Account)((Button)sender).Tag;
            if (game.SetConfig(account.Server, account.Locale))
            {
                if (game.Start())
                {
                    loginWorker.RunWorkerAsync(account);                    
                }
            }             
        }

        private void ToggleButtons(bool state)
        {
            UseWaitCursor = state;
            for (int i = 0; i < Controls.Count; i++)
            {
                if (Controls[i].GetType() == typeof(Button))
                {
                    (Controls[i] as Button).Enabled = state;
                }
            }
        }

        private void loginWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Account account = (Account)e.Argument;
            bool complete = false;
            Thread.Sleep(300);
            while (!complete)
            {
                if ((sender as BackgroundWorker).CancellationPending || !game.IsProcessesExist())
                {
                    e.Cancel = true;
                    complete = true;
                }
                else
                {
                    Thread.Sleep(300);
                    if (game.Login(account.Login, account.Password))
                    {
                        complete = true;
                    }
                    else
                    {
                        game.ClickPlay();
                    }
                    
                }
            }
        }

        private void loginWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ToggleButtons(true); 
            if (!e.Cancelled)
            {
                Close();
            }
        }

    }
}
