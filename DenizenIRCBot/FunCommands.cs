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
            BotSnack snack = new BotSnack();
            lock (LaserLock)
            {
                if (RecentSnacks.Count > 30)
                {
                    RecentSnacks.RemoveAt(0);
                }
                RecentSnacks.Add(snack);
            }
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

        DateTime LasersChargedAt = DateTime.Now;

        List<BotSnack> LaserFood = new List<BotSnack>();

        double LaserPower = 0;

        bool LasersCharged = false;

        Object LaserLock = new Object();

        void LaserbeamsCommand(CommandDetails command)
        {
            lock (LaserLock)
            {
                if (LasersCharged)
                {
                    Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Pew! ... Just kidding, I won't charge my lasers twice, unless you want something to explode :o");
                    return;
                }
                if (RecentSnacks.Count < 4)
                {
                    Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Do I look like a magician?! I can't charge lasers without snacks!");
                    return;
                }
                int count = Utilities.random.Next(4) + 1;
                LasersCharged = true;
                LaserFood = new List<BotSnack>();
                LaserPower = 0;
                for (int i = 0; i < count; i++)
                {
                    int choice = Utilities.random.Next(RecentSnacks.Count);
                    BotSnack food = RecentSnacks[choice];
                    RecentSnacks.RemoveAt(choice);
                    LaserFood.Add(food);
                    LaserPower += food.Deliciousness;
                }
                if (LaserPower <= 0)
                {
                    Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Lasers are now charged as good as they'll ever be.");
                }
                else if (LaserPower <= 5)
                {
                    Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Mini-beam charging, as ordered!");
                }
                else if (LaserPower <= 10)
                {
                    Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Laser-beam charging, as ordered!");
                }
                else
                {
                    Chat(command.Channel.Name, command.Pinger + ColorGeneral + "Just hypothetically speaking, what would you say if I were to, oh I don't know, charge 5 laser beams at once?");
                }
            }
        }

        void FireCommand(CommandDetails command)
        {
            lock (LaserLock)
            {
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
