using System.Diagnostics;
using OpenTelemetry.Trace;
using SessionRecorder.Internal;
using SessionRecorder.Constants;

namespace SessionRecorder.Trace;

public static class SessionRecorderTraceIdGenerator
{
    static byte[] ConvertHexStringToBytes(string hex)
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

    private static byte[] docPrefix = ConvertHexStringToBytes(SessionRecorderTraceIdPrefix.Document);

    // Custom trace ID generator function
    public static ActivityTraceId GenerateSessionRecorderTraceId(SessionRecorderTraceIdRatioBasedSampler docSpanSampler)
    {
        var traceIdBytes = new byte[16];
        Random random = new Random();
        random.NextBytes(traceIdBytes);

        if (
            docSpanSampler.ShouldSample(ActivityTraceId.CreateFromBytes(traceIdBytes)).Decision == SamplingDecision.RecordAndSample
        )
        {
            docPrefix.CopyTo(traceIdBytes, 0);
        }

        return ActivityTraceId.CreateFromBytes(traceIdBytes);
    }
}

public class SessionRecorderTraceIdConfiguration
{
    public static void ConfigureSessionRecorderTraceIdGenerator(double docProbability)
    {
        Guard.ThrowIfOutOfRange(docProbability, min: 0.0, max: 1.0);
        var docSpanSampler = new SessionRecorderTraceIdRatioBasedSampler(docProbability);

        // Assign the custom function to the TraceIdGenerator static property
        Activity.TraceIdGenerator = new Func<ActivityTraceId>(() => MultiplayerTraceIdGenerator.GenerateMultiplayerTraceId(docSpanSampler));
    }
}
