namespace Kerberos_lab_2_.Models
{
    internal class TicketGrantingTicket(string sessionKey, string principal)
    {
        public string SessionKey { get; set; } = sessionKey;
        //Метка времени
        public DateTime TimeStamp { get; init; } = DateTime.Now;
        //Срок жизни
        public long Duration {  get; set; }
        public string Principal { get; set; } = principal;
    }
}
