using System;
using System.ComponentModel.DataAnnotations;

namespace EventHost.Web.Models.Sections
{
    public class EditSectionViewModel
    {
        public int Id { get; set; }

        public int EventId { get; set; }
        public string EventName { get; set; }

        [MaxLength(50), Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime StartOn { get; set; }
        public DateTime EndOn { get; set; }
    }
}
