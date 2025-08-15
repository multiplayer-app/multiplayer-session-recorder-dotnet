using System.Collections.Generic;

namespace Multiplayer.SessionRecorder.Types
{
    public interface ISession
    {
        string? Name { get; set; }
        string? ShortId { get; set; }
        Dictionary<string, object>? ResourceAttributes { get; set; }
        Dictionary<string, object>? SessionAttributes { get; set; }
    }

    public class Session : ISession
    {
        public string? Name { get; set; }
        public string? ShortId { get; set; }
        public Dictionary<string, object>? ResourceAttributes { get; set; }
        public Dictionary<string, object>? SessionAttributes { get; set; }
    }

    public enum SessionType
    {
        PLAIN,
        CONTINUOUS
    }
}
