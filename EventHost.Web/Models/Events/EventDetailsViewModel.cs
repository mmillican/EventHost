using EventHost.Web.Models.Registrations;
using EventHost.Web.Models.Sections;
using EventHost.Web.Models.Sessions;
using System.Collections.Generic;

namespace EventHost.Web.Models.Events
{
    public class EventDetailsViewModel
    {
        public EventModel Event { get; set; }

        public int CurrentUserId { get; set; }
        public bool UserCanEdit { get; set; }
        public bool RegistrationIsOpen { get; set; }
        public bool UserIsMember { get; set; }

        public IEnumerable<SectionModel> Sections { get; set; } = new List<SectionModel>();
        public IEnumerable<SessionModel> Sessions { get; set; }
        public IEnumerable<RegistrationModel> Registrations { get; set; }
    }
}
