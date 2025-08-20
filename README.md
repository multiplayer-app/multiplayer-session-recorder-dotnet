![Description](.github/header-dotnet.png)

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

# Multiplayer Session Recorder - .NET

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
