using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using Multiplayer.SessionRecorder.Constants;

namespace Multiplayer.SessionRecorder.Exporters;

/// <summary>
/// Wrapper for any OTLP trace exporter that filters spans by SessionRecorder trace ID prefixes.
/// </summary>
public class SessionRecorderTraceExporterWrapper : BaseExporter<Activity>
{
    private readonly BaseExporter<Activity> _innerExporter;
    
    /// <summary>
    /// Initializes a new instance of the SessionRecorderTraceExporterWrapper class.
    /// </summary>
    /// <param name="innerExporter">The inner trace exporter to wrap.</param>
    /// <exception cref="ArgumentNullException">Thrown when innerExporter is null.</exception>
    public SessionRecorderTraceExporterWrapper(BaseExporter<Activity> innerExporter)
    {
        _innerExporter = innerExporter ?? throw new ArgumentNullException(nameof(innerExporter));
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
