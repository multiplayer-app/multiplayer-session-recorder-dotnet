# SessionRecorder HTTP Capture Middleware Example

This example demonstrates how to integrate the `SessionRecorderHttpCaptureMiddleware` into an ASP.NET Core Web API application.

## Features Demonstrated

- ✅ HTTP request and response body capture
- ✅ HTTP header capture with sensitive data masking
- ✅ Custom masking functions for security
- ✅ Integration with OpenTelemetry tracing
- ✅ Configurable payload size limits
- ✅ Error handling and logging

## How It Works

The `SessionRecorderHttpCaptureMiddleware` captures HTTP requests and responses and adds them as tags to OpenTelemetry Activity spans, which are then exported to your observability platform.

### Configuration

The example includes complete SessionRecorder setup in `Program.cs`:

#### 1. SessionRecorder Initialization
```csharp
// Initialize SessionRecorder with configuration
var sessionRecorderConfig = new SessionRecorderConfig
{
    ApiKey = Environment.GetEnvironmentVariable("MULTIPLAYER_OTLP_KEY") ?? "demo-api-key",
    TraceIdGenerator = new SessionRecorderIdGenerator(),
    resourceAttributes = new Dictionary<string, object>
    {
        { "service.name", "WebApiOpenApi" },
        { "service.version", "1.0.0" },
        { "deployment.environment", "development" },
        { "service.instance.id", Environment.MachineName }
    },
    GenerateSessionShortIdLocally = true
};

SessionRecorder.Init(sessionRecorderConfig);
```

#### 2. HTTP Capture Middleware Configuration
```csharp
// Configure HTTP Capture Options
builder.Services.Configure<HttpCaptureOptions>(options =>
{
    options.CaptureHeaders = true;
    options.CaptureBody = true;
    options.MaxPayloadSizeBytes = 1024 * 1024; // 1MB
    options.IsMaskHeadersEnabled = true;
    options.IsMaskBodyEnabled = true;
    
    // Custom masking functions
    options.MaskHeaders = (headers, activity) => {
        // Mask sensitive headers like authorization, cookie, etc.
    };
    
    options.MaskBody = (body, activity) => {
        // Mask sensitive fields in request/response bodies
    };
});

// Add middleware to pipeline
app.UseMiddleware<SessionRecorderHttpCaptureMiddleware>();
```

### Span Attributes

The middleware adds the following OpenTelemetry span attributes:

- `multiplayer.http.request.headers` - JSON serialized request headers
- `multiplayer.http.request.body` - Request body content
- `multiplayer.http.response.headers` - JSON serialized response headers  
- `multiplayer.http.response.body` - Response body content

## Running the Example

### Prerequisites

This example uses the local SessionRecorder project reference, so **no publishing is required**. The project automatically builds and uses the latest local SessionRecorder code.

### Setup

1. **Set Environment Variables:**
   ```bash
   export MULTIPLAYER_OTLP_KEY="your-api-key"  # Optional: for real API calls
   export PLATFORM_ENV="development"
   ```

   **Note:** The example will work without `MULTIPLAYER_OTLP_KEY` using a demo API key for local development.

2. **Build and Run:**
   ```bash
   cd examples/http
   dotnet build
   dotnet run
   ```

   The project automatically:
   - ✅ References the local SessionRecorder project (`../../SessionRecorder.csproj`)
   - ✅ Builds SessionRecorder from source
   - ✅ Initializes SessionRecorder with proper configuration
   - ✅ Configures the HTTP capture middleware

3. **Access the API:**
   - Swagger UI: `http://localhost:5000/api/v1/docs`
   - Health Check: `http://localhost:5000/api/v1/healthz`

## Test Endpoints

The example includes several endpoints to demonstrate middleware functionality:

### 1. Simple Request/Response (`POST /api/v1/session-recorder-demo/simple`)

```bash
curl -X POST http://localhost:5000/api/v1/session-recorder-demo/simple \
  -H "Content-Type: application/json" \
  -d '{"name": "John Doe", "userId": 123}'
```

### 2. Login with Sensitive Data (`POST /api/v1/session-recorder-demo/login`)

```bash
curl -X POST http://localhost:5000/api/v1/session-recorder-demo/login \
  -H "Content-Type: application/json" \
  -d '{"username": "john", "password": "secret123"}'
```

**Note:** The password in the request and secret in the response will be masked.

### 3. Large Data Processing (`POST /api/v1/session-recorder-demo/large-data`)

```bash
curl -X POST http://localhost:5000/api/v1/session-recorder-demo/large-data \
  -H "Content-Type: application/json" \
  -d '{
    "items": [
      {"id": 1, "name": "Item 1", "description": "First item"},
      {"id": 2, "name": "Item 2", "description": "Second item"}
    ]
  }'
```

### 4. Error Simulation (`POST /api/v1/session-recorder-demo/error`)

```bash
curl -X POST http://localhost:5000/api/v1/session-recorder-demo/error \
  -H "Content-Type: application/json" \
  -d '{"errorType": "badrequest"}'
```

## Security Features

### Header Masking
The middleware automatically masks sensitive headers:
- `authorization`
- `cookie`
- `x-api-key`
- `x-auth-token`

### Body Masking
The middleware masks sensitive fields in JSON bodies:
- Fields containing "password"
- Fields containing "secret"

### Payload Size Limits
- Default limit: 1MB
- Configurable via `MaxPayloadSizeBytes`
- Prevents memory issues with large payloads

## Observability

When running with a valid `MULTIPLAYER_OTLP_KEY`, you'll see:

1. **Console Output**: Activity traces with HTTP capture data
2. **OTLP Export**: Data sent to your observability platform
3. **Trace Headers**: `X-Trace-Id` header in responses

## Middleware Order

The middleware is positioned in the pipeline to:
1. Come after CORS handling
2. Come before security headers
3. Capture all requests/responses including errors

```csharp
app.UseCors("AllowAllOrigins");
app.UseMiddleware<SessionRecorderHttpCaptureMiddleware>(); // ← Here
app.UseSecurityHeaders();
app.UseSwagger();
// ... other middleware
```

## Troubleshooting

### Common Issues

1. **Missing HttpCaptureOptions**: Ensure `Configure<HttpCaptureOptions>` is called
2. **Stream Errors**: The middleware handles request body buffering automatically
3. **Large Payloads**: Adjust `MaxPayloadSizeBytes` for your use case
4. **No Activity**: Ensure OpenTelemetry is properly configured

### Debugging

Enable console exporter to see traces locally:
```csharp
.AddProcessor(new SimpleActivityExportProcessor(consoleExporter))
```

This will output captured HTTP data to the console for debugging.
