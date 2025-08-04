using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SessionRecorderCommon;
using SessionRecorder.Constants;

public class SessionRecorderHttpCaptureMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<OpenTelemetryHttpCaptureMiddleware> _logger;
    private readonly HttpCaptureOptions _options;

    public OpenTelemetryHttpCaptureMiddleware(
        RequestDelegate next,
        ILogger<OpenTelemetryHttpCaptureMiddleware> logger,
        IOptions<HttpCaptureOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = HttpCaptureOptions.WithDefaults(options.Value);
    }

    public async Task Invoke(HttpContext context)
    {
        var activity = Activity.Current;
        if (activity == null)
        {
            await _next(context);
            return;
        }

        // Capture Request
        await CaptureRequest(context, activity);

        // Swap response stream
        var originalResponseBody = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        await _next(context); // proceed down the pipeline

        // Capture Response
        await CaptureResponse(context, activity, responseBodyStream);

        // Copy back to original stream
        responseBodyStream.Seek(0, SeekOrigin.Begin);
        await responseBodyStream.CopyToAsync(originalResponseBody);
    }

    private async Task CaptureRequest(HttpContext context, Activity span)
    {
        if (!_options.CaptureHeaders && !_options.CaptureBody) return;

        // Headers
        if (_options.CaptureHeaders)
        {
            var headers = context.Request.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());
            if (_options.IsMaskHeadersEnabled)
                headers = _options.MaskHeaders(headers, span);

            span.SetTag("multiplayer.http.request.headers", JsonSerializer.Serialize(headers));
        }

        // Body
        if (_options.CaptureBody && context.Request.ContentLength > 0 && context.Request.Body.CanRead)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (body.Length < _options.MaxPayloadSizeBytes)
            {
                if (_options.IsMaskBodyEnabled)
                    body = _options.MaskBody(body, span);

                span.SetTag("multiplayer.http.request.body", body);
            }
        }
    }

    private async Task CaptureResponse(HttpContext context, Activity span, MemoryStream responseBodyStream)
    {
        if (!_options.CaptureHeaders && !_options.CaptureBody) return;

        // Body
        string? body = null;
        if (_options.CaptureBody && responseBodyStream.Length < _options.MaxPayloadSizeBytes)
        {
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(responseBodyStream);
            body = await reader.ReadToEndAsync();

            if (_options.IsMaskBodyEnabled)
                body = _options.MaskBody(body, span);

            span.SetTag("multiplayer.http.response.body", body);
        }

        // Headers
        if (_options.CaptureHeaders)
        {
            var headers = context.Response.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());
            if (_options.IsMaskHeadersEnabled)
                headers = _options.MaskHeaders(headers, span);

            span.SetTag("multiplayer.http.response.headers", JsonSerializer.Serialize(headers));
        }
    }
}
