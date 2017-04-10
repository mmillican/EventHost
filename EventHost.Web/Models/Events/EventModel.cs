using System;

namespace EventHost.Web.Models.Events
{
    public class EventModel
    {
        public int Id { get; set; }
        
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime StartOn { get; set; }
        public DateTime EndOn { get; set; }

        public DateTime? RegistrationStartOn { get; set; }
        public DateTime? RegistrationEndOn { get; set; }

        public bool EnableWaitLists { get; set; }
        public bool EnableAutomaticApproval { get; set; }

        public int OwnerUserId { get; set; }
        public string OwnerName { get; set; }
    }
}
