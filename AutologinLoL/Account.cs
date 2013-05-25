using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutologinLoL
{
    public struct Account
    {
        public string Server;
        public string Locale;
        public string Login;
        public string Password;

        public override string ToString()
        {
            return String.Format("{0} ({1} - {2})", Login, Server, Locale);
        }
    }
}
