namespace EventHost.Web.Models.Events
{
    public class JoinEventViewModel
    {
        public EventModel Event { get; set; }

        public bool IsCurrentUserMember { get; set; }

        public string Password { get; set; }
    }
}
