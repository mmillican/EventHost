using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EventHost.Web.Entities.Users
{
    public class User : IdentityUser<int>
    {
        [MaxLength(50), Required]
        public string FirstName { get; set; }
        [MaxLength(50), Required]
        public string LastName { get; set; }
    }
}
