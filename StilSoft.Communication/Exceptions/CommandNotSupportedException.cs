using System;

namespace StilSoft.Communication.Exceptions
{
    public class CommandNotSupportedException : CommandException
    {
        private const string DefaultMessage = "Command not supported error occurred, contact support.";

        public CommandNotSupportedException()
            : base(DefaultMessage)
        {
        }

        public CommandNotSupportedException(Exception innerException)
            : base(DefaultMessage, innerException)
        {
        }

        public CommandNotSupportedException(string message)
            : base(message)
        {
        }

        public CommandNotSupportedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}