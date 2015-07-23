using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DenizenIRCBot
{
    public partial class dIRCBot
    {
        public void TryCommand(CommandDetails command)
        {
            switch (command.Name.ToLower())
            {
                case "hello":
                case "allo":
                case "ello":
                case "hi":
                case "ohai":
                case "helo":
                case "halo":
                case "hai":
                case "howdy":
                case "website":
                case "web":
                case "site":
                case "link":
                case "url":
                case "greet":
                case "say":
                    Chat(command.Channel.Name, command.Pinger + ColorGeneral +
                        "Hello! I am a bot designed to assist with Denizen Scripting! I have a website too, at" +
                        ColorLink + " http://mcmonkey.org/logs " + ColorGeneral + "!");
                    break;
                default:
                    // Unknown command.
                    break;
            }
        }
    }
}
