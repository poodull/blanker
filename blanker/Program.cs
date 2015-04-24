using log4net;
using System;

[assembly: log4net.Config.XmlConfigurator(ConfigFileExtension = "config", Watch = true)]
namespace blanker
{
    class Program
    {
        private static readonly ILog _log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string FILENAME = "tshark";

        private const string ARGS = @"-n -i {0} -I -l -t e -T fields -e frame.time_epoch -e wlan.sa -E header=y -E separator={1}"; //RPi
        //private const string ARGS = @"-i {0} -l -t e -T fields -e frame.time_epoch -e eth.src -E header=y -E separator={1}"; //windows
        public const char SEPARATOR = '+';
        private const int MAXFAIL = 3;

        private static string iface = "2";

        private static DateTime _runningSince = DateTime.Now;

        internal static int BeaconId
        { get; private set; }
        internal static string Uri
        { get; private set; }

        static void Main(string[] args)
        {
            _log.Info("***********************  BLANKER Starting Up!  ***********************");

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            //Uri = "http://localhost:51284/";
            Uri = "http://displaydynamic.azure-mobile.net/";

            _log.DebugFormat("Going to {0}", Uri);

            try
            {
                if (args[0] == "test")
                {
                    _log.Warn("TEST MODE");
                    WebAccess.TestPost().Wait();
                    return;
                }

                iface = args[0];
                _log.DebugFormat("Using iface {0}", iface);
                BeaconId = int.Parse(args[1]);
                _log.DebugFormat("Using BeaconId {0}", BeaconId);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                _log.Info("\nUSAGE: blanker.exe <interface> <beaconid>");
                return;
            }

            Proc proc = new Proc(FILENAME, string.Format(ARGS, iface, SEPARATOR));
            int failed = 0;

            //while (failed < MAXFAIL)
            {
                if (!proc.Start())
                {
                    failed++;
                    _log.ErrorFormat("Failed attempt #{0}...", failed);
                    //continue;
                }
                while (proc.IsRunning)
                {
                    failed = 0;
                    System.Threading.Thread.Sleep(5000);
                }
            }
            //if (failed > MAXFAIL)
            //    _log.Error("Failed to start.  quitting.");
            //else
            _log.InfoFormat("Exiting at {0}.  Ran for {1}", DateTime.Now, DateTime.Now - _runningSince);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (_log != null)
                _log.Warn("Unhandled Exception Encoutered:");
            Console.WriteLine("Unhandled Exception!");
            if (e != null && e.ExceptionObject != null)
            {
                Console.WriteLine(e.ExceptionObject.ToString());
                if (_log != null)
                    _log.FatalFormat("Unhandled Exception: {0}", e.ExceptionObject.ToString());
            }
        }
    }
    #region * OLD TESTING EQ *
    //    {
    //        var httpClient = new HttpClient();
    //        //httpClient.BaseAddress = new Uri(string.Format("https://{0}.servicebus.windows.net/", serviceNamespace));
    //        string uri = "http://localhost:51284/";
    //        //string uri = "http://displaydynamic.azure-mobile.net/";
    //        httpClient.BaseAddress = new Uri(uri);

    //        try
    //        {
    //            beaconApiJsonGetTest(uri).Wait();
    //            beaconApiJsonPostTest(uri).Wait();
    //            //beaconApiJsonPutTest(uri).Wait();

    //            //beaconPacketJsonPostTest(httpClient); //works
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine("Error");
    //            Console.WriteLine(ex);
    //        }
    //        Console.WriteLine("Press the ANY key.");
    //        Console.ReadLine();
    //    }

    //    static async Task beaconApiJsonGetTest(string uri)
    //    {
    //        using (var client = new HttpClient())
    //        {
    //            client.BaseAddress = new Uri(uri);
    //            client.DefaultRequestHeaders.Accept.Clear();
    //            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

    //            HttpResponseMessage response = await client.GetAsync("api/BeaconApi");
    //            if (response.IsSuccessStatusCode)
    //            {
    //                Console.WriteLine(await response.Content.ReadAsStringAsync());
    //                //var beaconPacket = await response.Content.ReadAsAsync() > beaconPacket();
    //                //Console.WriteLine("{0}\t{1}\t{2}", beaconPacket.beaconId, beaconPacket.minuteIndex, beaconPacket.mac);
    //            }
    //            else
    //            {
    //                Console.WriteLine("fail!");
    //                Console.WriteLine(response.StatusCode + " " + response.ReasonPhrase);
    //            }
    //        }

    //    }

    //    static async Task beaconApiJsonPostTest(string uri)
    //    {
    //        using (var client = new HttpClient())
    //        {
    //            client.BaseAddress = new Uri(uri);
    //            client.DefaultRequestHeaders.Accept.Clear();
    //            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

