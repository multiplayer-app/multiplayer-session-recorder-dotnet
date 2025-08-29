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
/// HTTP OTLP log exporter that filters log records by SessionRecorder trace ID prefixes.
/// </summary>
public class SessionRecorderHttpOtlpLogExporter : BaseExporter<LogRecord>
{
    private readonly OtlpLogExporter _innerExporter;
    private readonly string _apiKey;
    
    /// <summary>
    /// Initializes a new instance of the SessionRecorderHttpOtlpLogExporter class.
    /// </summary>
    /// <param name="apiKey">The API key for authorization header.</param>
    /// <param name="endpoint">The OTLP endpoint URL.</param>
    public SessionRecorderHttpOtlpLogExporter(string? apiKey = null, string? endpoint = null)
    {
        _apiKey = apiKey ?? string.Empty;
        
        var options = new OtlpExporterOptions
        {
            Endpoint = new Uri(endpoint ?? SessionRecorderExporterEndpoint.HttpLogsEndpoint),
            Protocol = OtlpExportProtocol.HttpProtobuf
        };

        if (!string.IsNullOrEmpty(_apiKey))
        {
            options.Headers = $"Authorization={_apiKey}";
        }

        _innerExporter = new OtlpLogExporter(options);
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
        return traceIdString.StartsWith(SessionRecorderTraceIdPrefix.Debug, StringComparison.OrdinalIgnoreCase) ||
               traceIdString.StartsWith(SessionRecorderTraceIdPrefix.ContinuousDebug, StringComparison.OrdinalIgnoreCase);
    }
}
