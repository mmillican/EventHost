using EventHost.Web.Entities.Users;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventHost.Web.Entities.Events
{
    public class Registration
    {
        public int Id { get; set; }

        public int EventId { get; set; }
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }

        public int SessionId { get; set; }
        [ForeignKey("SessionId")]
        public virtual Session Session { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime? ApprovedOn { get; set; }
    }
}
