# Multiplayer Session Recorder .NET

A .NET library for recording and managing debugging sessions with Multiplayer.

## Features

- Start and stop debugging sessions
- Support for both plain and continuous sessions
- HTTP request/response capture and masking
- Custom trace ID generation
- Session attributes and resource attributes support

## Installation

```bash
dotnet add package SessionRecorder
```

## Quick Start

The `SessionRecorder` is implemented as a singleton, ensuring that only one instance exists throughout your application. This makes it easy to access the session recorder from anywhere in your code.

```csharp
using SessionRecorder;
using SessionRecorder.Types;

// Initialize the session recorder
var config = new SessionRecorderConfig
{
    ApiKey = "your-api-key",
    TraceIdGenerator = new SessionRecorderIdGenerator()
    ResourceAttributes = new Dictionary<string, object>
    {
        { "service.name", "my-service" },
        { "service.version", "1.0.0" }
    }
};

// Initialize the session recorder
SessionRecorder.Init(config);

// Start a session
var session = new Session
{
    Name = "My Debug Session",
    SessionAttributes = new Dictionary<string, object>
    {
        { "user.id", "12345" },
        { "environment", "production" }
    },
    ResourceAttributes = new Dictionary<string, object>
    {
        { "host.name", "server-01" }
    }
};

await SessionRecorder.Start(SessionType.PLAIN, session);

// Stop the session
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

## API Reference

### Request Structures

#### Start Session Request

When starting a session, the library sends the following structure to the API:

```json
{
  "name": "string",
  "tags": [
    {
      "key": "string",
      "value": "string"
    }
  ],
  "sessionAttributes": {},
  "resourceAttributes": {}
}
```

#### Stop Session Request

When stopping a session, the library sends the following structure to the API:

```json
{
  "sessionAttributes": {}
}
```

### Session Types

- `SessionType.PLAIN`: Standard debugging session
- `SessionType.CONTINUOUS`: Continuous debugging session with auto-save capabilities

### Configuration

The `SessionRecorderConfig` class supports the following options:

- `ApiKey`: Your Multiplayer API key
- `TraceIdGenerator`: Custom trace ID generator
- `ResourceAttributes`: Global resource attributes for all sessions
- `GenerateSessionShortIdLocally`: Whether to generate session short IDs locally

## HTTP Capture Middleware

The library includes middleware for capturing HTTP requests and responses:

```csharp
app.UseMiddleware<SessionRecorderHttpCaptureMiddleware>();
```

## License

MIT License - see LICENSE file for details.
