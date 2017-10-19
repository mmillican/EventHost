using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace EventHost.Web.Entities.Users
{
    public class User : IdentityUser<int>
    {
        [MaxLength(50), Required]
        public string FirstName { get; set; }
        [MaxLength(50), Required]
        public string LastName { get; set; }

        public virtual ICollection<IdentityUserRole<int>> Roles { get; set; } = new List<IdentityUserRole<int>>();
        public virtual ICollection<IdentityUserClaim<int>> Claims { get; set; } = new List<IdentityUserClaim<int>>();
        public virtual ICollection<IdentityUserLogin<int>> Logins { get; set; } = new List<IdentityUserLogin<int>>();
    }
}
