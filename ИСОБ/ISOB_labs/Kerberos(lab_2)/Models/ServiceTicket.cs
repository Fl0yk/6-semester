namespace Kerberos_lab_2_.Models
{
    internal class ServiceTicket(string serviceSessionKey, string principal, int duration)
    {
        public string ServiceSessionKey { get; set; } = serviceSessionKey;
        //Метка времени
        public DateTime TimeStamp { get; init; } = DateTime.Now;
        //Срок жизни
        public int Duration { get; set; } = duration;
        public string Principal { get; set; } = principal;
    }
}
