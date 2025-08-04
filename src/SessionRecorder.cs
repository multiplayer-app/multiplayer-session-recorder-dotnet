using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SessionRecorderCommon;
using SessionRecorder.Services;
using SessionRecorder.Types;
using SessionRecorder.Helpers;

public class SessionRecorderConfig
{
    public string ApiKey { get; set; } = default!;

    public SessionRecorderIdGenerator TraceIdGenerator { get; set; } = default!;

    public Dictionary<string, object>? ResourceAttributes { get; set; }

    // Either a bool or a delegate (Func<string>) — nullable union
    public bool? GenerateSessionShortIdLocally { get; set; }

    public Func<string>? GenerateSessionShortIdFunc { get; set; }
}

namespace SessionRecorder
{
    public class SessionRecorder
    {
        private bool _isInitialized = false;
        private object _shortSessionId = false;
        private SessionRecorderIdGenerator _traceIdGenerator;
        private SessionType _sessionType = SessionType.PLAIN;
        private string _sessionState = "STOPPED";
        private readonly ApiService _apiService = new ApiService();
        private Func<string> _sessionShortIdGenerator = SessionRecorderSdk.GetIdGenerator(Constants.MULTIPLAYER_TRACE_DEBUG_SESSION_SHORT_ID_LENGTH);
        private Dictionary<string, object> _resourceAttributes = new();

        public void Init(SessionRecorderConfig config)
        {
            _resourceAttributes = config.ResourceAttributes ?? new Dictionary<string, object>
            {
                { Constants.ATTR_MULTIPLAYER_SESSION_RECORDER_VERSION, Constants.SESSION_RECORDER_VERSION }
            };

            _isInitialized = true;

            if (config.GenerateSessionShortIdLocally is Func<string> func)
                _sessionShortIdGenerator = func;

            if (string.IsNullOrEmpty(config.ApiKey))
                throw new Exception("Api key not provided");

            if (config.TraceIdGenerator?.SetSessionId == null)
                throw new Exception("Incompatible trace id generator");

            _traceIdGenerator = config.TraceIdGenerator;
            _apiService.Init(config.ApiKey);
        }

        public async Task Start(SessionType sessionType, ISession sessionPayload = null)
        {
            if (!_isInitialized)
                throw new Exception("Configuration not initialized. Call Init() before performing any actions.");

            if (sessionPayload?.ShortId?.Length > 0 && sessionPayload.ShortId.Length != Constants.MULTIPLAYER_TRACE_DEBUG_SESSION_SHORT_ID_LENGTH)
                throw new Exception("Invalid short session id");

            sessionPayload ??= new Session();

            if (_sessionState != "STOPPED")
                throw new Exception("Session should be ended before starting new one.");

            _sessionType = sessionType;

            sessionPayload.Name ??= $"Session on {DateHelper.GetFormattedDate(DateTime.UtcNow)}";
            sessionPayload.ResourceAttributes = MergeAttributes(_resourceAttributes, sessionPayload.ResourceAttributes);

            ISession session = sessionType == SessionType.CONTINUOUS
                ? await _apiService.StartContinuousSession(sessionPayload)
                : await _apiService.StartSession(sessionPayload);

            _shortSessionId = session.ShortId;
            _traceIdGenerator.SetSessionId((string)_shortSessionId, _sessionType);
            _sessionState = "STARTED";
        }

        public static async Task Save(string reason = null)
        {
            await SessionRecorderSdk.SaveContinuousSession(reason);
        }

        public async Task Save(ISession sessionData = null)
        {
            ValidateSession("CONTINUOUS");

            sessionData ??= new Session();
            sessionData.Name ??= $"Session on {DateHelper.GetFormattedDate(DateTime.UtcNow)}";

            await _apiService.SaveContinuousSession((string)_shortSessionId, sessionData);
        }

        public async Task Stop(ISession sessionData = null)
        {
            ValidateSession("PLAIN");

            await _apiService.StopSession((string)_shortSessionId, sessionData ?? new Session());

            _traceIdGenerator.SetSessionId("");
            _shortSessionId = false;
            _sessionState = "STOPPED";
        }

        public async Task Cancel()
        {
            ValidateSession();

            if (_sessionType == SessionType.CONTINUOUS)
                await _apiService.StopContinuousSession((string)_shortSessionId);
            else if (_sessionType == SessionType.PLAIN)
                await _apiService.CancelSession((string)_shortSessionId);

            _traceIdGenerator.SetSessionId("");
            _shortSessionId = false;
            _sessionState = "STOPPED";
        }

        public async Task CheckRemoteContinuousSession(ISession sessionPayload = null)
        {
            if (!_isInitialized)
                throw new Exception("Configuration not initialized. Call Init() before performing any actions.");

            sessionPayload ??= new Session();
            sessionPayload.ResourceAttributes = MergeAttributes(sessionPayload.ResourceAttributes, _resourceAttributes);

            var state = (await _apiService.CheckRemoteSession(sessionPayload)).State;

            if (state == "START" && _sessionState != "STARTED")
                await Start(SessionType.CONTINUOUS, sessionPayload);
            else if (state == "STOP" && _sessionState != "STOPPED")
                await Stop();
        }

        private void ValidateSession(string expectedType = null)
        {
            if (!_isInitialized)
                throw new Exception("Configuration not initialized. Call Init() before performing any actions.");

            if (_sessionState == "STOPPED" || _shortSessionId is not string)
                throw new Exception("Session should be active or paused");

            if (expectedType != null && _sessionType.ToString() != expectedType)
                throw new Exception("Invalid session type");
        }

        private Dictionary<string, object> MergeAttributes(Dictionary<string, object> a, Dictionary<string, object> b)
        {
            var result = new Dictionary<string, object>(a ?? new());
            if (b != null)
            {
                foreach (var kv in b)
                {
                    result[kv.Key] = kv.Value;
                }
            }
            return result;
        }
    }
}