    //            var beaconPacket = new beaconPacket()
    //            {
    //                beaconId = 1,
    //                minuteIndex = (int)(DateTime.Now - DateTime.Today).TotalMinutes,
    //                mac = "ff:ff:ff:ff:ff:ff"
    //            };

    //            HttpResponseMessage postResult = await client.PostAsync("api/BeaconApi", new List<beaconPacket>() { beaconPacket }, new JsonMediaTypeFormatter());
    //            //HttpResponseMessage postResult = await client.PostAsync("api/BeaconApi", beaconPacket, new JsonMediaTypeFormatter());

    //            if (postResult.IsSuccessStatusCode)
    //            {
    //                Console.WriteLine("success!");
    //            }
    //            else
    //            {
    //                Console.WriteLine("fail!");
    //                Console.WriteLine(postResult.StatusCode + " " + postResult.ReasonPhrase);
    //            }
    //        }
    //    }
    //    private class beacon
    //    {
    //        public int beaconId = -1;
    //    }
    //    private class beaconPacket
    //    {
    //        public int beaconId { get; set; }
    //        public int minuteIndex { get; set; }
    //        public string mac { get; set; }
    //    }
    //    static async Task beaconApiJsonPutTest(string uri)
    //    {
    //        using (var client = new HttpClient())
    //        {
    //            client.BaseAddress = new Uri(uri);
    //            client.DefaultRequestHeaders.Accept.Clear();
    //            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

    //            var beaconId = new beacon() { beaconId = 99 };

    //            HttpResponseMessage response = await client.PostAsync("api/BeaconApi/?beaconId=0", beaconId, new JsonMediaTypeFormatter());

    //            if (response.IsSuccessStatusCode)
    //            {
    //                Console.WriteLine("success!");
    //            }
    //            else
    //            {
    //                Console.WriteLine("fail!");
    //                Console.WriteLine(response.StatusCode + " " + response.ReasonPhrase);
    //            }
    //        }
    //    }
    //    static void beaconPacketJsonPostTest(HttpClient httpClient)
    //    {
    //        httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    //        var data = new
    //        {
    //            beaconId = 0,
    //            minuteIndex = (int)(DateTime.Now - DateTime.Today).TotalMinutes,
    //            mac = "ff:ff:ff:ff:ff:ff"
    //        };
    //        //string dataJson = string.Format(
    //        var postResult = httpClient.PostAsJsonAsync("tables/BeaconPacket", data).Result;

    //        //var postResult = httpClient.PostAsync("api/beaconApi", dataJson).ContinueWith(r =>
    //        //  {
    //        if (postResult.IsSuccessStatusCode)
    //        {
    //            Console.WriteLine("success!");
    //        }
    //        else
    //        {
    //            Console.WriteLine("fail!");
    //            Console.WriteLine(postResult.StatusCode + " " + postResult.ReasonPhrase);
    //        }
    //        //}).Wait(5000);
    //    }


    //    //// Returns JSON string
    //    //string GET(string url)
    //    //{
    //    //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
    //    //    try
    //    //    {
    //    //        WebResponse response = request.GetResponse();
    //    //        using (Stream responseStream = response.GetResponseStream())
    //    //        {
    //    //            StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
    //    //            return reader.ReadToEnd();
    //    //        }
    //    //    }
    //    //    catch (WebException ex)
    //    //    {
    //    //        WebResponse errorResponse = ex.Response;
    //    //        using (Stream responseStream = errorResponse.GetResponseStream())
    //    //        {
    //    //            StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
    //    //            String errorText = reader.ReadToEnd();
    //    //            // log errorText
    //    //        }
    //    //        throw;
    //    //    }
    //    //}

    //    //// POST a JSON string
    //    //void POST(string url, string jsonContent)
    //    //{
    //    //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
    //    //    request.Method = "POST";

    //    //    System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
    //    //    Byte[] byteArray = encoding.GetBytes(jsonContent);

    //    //    request.ContentLength = byteArray.Length;
    //    //    request.ContentType = @"application/json";

    //    //    using (Stream dataStream = request.GetRequestStream())
    //    //    {
    //    //        dataStream.Write(byteArray, 0, byteArray.Length);
    //    //    }
    //    //    long length = 0;
    //    //    try
    //    //    {
    //    //        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
    //    //        {
    //    //            length = response.ContentLength;
    //    //        }
    //    //    }
    //    //    catch (WebException ex)
    //    //    {
    //    //        // Log exception and throw as for GET example above
    //    //    }
    //    //}
    //}
    #endregion
}
