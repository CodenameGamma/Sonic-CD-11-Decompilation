using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Text.RegularExpressions;

using System.Numerics;

using System.Threading;


namespace ChatVSSonicCD
{
    public class ChatParse
    {
        public static Random random = new Random();
        public static int RandomNumber(int min, int max)
        {
            return random.Next(min, max);
        }
        // Generate a random string with a given size  
        public static string RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }
        // Generate a random 14 characters    
        public static string Random14Char()
        {
            string s = "";
            for (int i = 0; i < 14; ++i)
                s += (char)(0x30 + random.Next(10));
            return s;
        }
        public static int Timer = 0;


        public static string ServerIP = "irc.twitch.tv";
        public static string Room = "codenamegamma";
        public static string User = $"justinfan{Random14Char()}";
        public static string Key = "";
        public static int Port = 6667;

        public static TwitchIRCClient TwitchClient = new TwitchIRCClient(ServerIP, Port, User, Key);


        public static void StartChatBots()
        {
            //Debugger.Launch();
            try
            {
                Console.Write("Starting ChatBots! \n");

                LoadINIFile(AppDomain.CurrentDomain.BaseDirectory);
                /*
                if(AllowControlForm)
                {
                    Console.Write("------------------------------------------ \n");
                    Console.Write("Showing Control From \n");
                    new Thread(() => ShowControlForm()).Start();
                    Console.Write("------------------------------------------ \n");
                }*/


                //Assembly.LoadFile(Path.GetFullPath(Path.Combine(s, "Newtonsoft.Json.dll")));


                TwitchIRCStart();



            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
            }

        }

        public static void ProcessTwitchClient()
        {
            TwitchClient.JoinChannel(Room);
            while (TwitchClient.connected)
            {
                string message = TwitchClient.ReadMessage();
                if (message != null && message.Length > 1)
                {
                    if (message.ToLower().Contains("tmi.twitch.tv privmsg #"))
                    {
                        string name = "Unknown";
                        try
                        {
                            name = message.Split(':')[1].Split('!')[0];
                        }
                        catch { }
                        ProcessMessage(false, 0, message.Split(new[] { ("#" + Room + " :") }, StringSplitOptions.None)[1], name, null);
                    }
                    else if (message.ToLower().Contains("tmi.twitch.tv 366"))
                    {
                        // Comment the line below if you want to see what we're sending and receiving
                        //irc.printIRC = false;
                    }
                }
                Thread.Sleep(50);
            }

        }

        public static void ProcessDiscord()
        {
            //Discord.StartDiscord(MessageReceived);
        }

