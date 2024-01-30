using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kerberos_lab_2_
{
    public static class Configuration
    {
        public const int ClientPort = 1000;
        public const int AuthenticationPort = 1001;
        public const int TicketGrantingServicePort = 1002;
        public const int ServiceServerPort = 1003;

        public static readonly TimeSpan TicketDuration = new(0, 30, 0);
    }
}
