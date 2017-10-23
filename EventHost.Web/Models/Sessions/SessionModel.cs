using System;
using System.ComponentModel.DataAnnotations;

namespace EventHost.Web.Models.Sessions
{
    public class SessionModel
    {
        public int Id { get; set; }

        [Display(Name = "Event")]
        public int EventId { get; set; }
        public string EventName { get; set; }

        [Display(Name = "Section")]
        public int SectionId { get; set; }
        public string SessionName { get; set; }

        [MaxLength(100), Required]
        [Display(Name = "Name")]
        public string Name { get; set; }
        [Display(Name = "Description")]
        public string Description { get; set; }

        [MaxLength(100)]
        [Display(Name = "Address")]
        public string Address { get; set; }
        [MaxLength(50)]
        [Display(Name = "City")]
        public string City { get; set; }
        [MaxLength(5)]
        [Display(Name = "State (abbr)")]
        public string State { get; set; }
        [MaxLength(10)]
        [Display(Name = "Zip code")]
        public string PostalCode { get; set; }

        [Display(Name = "Allow registrations")]
        public bool AllowRegistrations { get; set; }

        [Display(Name = "Available spots")]
        public int MaxSpots { get; set; }
        [Display(Name = "Reserved spots")]
        public int ReservedSpots { get; set; }

        public int RegistrationCount { get; set; }
        public int AvailableSpots => (MaxSpots - ReservedSpots - RegistrationCount);

        [Display(Name = "Allow wait list")]
        public bool AllowWaitList { get; set; }

        [Display(Name = "Host (user)")]
        public int? HostUserId { get; set; }

        [Display(Name = "Host name")]
        public string HostName { get; set; }

        public DateTime LastUpdatedOn { get; set; }
    }
}
