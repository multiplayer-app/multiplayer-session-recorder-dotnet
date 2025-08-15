using System;
using System.Collections.Generic;
using Multiplayer.SessionRecorder;
using Multiplayer.SessionRecorder.Types;
using Multiplayer.SessionRecorder.Trace;

namespace WorkingExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== SessionRecorder Library Working Example ===\n");

            try
            {
                // Test 1: Create configuration
                Console.WriteLine("1. Creating configuration...");
                var config = new SessionRecorderConfig
                {
                    ApiKey = "test-api-key",
                    TraceIdGenerator = new Multiplayer.SessionRecorder.Trace.SessionRecorderIdGenerator(),
                    ResourceAttributes = new Dictionary<string, object>
                    {
                        { "service.name", "test-service" },
                        { "service.version", "1.0.0" }
                    },
                    GenerateSessionShortIdLocally = true
                };
                Console.WriteLine("✓ Configuration created successfully");

                // Test 2: Access singleton instance
                Console.WriteLine("\n2. Accessing singleton instance...");
                var instance = Multiplayer.SessionRecorder.SessionRecorder.Instance;
                Console.WriteLine("✓ Singleton instance accessed successfully");

                // Test 3: Create session objects
                Console.WriteLine("\n3. Creating session objects...");
                var session = new Session
                {
                    Name = "Test Session",
                    SessionAttributes = new Dictionary<string, object>
                    {
                        { "user.id", "12345" },
                        { "environment", "test" }
                    },
                    ResourceAttributes = new Dictionary<string, object>
                    {
                        { "host.name", "test-host" }
                    }
                };
                Console.WriteLine("✓ Session object created successfully");

                // Test 4: Test enum values
                Console.WriteLine("\n4. Testing enum values...");
                Console.WriteLine($"SessionType.PLAIN: {SessionType.PLAIN}");
                Console.WriteLine($"SessionType.CONTINUOUS: {SessionType.CONTINUOUS}");
                Console.WriteLine($"SessionState.STOPPED: {SessionState.STOPPED}");
                Console.WriteLine($"SessionState.STARTED: {SessionState.STARTED}");
                Console.WriteLine($"SessionState.PAUSED: {SessionState.PAUSED}");
                Console.WriteLine("✓ Enum values accessed successfully");

                // Test 5: Get version information
                Console.WriteLine("\n5. Getting version information...");
                Console.WriteLine($"Current Version: {Multiplayer.SessionRecorder.Constants.Constants.SESSION_RECORDER_VERSION}");
                Console.WriteLine("✓ Version information retrieved successfully");

                Console.WriteLine("\n=== All Tests Passed! ===");
                Console.WriteLine("\nNote: This example demonstrates the library structure.");
                Console.WriteLine("To make actual API calls, you would need:");
                Console.WriteLine("1. A valid API key in MULTIPLAYER_OTLP_KEY environment variable");
                Console.WriteLine("2. Network connectivity to the Multiplayer API");
                Console.WriteLine("3. Proper error handling for API calls");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
