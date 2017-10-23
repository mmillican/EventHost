using EventHost.Web.Entities.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventHost.Web.Entities.Events
{
    public class Session
    {
        public int Id { get; set; }

        public int EventId { get; set; }
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }

        public int SectionId { get; set; }
        [ForeignKey("SectionId")]
        public virtual Section Section { get; set; }

        [MaxLength(100), Required]
        public string Name { get; set; }
        public string Description { get; set; }
        
        [MaxLength(100)]
        public string Address { get; set; }
        [MaxLength(50)]
        public string City { get; set; }
        [MaxLength(5)]
        public string State { get; set; }
        [MaxLength(10)]
        public string PostalCode { get; set; }

        public bool AllowRegistrations { get; set; }

        public int MaxSpots { get; set; }
        public int ReservedSpots { get; set; }

        public bool AllowWaitList { get; set; }

        public int? HostUserId { get; set; }
        [ForeignKey("HostUserId")]
        public virtual User Host { get; set; }

        /// <summary>
        /// Non-user host name
        /// </summary>
        /// <remarks>If <see cref="HostUserId"/> is null, HostName should be set</remarks>
        [MaxLength(100)]
        public string HostName { get; set; }

        public DateTime LastUpdatedOn { get; set; }

        public virtual IList<Registration> Registrations { get; set; } = new List<Registration>();
    }
}
