using System;
using System.ComponentModel.DataAnnotations;

namespace EventHost.Web.Models.Events
{
    public class EditEventViewModel
    {
        public int Id { get; set; }

        [MaxLength(100), Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime StartOn { get; set; }
        public DateTime EndOn { get; set; }

        public DateTime? RegistrationStartOn { get; set; }
        public DateTime? RegistrationEndOn { get; set; }

        public bool EnableWaitLists { get; set; }
        public bool EnableAutomaticApproval { get; set; }

        public bool HideFromPublicLists { get; set; }
        public bool RequirePassword { get; set; }
        [MaxLength(50)]
        public string JoinPassword { get; set; }

        public int OwnerUserId { get; set; }
    }
}
