using EventHost.Web.Models.Sessions;
using System.Collections.Generic;

namespace EventHost.Web.Models.Registrations
{
    public class SessionRegistrationViewModel
    {
        public SessionModel Session { get; set; }

        public bool CurrentUserIsRegistered { get; set; }
        public IEnumerable<RegistrationModel> Registrations { get; set; }

        public bool RequiresApproval { get; set; }
        public bool UserCanManageRegistrations { get; set; }

        public bool RegistrationIsOpen { get; set; }
    }
}
