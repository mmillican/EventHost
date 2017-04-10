using System;

namespace EventHost.Web.Models.Registrations
{
    public class CreateRegistrationModel
    {
        public int Id { get; set; }

        public int EventId { get; set; }
        public int SessionId { get; set; }
        public int UserId { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime? ApprovedOn { get; set; }
    }
}
