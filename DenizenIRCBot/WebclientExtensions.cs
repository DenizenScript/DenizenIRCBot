using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace DenizenIRCBot
{
    [System.ComponentModel.DesignerCategory("")]
    class LowTimeoutWebclient : WebClient
    {
        public LowTimeoutWebclient()
        {
        }
        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = 5 * 1000;
            return w;
        }
    }
    class HighTimeoutWebclient : WebClient
    {
        public HighTimeoutWebclient()
        {
        }
        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = 20 * 1000;
            return w;
        }
    }
}
