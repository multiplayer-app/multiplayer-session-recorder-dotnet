using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using Multiplayer.SessionRecorder.Constants;

namespace Multiplayer.SessionRecorder.Exporters;

/// <summary>
/// HTTP OTLP trace exporter that filters spans by SessionRecorder trace ID prefixes.
/// </summary>
public class SessionRecorderHttpOtlpTraceExporter : BaseExporter<Activity>
{
    private readonly OtlpTraceExporter _innerExporter;
    private readonly string _apiKey;
    
    /// <summary>
    /// Initializes a new instance of the SessionRecorderHttpOtlpTraceExporter class.
    /// </summary>
    /// <param name="apiKey">The API key for authorization header.</param>
    /// <param name="endpoint">The OTLP endpoint URL.</param>
    public SessionRecorderHttpOtlpTraceExporter(string? apiKey = null, string? endpoint = null)
    {
        _apiKey = apiKey ?? string.Empty;
        
        var options = new OtlpExporterOptions
        {
            Endpoint = new Uri(endpoint ?? SessionRecorderExporterEndpoint.HttpTracesEndpoint),
            Protocol = OtlpExportProtocol.HttpProtobuf
        };

        if (!string.IsNullOrEmpty(_apiKey))
        {
            options.Headers = $"Authorization={_apiKey}";
        }

        _innerExporter = new OtlpTraceExporter(options);
    }

    /// <summary>
    /// Exports a batch of activities, filtering by SessionRecorder trace ID prefixes.
    /// </summary>
    /// <param name="batch">The batch of activities to export.</param>
    /// <returns>The export result.</returns>
    public override ExportResult Export(in Batch<Activity> batch)
    {
        var filteredActivities = new List<Activity>();
        
        foreach (var activity in batch)
        {
            if (ShouldExportActivity(activity))
            {
                filteredActivities.Add(activity);
            }
        }

        if (filteredActivities.Count == 0)
        {
            return ExportResult.Success;
        }

        // Create a new batch with filtered activities
        var filteredBatch = new Batch<Activity>(filteredActivities.ToArray(), filteredActivities.Count);
        return _innerExporter.Export(filteredBatch);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the exporter.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _innerExporter?.Dispose();
        }
        base.Dispose(disposing);
    }

    private static bool ShouldExportActivity(Activity activity)
    {
        if (activity.TraceId == default)
        {
            return false;
        }

        var traceIdString = activity.TraceId.ToString();
        return traceIdString.StartsWith(SessionRecorderTraceIdPrefix.Debug, StringComparison.OrdinalIgnoreCase) ||
               traceIdString.StartsWith(SessionRecorderTraceIdPrefix.ContinuousDebug, StringComparison.OrdinalIgnoreCase);
    }
}
