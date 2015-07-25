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
                Logger.Output(LogType.ERROR, "Failed to read MassiveRemoteZip: " + ex.ToString());
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

        public void ReaddBukkit()
        {
        }

        public bool dmonkey = false;

        public string[] CoreLines = null;

        public Object MetaLock = new Object();

        public void LoadMeta(bool monkey_repo)
        {
            lock (MetaLock)
            {
                dmonkey = monkey_repo;
                CoreLines = null;
                Task.Factory.StartNew(() => { ReadCore(); });
                while (CoreLines == null)
                {
                    Thread.Sleep(16);
                }
                CoreMeta = new MetaSet();
                CoreMeta.LoadFrom(CoreLines);
                AllMeta = new MetaSet();
            }
        }

        public MetaSet CoreMeta;

        public MetaSet AllMeta;
    }

    public class MetaSet
    {
        public List<dObject> Objects = new List<dObject>();

        public void LoadFrom(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                string cline = lines[i].Trim();
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
                    Logger.Output(LogType.DEBUG, "Found object of type " + objtype);
                    dObject nobj = null;
                    switch (objtype)
                    {
                        case "action":
                            nobj = new dAction();
                            break;
                        case "example":
                        case "tutorial":
                            nobj = new dTutorial();
                            break;
                        case "mechanism":
                            nobj = new dMechanism();
                            break;
                        case "tag":
                            nobj = new dTag();
                            break;
                        case "command":
                            nobj = new dCommand();
                            break;
                        case "language":
                            nobj = new dLanguage();
                            break;
                        case "event":
                            nobj = new dEvent();
                            break;
                        default:
                            Logger.Output(LogType.ERROR, "Unknown object type " + objtype);
                            break;
                    }
                    if (nobj == null)
                    {
                        continue;
                    }
                    Objects.Add(nobj);
                    i++;
                    while (i < lines.Length)
                    {
                        cline = lines[i].Trim();
                        if (!cline.StartsWith("//"))
                        {
                            Logger.Output(LogType.ERROR, "Found line <<" + cline + ">> in the middle of an object declaration!");
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
                            Logger.Output(LogType.ERROR, "Found line '// " + cline + "' in the middle of an object declaration!");
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
                        Logger.Output(LogType.DEBUG, ":: Applying " + typer.ToLower() + " = " + value);
                        nobj.ApplyVar(typer.ToLower(), value);
                        i++;
                    }
                }
            }
        }
    }
}
