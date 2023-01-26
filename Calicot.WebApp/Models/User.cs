using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace Calicot.WebApp.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;

        [JsonIgnore]
        public string Password { get; set; } = default!;
    }
}