using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Multiplayer.SessionRecorder.Helpers
{
    public static class SessionRecorderSdk
    {
        public static Func<string> GetIdGenerator(int length)
        {
            return () => GenerateRandomId(length);
        }

        public static async Task SaveContinuousSession(string? reason = null)
        {
            // Implementation would depend on the actual SDK requirements
            await Task.CompletedTask;
        }

        private static string GenerateRandomId(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
