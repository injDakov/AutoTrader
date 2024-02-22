namespace AT.Business.Models.AppSettings
{
    public class SmtpServer
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Recipients { get; set; }

        public bool IsActive { get; set; }
    }
}