using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EventHost.Web.Models.Sections
{
    public class SectionModel
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
