using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public const string KDCKey =        "aacgkdcbghkeynky";
        public const string SessionKey =    "arsessionffkeyvg";
        public const string ServiceKey =    "tyservicenhkeycg";
        public const string ClientKey =     "qclientbfkeypgbt";
        public const string ServiceSessionKey = "servicesessionke";

        public static readonly int BaseDuration = 600;

        public static readonly IPEndPoint AuthEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), AuthenticationPort);
        public static readonly IPEndPoint TGServerEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), TicketGrantingServicePort);
        public static readonly IPEndPoint ServiceServerEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), ServiceServerPort);
    }
}
