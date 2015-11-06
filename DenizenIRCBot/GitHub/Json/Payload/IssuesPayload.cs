using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DenizenIRCBot.GitHub.Json
{
    public class IssuesPayload
    {

        string action;
        int number;
        string title;
        string html_url;

        public IssuesPayload(Event issuesEvent)
        {
            Event.Load Payload = issuesEvent.Payload;
            action = Payload.action;
            Event.SimplifiedIssue issue = Payload.issue != null ? Payload.issue : Payload.pull_request;
            number = issue.number;
            title = issue.title;
            html_url = Payload.comment != null ? Payload.comment.html_url : issue.html_url;
        }

        public string Action
        {
            get
            {
                return action;
            }
        }

        public int Number
        {
            get
            {
                return number;
            }
        }

        public string Title
        {
            get
            {
                return title;
            }
        }

        public string ShortUrl
        {
            get
            {
                return Bitly.ShortenUrl(html_url);
            }
        }
    }
}
