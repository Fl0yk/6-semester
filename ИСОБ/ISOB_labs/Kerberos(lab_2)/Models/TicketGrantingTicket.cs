namespace Kerberos_lab_2_.Models
{
    internal class TicketGrantingTicket(string sessionKey, string principal, int duration)
    {
        public string SessionKey { get; set; } = sessionKey;
        //Метка времени
        public DateTime TimeStamp { get; init; } = DateTime.Now;
        //Срок жизни
        public int Duration { get; set; } = duration;
        public string Principal { get; set; } = principal;
    }
}
