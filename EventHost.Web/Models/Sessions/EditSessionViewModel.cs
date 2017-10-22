using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventHost.Web.Models.Sessions
{
    public class EditSessionViewModel : SessionModel
    {
        [NotMapped]
        public IEnumerable<SelectListItem> SectionOptions { get; set; } = new List<SelectListItem>();
        [NotMapped]
        public IEnumerable<SelectListItem> UserOptions { get; set; } = new List<SelectListItem>();

        public bool EnableWaitList { get; set; }
    }
}
