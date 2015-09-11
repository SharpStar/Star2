using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarLib.Database;
using StarLib.Packets.Starbound;
using StarLib.Server;

namespace StarAccounts
{
    public class AuthenticatingProxy
    {
        public string AccountName { get; set; }

        public User User { get; set; }
    }
}
