using System;
using System.Diagnostics;
using OpenTelemetry.Trace;
using Multiplayer.SessionRecorder.Constants;
using Multiplayer.SessionRecorder.Types;

namespace Multiplayer.SessionRecorder.Trace
{
    public class SessionRecorderIdGenerator
    {
        private string _sessionShortId = string.Empty;
        private SessionType _sessionType = SessionType.PLAIN;

        public SessionRecorderIdGenerator(){}

        public void SetSessionId(string sessionShortId, SessionType sessionType = SessionType.PLAIN)
        {
            _sessionShortId = sessionShortId;
            _sessionType = sessionType;
        }

        public ActivityTraceId GenerateTraceId()
        {
            var traceIdBytes = new byte[16];
            var random = new Random();
            random.NextBytes(traceIdBytes);

            if (!string.IsNullOrEmpty(_sessionShortId))
            {
                string sessionTypePrefix = _sessionType switch
                {
                    SessionType.CONTINUOUS => SessionRecorderTraceIdPrefix.ContinuousDebug,
                    _ => SessionRecorderTraceIdPrefix.Debug
                };

                var prefix = $"{sessionTypePrefix}{_sessionShortId}";
                var prefixBytes = ConvertHexStringToBytes(prefix);
                
                // Copy prefix bytes to the beginning of traceIdBytes
                Array.Copy(prefixBytes, 0, traceIdBytes, 0, Math.Min(prefixBytes.Length, traceIdBytes.Length));
            }

            return ActivityTraceId.CreateFromBytes(traceIdBytes);
        }

        private static byte[] ConvertHexStringToBytes(string hex)
        {
            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException("Hex string must have an even number of characters.");
            }

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return bytes;
        }
    }

    public static class SessionRecorderTraceIdConfiguration
    {
        public static void ConfigureSessionRecorderTraceIdGenerator()
        {
            var generator = new SessionRecorderIdGenerator();
            Activity.TraceIdGenerator = new Func<ActivityTraceId>(() => generator.GenerateTraceId());
        }
    }
}
