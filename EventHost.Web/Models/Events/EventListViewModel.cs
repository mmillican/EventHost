using System.Collections.Generic;

namespace EventHost.Web.Models.Events
{
    public class EventListViewModel
    {
        public List<EventModel> Events { get; set; } = new List<EventModel>();
    }
}
