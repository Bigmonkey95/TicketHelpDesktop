namespace Smart.TicketHelpDesktop.Model
{
    public class DetailTicket
    {
        public int? Id { get; set; }

        public int? IdUser { get; set; }

        public string? Operation { get; set; }


        public int? IdTicket { get; set; }

        public string? CreateBy { get; set; }

        public string? InChargeOf { get; set; }

        public DateTime? ChargeDateTime { get; set; }
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDateTime { get; set; }
        public string? ClosedBy { get; set; }

        public string? NoteUpdated { get; set; }

        public DateTime? DatedClosed { get; set; }

        public DateTime? TicketCreationDateTime { get; set; }

        public List<Ticket>? Ticket { get; set; }
        public override string ToString()
        {
            return Id.ToString() +

                   IdTicket?.ToString() +
                    IdUser?.ToString() +
                    NoteUpdated?.ToString() +
                   Operation?.ToString() +
                   TicketCreationDateTime?.ToString() +
                   CreateBy?.ToString() +
                   InChargeOf?.ToString() +
                   ChargeDateTime?.ToString() +
                   ModifiedBy?.ToString() +
                   ModifiedDateTime?.ToString() +
                   ClosedBy?.ToString() +
                   DatedClosed?.ToString();

        }
    }

}

