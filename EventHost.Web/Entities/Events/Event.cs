using EventHost.Web.Entities.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventHost.Web.Entities.Events
{
    public class Event
    {
        public int Id { get; set; }

        [MaxLength(100), Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime StartOn { get; set; }
        public DateTime EndOn { get; set; }

        public DateTime? RegistrationStartOn { get; set; }
        public DateTime? RegistrationEndOn { get; set; }

        public bool EnableWaitLists { get; set; }
        public bool EnableAutomaticApproval { get; set; }

        public int OwnerUserId { get; set; }
        [ForeignKey("OwnerUserId")]
        public virtual User Owner { get; set; }

        [MaxLength(500)]
        public string Slug { get; set; }

        public bool HideFromPublicLists { get; set; }
        public bool RequirePassword { get; set; }
        [MaxLength(50)]
        public string JoinPassword { get; set; }

        // TODO: Invite only

        public virtual IList<Section> Sections { get; set; } = new List<Section>();
        public virtual IList<Session> Sessions { get; set; } = new List<Session>();
        public virtual IList<EventMember> Members { get; set; } = new List<EventMember>();
    }
}
