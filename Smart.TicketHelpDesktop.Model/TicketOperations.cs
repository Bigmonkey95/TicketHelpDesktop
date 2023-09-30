using System.ComponentModel.DataAnnotations;

namespace Smart.TicketHelpDesktop.Model
{
    public class TicketOperations
    {

        public int? Id { get; set; }



        public int? IdUser { get; set; }



        public string? Applicant { get; set; }



        public string? Subject { get; set; }


        public string? Text { get; set; }

 

        public string? Priority { get; set; }


        public string? AffectedApplication { get; set; }


        public string? UserCreation { get; set; }


        public int? AssignedUser { get; set; }


        public string? Notes { get; set; }


        public string? Status { get; set; }


        public DateTime? TicketCreationDatetime { get; set; }



    }
}
