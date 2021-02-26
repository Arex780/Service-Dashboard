using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebArchive.Models.Smtp
{
    public class SmtpConfig
    {
        public string Sender { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool Ssl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
