using System.Collections.Generic;
using Multiplayer.SessionRecorder.Services;

namespace Multiplayer.SessionRecorder.Types
{
    public interface ISession
    {
        string? name { get; set; }
        string? shortId { get; set; }
        List<Tag>? tags { get; set; }
        Dictionary<string, object>? resourceAttributes { get; set; }
        Dictionary<string, object>? sessionAttributes { get; set; }
    }

    public class Session : ISession
    {
        public string? name { get; set; }
        public string? shortId { get; set; }
        public List<Tag>? tags { get; set; }
        public Dictionary<string, object>? resourceAttributes { get; set; }
        public Dictionary<string, object>? sessionAttributes { get; set; }
    }

    public enum SessionType
    {
        PLAIN,
        CONTINUOUS
    }
}
