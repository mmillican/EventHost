using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace EventHost.Web.Entities.Users
{
    public class Role : IdentityRole<int>
    {
        public virtual ICollection<IdentityUserRole<int>> Users { get; set; } = new List<IdentityUserRole<int>>();

        public virtual ICollection<IdentityRoleClaim<int>> Claims { get; set; } = new List<IdentityRoleClaim<int>>();
    }
}
