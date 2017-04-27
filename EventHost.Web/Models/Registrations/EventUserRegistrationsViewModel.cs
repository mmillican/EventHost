using EventHost.Web.Models.Events;
using EventHost.Web.Models.Sections;
using EventHost.Web.Models.Sessions;
using EventHost.Web.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventHost.Web.Models.Registrations
{
    public class EventUserRegistrationsViewModel
    {
        public EventModel Event { get; set; }
        public UserModel User { get; set; }

        public List<SectionModel> Sections { get; set; }

        public List<SessionModel> RegisteredSessions { get; set; }
    }
}
