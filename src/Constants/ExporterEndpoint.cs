namespace Multiplayer.SessionRecorder.Constants;

struct SessionRecorderExporterEndpoint
{
    public const string HttpEndpoint = "https://api.multiplayer.app";

    public const string HttpLogsEndpoint = "https://api.multiplayer.app/v1/logs";

    public const string HttpTracesEndpoint = "https://api.multiplayer.app/v1/traces";

    public const string GrpcEndpoint = "https://api.multiplayer.app:4317";

    public const string GrpcLogsEndpoint = "https://api.multiplayer.app:4317/v1/logs";

    public const string GrpcTracesEndpoint = "https://api.multiplayer.app:4317/v1/traces";
}
