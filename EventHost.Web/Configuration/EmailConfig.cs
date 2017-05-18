namespace EventHost.Web.Configuration
{
    public class EmailConfig
    {
        public string Server { get; set; }
        public int Port { get; set; } = 25;
        public bool UseSsl { get; set; } = false;

        public string User { get; set; } = null;
        public string Password { get; set; } = null;

        public string FromName { get; set; }
        public string FromAddress { get; set; }
    }
}
