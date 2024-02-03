using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kerberos_lab_2_.Models
{
    public record class AuthServerResponse(string UserPrincipal,
        byte[] TGSEncryptByClientKey,
        byte[] TGSEncryptByKDCKey);

    public record class TGServerResponse(string ServicePrincipal,
        byte[] STEncryptBySessionKey,
        byte[] STEncryptByServiceKey);

    public record class AppServerResponse(byte[] ServiceResEncryptByServiceSessionKey);
}
