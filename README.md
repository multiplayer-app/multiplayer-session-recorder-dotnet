![Description](./docs/img/header-dotnet.png)

<div align="center">
<a href="https://github.com/multiplayer-app/multiplayer-session-recorder-dotnet">
  <img src="https://img.shields.io/github/stars/multiplayer-app/multiplayer-session-recorder-dotnet.svg?style=social&label=Star&maxAge=2592000" alt="GitHub stars">
</a>
  <a href="https://github.com/multiplayer-app/multiplayer-session-recorder-dotnet/blob/main/LICENSE">
    <img src="https://img.shields.io/github/license/multiplayer-app/multiplayer-session-recorder-dotnet" alt="License">
  </a>
  <a href="https://multiplayer.app">
    <img src="https://img.shields.io/badge/Visit-multiplayer.app-blue" alt="Visit Multiplayer">
  </a>
  
</div>
<div>
  <p align="center">
    <a href="https://x.com/trymultiplayer">
      <img src="https://img.shields.io/badge/Follow%20on%20X-000000?style=for-the-badge&logo=x&logoColor=white" alt="Follow on X" />
    </a>
    <a href="https://www.linkedin.com/company/multiplayer-app/">
      <img src="https://img.shields.io/badge/Follow%20on%20LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white" alt="Follow on LinkedIn" />
    </a>
    <a href="https://discord.com/invite/q9K3mDzfrx">
      <img src="https://img.shields.io/badge/Join%20our%20Discord-5865F2?style=for-the-badge&logo=discord&logoColor=white" alt="Join our Discord" />
    </a>
  </p>
</div>

# Multiplayer Full Stack Session Recorder

The Multiplayer Full Stack Session Recorder is a powerful tool that offers deep session replays with insights spanning frontend screens, platform traces, metrics, and logs. It helps your team pinpoint and resolve bugs faster by providing a complete picture of your backend system architecture. No more wasted hours combing through APM data; the Multiplayer Full Stack Session Recorder does it all in one place.

## Install

```bash
dotnet add package SessionRecorder
```

## Set up backend services

### Route traces and logs to Multiplayer

Multiplayer Full Stack Session Recorder is built on top of OpenTelemetry.

### New to OpenTelemetry?

