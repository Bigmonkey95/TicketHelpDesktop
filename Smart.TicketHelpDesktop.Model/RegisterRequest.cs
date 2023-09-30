using System.ComponentModel.DataAnnotations;

namespace Smart.TicketHelpDesktop.Model
{
    public class RegisterRequest
    {


        [Required]
        public string Name { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        [RegularExpression(@"^(?=.*[A-Z]).+$", ErrorMessage = "Password must contain at least one uppercase letter.")]
        public string Password { get; set; }


        [Required]
        [MinLength(8)]
        [RegularExpression(@"^(?=.*[A-Z]).+$", ErrorMessage = "Password must contain at least one uppercase letter.")]
        public string ConfirmPassword { get; set; }




    }
}
