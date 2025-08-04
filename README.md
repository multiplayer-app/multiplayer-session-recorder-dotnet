# Session Recorder .Net
============================================================================
##  Introduction
The `session-recorder` module integrates OpenTelemetry with the Multiplayer platform to enable seamless trace collection and analysis. This library helps developers monitor, debug, and document application performance with detailed trace data. It supports flexible trace ID generation, sampling strategies.


## Installation

To install the `session-recorder` module, use the following command:

```bash
dotnet add package SessionRecorder
```

## Session Recorder Initialization

```cs
using SessionRecorder;
using SessionRecorder.Types;
using SessionRecorder.Config;

var config = new SessionRecorderConfig
{
    ApiKey = "your-multiplayer-api-key",
    TraceIdGenerator = traceIdGenerator,
    ResourceAttributes = new Dictionary<string, object>
    {
        { "service.name", "your-service" },
        { "host.name", Environment.MachineName },
    },
    GenerateSessionShortIdLocally = true,
    GenerateSessionShortIdFunc = shortIdGenerator
};

sessionRecorder.init(config)
```

## Example Usage

```cs
using SessionRecorder;
using SessionRecorder.Types;
using SessionRecorder.Config;

var config = new SessionRecorderConfig
{
    ApiKey = "your-multiplayer-api-key",
    TraceIdGenerator = traceIdGenerator,
    ResourceAttributes = new Dictionary<string, object>
    {
        { "service.name", "{YOUR_APPLICATION_NAME}" },
        { "environment": '{YOUR_APPLICATION_ENVIRONMENT}' },
        { "host.name", Environment.MachineName },
    },
};

sessionRecorder.Init(config);

sessionRecorder.Start(SessionType.PLAIN);

// do something here

sessionRecorder.Stop();
```


## Session Recorder trace Id generator

```cs
using SessionRecorder.Trace;

// NOTE: this will set 3% of the traces for auto documentation
SessionRecorderTraceIdConfiguration.ConfigureSessionRecorderTraceIdGenerator(0.03);

// or

Activity.TraceIdGenerator = new Func<ActivityTraceId>(() => SessionRecorderTraceIdGenerator.GenerateSessionRecorderTraceId(docSpanSampler));
```

## Session Recorder trace id ratio based sampler

```cs
using SessionRecorder.Exporter;
using SessionRecorder.Trace;

// ...

// NOTE: this config will send 5% of all traces
builder.Services.AddOpenTelemetry().WithTracing(tracing => {
    tracing.SetSampler(new SessionRecorderTraceIdRatioBasedSampler(0.05));
});
```
