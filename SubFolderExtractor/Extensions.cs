using System;
using System.Threading.Tasks;

namespace SubFolderExtractor
{
    public static class Extensions
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        ///   Extracts the base error message and all inner messages from an <see cref="Exception" /> object.
        ///   <para />
        ///   Returns a concatenated string with each exception message on a new line.
        /// </summary>
        /// <param name="exception"> <see cref="Exception" /> object </param>
        /// <returns> Concatenated string with each exception message on a new line </returns>
        public static string ExtractErrorMessage(this Exception exception)
        {
            if (exception == null)
                return null;

            var errorMessage = String.Empty;
            if (!String.IsNullOrEmpty(exception.Message) && !(exception is AggregateException))
                errorMessage = exception.Message;

            // Avoiding large recursion... just in case
            int count = 0;

            while (exception.InnerException != null && count <= 10)
            {
                count++;
                exception = exception.InnerException;
                if (!String.IsNullOrEmpty(exception.Message) && !errorMessage.Contains(exception.Message))
                    errorMessage = count == 1
                                       ? exception.Message
                                       : errorMessage + Environment.NewLine + exception.Message;
            }

            return errorMessage;
        }

        public static void IgnoreExceptions(this Task task)
        {
            if (task == null)
                return;

            task.ContinueWith(t => Logger.ErrorException(t.Exception.ExtractErrorMessage(), t.Exception),
                TaskContinuationOptions.OnlyOnFaulted);
        }

        public static bool IsEqualTo(this string original, string value, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            return string.Compare(original, value, stringComparison) == 0;
        }
    }
}