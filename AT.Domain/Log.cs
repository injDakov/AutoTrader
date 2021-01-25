using System;
using AT.Domain.Enums;

namespace AT.Domain
{
    /// <summary>Log entity class.</summary>
    public class Log
    {
        /// <summary>Initializes a new instance of the <see cref="Log" /> class.</summary>
        /// <param name="type">The type.</param>
        /// <param name="action">The action.</param>
        /// <param name="message">The message.</param>
        /// <param name="detailedMessage">The detailed message.</param>
        public Log(LogType type, string action, string message, string detailedMessage = null)
        {
            CreateDate = DateTime.UtcNow;
            Type = type;
            Action = action;
            Message = message;
            DetailedMessage = detailedMessage;
        }

        /// <summary>Gets or sets the identifier for the log entity.</summary>
        /// <value>The identifier.</value>
        public long Id { get; set; }

        /// <summary>Gets or sets the create date.</summary>
        /// <value>The create date.</value>
        public DateTime CreateDate { get; set; }

        /// <summary>Gets or sets the type.</summary>
        /// <value>The type.</value>
        public LogType Type { get; set; }

        /// <summary>Gets or sets the action.</summary>
        /// <value>The action.</value>
        public string Action { get; set; }

        /// <summary>Gets or sets the message.</summary>
        /// <value>The message.</value>
        public string Message { get; set; }

        /// <summary>Gets or sets the detailed message.</summary>
        /// <value>The detailed message.</value>
        public string DetailedMessage { get; set; }
    }
}