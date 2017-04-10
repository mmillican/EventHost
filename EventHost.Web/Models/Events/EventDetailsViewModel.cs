using EventHost.Web.Models.Registrations;
using EventHost.Web.Models.Sections;
using EventHost.Web.Models.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventHost.Web.Models.Events
{
    public class EventDetailsViewModel
    {
        public EventModel Event { get; set; }

        public int CurrentUserId { get; set; }
        public bool UserCanEdit { get; set; }
        public bool RegistrationIsOpen { get; set; }

        public IEnumerable<SectionModel> Sections { get; set; } = new List<SectionModel>();
        public IEnumerable<SessionModel> Sessions { get; set; }
        public IEnumerable<RegistrationModel> Registrations { get; set; }
        public IDictionary<int, List<RegistrationModel>> RegistrationsBySession { get; set; }
    }
}
