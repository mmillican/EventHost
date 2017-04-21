using System;

namespace EventHost.Web.Models.Registrations
{
    public class RegistrationModel
    {
        public int Id { get; set; }

        public int EventId { get; set; }
        public string EventName { get; set; }

        public int SectionId { get; set; }
        public string SectionName { get; set; }

        public int SessionId { get; set; }
        public string SessionName { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime? ApprovedOn { get; set; }
    }
}
