using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.RepresentationModel;
using System.IO;

namespace DenizenIRCBot
{
    public partial class dIRCBot
    {
        string scriptcommand_base(CommandDetails command)
        {
            if (command.Arguments.Count < 1)
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "That command is written as: " + ColorHighlightMajor
                    + Prefixes[0] + command.Name + " <link to a" + ColorLink + " http://mcmonkey.org/haste " + ColorGeneral + "paste>");
                return null;
            }
            string cmd = command.Arguments[0];
            if (!cmd.StartsWith("http://mcmonkey.org/haste/") && !cmd.StartsWith("http://mcmonkey.org/paste/"))
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "I am only trained to read pastes from" + ColorLink + " http://mcmonkey.org/haste");
                return null;
            }
            if (!cmd.EndsWith(".txt"))
            {
                cmd = cmd + ".txt";
            }
            string outp = null;
            try
            {
                LowTimeoutWebclient ltwc = new LowTimeoutWebclient();
                outp = ltwc.DownloadString(cmd);
            }
            catch (Exception)
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Failed to read that link - is the paste too long, or is there an error with the site?");
                return null;
            }
            if (outp == null)
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Failed to read that link - is the paste too long, or is there an error with the site?");
            }
            return outp;
        }

        void YAMLCommand(CommandDetails command)
        {
            string outp = scriptcommand_base(command);
            if (outp == null)
            {
                return;
            }
            try
            {
                YamlStream ys = new YamlStream();
                ys.Load(new StringReader(outp));
                int nodes = 0;
                for (int i = 0; i < ys.Documents.Count; i++)
                {
                    nodes += ys.Documents[i].AllNodes.ToArray().Length;
                }
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Successfully read a YAML file with " + ColorHighlightMajor + nodes + ColorGeneral + " nodes!");
            }
            catch (Exception ex)
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Error in your YAML: " + ColorHighlightMajor + ex.Message);
            }
        }

        void OLDdScriptCommand(CommandDetails command)
        {
            string outp = scriptcommand_base(command);
            if (outp == null)
            {
                return;
            }
            Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Scanning " + ColorHighlightMajor + (Utilities.CountChar(outp, '\n') + 1) + ColorGeneral + " lines...");
            try
            {
                YamlStream ys = new YamlStream();
                ys.Load(new StringReader(outp));
                int nodes = 0;
                for (int i = 0; i < ys.Documents.Count; i++)
                {
                    nodes += ys.Documents[i].AllNodes.ToArray().Length;
                }
            }
            catch (Exception ex)
            {
                Chat(command.Channel.Name, ColorGeneral + "Error in your YAML: " + ColorHighlightMajor + ex.Message);
            }
            List<string> warnings = dsCheck(outp);
            Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Found " + ColorHighlightMajor + warnings.Count + ColorGeneral + " potential issues.");
            for (int i = 0; i < warnings.Count && i < 8; i++)
            {
                Chat(command.Channel.Name, ColorHighlightMinor + "- " + i + ") " + warnings[i]);
            }
        }
    }
}
