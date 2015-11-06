using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DenizenIRCBot.GitHub.Json
{
    public class PushEventPayload
    {

        string _ref;
        List<Event.SimplifiedCommit> commits;

        public PushEventPayload(Event pushEvent)
        {
            _ref = pushEvent.Payload._ref;
            commits = pushEvent.Payload.commits;
        }

        public string Ref
        {
            get
            {
                return _ref.StartsWith("refs/heads/") ? _ref.Substring(11) : _ref;
            }
        }

        public List<Event.SimplifiedCommit> Commits
        {
            get
            {
                return commits;
            }
        }
    }
}
