namespace AT.Business.Models.AppSettings
{
    /// <summary>SmtpServer class.</summary>
    public class SmtpServer
    {
        /// <summary>Gets or sets the host.</summary>
        /// <value>The host.</value>
        public string Host { get; set; }

        /// <summary>Gets or sets the port.</summary>
        /// <value>The port.</value>
        public int Port { get; set; }

        /// <summary>Gets or sets the username.</summary>
        /// <value>The username.</value>
        public string Username { get; set; }

        /// <summary>Gets or sets the password.</summary>
        /// <value>The password.</value>
        public string Password { get; set; }

        /// <summary>Gets or sets the recipients.</summary>
        /// <value>The recipients.</value>
        public string Recipients { get; set; }

        /// <summary>Gets or sets a value indicating whether this instance is active.</summary>
        /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
        public bool IsActive { get; set; }
    }
}