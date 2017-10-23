using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;


namespace TracerouteConsole
{
    class Program
    {
        static string _fileName = "D:\\tmp.txt";
        static void Main(string[] args)
        {

            while (true)
            {
                Console.WriteLine("Begin writing");

                FileStream FS = new FileStream(_fileName, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter SW = new StreamWriter(FS);
                FS.Position = FS.Length;
                var sites = new List<Task>();
                sites.Add(MyTraceRoute.PrintTraceAsync("mail.ru", SW));
                sites.Add(MyTraceRoute.PrintTraceAsync("ya.ru", SW));
                sites.Add(MyTraceRoute.PrintTraceAsync("vk.com", SW));
                sites.Add(MyTraceRoute.PrintTraceAsync("Google.com", SW));
                foreach (var t in sites)
                    t.Wait();
                SW.Close();
                FS.Close();
                Console.WriteLine("End writing");
                Thread.Sleep(120000);
            }
        }
    }

    public class MyTraceRoute
    {
        private const string Data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

        public static async Task PrintTraceAsync(string hostNameOrAddress, StreamWriter writer)
        {
            await PrintTrace(hostNameOrAddress, writer);
        }

        private static Task PrintTrace(string hostNameOrAddress, StreamWriter writer)
        {
            return Task.Run(() =>
            {
                int i = 1;
                foreach (var ip in MyTraceRoute.GetTraceRoute(hostNameOrAddress))
                {
                    writer.WriteLineAsync(i.ToString() + ", " + hostNameOrAddress + ", " + ip.ToString());
                    i++;
                }
            });
        }

        private static IEnumerable<IPAddress> GetTraceRoute(string hostNameOrAddress)
        {
            for (int ttl = 1; ; ttl++)
            {
                PingReply reply = default(PingReply);
                try
                {
                    Ping pinger = new Ping();
                    PingOptions pingerOptions = new PingOptions(ttl, true);
                    int timeout = 10000;
                    byte[] buffer = Encoding.ASCII.GetBytes(Data);

                    reply = pinger.Send(hostNameOrAddress, timeout, buffer, pingerOptions);
                }
                catch (PingException e)
                {
                    Console.WriteLine("{0} Exception caught.", e);
                    break;
                }


                if (reply.Status == IPStatus.Success)
                {
                    yield return reply.Address;
                    break;
                }
                else if (reply.Status == IPStatus.TtlExpired || reply.Status == IPStatus.TimedOut)
                {
                    if (reply.Status == IPStatus.TtlExpired)
                        yield return reply.Address;
                }
                else
                {
                    break;
                }
            }
        }
    }
}
