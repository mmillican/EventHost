using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EventHost.Web.Entities.Users;
using EventHost.Web.Entities.Events;

namespace EventHost.Web.Data
{
    public class EventsDbContext : IdentityDbContext<User, Role, int>
    {
        public DbSet<Event> Events { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Registration> Registrations { get; set; }

        public EventsDbContext(DbContextOptions<EventsDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>().ForSqlServerToTable("Users");
            builder.Entity<Role>().ForSqlServerToTable("Roles");
            builder.Entity<IdentityUserRole<int>>().ForSqlServerToTable("UserRoles");
            builder.Entity<IdentityUserLogin<int>>().ForSqlServerToTable("UserLogins");
            builder.Entity<IdentityUserClaim<int>>().ForSqlServerToTable("UserClaims");
            builder.Entity<IdentityUserToken<int>>().ForSqlServerToTable("UserTokens");
            builder.Entity<IdentityRoleClaim<int>>().ForSqlServerToTable("RoleClaims");
        }
    }
}
