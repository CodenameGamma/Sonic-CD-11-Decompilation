using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading;
using System.Timers;

namespace ChatVSSonicCD
{
    class Youtube
    {
        const int EVENT_OTHER_SOCIAL = 70;
        const string LiveID = "lD7vXiGDJbQ";
        public static bool ReadyToCheck = false;
        public static YouTubeMessage YoutubeMessage = new YouTubeMessage();
        public static List<YouTubeMessage> YoutubeMessageList = new List<YouTubeMessage>();
        public static List<YouTubeMessage> SpentYoutubeMessages = new List<YouTubeMessage>();
        public struct YouTubeMessage
        {
            public string UUID;
            public string Author;
            public string Message;
            public bool Donator;
            public int Userlevel;
            public DateTime PublishedTime;


            public YouTubeMessage(int IsUserLevel, bool IsDonator, string UniqueID, string author, string msg, DateTime PubTime)
            {
                Userlevel = IsUserLevel;
                Donator = IsDonator;
                UUID = UniqueID;
                Author = author;
                Message = msg;
                PublishedTime = PubTime;
            }
        }
        public static DateTime TimeDateStarted;
        public static void ProcessYoutubeThread()
        {
            if (false)
            {
                Console.Write("------------------------------------------ \n");
                Console.Write("Starting Youtube Client Form To Check Video ID \n");
                //URLForm FrmURLCHECK = new URLForm();
                //FrmURLCHECK.ShowDialog();

                Console.Write("------------------------------------------ \n");
                while (ReadyToCheck == false)
                {
                    Thread.Sleep(10);
                }
            }
            TimeDateStarted = DateTime.UtcNow;
            Console.Write("Starting Youtube Client With Video ID {0} \n", ChatParse.YoutubeVideoIDFromSettings);
            System.Timers.Timer GetMessages = new System.Timers.Timer();
            GetMessages.Interval = 10000;
            GetMessages.Elapsed += TimerOnElapsed;
            GetMessages.Start();
            while (true)
            {
                lock (YoutubeMessageList)
                {
                    //Console.Write("Codehit! \n");
                    CheckMessageList();

                }
                Thread.Sleep(10);
            }
        }
        public static bool GetMessagesIsRuning = false;
        public static void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            GetMessagesIsRuning = true;

