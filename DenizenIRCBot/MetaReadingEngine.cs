using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace DenizenIRCBot
{
    public partial class dIRCBot
    {
        static string ReadMassiveRemoteZipFileButJavaCodeOnly(string url)
        {
            HighTimeoutWebclient wc = new HighTimeoutWebclient();
            wc.Encoding = UTF8;
            byte[] received = null;
            try
            {
                received = wc.DownloadData(url);
            }
            catch (Exception ex)
            {
                Logger.Output(LogType.ERROR, "Failed to read MassiveRemoteZip: " + ex.ToString() + " for " + url);
                return "";
            }
            wc.Dispose();
            MemoryStream ms = new MemoryStream(received);
            Logger.Output(LogType.DEBUG, "Received " + received.Length + " bytes to parse from <" + url + ">...");
            ZipStorer zs = ZipStorer.Open(ms, FileAccess.Read);
            StringBuilder toret = new StringBuilder();
            foreach (ZipStorer.ZipFileEntry zfe in zs.ReadCentralDir())
            {
                if (zfe.FilenameInZip.Contains(".java"))
                {
                    MemoryStream mes = new MemoryStream();
                    zs.ExtractFile(zfe, mes);
                    byte[] bytes = mes.ToArray();
                    mes.Close();
                    toret.Append("/<FILE:" + zfe.FilenameInZip + "\n");
                    toret.Append(UTF8.GetString(bytes));
                }
            }
            ms.Close();
            return toret.ToString();
        }

        public void ReadCore()
        {
            CoreLines = ReadMassiveRemoteZipFileButJavaCodeOnly("https://github.com/" + (dmonkey ? "mcmonkey4eva" : "DenizenScript") + "/Denizen-Core" + "/archive/master.zip").Replace("\r", "").Split('\n');
            Logger.Output(LogType.DEBUG, "Read CORE meta");
        }

        public void ReadBukkit()
        {
            BukkitLines = ReadMassiveRemoteZipFileButJavaCodeOnly("https://github.com/" + (dmonkey ? "mcmonkey4eva/Denizen" : "DenizenScript/Denizen-For-Bukkit") + "/archive/master.zip").Replace("\r", "").Split('\n');
            Logger.Output(LogType.DEBUG, "Read BUKKIT meta");
        }

        public void ReadDepenizenB()
        {
            DepenizenBLines = ReadMassiveRemoteZipFileButJavaCodeOnly("https://github.com/DenizenScript/Depenizen-For-Bukkit/archive/master.zip").Replace("\r", "").Split('\n');
            Logger.Output(LogType.DEBUG, "Read Depenizen-For-Bukkit meta");
        }

        public void ReadDIRCBOT()
        {
            DIRCBOTLines = ReadMassiveRemoteZipFileButJavaCodeOnly("https://github.com/DenizenScript/dIRCBot/archive/master.zip").Replace("\r", "").Split('\n');
            Logger.Output(LogType.DEBUG, "Read dIRCBot meta");
        }

        public bool dmonkey = false;

        public string[] CoreLines = null;

        public string[] BukkitLines = null;

        public string[] DepenizenBLines = null;

        public string[] DIRCBOTLines = null;

        public Object MetaLock = new Object();

        public void LoadMeta(bool monkey_repo)
        {
            lock (MetaLock)
            {
                dmonkey = monkey_repo;
                CoreLines = null;
                BukkitLines = null;
                Task.Factory.StartNew(() => { ReadCore(); });
                Task.Factory.StartNew(() => { ReadBukkit(); });
                Task.Factory.StartNew(() => { ReadDepenizenB(); });
                Task.Factory.StartNew(() => { ReadDIRCBOT(); });
                while (CoreLines == null || BukkitLines == null || DepenizenBLines == null || DIRCBOTLines == null)
                {
                    Thread.Sleep(16);
                }
                CoreMeta = new MetaSet();
                CoreMeta.LoadFrom(CoreLines);
                BukkitMeta = new MetaSet();
                BukkitMeta.LoadFrom(BukkitLines);
                ExternalMeta = new MetaSet();
                ExternalMeta.LoadFrom(DepenizenBLines);
                ExternalMeta.LoadFrom(DIRCBOTLines);
                AllMeta = new MetaSet();
                AllMeta.TakeAllFrom(CoreMeta);
                AllMeta.TakeAllFrom(BukkitMeta);
                AllMeta.TakeAllFrom(ExternalMeta);
            }
        }

        public MetaSet CoreMeta;

        public MetaSet BukkitMeta;

        public MetaSet ExternalMeta;

        public MetaSet AllMeta;
    }

    public class MetaSet
    {
        public void TakeAllFrom(MetaSet set)
        {
            Objects.AddRange(set.Objects);
            Actions.AddRange(set.Actions);
            Tutorials.AddRange(set.Tutorials);
            Mechanisms.AddRange(set.Mechanisms);
            Tags.AddRange(set.Tags);
            Commands.AddRange(set.Commands);
            Languages.AddRange(set.Languages);
            Events.AddRange(set.Events);
        }
        
        public List<dObject> Objects = new List<dObject>();

        public List<dAction> Actions = new List<dAction>();

        public List<dTutorial> Tutorials = new List<dTutorial>();

        public List<dMechanism> Mechanisms = new List<dMechanism>();

        public List<dTag> Tags = new List<dTag>();

        public List<dCommand> Commands = new List<dCommand>();

        public List<dLanguage> Languages =  new List<dLanguage>();

        public List<dEvent> Events = new List<dEvent>();

        public void LoadFrom(string[] lines)
        {
            string fname = "UNKNOWN";
            for (int i = 0; i < lines.Length; i++)
            {
                string cline = lines[i].Trim();
                if (cline.StartsWith("/<FILE:"))
                {
                    fname = cline.Substring("/<FILE:".Length);
                }
                if (!cline.StartsWith("//"))
                {
                    continue;
                }
                if (cline.Length < 4)
                {
                    cline = "//  ";
                }
                cline = cline.Substring(3).Trim();
                if (cline.StartsWith("<--["))
                {
                    string objtype = cline.Substring(4, cline.Length - 5).ToLower();
                    dObject nobj = null;
                    switch (objtype)
                    {
                        case "action":
                            nobj = new dAction();
                            Actions.Add((dAction)nobj);
                            break;
                        case "example":
                        case "tutorial":
                            nobj = new dTutorial();
                            Tutorials.Add((dTutorial)nobj);
                            break;
                        case "mechanism":
                            nobj = new dMechanism();
                            Mechanisms.Add((dMechanism)nobj);
                            break;
                        case "tag":
                            nobj = new dTag();
                            Tags.Add((dTag)nobj);
                            break;
                        case "command":
                            nobj = new dCommand();
                            Commands.Add((dCommand)nobj);
                            break;
                        case "language":
                            nobj = new dLanguage();
                            Languages.Add((dLanguage)nobj);
                            break;
                        case "event":
                            nobj = new dEvent();
                            Events.Add((dEvent)nobj);
                            break;
                        case "requirement":
                            break;
                        default:
                            Logger.Output(LogType.ERROR, "Unknown object type " + objtype + " in " + fname);
                            break;
                    }
                    if (nobj == null)
                    {
                        continue;
                    }
                    nobj.FileName = fname;
                    Objects.Add(nobj);
                    i++;
                    while (i < lines.Length)
                    {
                        cline = lines[i].Trim();
                        if (!cline.StartsWith("//"))
                        {
                            Logger.Output(LogType.ERROR, "Found line <<" + cline + ">> in the middle of an object declaration in " + fname);
                            i++;
                            continue;
                        }
                        if (cline.Length < 4)
                        {
                            cline = "//  ";
                        }
                        cline = cline.Substring(3);
                        if (cline == "-->")
                        {
                            break;
                        }
                        if (!cline.StartsWith("@"))
                        {
                            Logger.Output(LogType.ERROR, "Found line '// " + cline + "' in the middle of an object declaration in " + fname);
                            i++;
                            continue;
                        }
                        string typer = cline.Substring(1);
                        string value = "";
                        if (typer.Contains(' '))
                        {
                            value += typer.Substring(typer.IndexOf(' ') + 1);
                            typer = typer.Substring(0, typer.IndexOf(' '));
                        }
                        while (i + 1 < lines.Length)
                        {
                            cline = lines[i + 1].Trim();
                            if (cline.Length < 4)
                            {
                                cline = "//  ";
                            }
                            cline = cline.Substring(3);
                            if ((cline.StartsWith("@") && !cline.StartsWith("@ ")) || cline == "-->")
                            {
                                break;
                            }
                            value += "\n" + cline;
                            i++;
                        }
                        nobj.ApplyVar(typer.ToLower(), value);
                        i++;
                    }
                }
            }
        }
    }
}
