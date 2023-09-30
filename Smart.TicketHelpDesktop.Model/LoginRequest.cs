using System.ComponentModel.DataAnnotations;

namespace Smart.TicketHelpDesktop.Model
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "The Field 'Email' is mandatory")]
        [EmailAddress(ErrorMessage = " The Field 'Email' dont its a valid email valid")]

        public string? Email { get; set; }

        [Required(ErrorMessage = "The Field 'Password' is mandatory")]
        public string? Password { get; set; }
    }
}
