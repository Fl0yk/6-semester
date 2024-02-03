using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kerberos_lab_2_.Models
{
    public record class AppServerRequest(byte[] AuthEncryptBySessionServiceKey, //Аутентификатор, зашифрованный сессионным ключем сервиса
        byte[] TGSEncryptByServiceKey);      //TGS, зашифрованный ключем сервиса

    public record class AuthServerRequest(string UserPrincipal, //Принципал пользователя
        int Duration); //Запрашиваемое время жизни билета

    public record class TGServerRequest(string ServicePrincipal, //Принципал сервиса
        byte[] TGTEncryptByKDC,     //TGT, зашифрованный ключем центра распределения ключей (KDC)
        byte[] AuthEncryptBySessionKey);    //Аутентификатор, зашифрованный сессионным ключем
}
