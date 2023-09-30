namespace Smart.TicketHelpDesktop.Model
{
    public class User
    {

        public int? Id { get; set; }

        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Permission { get; set; }
        public string? Token { get; set; }
        public List<Ticket> Tickets { get; set; }
        public override string ToString()
        {
            return Id.ToString() +
                   Name?.ToString() +
                   LastName?.ToString() +
                   Email?.ToString() +
                   Permission?.ToString() +
                   Password?.ToString() +
                   Token?.ToString();
        }

    }



}