            GetMessagesIsRuning = false;
            lock (YoutubeMessageList)
            {
                Console.Write("Youtube Message Pull Running! \n");
                Get_Chat(ChatParse.YoutubeVideoIDFromSettings);
            }
        }
        public static void CheckMessageList()
        {
            if (YoutubeMessageList.Count > 0)
            {
                foreach (YouTubeMessage CurrentTYMessage in YoutubeMessageList)
                {
                    DateTime OffsetTime;
                    TimeSpan TimeDiffrence;
                    TimeDiffrence = DateTime.UtcNow - CurrentTYMessage.PublishedTime;
                    OffsetTime = DateTime.UtcNow + TimeDiffrence;
                    //Console.Write("MSG: {0}, DATEPUB: {1}, DATEOFF: {2}, DiffrenceL {3) \n",CurrentTYMessage.Message.ToString() , CurrentTYMessage.PublishedTime.ToShortTimeString(), OffsetTime.ToShortTimeString(), TimeDiffrence.ToString());

                    ChatParse.ProcessMessage(CurrentTYMessage.Donator, CurrentTYMessage.Userlevel, CurrentTYMessage.Message, CurrentTYMessage.Author, null);
                    Console.Write("Processed Message - Removing");
                    SpentYoutubeMessages.Add(CurrentTYMessage); //We're done with it. 
                    Thread.Sleep(9000 / YoutubeMessageList.Count);



                }
                YoutubeMessageList.RemoveAll(x => SpentYoutubeMessages.Contains(x)); //Remove Dead Messages
            }

        }
        public static bool isCurrenctDateBetween(DateTime fromDate, DateTime toDate)
        {
            DateTime curent = DateTime.Now.Date;
            if (fromDate.CompareTo(toDate) >= 1)
            {

            }
            int cd_fd = curent.CompareTo(fromDate);
            int cd_td = curent.CompareTo(toDate);

            if (cd_fd == 0 || cd_td == 0)
            {
                return true;
            }

            if (cd_fd >= 1 && cd_td <= -1)
            {
                return true;
            }
            return false;
        }
        public static List<string> SeenUUIDs = new List<string>();
        public static string LastMessageUUID = "";
        public static bool CheckUUID(string MessageUUID)
        {

            foreach (string StoredUUID in SeenUUIDs)
            {
                if (MessageUUID == StoredUUID)
                {
                    return false;
                }
            }
            return true;
        }
        public static string CurrentActiveLiveChatIDLink = "";
        //string[] Timeformats = { "yyyy-MM-ddTHH\:mm\:ss.fffffffzzz" };
        //[DllExport("Get_Chat", CallingConvention.Cdecl)]
        public static void Get_Chat(string str)
        {
            string MessageType = "";
            string ReturnString = "";
            string MessageAuthor = "";
            string AuthorIsOwner = "";
            string AuthorIsModerator = "";
            string AuthorIsSponsor = "";
            string MessageUUID = "";
            string AuthorType = "";
            int UserLevel = 0;
            DateTime MessageDateTime = DateTime.UtcNow;
            string Key = "AIzaSyAifpi10aEuUbuyEyP0KJsAqTI3Xvo8mRw";
            string videoId = "NpUWbPg3yyc";
            string videoLink;
            if (CurrentActiveLiveChatIDLink == "")
            {
                Console.WriteLine("No Active Chat ID Stored - Grabbing Current Video's LiveChatID");
                videoLink = "https://www.googleapis.com/youtube/v3/videos?id=" + videoId + "&key=" + Key + "&part=liveStreamingDetails,snippet";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(videoLink);
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (System.IO.Stream stream = response.GetResponseStream())
                using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
                {


                    JObject joResponse = JObject.Parse(reader.ReadToEnd());
                    String liveChatId = joResponse["items"][0]["liveStreamingDetails"]["activeLiveChatId"].ToString();
                    String chatListLink = "https://www.googleapis.com/youtube/v3/liveChat/messages?liveChatId=" + liveChatId + "&part=id%2C+snippet%2C+authorDetails&key=" + Key;
                    CurrentActiveLiveChatIDLink = chatListLink;

                }
                Console.WriteLine("Should of set a video ID. ");
            }
            else
            {
                HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create(CurrentActiveLiveChatIDLink);
                using (HttpWebResponse response2 = (HttpWebResponse)request2.GetResponse())
                using (System.IO.Stream stream2 = response2.GetResponseStream())
                using (System.IO.StreamReader reader2 = new System.IO.StreamReader(stream2))
                {

                    JObject joResponse2 = JObject.Parse(reader2.ReadToEnd());
                    foreach (var item in joResponse2["items"])
                    {
                        MessageType = Convert.ToString(item["snippet"]["type"]);
                        ReturnString = Convert.ToString(item["snippet"]["displayMessage"]);
                        MessageUUID = Convert.ToString(item["id"]);
                        MessageAuthor = Convert.ToString(item["authorDetails"]["displayName"]);
                        AuthorIsOwner = Convert.ToString(item["authorDetails"]["isChatOwner"]);
                        AuthorIsModerator = Convert.ToString(item["authorDetails"]["isChatModerator"]);
                        AuthorIsSponsor = Convert.ToString(item["authorDetails"]["isChatSponsor"]);
                        UserLevel = 0;
                        MessageDateTime = DateTime.Parse(Convert.ToString(item["snippet"]["publishedAt"]));
                        if (MessageDateTime < TimeDateStarted) continue;
                        //Console.Write("YTU-UID: {0} - FROM: {1} - Message: {2} \n", MessageUUID, MessageAuthor, ReturnString);
                        if (SeenUUIDs.Count > 0)
                        {
                            if (CheckUUID(MessageUUID))
                            {
                                //Message Is Unique - send it!
                                YoutubeMessage.Donator = false;
                                UserLevel = 0;
                                if (AuthorIsOwner == "True")
                                {
                                    UserLevel = 3;
                                }
                                if (AuthorIsModerator == "True")
                                {
                                    UserLevel = 2;
                                }
                                if (AuthorIsSponsor == "True")
                                {
                                    UserLevel = 1;
                                }
                                if (MessageType == "textMessageEvent")
                                {
                                    YoutubeMessage.Donator = false;
                                    Console.WriteLine("NORMAL");
                                }
                                if (MessageType == "superChatEvent" || MessageType == "superStickerEvent")
                                {
                                    YoutubeMessage.Donator = true;
                                    Console.WriteLine("IS DONATOR!!!!!");
                                }
                                Console.WriteLine(" ");
                                //Console.WriteLine("MessageType IS {0}", MessageType);
                                YoutubeMessage.Userlevel = UserLevel;
                                YoutubeMessage.UUID = MessageUUID;
                                YoutubeMessage.Author = MessageAuthor;
                                YoutubeMessage.Message = ReturnString;
                                YoutubeMessage.PublishedTime = MessageDateTime;

                                YoutubeMessageList.Add(YoutubeMessage);

                                Console.WriteLine("UserLevel: {3} Type: {0} - FROM: {1} - Message: {2} \n", MessageType, MessageAuthor, ReturnString, UserLevel.ToString());

                                SeenUUIDs.Add(MessageUUID);
                            }

                        }
                        else
                        {
                            UserLevel = 0;
                            if (AuthorIsOwner == "True")
                            {
                                UserLevel = 3;
                            }
                            if (AuthorIsModerator == "True")
                            {
                                UserLevel = 2;
                            }
                            if (AuthorIsSponsor == "True")
                            {
                                UserLevel = 1;
                            }
                            YoutubeMessage.Donator = false;
                            if (MessageType == "textMessageEvent")
                            {
                                YoutubeMessage.Donator = false;
                            }
                            if (MessageType == "superChatEvent" || MessageType == "superStickerEvent")
                            {
                                YoutubeMessage.Donator = true;
                            }
                            Console.WriteLine(" ");

                            //we haven't seen any messages before...Lose it.
                            YoutubeMessage.UUID = MessageUUID;
                            YoutubeMessage.Author = MessageAuthor;
                            YoutubeMessage.Message = ReturnString;
                            YoutubeMessage.PublishedTime = MessageDateTime;

                            YoutubeMessageList.Add(YoutubeMessage);
                            Console.WriteLine("UserLevel: {3} Type: {0} - FROM: {1} - Message: {2} \n", MessageType, MessageAuthor, ReturnString, UserLevel.ToString());
                            SeenUUIDs.Add(MessageUUID);
                        }
                        if (SeenUUIDs.Count > 5000)
                        {
                            SeenUUIDs.Clear();
                            Console.Write("Cleared Seen Messages - Reached 5000");
                        }

                    }

                    Console.Write("Done with Youtube Message Pull and Check. \n");
                }
            }


        }
    }
}
