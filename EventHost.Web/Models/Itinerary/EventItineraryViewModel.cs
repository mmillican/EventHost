using EventHost.Web.Models.Events;
using EventHost.Web.Models.Sections;
using EventHost.Web.Models.Sessions;
using EventHost.Web.Models.Users;
using System.Collections.Generic;

namespace EventHost.Web.Models.Itinerary
{
    public class EventItineraryViewModel
    {
        public EventModel Event { get; set; }
        public UserModel User { get; set; }

        public List<SectionModel> Sections { get; set; }

        public List<SessionModel> RegisteredSessions { get; set; }

        public string SerializedSessions { get; set; }
    }

    public class EventItineraryEmailModel : EventItineraryViewModel
    {
        public string EventUrl { get; set; }
        public string ItineraryUrl { get; set; }
    }
}
