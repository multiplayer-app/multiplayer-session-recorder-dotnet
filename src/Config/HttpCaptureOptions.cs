using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Diagnostics;

namespace SessionRecorder.Config
{
    public class HttpCaptureOptions
    {
        public bool CaptureHeaders { get; set; } = true;
        public bool CaptureBody { get; set; } = true;
        public bool IsMaskHeadersEnabled { get; set; } = false;
        public bool IsMaskBodyEnabled { get; set; } = false;
        public int MaxPayloadSizeBytes { get; set; } = 1024 * 1024; // 1MB default

        public Func<Dictionary<string, string>, Activity, Dictionary<string, string>> MaskHeaders { get; set; } = 
            (headers, activity) => headers;

        public Func<string, Activity, string> MaskBody { get; set; } = 
            (body, activity) => body;

        public static HttpCaptureOptions WithDefaults(HttpCaptureOptions options)
        {
            return new HttpCaptureOptions
            {
                CaptureHeaders = options.CaptureHeaders,
                CaptureBody = options.CaptureBody,
                IsMaskHeadersEnabled = options.IsMaskHeadersEnabled,
                IsMaskBodyEnabled = options.IsMaskBodyEnabled,
                MaxPayloadSizeBytes = options.MaxPayloadSizeBytes,
                MaskHeaders = options.MaskHeaders,
                MaskBody = options.MaskBody
            };
        }
    }
}
