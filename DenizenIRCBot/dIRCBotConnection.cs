using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DenizenIRCBot
{
    public partial class dIRCBot
    {
        /// <summary>
        /// The server address connected to.
        /// </summary>
        public string ServerAddress;

        /// <summary>
        /// The port of the server connected to.
        /// </summary>
        public ushort ServerPort;

        /// <summary>
        /// The socket that connects this bot to IRC.
        /// </summary>
        public Socket IRCSocket;

        /// <summary>
        /// The current name of the bot.
        /// </summary>
        public string Name;
    }
}
