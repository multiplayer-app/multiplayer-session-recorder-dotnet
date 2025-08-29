using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using Multiplayer.SessionRecorder.Constants;

namespace Multiplayer.SessionRecorder.Exporters;

/// <summary>
/// Wrapper for any OTLP log exporter that filters log records by SessionRecorder trace ID prefixes.
/// </summary>
public class SessionRecorderLogsExporterWrapper : BaseExporter<LogRecord>
{
    private readonly BaseExporter<LogRecord> _innerExporter;
    
    /// <summary>
    /// Initializes a new instance of the SessionRecorderLogsExporterWrapper class.
    /// </summary>
    /// <param name="innerExporter">The inner log exporter to wrap.</param>
    /// <exception cref="ArgumentNullException">Thrown when innerExporter is null.</exception>
    public SessionRecorderLogsExporterWrapper(BaseExporter<LogRecord> innerExporter)
    {
        _innerExporter = innerExporter ?? throw new ArgumentNullException(nameof(innerExporter));
    }

    /// <summary>
    /// Exports a batch of log records, filtering by SessionRecorder trace ID prefixes.
    /// </summary>
    /// <param name="batch">The batch of log records to export.</param>
    /// <returns>The export result.</returns>
    public override ExportResult Export(in Batch<LogRecord> batch)
    {
        var filteredLogs = new List<LogRecord>();
        
        foreach (var logRecord in batch)
        {
            if (ShouldExportLogRecord(logRecord))
            {
                filteredLogs.Add(logRecord);
            }
        }

        if (filteredLogs.Count == 0)
        {
            return ExportResult.Success;
        }

        // Create a new batch with filtered logs
        var filteredBatch = new Batch<LogRecord>(filteredLogs.ToArray(), filteredLogs.Count);
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

    private static bool ShouldExportLogRecord(LogRecord logRecord)
    {
        if (logRecord.TraceId == default)
        {
            return false;
        }

        var traceIdString = logRecord.TraceId.ToString();
        return !traceIdString.StartsWith(SessionRecorderTraceIdPrefix.Debug, StringComparison.OrdinalIgnoreCase) &&
               !traceIdString.StartsWith(SessionRecorderTraceIdPrefix.ContinuousDebug, StringComparison.OrdinalIgnoreCase);
    }
}
