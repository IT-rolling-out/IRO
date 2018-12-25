using ItRollingOut.Common.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace ItRollingOut.Common.Services
{
    public static class NetworkHelpers
    {
        /// <summary>
        /// Return null on exception.
        /// </summary>
        public async static Task<string> TrySendGetHttpRequest(string url, int timeoutSeconds = 20)
        {
            string res = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));
                request.Method = "GET";
                request.Timeout = timeoutSeconds * 1000;

                WebResponse response = await request.GetResponseAsync().ConfigureAwait(false); ;

                using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                {
                    res = await stream.ReadToEndAsync();

                }
                response.Dispose();
            }
            catch
            {

            }
            return res;
        }

        /// <summary>
        /// Return null on exception.
        /// </summary>
        public async static Task<string> TrySendPostHttpRequest(string url, Dictionary<string,string> data, int timeoutSeconds = 20)
        {
            string res = null;
            try
            {

                var content = new FormUrlEncodedContent(data);
                HttpClient client = new HttpClient();
                var response = await client.PostAsync(url, content).ConfigureAwait(false);
                res = await response.Content.ReadAsStringAsync();

            }
            catch
            {

            }
            return res;
        }

        public static List<string> GetAllPossibleIPsInLocalNetwork()
        {
            List<string> res = new List<string>();
            string startPart = "192.168.";

            for (int i = 0; i < 255; i++)
            {
                for (int j = 0; j < 255; j++)
                {
                    res.Add(startPart + i + "." + j);

                }
                //break;
            }

            return res;
        }


        /// <summary>
        /// Will ping all IPs from list and return new list with respounded IPs.
        /// </summary>
        public static async Task<List<string>> PingAll(List<string> inputIPs, int timeoutMS = 50)
        {
            List<string> res = new List<string>();

            Pool<Ping> pingersPool = new Pool<Ping>(null, 500);
            List<Task<PingReply>> pingersTasks = new List<Task<PingReply>>(inputIPs.Count);
            PingCompletedEventHandler ev = null;
            ev = (sender, evArgs) =>
            {
                ((Ping)sender).PingCompleted -= ev;
                if (evArgs.Reply.Status == IPStatus.Success)
                    res.Add(evArgs.Reply.Address.ToString());
                pingersPool.PutObject((Ping)sender);
            };
            foreach (string ipStr in inputIPs)
            {
                IPAddress address = IPAddress.Parse(ipStr);
                var pinger = pingersPool.GetObject();

                pinger.PingCompleted += ev;
                pingersTasks.Add(pinger.SendPingAsync(ipStr, timeoutMS));
            }

            await Task.WhenAll(pingersTasks).ConfigureAwait(false); ;

            pingersPool.ClearPool();
            return res;

        }

        /// <summary>
        /// Return true if get response.
        /// </summary>
        public static async Task<bool> Ping(string ip, int timeoutMS = 5000)
        {
            var reply = await new Ping().SendPingAsync(ip, timeoutMS).ConfigureAwait(false); 
            return reply.Status == IPStatus.Success;

        }

        
    }
}
