using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using SessionRecorder.Services;
using SessionRecorder.Types;
using SessionRecorder.Helpers;
using SessionRecorder.Constants;
using SessionRecorder.Trace;

namespace SessionRecorder
{
    public enum SessionState
    {
        STOPPED,
        STARTED,
        PAUSED
    }

    public class SessionRecorderConfig
    {
        public string ApiKey { get; set; } = default!;

        public SessionRecorderIdGenerator TraceIdGenerator { get; set; } = default!;

        public Dictionary<string, object>? ResourceAttributes { get; set; }

        // Either a bool or a delegate (Func<string>) — nullable union
        public bool? GenerateSessionShortIdLocally { get; set; }

        public Func<string>? GenerateSessionShortIdFunc { get; set; }
    }

    public class SessionRecorder
    {
        private static readonly object _lock = new object();
        private static SessionRecorder _instance;
        
        public static SessionRecorder Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new SessionRecorder();
                        }
                    }
                }
                return _instance;
            }
        }

        private bool _isInitialized = false;
        private object _shortSessionId = false;
        private SessionRecorderIdGenerator _traceIdGenerator;
        private SessionType _sessionType = SessionType.PLAIN;
        private SessionState _sessionState = SessionState.STOPPED;
        private readonly ApiService _apiService = new ApiService();
        private Func<string> _sessionShortIdGenerator = SessionRecorderSdk.GetIdGenerator(Constants.Constants.MULTIPLAYER_TRACE_DEBUG_SESSION_SHORT_ID_LENGTH);
        private Dictionary<string, object> _resourceAttributes = new();

        // Private constructor to prevent direct instantiation
        private SessionRecorder()
        {
        }

        public static void Init(SessionRecorderConfig config)
        {
            Instance.InitInternal(config);
        }

        private void InitInternal(SessionRecorderConfig config)
        {
            _resourceAttributes = config.ResourceAttributes ?? new Dictionary<string, object>
            {
                { SessionRecorderSpanAttribute.ATTR_MULTIPLAYER_SESSION_RECORDER_VERSION, Constants.Constants.SESSION_RECORDER_VERSION }
            };

            _isInitialized = true;

            if (config.GenerateSessionShortIdLocally == true)
                _sessionShortIdGenerator = SessionRecorderSdk.GetIdGenerator(Constants.Constants.MULTIPLAYER_TRACE_DEBUG_SESSION_SHORT_ID_LENGTH);

            if (string.IsNullOrEmpty(config.ApiKey))
                throw new Exception("Api key not provided");

            if (config.TraceIdGenerator == null)
                throw new Exception("Trace ID generator not provided");

            _traceIdGenerator = config.TraceIdGenerator;
            _apiService.Init(new ApiServiceConfig { ApiKey = config.ApiKey });
        }

        public static async Task Start(SessionType sessionType, ISession sessionPayload = null)
        {
            await Instance._Start(sessionType, sessionPayload);
        }

        private async Task _Start(SessionType sessionType, ISession sessionPayload = null)
        {
            if (!_isInitialized)
                throw new Exception("Configuration not initialized. Call Init() before performing any actions.");

            if (sessionPayload?.ShortId?.Length > 0 && sessionPayload.ShortId.Length != Constants.Constants.MULTIPLAYER_TRACE_DEBUG_SESSION_SHORT_ID_LENGTH)
                throw new Exception("Invalid short session id");

            sessionPayload ??= new Session();

            if (_sessionState != SessionState.STOPPED)
                throw new Exception("Session should be ended before starting new one.");

            _sessionType = sessionType;

            sessionPayload.Name ??= DateHelper.GetDefaultSessionName(DateTime.UtcNow);
            sessionPayload.ResourceAttributes = MergeAttributes(_resourceAttributes, sessionPayload.ResourceAttributes);

            ISession session = sessionType == SessionType.CONTINUOUS
                ? await _apiService.StartContinuousSession(ConvertToStartSessionRequest(sessionPayload))
                : await _apiService.StartSession(ConvertToStartSessionRequest(sessionPayload));

            _shortSessionId = session.ShortId;
            _traceIdGenerator.SetSessionId((string)_shortSessionId, _sessionType);
            _sessionState = SessionState.STARTED;
        }

        public static async Task Save(string reason = null)
        {
            await Instance.SaveContinuousSession(reason);
        }

        private async Task SaveContinuousSession(string reason = null)
        {
            await SessionRecorderSdk.SaveContinuousSession(reason);
        }

        public static async Task Save(ISession sessionData = null)
        {
            await Instance._Save(sessionData);
        }

        private async Task _Save(ISession sessionData = null)
        {
            ValidateSession("CONTINUOUS");

            sessionData ??= new Session();
            sessionData.Name ??= DateHelper.GetDefaultSessionName(DateTime.UtcNow);

            await _apiService.SaveContinuousSession((string)_shortSessionId, ConvertToStartSessionRequest(sessionData));
        }

        public static async Task Stop(ISession sessionData = null)
        {
            await Instance._Stop(sessionData);
        }

        private async Task _Stop(ISession sessionData = null)
        {
            ValidateSession("PLAIN");

            await _apiService.StopSession((string)_shortSessionId, ConvertToStopSessionRequest(sessionData ?? new Session()));

            _traceIdGenerator.SetSessionId("", SessionType.PLAIN);
            _shortSessionId = false;
            _sessionState = SessionState.STOPPED;
        }

        public static async Task Cancel()
        {
            await Instance._Cancel();
        }

        private async Task _Cancel()
        {
            ValidateSession();

            if (_sessionType == SessionType.CONTINUOUS)
                await _apiService.StopContinuousSession((string)_shortSessionId);
            else if (_sessionType == SessionType.PLAIN)
                await _apiService.CancelSession((string)_shortSessionId);

            _traceIdGenerator.SetSessionId("", _sessionType);
            _shortSessionId = false;
            _sessionState = SessionState.STOPPED;
        }

        public static async Task CheckRemoteContinuousSession(ISession sessionPayload = null)
        {
            await Instance._CheckRemoteContinuousSession(sessionPayload);
        }

        private async Task _CheckRemoteContinuousSession(ISession sessionPayload = null)
        {
            if (!_isInitialized)
                throw new Exception("Configuration not initialized. Call Init() before performing any actions.");

            sessionPayload ??= new Session();
            sessionPayload.ResourceAttributes = MergeAttributes(sessionPayload.ResourceAttributes, _resourceAttributes);

            var state = (await _apiService.CheckRemoteSession(ConvertToStartSessionRequest(sessionPayload))).State;

            if (state == "START" && _sessionState != SessionState.STARTED)
                await _Start(SessionType.CONTINUOUS, sessionPayload);
            else if (state == "STOP" && _sessionState != SessionState.STOPPED)
                await _Stop();
        }

        private void ValidateSession(string expectedType = null)
        {
            if (!_isInitialized)
                throw new Exception("Configuration not initialized. Call Init() before performing any actions.");

            if (_sessionState == SessionState.STOPPED || _shortSessionId is not string)
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

        private StartSessionRequest ConvertToStartSessionRequest(ISession session)
        {
            return new StartSessionRequest
            {
                Name = session.Name ?? DateHelper.GetDefaultSessionName(DateTime.UtcNow),
                Tags = new List<Tag>(), // Initialize empty tags list
                SessionAttributes = session.SessionAttributes ?? new Dictionary<string, object>(),
                ResourceAttributes = session.ResourceAttributes ?? new Dictionary<string, object>()
            };
        }

        private StopSessionRequest ConvertToStopSessionRequest(ISession session)
        {
            return new StopSessionRequest
            {
                SessionAttributes = session.SessionAttributes ?? new Dictionary<string, object>()
            };
        }
    }
}
