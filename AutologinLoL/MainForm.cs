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
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace AutologinLoL
{
   public partial class MainForm : Form
    {
        public List<Account> accounts = new List<Account>();
        public Game game = new Game(Application.StartupPath);
        private string settingsPath = Path.Combine(Application.StartupPath, "AutologinLoL.xml");
        XmlSerializer serializer = new XmlSerializer(typeof(List<Account>));
        private Timer timer;

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
        }

        private void button_Click(object sender, EventArgs e)
        {
            Account account = (Account)((Button)sender).Tag;
            if (game.SetConfig(account.Server, account.Locale))
            {
                if (game.Start())
                {
                    if (timer != null)
                    {
                        timer.Stop();
                        timer.Dispose();
                    }
                    timer = new Timer();
                    timer.Interval = 500;
                    timer.Tick += new EventHandler(delegate(object _sender, EventArgs _event)
                    {
                        if (game.Login(account.Login, account.Password))
                        {
                            this.Close();
                        }
                    });
                    timer.Enabled = true;
                }
            }            
        }

    }
}
