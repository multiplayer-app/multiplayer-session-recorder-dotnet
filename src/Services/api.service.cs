using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using SessionRecorder.Types;
using SessionRecorder.Config;
using SessionRecorder.Constants;

namespace SessionRecorder.Services
{
    public class StartSessionRequest
{
    public string Name { get; set; } = string.Empty;
    public List<Tag> Tags { get; set; } = new List<Tag>();
    public Dictionary<string, object> SessionAttributes { get; set; } = new Dictionary<string, object>();
    public Dictionary<string, object> ResourceAttributes { get; set; } = new Dictionary<string, object>();
}

public class StopSessionRequest
{
    public Dictionary<string, object> SessionAttributes { get; set; } = new Dictionary<string, object>();
}

public class Tag
{
    public string? Key { get; set; }
    public string Value { get; set; } = string.Empty;
}

public class RemoteSessionResponse
{
    public string State { get; set; } = "STOP";
}


public class ApiServiceConfig
{
    public string? ApiKey { get; set; }
    public string ExporterApiBaseUrl { get; set; } = Constants.Constants.MULTIPLAYER_BASE_API_URL;
    public bool? ContinuousDebugging { get; set; }
}

public class ApiService
{
    private ApiServiceConfig _config;
    private readonly HttpClient _httpClient;

    public ApiService()
    {
        _config = new ApiServiceConfig();
        _httpClient = new HttpClient();
    }

    public void Init(ApiServiceConfig config)
    {
        _config = MergeConfig(_config, config);
    }

    public void UpdateConfigs(ApiServiceConfig config)
    {
        _config = MergeConfig(_config, config);
    }

    public async Task<ISession> StartSession(StartSessionRequest requestBody, CancellationToken cancellationToken = default)
    {
        return await MakeRequest<ISession>("/debug-sessions/start", HttpMethod.Post, requestBody, cancellationToken);
    }

    public async Task<object?> StopSession(string sessionId, StopSessionRequest requestBody)
    {
        return await MakeRequest<object>($"/debug-sessions/{sessionId}/stop", new HttpMethod("PATCH"), requestBody);
    }

    public async Task<object?> CancelSession(string sessionId)
    {
        return await MakeRequest<object>($"/debug-sessions/{sessionId}/cancel", HttpMethod.Delete);
    }

    public async Task<ISession> StartContinuousSession(StartSessionRequest requestBody, CancellationToken cancellationToken = default)
    {
        return await MakeRequest<ISession>("/continuous-debug-sessions/start", HttpMethod.Post, requestBody, cancellationToken);
    }

    public async Task<object?> SaveContinuousSession(string sessionId, StartSessionRequest requestBody, CancellationToken cancellationToken = default)
    {
        return await MakeRequest<object>($"/continuous-debug-sessions/{sessionId}/save", HttpMethod.Post, requestBody, cancellationToken);
    }

    public async Task<object?> StopContinuousSession(string sessionId)
    {
        return await MakeRequest<object>($"/continuous-debug-sessions/{sessionId}/cancel", HttpMethod.Delete);
    }

    public async Task<RemoteSessionResponse> CheckRemoteSession(StartSessionRequest requestBody, CancellationToken cancellationToken = default)
    {
        return await MakeRequest<RemoteSessionResponse>("/remote-debug-session/check", HttpMethod.Post, requestBody, cancellationToken);
    }

    private async Task<T?> MakeRequest<T>(string path, HttpMethod method, object? body = null, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(method, $"{_config.ExporterApiBaseUrl}/v0/radar{path}");

        if (body != null)
        {
            var json = JsonSerializer.Serialize(body);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        if (!string.IsNullOrEmpty(_config.ApiKey))
        {
            request.Headers.Add("X-Api-Key", _config.ApiKey);
        }

        try
        {
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Network response was not ok: {response.StatusCode}");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return default;
            }

            var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken);
        }
        catch (TaskCanceledException)
        {
            throw new OperationCanceledException("Request aborted");
        }
    }

    private ApiServiceConfig MergeConfig(ApiServiceConfig original, ApiServiceConfig update)
    {
        return new ApiServiceConfig
        {
            ApiKey = update.ApiKey ?? original.ApiKey,
            ExporterApiBaseUrl = update.ExporterApiBaseUrl ?? original.ExporterApiBaseUrl,
            ContinuousDebugging = update.ContinuousDebugging ?? original.ContinuousDebugging
        };
    }
}
}
