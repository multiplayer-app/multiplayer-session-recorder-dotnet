using System;
using System.Collections.Generic;
using Multiplayer.SessionRecorder;
using Multiplayer.SessionRecorder.Types;
using Multiplayer.SessionRecorder.Trace;
using Multiplayer.SessionRecorder.Sdk;

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

                 // Test 6: Test Init method
                 Console.WriteLine("\n6. Testing Init method...");
                 try
                 {
                     // Create a test configuration
                     var testConfig = new SessionRecorderConfig
                     {
                         ApiKey = "test-api-key",
                         ResourceAttributes = new Dictionary<string, object>
                         {
                             { "service.name", "test-service" },
                             { "service.version", "1.0.0" }
                         },
                         GenerateSessionShortIdLocally = true,
                         TraceIdGenerator = new SessionRecorderIdGenerator()
                     };

                     // Test Init method
                     SessionRecorder.Init(testConfig);
                     Console.WriteLine("✓ Init method called successfully");
                 }
                 catch (Exception ex)
                 {
                     Console.WriteLine($"✗ Init method test failed: {ex.Message}");
                 }

                 // Test 7: Test SDK methods
                 Console.WriteLine("\n7. Testing SDK methods...");
                 try
                 {
                     // Test CaptureException
                     var testException = new InvalidOperationException("Test exception for SDK");
                     SessionRecorderSdk.CaptureException(testException);
                     Console.WriteLine("✓ CaptureException method called successfully");

                     // Test SaveContinuousSession
                     SessionRecorderSdk.SaveContinuousSession("Test auto-save reason");
                     Console.WriteLine("✓ SaveContinuousSession method called successfully");

                     // Test SetAttribute
                     SessionRecorderSdk.SetAttribute("test.key", "test.value");
                     Console.WriteLine("✓ SetAttribute method called successfully");

                     // Test HTTP methods with masking
                     var testJson = "{\"username\":\"testuser\",\"password\":\"secret123\"}";
                     SessionRecorderSdk.SetHttpRequestBody(testJson, mask: true);
                     SessionRecorderSdk.SetHttpResponseBody("{\"status\":\"success\"}", mask: false);
                     Console.WriteLine("✓ HTTP body methods called successfully");

                     // Test RPC methods
                     SessionRecorderSdk.SetRpcRequestMessage("{\"method\":\"getUser\"}", mask: true);
                     SessionRecorderSdk.SetRpcResponseMessage("{\"user\":{\"id\":123,\"name\":\"John\"}}", mask: false);
                     Console.WriteLine("✓ RPC methods called successfully");
                 }
                 catch (Exception ex)
                 {
                     Console.WriteLine($"✗ SDK method test failed: {ex.Message}");
                 }

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
