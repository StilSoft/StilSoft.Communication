using System;

namespace StilSoft.Communication.Exceptions
{
    public class CommandTimeoutException : CommandException
    {
        private const string DefaultMessage = "Command timeout error occurred. Please try again.";

        public CommandTimeoutException()
            : base(DefaultMessage)
        {
        }

        public CommandTimeoutException(Exception innerException)
            : base(DefaultMessage, innerException)
        {
        }

        public CommandTimeoutException(string message)
            : base(message)
        {
        }

        public CommandTimeoutException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}