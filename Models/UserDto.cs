using System.ComponentModel.DataAnnotations;

namespace Uploader_Web.Models
{
    public class UserDto
    {
        [Required]
        public string firstname { get; set; }
        [Required]
        public string lastname { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        [RegularExpression(@"^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,32}$", ErrorMessage = "Password should contain " + "8 - 32 characters long with at least one digit, one lower case alphabet, one uppercase alphabet and one special character.")]
        public string password { get; set; }
    }
}
