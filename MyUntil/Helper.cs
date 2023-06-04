using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MyUntil
{
    public class Helper
    {
        private readonly ILogger<Helper> _logger;
        private List<string> OnLineIp = new List<string>();
        private int count = 0;
        private List<Task> tasks = new List<Task>();
        private List<Thread> threads = new List<Thread>();

        public Helper(ILogger<Helper> logger)
        {
            _logger = logger;
        }
        public Dictionary<string, object> PingSegmentIp()
        {
            Dictionary<string, object> pairs = new Dictionary<string, object>();
            var time = DateTime.Now;
            string[] localIp = null;
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("192.168.0.111", 65530);
                var localEndPoint = socket.LocalEndPoint as IPEndPoint;
                var ipAddress = localEndPoint.Address;
                localIp = ipAddress.ToString().Split(".");
            }
            pairs.Add("GetIp", (DateTime.Now - time).TotalSeconds);
            time = DateTime.Now;

            for (int i = 1; i <= 255; i++)
            {
                if (i%50==0)
                {
                    _logger.LogDebug("Sleep....3000");
                    Thread.Sleep(3000);
                }
                if (i.ToString() == localIp[3]) continue;
                var pingIp = $"{localIp[0]}.{localIp[1]}.{localIp[2]}.{i}";
                var thread = new Thread(() => _Ping(pingIp));
                thread.Start();
                threads.Add(thread);
            }
            pairs.Add("PingTaskAll", (DateTime.Now - time).TotalSeconds);
            time = DateTime.Now;

            var isSleep = true;
            while (isSleep)
            {
                isSleep = false;
                threads = threads.Where(thread =>
                    thread.ThreadState != ThreadState.Stopped
                ).ToList();
                if (threads.Any()) isSleep = true;
            }


            pairs.Add("TaskCompletion", (DateTime.Now - time).TotalSeconds);
            pairs.Add("OnLineIp", OnLineIp);
            
            return pairs;
        }

        private void _Ping(string ip)
        {
            var pingIp = ip;//new string(ip);
            var ping = new Ping();
            var pingReply = ping.Send(pingIp, 2000);
            if (pingReply.Status == IPStatus.Success)
            {
                AddIp(pingReply.Address.ToString());
            }
        }

        private void AddIp(string ip)
        {
            //lock (OnLineIp)
            {
                OnLineIp.Add(ip);
            }
        }

        public static string HttpGet(string Url, Dictionary<String, object> parames, string cookie = null, bool isencode = false)
        {
            String postDataStr = buildParamStr(parames);
            if (isencode)
            {
                //postDataStr = System.Web.HttpUtility.UrlEncode(postDataStr);
            }
            string serviceAddress = Url + "?" + postDataStr;
            try
            {
                HttpWebRequest request =
                    (HttpWebRequest)WebRequest.Create(serviceAddress);
                request.Method = "GET";//GET请求的方式
                request.ContentType = "application/x-www-form-urlencoded";//编码格式以及请求类型，这行代码很关键，不设置ContentType将可能导致后台参数获取不到值
                request.Credentials = CredentialCache.DefaultCredentials;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream,
                    Encoding.UTF8);
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                return retString;//返回链接字符串
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        /// <summary>
        /// 转换请求参数,拼接请求串
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        static String buildParamStr(Dictionary<string, object> param)
        {
            String paramStr = String.Empty;
            if (param != null)
            {
                foreach (var key in param.Keys.ToList())
                {
                    if (param.Keys.ToList().IndexOf(key) == 0)
                    {
                        paramStr += (key + "=" + param[key]);
                    }
                    else
                    {
                        paramStr += ("&" + key + "=" + param[key]);
                    }
                }
            }
            return paramStr;
        }
    }
}
