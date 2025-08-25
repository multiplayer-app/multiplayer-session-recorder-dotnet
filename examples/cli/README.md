# SessionRecorder Examples

This directory contains examples demonstrating how to use the SessionRecorder library.

## Prerequisites

1. **Set your API key** as an environment variable (optional for this example):
   ```bash
   # On macOS/Linux
   export MULTIPLAYER_OTLP_KEY="your-api-key-here"
   
   # On Windows
   set MULTIPLAYER_OTLP_KEY=your-api-key-here
   ```

2. **Make sure you have .NET 9.0 SDK** installed on your machine.

## How to Run

### Option 1: Run from the examples directory
```bash
cd examples
dotnet run
```

### Option 2: Run from the project root
```bash
dotnet run --project examples
```

### Option 3: Build and run separately
```bash
cd examples
dotnet build
dotnet run
```

## What the Example Demonstrates

1. **Basic Setup and Configuration** - How to create SessionRecorderConfig
2. **Singleton Access** - How to access the SessionRecorder singleton instance
3. **Session Object Creation** - How to create Session objects with attributes
4. **Enum Values** - How to use SessionType and SessionState enums
5. **Library Structure** - Demonstrates the basic structure without making API calls

## Troubleshooting

- **Build errors**: Ensure you're using .NET 9.0 SDK and the SessionRecorder library builds successfully
- **Missing .NET version**: Install the required .NET version if prompted

## Note

This is a **demonstration example** that shows the library structure without making actual API calls. 
To make real API calls, you would need:
- A valid API key in the `MULTIPLAYER_OTLP_KEY` environment variable
- Network connectivity to the Multiplayer API
- Proper error handling for API calls

This example is for development and testing purposes only. It is not included in the NuGet package.
