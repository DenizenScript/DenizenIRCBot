using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DenizenIRCBot.GitHub.Json
{
    [DataContract]
    public class RateLimit
    {
        public int Limit
        {
            get
            {
                return resources.core.limit;
            }
        }

        public int Remaining
        {
            get
            {
                return resources.core.remaining;
            }
        }

        public DateTime NextReset
        {
            get
            {
                DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return epoch.AddSeconds(resources.core.reset);
            }
        }

        [DataMember]
        Resources resources = null;

        [DataContract]
        class Resources
        {
            [DataMember] public Data core = null;
            [DataMember] public Data search = null;
        }

        [DataContract]
        class Data
        {
            [DataMember] public int limit = 0;
            [DataMember] public int remaining = 0;
            [DataMember] public int reset = 0;
        }
    }
}
