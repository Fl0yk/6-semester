namespace Kerberos_lab_2_.Models
{
    internal class ServiceTicket(string serviceSessionKey, string principal)
    {
        public string ServiceSessionKey { get; set; } = serviceSessionKey;
        //Метка времени
        public DateTime TimeStamp { get; init; } = DateTime.Now;
        //Срок жизни
        public long Duration { get; set; }
        public string Principal { get; set; } = principal;
    }
}
