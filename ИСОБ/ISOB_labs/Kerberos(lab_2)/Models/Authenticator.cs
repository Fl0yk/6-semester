namespace Kerberos_lab_2_.Models
{
    internal class Authenticator(string principal)
    {
        //Принципал - строка, полностью идентифицирующая учамтника протокола
        public string Principal { get; set; } = principal;
        //Метка времени
        public DateTime TimeStamp { get; init; } = DateTime.Now;
    }
}