        private static Task MessageReceived(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage userMessage))
                return Task.CompletedTask;

            if (userMessage.Author.IsBot)
                return Task.CompletedTask;


            ProcessMessage(false, 0, userMessage.Content, userMessage.Author.Username + "#" + userMessage.Author.Discriminator, (message) =>
            {
                arg.Channel.SendMessageAsync(message);
                return 0;
            });
            return Task.CompletedTask;
        }
        public static string RemoveBadChars(string word)
        {
            Regex reg = new Regex("[^a-zA-Z' ]");
            return reg.Replace(word, string.Empty);
        }


        public static ConcurrentQueue<Action> Commands { get; } = new ConcurrentQueue<Action>();

        public static void ProcessMessage(bool IsDonator, int Myuserlevel, string message, string channelName, Func<string, int> sendChatMessage)
        {
            // A shitty way of doing this. (Using Relection would look nicer but takes alot more work)
            string msg = message.ToLower();

            if (channelName.ToLower() == "streamlabs")
            {

                msg = msg.Substring(2);
                Console.WriteLine("Message:{0}", msg);
            }

      
            if (msg.Contains("!shove"))
            {
                Commands.Enqueue(() =>
                {
                    

                    Console.WriteLine("Shoved the Player!");
                });
            }

        }
        public static bool EnableDiscordSupport = false;
        public static bool EnableTwitchSupport = false;
        public static bool EnableYoutubeSupport = false;
        public static bool AllowControlForm = false;
        public static bool AllowVideoIDForm = false;
        public static bool AllowBuildHTMLPagesForOverlay = true;
        public static string YoutubeVideoIDFromSettings = "";
        public static string YoutubeAPIKey = "";
        public static string SupportersEnableSecret = "BirthdaySandwich";
        public static string twitchusername = "";
        public static string twitchOAuthKey = "";


        public static bool TwitchIRCStart()
        {
            //TwitchIRCThread Tirc = new Thread(PulseTwitchIRC);
            //Tirc.Start();

            if (EnableTwitchSupport)
            {
                Console.Write("------------------------------------------ \n");
                Console.Write("Starting Twitch IRC Client. \n");
                new Thread(() => ProcessTwitchClient()).Start();
                Console.Write("------------------------------------------ \n");
            }
            if (EnableDiscordSupport)
            {
                Console.Write("------------------------------------------ \n");
                ProcessDiscord();
                Console.Write("Starting Discord Client \n");
                Console.Write("------------------------------------------ \n");
            }
            if (EnableYoutubeSupport)
            {
                new Thread(() => Youtube.ProcessYoutubeThread()).Start();
            }
            return true;
        }
        public static void LoadINIFile(string path)
        {

            Console.Write("Loading Settings. \n");
            var MyIni = new IniFile(@path + "/ChatVsSettings.ini");
            if (!MyIni.KeyExists("ShowControlFormWhenLaunching", "General"))
            {
                MyIni.Write("ShowControlFormWhenLaunching", true.ToString(), "General");
            }
            if (!MyIni.KeyExists("EnableTwitchSupport", "Twitch"))
            {
                MyIni.Write("EnableTwitchSupport", true.ToString(), "Twitch");
            }
            if (!MyIni.KeyExists("TwitchChannelToJoin", "Twitch"))
            {
                MyIni.Write("TwitchChannelToJoin", "codenamegamma", "Twitch");
            }
            if (!MyIni.KeyExists("EnableDiscordSupport", "Discord"))
            {
                MyIni.Write("EnableDiscordSupport", false.ToString(), "Discord");
            }
            if (!MyIni.KeyExists("DiscordBotKey", "Discord"))
            {
                MyIni.Write("DiscordBotKey", "SupplyYourOwnBotKey", "Discord");
            }
            if (!MyIni.KeyExists("EnableYoutubeSupport", "Youtube"))
            {
                MyIni.Write("EnableYoutubeSupport", false.ToString(), "Youtube");
            }
            if (!MyIni.KeyExists("YoutubeVideoIdToListen", "Youtube"))
            {
                MyIni.Write("YoutubeVideoIdToListen", "VideoIdGoesHere", "Youtube");
            }
            if (!MyIni.KeyExists("YoutubeAPIKey", "Youtube"))
            {
                MyIni.Write("YoutubeAPIKey", "SupplyYourOwnAPIKey", "Youtube");
            }

            bool.TryParse(MyIni.Read("ShowControlFormWhenLaunching", "General"), out bool ToggleAllowControlForm);
            AllowControlForm = ToggleAllowControlForm;

            bool.TryParse(MyIni.Read("EnableTwitchSupport", "Twitch"), out bool ToggleTwitch);
            EnableTwitchSupport = ToggleTwitch;
            Room = MyIni.Read("TwitchChannelToJoin", "Twitch");
            bool.TryParse(MyIni.Read("EnableDiscordSupport", "Discord"), out bool ToggleDiscord);
            EnableDiscordSupport = ToggleDiscord;
            Discord.TempTokenToLoad = MyIni.Read("DiscordBotKey", "Discord");
            bool.TryParse(MyIni.Read("EnableYoutubeSupport", "Youtube"), out bool ToggleYoutube);
            EnableYoutubeSupport = ToggleYoutube;

            YoutubeVideoIDFromSettings = MyIni.Read("YoutubeVideoIdToListen", "Youtube");
            Console.Write("------------------------------------------ \n");
            Console.Write("Twitch Support - {0} \n", EnableTwitchSupport.ToString());
            Console.Write("Discord Support - {0} \n", EnableDiscordSupport.ToString());
            Console.Write("Youtube Support - {0} \n", EnableYoutubeSupport.ToString());
            Console.Write("------------------------------------------ \n");
            //Command Toggles 

            Console.Write("Done Loading Settings, \n");

        }

        
    }
    //public static extern void DoThing(int id);
    //[DllImport("ChatVSSonicCD.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
}
