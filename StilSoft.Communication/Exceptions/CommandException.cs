using System;

namespace StilSoft.Communication.Exceptions
{
    public class CommandException : Exception
    {
        private const string DefaultMessage = "Command error occurred, contact support.";

        public CommandException()
            : base(DefaultMessage)
        {
        }

        public CommandException(Exception innerException)
            : base(DefaultMessage, innerException)
        {
        }

        public CommandException(string message)
            : base(message)
        {
        }

        public CommandException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}