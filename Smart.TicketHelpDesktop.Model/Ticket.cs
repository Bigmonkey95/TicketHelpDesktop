namespace Smart.TicketHelpDesktop.Model
{
    public class Ticket
    {
        public int? Id { get; set; }
        public int? IdUser { get; set; }

        public string? Applicant { get; set; }

        public DateTime? TicketCreationDateTime { get; set; }
        public string? Subject { get; set; }
        public string? Text { get; set; }

        public string? Priority { get; set; }
        public string? AffectedApplication { get; set; }
        public string? UserCreation { get; set; }

        public string? Status { get; set; }

        public string? Notes { get; set; }

        public DateTime? DatedClosed { get; set; }

        public string? ClosedBy { get; set; }

        public string? ModifiedBy { get; set; }

        public int? AssignedUser { get; set; }
        public DateTime? ModifiedDateTime { get; set; }

        public List<User>? Users { get; set; }
        public List<DetailTicket>? DetailTickets { get; set; }


        public override string ToString()
        {
            return Id?.ToString() +
                IdUser?.ToString() +
                Applicant?.ToString() +
                TicketCreationDateTime?.ToString() +
                Subject?.ToString() +
                Text?.ToString() +
                Priority?.ToString() +
                AffectedApplication?.ToString() +
                AssignedUser?.ToString() +
                UserCreation?.ToString() +
                Status?.ToString() +
                Notes?.ToString() +
                DatedClosed?.ToString() +
                ClosedBy?.ToString() +
                ModifiedBy?.ToString() +
                ModifiedDateTime?.ToString();
        }
    }
}