No problem. You can set it up in a few minutes. If your services don't already use OpenTelemetry, you'll first need to install the OpenTelemetry libraries. Detailed instructions for this can be found in the [OpenTelemetry documentation](https://opentelemetry.io/docs/).

### Already using OpenTelemetry?

You have two primary options for routing your data to Multiplayer:

***Direct Exporter***: This option involves using the Multiplayer Exporter directly within your services. It's a great choice for new applications or startups because it's simple to set up and doesn't require any additional infrastructure. You can configure it to send all session recording data to Multiplayer while optionally sending a sampled subset of data to your existing observability platform.

***OpenTelemetry Collector***: For large, scaled platforms, we recommend using an OpenTelemetry Collector. This approach provides more flexibility by having your services send all telemetry to the collector, which then routes specific session recording data to Multiplayer and other data to your existing observability tools.

### Option 1: Direct Exporter

Send OpenTelemetry data from your services to Multiplayer and optionally other destinations (e.g., OpenTelemetry Collectors, observability platforms, etc.).

This is the quickest way to get started, but consider using an OpenTelemetry Collector (see [Option 2](#option-2-opentelemetry-collector) below) if you're scalling or a have a large platform.

```cs
using OpenTelemetry.Exporter;
using Multiplayer.SessionRecorder.Exporters;

// set up Multiplayer exporters. Note: GRPC exporters are also available.
// see: `SessionRecorderGrpcTraceExporter` and `SessionRecorderGrpcLogsExporter`
var multiplayerTraceExporter = new SessionRecorderHttpOtlpTraceExporter(
  apiKey: "MULTIPLAYER_OTLP_KEY", // note: replace with your Multiplayer OTLP key
)
var multiplayerLogExporter = new SessionRecorderHttpOtlpLogExporter(
  apiKey: "MULTIPLAYER_OTLP_KEY", // note: replace with your Multiplayer OTLP key
)

var wrappedTraceExporter = new SessionRecorderTraceExporterWrapper(
  new OtlpTraceExporter(
    new OtlpExporterOptions
      {
        // ...
      }
  )
);
var wrappedLogExporter = new SessionRecorderLogsExporterWrapper(
  new OtlpLogExporter(
    new OtlpExporterOptions
      {
        // ..
      }
  )
);
```

### Option 2: OpenTelemetry Collector

If you're scalling or a have a large platform, consider running a dedicated collector. See the Multiplayer OpenTelemetry collector [repository](https://github.com/multiplayer-app/multiplayer-otlp-collector) which shows how to configure the standard OpenTelemetry Collector to send data to Multiplayer and optional other destinations.

Add standard [OpenTelemetry code](https://opentelemetry.io/docs/languages/dotnet/exporters/) to export OTLP data to your collector.

See a basic example below:

```cs
using OpenTelemetry.Exporter;

var traceExporter = new OtlpTraceExporter(
  new OtlpExporterOptions
    {
      Endpoint = new Uri("http://<OTLP_COLLECTOR_URL>/v1/traces"),
    }
);
var logExporter = new OtlpLogExporter(
    new OtlpExporterOptions
      {
        Endpoint = new Uri("http://<OTLP_COLLECTOR_URL>/v1/logs"),
      }
  )
);
```

### Capturing request/response and header content

In addition to sending traces and logs, you need to capture request and response content. We offer two solutions for this:

***In-Service Code Capture:*** You can use our libraries to capture, serialize, and mask request/response and header content directly within your service code. This is an easy way to get started, especially for new projects, as it requires no extra components in your platform.

***Multiplayer Proxy:*** Alternatively, you can run a [Multiplayer Proxy](https://github.com/multiplayer-app/multiplayer-proxy) to handle this outside of your services. This is ideal for large-scale applications and supports all languages, including those like Java that don't allow for in-service request/response hooks. The proxy can be deployed in various ways, such as an Ingress Proxy, a Sidecar Proxy, or an Embedded Proxy, to best fit your architecture.

### Option 1: In-Service Code Capture

The Multiplayer Session Recorder library provides utilities for capturing request, response and header content. See example below:


```cs
using NetEscapades.AspNetCore.SecurityHeaders.Infrastructure;
using Multiplayer.SessionRecorder;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls($"http://0.0.0.0:3000");

builder.Services.Configure<HttpCaptureOptions>(options =>
{
    options.CaptureHeaders = true;
    options.CaptureBody = true;
    options.MaxPayloadSizeBytes = 1024 * 1024; // 1MB
    options.IsMaskHeadersEnabled = true;
    options.IsMaskBodyEnabled = true;
});

// add SessionRecorder HTTP payload capture middleware
app.UseMiddleware<SessionRecorderHttpCaptureMiddleware>();
```

### Option 2: Multiplayer Proxy

The Multiplayer Proxy enables capturing request/response and header content without changing service code. See instructions at the [Multiplayer Proxy repository](https://github.com/multiplayer-app/multiplayer-proxy).

## Set up CLI app

The Multiplayer Full Stack Session Recorder can be used inside the CLI apps.

The [Multiplayer Time Travel Demo](https://github.com/multiplayer-app/multiplayer-time-travel-platform) includes an example [.Net CLI app](https://github.com/multiplayer-app/multiplayer-time-travel-platform/tree/main/clients/dotnet-cli-app).

See an additional example below.

### Quick start

Use the following code below to initialize and run the session recorder.

```cs
using System;
using System.Collections.Generic;
using Multiplayer.SessionRecorder;
using Multiplayer.SessionRecorder.Types;
using Multiplayer.SessionRecorder.Trace;
using Multiplayer.SessionRecorder.Sdk;

// Initialize the session recorder
var config = new SessionRecorderConfig
{
    ApiKey = "MULTIPLAYER_OTLP_KEY", // note: replace with your Multiplayer OTLP key
    TraceIdGenerator = new Multiplayer.SessionRecorder.Trace.SessionRecorderIdGenerator(),
    ResourceAttributes = new Dictionary<string, object>
    {
        { "host.name", "server-01" }
        { "serviceName": "{YOUR_APPLICATION_NAME}" },
        { "version": "{YOUR_APPLICATION_VERSION}" },
        { "environment": "{YOUR_APPLICATION_ENVIRONMENT}" },
    }
};

// Initialize the session recorder
SessionRecorder.Init(config);

// Start a session
var session = new Session
{
    Name = "This is test session",
    SessionAttributes = new Dictionary<string, object>
    {
        { "user.id", "12345" },
        { "environment", "production" }
    },
};

await SessionRecorder.Start(SessionType.PLAIN, session);

// do something here

var stopSession = new Session
{
    SessionAttributes = new Dictionary<string, object>
    {
        { "completion.status", "success" },
        { "duration.minutes", 15 }
    }
};

await SessionRecorder.Stop(stopSession);
```

Replace the placeholders with your application’s version, name, environment, and API key.

## License

MIT — see [LICENSE](./LICENSE).
