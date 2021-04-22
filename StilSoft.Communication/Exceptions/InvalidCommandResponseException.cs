using System;

namespace StilSoft.Communication.Exceptions
{
    public class InvalidCommandResponseException : CommandException
    {
        private const string DefaultMessage = "Invalid Command Response error occurred. Please try again.";

        public InvalidCommandResponseException()
            : base(DefaultMessage)
        {
        }

        public InvalidCommandResponseException(Exception innerException)
            : base(DefaultMessage, innerException)
        {
        }

        public InvalidCommandResponseException(string message)
            : base(message)
        {
        }

        public InvalidCommandResponseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}