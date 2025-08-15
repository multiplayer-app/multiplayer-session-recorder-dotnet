using System;
using System.Diagnostics;
using System.Text.Json;
using OpenTelemetry.Trace;
using Multiplayer.SessionRecorder.Constants;

namespace Multiplayer.SessionRecorder.Sdk
{
    /// <summary>
    /// SDK methods for SessionRecorder functionality
    /// </summary>
    public static class SessionRecorderSdk
    {
        /// <summary>
        /// Add error to current span
        /// </summary>
        /// <param name="error">The error to capture</param>
        public static void CaptureException(Exception? error)
        {
            if (error == null) return;

            var span = Activity.Current;
            if (span == null) return;

            span.AddException(error);
            span.SetStatus(ActivityStatusCode.Error, error.Message);
        }

        /// <summary>
        /// Set auto save attribute to span
        /// </summary>
        /// <param name="reason">Optional reason for the auto save</param>
        public static void SaveContinuousSession(string? reason = null)
        {
            var span = Activity.Current;

            if (span == null)
            {
                return;
                // TODO: create span if needed
            }

            span.SetTag(SessionRecorderSpanAttribute.ATTR_MULTIPLAYER_CONTINUOUS_SESSION_AUTO_SAVE, true);

            if (!string.IsNullOrEmpty(reason))
            {
                span.SetTag("reason", reason);
            }
        }

        /// <summary>
        /// Set attribute to current span
        /// </summary>
        /// <param name="key">Attribute key</param>
        /// <param name="value">Attribute value</param>
        public static void SetAttribute(string key, object value)
        {
            var span = Activity.Current;
            if (span == null) return;

            span.SetTag(key, value);
        }

        /// <summary>
        /// Set HTTP request body to current span attributes
        /// </summary>
        /// <param name="body">Request body</param>
        /// <param name="mask">Whether to mask sensitive data</param>
        public static void SetHttpRequestBody(object body, bool mask = true)
        {
            var span = Activity.Current;
            if (span == null) return;

            var processedBody = mask ? MaskBody(body) : body;
            span.SetTag(SessionRecorderSpanAttribute.ATTR_MULTIPLAYER_HTTP_REQUEST_BODY, processedBody);
        }

        /// <summary>
        /// Set HTTP request headers to current span attributes
        /// </summary>
        /// <param name="headers">Request headers</param>
        /// <param name="mask">Whether to mask sensitive data</param>
        public static void SetHttpRequestHeaders(object headers, bool mask = true)
        {
            var span = Activity.Current;
            if (span == null) return;

            var processedHeaders = mask ? MaskHeaders(headers) : headers;
            span.SetTag(SessionRecorderSpanAttribute.ATTR_MULTIPLAYER_HTTP_REQUEST_HEADERS, processedHeaders);
        }

        /// <summary>
        /// Set HTTP response body to current span attributes
        /// </summary>
        /// <param name="body">Response body</param>
        /// <param name="mask">Whether to mask sensitive data</param>
        public static void SetHttpResponseBody(object body, bool mask = true)
        {
            var span = Activity.Current;
            if (span == null) return;

            var processedBody = mask ? MaskBody(body) : body;
            span.SetTag(SessionRecorderSpanAttribute.ATTR_MULTIPLAYER_HTTP_RESPONSE_BODY, processedBody);
        }

        /// <summary>
        /// Set HTTP response headers to current span attributes
        /// </summary>
        /// <param name="headers">Response headers</param>
        /// <param name="mask">Whether to mask sensitive data</param>
        public static void SetHttpResponseHeaders(object headers, bool mask = true)
        {
            var span = Activity.Current;
            if (span == null) return;

            var processedHeaders = mask ? MaskHeaders(headers) : headers;
            span.SetTag(SessionRecorderSpanAttribute.ATTR_MULTIPLAYER_HTTP_RESPONSE_HEADERS, processedHeaders);
        }

        /// <summary>
        /// Set message body to current span attributes
        /// </summary>
        /// <param name="body">Message body</param>
        /// <param name="mask">Whether to mask sensitive data</param>
        public static void SetMessageBody(object body, bool mask = true)
        {
            var span = Activity.Current;
            if (span == null) return;

            var processedBody = mask ? MaskBody(body) : body;
            span.SetTag(SessionRecorderSpanAttribute.ATTR_MULTIPLAYER_MESSAGING_MESSAGE_BODY, processedBody);
        }

        /// <summary>
        /// Set RPC request message to current span attributes
        /// </summary>
        /// <param name="message">RPC request message</param>
        /// <param name="mask">Whether to mask sensitive data</param>
        public static void SetRpcRequestMessage(object message, bool mask = true)
        {
            var span = Activity.Current;
            if (span == null) return;

            var processedMessage = mask ? MaskBody(message) : message;
            span.SetTag(SessionRecorderSpanAttribute.ATTR_MULTIPLAYER_RPC_REQUEST_MESSAGE, processedMessage);
        }

        /// <summary>
        /// Set RPC response message to current span attributes
        /// </summary>
        /// <param name="message">RPC response message</param>
        /// <param name="mask">Whether to mask sensitive data</param>
        public static void SetRpcResponseMessage(object message, bool mask = true)
        {
            var span = Activity.Current;
            if (span == null) return;

            var processedMessage = mask ? MaskBody(message) : message;
            span.SetTag(SessionRecorderSpanAttribute.ATTR_MULTIPLAYER_RPC_RESPONSE_MESSAGE, processedMessage);
        }

        /// <summary>
        /// Set gRPC request message to current span attributes
        /// </summary>
        /// <param name="message">gRPC request message</param>
        /// <param name="mask">Whether to mask sensitive data</param>
        public static void SetGrpcRequestMessage(object message, bool mask = true)
        {
            var span = Activity.Current;
            if (span == null) return;

            var processedMessage = mask ? MaskBody(message) : message;
            span.SetTag(SessionRecorderSpanAttribute.ATTR_MULTIPLAYER_GRPC_REQUEST_MESSAGE, processedMessage);
        }

        /// <summary>
        /// Set gRPC response message to current span attributes
        /// </summary>
        /// <param name="message">gRPC response message</param>
        /// <param name="mask">Whether to mask sensitive data</param>
        public static void SetGrpcResponseMessage(object message, bool mask = true)
        {
            var span = Activity.Current;
            if (span == null) return;

            var processedMessage = mask ? MaskBody(message) : message;
            span.SetTag(SessionRecorderSpanAttribute.ATTR_MULTIPLAYER_GRPC_RESPONSE_MESSAGE, processedMessage);
        }

        /// <summary>
        /// Mask body content using sensitive fields
        /// </summary>
        /// <param name="body">Body to mask</param>
        /// <returns>Masked body</returns>
        private static object MaskBody(object body)
        {
            if (body is string jsonString)
            {
                return Masking.MaskJson(jsonString, Masking.SensitiveFields);
            }
            return body;
        }

        /// <summary>
        /// Mask headers content using sensitive headers
        /// </summary>
        /// <param name="headers">Headers to mask</param>
        /// <returns>Masked headers</returns>
        private static object MaskHeaders(object headers)
        {
            if (headers is string jsonString)
            {
                return Masking.MaskJson(jsonString, Masking.SensitiveHeaders);
            }
            return headers;
        }
    }
}
