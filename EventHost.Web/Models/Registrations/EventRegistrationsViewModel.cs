using EventHost.Web.Models.Events;
using EventHost.Web.Models.Sections;
using EventHost.Web.Models.Users;
using System.Collections.Generic;

namespace EventHost.Web.Models.Registrations
{
    public class EventRegistrationsViewModel
    {
        public EventModel Event { get; set; }

        public List<SectionModel> Sections { get; set; }
        public List<UserModel> RegisteredUsers { get; set; }
        public List<RegistrationModel> Registrations { get; set; }
    }
}
