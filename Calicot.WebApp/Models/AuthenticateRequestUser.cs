using System.ComponentModel.DataAnnotations;

namespace Calicot.WebApp.Models
{
    public class AuthenticateRequestUser
    {
        [Required]
        public string UserName { get; set; } = default!;

        [Required]
        public string Password { get; set; } = default!;
    }
}