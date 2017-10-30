using System;
using System.ComponentModel.DataAnnotations.Schema;
using EventHost.Web.Entities.Users;

namespace EventHost.Web.Entities.Events
{
    public class EventMember
    {
        public int Id { get; set; }

        public int EventId { get; set; }
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public DateTime JoinDate { get; set; }

        public EventMemberJoinMethod JoinMethod { get; set; }
    }

    public enum EventMemberJoinMethod
    {
        Public = 0,
        Password = 1,
        Registered = 2,
        Invited = 5
    }
}
