using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DenizenIRCBot
{
    public partial class dIRCBot
    {
        public List<BotSnack> RecentSnacks = new List<BotSnack>();

        public Dictionary<string, double> TasteForSnacks = new Dictionary<string, double>();

        void BotsnackCommand(CommandDetails command)
        {
            if (command.Arguments.Count < 1)
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "That command is written " + ColorHighlightMajor + Prefixes[0] + command.Name + " <food item>");
                return;
            }
            if (RecentSnacks.Count > 30)
            {
                RecentSnacks.RemoveAt(0);
            }
            BotSnack snack = new BotSnack();
            RecentSnacks.Add(snack);
            snack.Giver = command.User.Name;
            snack.TimeGiven = DateTime.Now;
            snack.Target = command.Arguments[0];
            double delic;
            if (TasteForSnacks.TryGetValue(snack.Target.ToLower(), out delic))
            {
                snack.Deliciousness = delic;
            }
            else
            {
                snack.Deliciousness = Utilities.random.NextDouble() * 10 - 3;
                TasteForSnacks.Add(snack.Target.ToLower(), snack.Deliciousness);
            }
            if (TasteForSnacks.Count > 500)
            {
                TasteForSnacks.Remove(TasteForSnacks.Keys.First());
            }
            if (snack.Deliciousness <= 0)
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Gross, I hate the taste of " + ColorHighlightMajor + snack.Target);
            }
            else if (snack.Deliciousness <= 3)
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Thanks, I can't complain about a little " + ColorHighlightMajor + snack.Target);
            }
            else if (snack.Deliciousness <= 6)
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Thanks! I really enjoyed that " + ColorHighlightMajor + snack.Target);
            }
            else
            {
                Chat(command.Channel.Name, command.Pinger + ColorGeneral + "WOW THAT " + ColorHighlightMajor + snack.Target + ColorGeneral + " WAS THE BEST SNACK I'VE EVER HAD!");
            }
        }
    }

    public class BotSnack
    {
        public string Giver;

        public string Target;

        public DateTime TimeGiven;

        public double Deliciousness = 0;
    }
}
