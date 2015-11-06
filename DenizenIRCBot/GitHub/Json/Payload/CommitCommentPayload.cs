using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DenizenIRCBot.GitHub.Json
{
    public class CommitCommentPayload
    {

        string html_url;

        public CommitCommentPayload(Event commentEvent)
        {
            html_url = commentEvent.Payload.comment.html_url;
        }

        public string CommitId
        {
            get
            {
                int index = html_url.IndexOf("/commit/") + 8;
                return html_url.Substring(index, 7);
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
