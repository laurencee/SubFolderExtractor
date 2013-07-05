using System;

namespace SubFolderExtractor
{
    public class RegistrationExceptionEvent : EventArgs
    {
        public RegistrationExceptionEvent(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");
            Exception = exception;
        }

        public Exception Exception { get; private set; }
    }
}