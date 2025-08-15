using System;

namespace Multiplayer.SessionRecorder.Helpers
{
    public static class DateHelper
    {
        public static string GetFormattedDate(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss UTC");
        }

        public static string GetDefaultSessionName(DateTime dateTime)
        {
            return $"Session on {dateTime:MMM dd, yyyy, HH:mm:ss}";
        }
    }
}
