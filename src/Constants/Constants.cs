using System.Reflection;

namespace SessionRecorder.Constants
{
    public static class Constants
    {
        public const string MULTIPLAYER_BASE_API_URL = "https://api.multiplayer.app";
        public const int MULTIPLAYER_TRACE_DEBUG_SESSION_SHORT_ID_LENGTH = 8;
        
        /// <summary>
        /// Gets the current version from the assembly
        /// </summary>
        public static string SESSION_RECORDER_VERSION
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                return version?.ToString() ?? "0.0.0";
            }
        }
    }
}
