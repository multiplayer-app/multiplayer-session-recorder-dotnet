using System.Globalization;
using System.Diagnostics;
using OpenTelemetry.Trace;
using Multiplayer.SessionRecorder.Constants;
using Multiplayer.OpenTelemetry.Internal;


namespace Multiplayer.SessionRecorder.Trace;

public sealed class SessionRecorderTraceIdRatioBasedSampler
    : Sampler
{
    private readonly long idUpperBound;
    private readonly double probability;

    public SessionRecorderTraceIdRatioBasedSampler(double probability)
    {
        Guard.ThrowIfOutOfRange(probability, min: 0.0, max: 1.0);

        this.probability = probability;

        // The expected description is like TraceIdRatioBasedSampler{0.000100}
        this.Description = "SessionRecorderTraceIdRatioBasedSampler{" + this.probability.ToString("F6", CultureInfo.InvariantCulture) + "}";

        // Special case the limits, to avoid any possible issues with lack of precision across
        // double/long boundaries. For probability == 0.0, we use Long.MIN_VALUE as this guarantees
        // that we will never sample a trace, even in the case where the id == Long.MIN_VALUE, since
        // Math.Abs(Long.MIN_VALUE) == Long.MIN_VALUE.
        if (this.probability == 0.0)
        {
            this.idUpperBound = long.MinValue;
        }
        else if (this.probability == 1.0)
        {
            this.idUpperBound = long.MaxValue;
        }
        else
        {
            this.idUpperBound = (long)(probability * long.MaxValue);
        }
    }

    /// <inheritdoc />
    public override SamplingResult ShouldSample(in SamplingParameters samplingParameters)
    {
        // Always sample if we are within probability range. This is true even for child activities (that
        // may have had a different sampling decision made) to allow for different sampling policies,
        // and dynamic increases to sampling probabilities for debugging purposes.
        // Note use of '<' for comparison. This ensures that we never sample for probability == 0.0,
        // while allowing for a (very) small chance of *not* sampling if the id == Long.MAX_VALUE.
        // This is considered a reasonable trade-off for the simplicity/performance requirements (this
        // code is executed in-line for every Activity creation).
        Span<byte> traceIdBytes = stackalloc byte[16];
        samplingParameters.TraceId.CopyTo(traceIdBytes);

        var traceIdString = samplingParameters.TraceId.ToString();

        if (traceIdString.StartsWith(SessionRecorderTraceIdPrefix.ContinuousDebug) || traceIdString.StartsWith(SessionRecorderTraceIdPrefix.Debug))
        {
            return new SamplingResult(SamplingDecision.RecordAndSample);
        }

        return new SamplingResult(Math.Abs(GetLowerLong(traceIdBytes)) < this.idUpperBound);
    }

    public SamplingResult ShouldSample(ActivityTraceId traceId)
    {
        Span<byte> traceIdBytes = stackalloc byte[16];
        traceId.CopyTo(traceIdBytes);
        return new SamplingResult(Math.Abs(GetLowerLong(traceIdBytes)) < this.idUpperBound);
    }

    private static long GetLowerLong(ReadOnlySpan<byte> bytes)
    {
        long result = 0;
        for (var i = 0; i < 8; i++)
        {
            result <<= 8;
#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
            result |= bytes[i] & 0xff;
#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand
        }

        return result;
    }
}
