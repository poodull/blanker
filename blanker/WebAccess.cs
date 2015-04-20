using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace blanker
{
    internal class BeaconPacket
    {
        public int beaconId { get; set; }
        public int minuteIndex { get; set; }
        public string mac { get; set; }
    }

    class WebAccess
    {
        private static readonly ILog _log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static Stopwatch __sw = new Stopwatch();

        private static async Task<bool> postBeaconPackets(List<BeaconPacket> packets)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(Program.Uri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                __sw.Restart();
                using (HttpResponseMessage postResult = await client.PostAsync("api/BeaconApi",
                    packets, new JsonMediaTypeFormatter()))
                {
                    if (postResult.IsSuccessStatusCode)
                    {
                        _log.InfoFormat("############## success!  Upload Took {0:f2}ms", __sw.ElapsedMilliseconds.ToString());
                    }
                    else
                    {
                        _log.ErrorFormat("############## fail!  Failure Took {0:f2}ms", __sw.ElapsedMilliseconds.ToString());
                        _log.ErrorFormat("############## " + postResult.StatusCode.ToString() + " " + postResult.ReasonPhrase);
                    }
                }
            }
            __sw.Stop();
            return true;
        }

        internal static async Task TestPost()
        {
            var packet = new BeaconPacket()
            {
                beaconId = 0,
                minuteIndex = 1,
                mac = "ff:ff:ff:ff:ff:ff"
            };

            _log.Debug("#####test##### Creating beaconPacket beaconId=0, minuteIndex = 1, mac = 'ff:ff:ff:ff:ff:ff'");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Program.Uri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                _log.DebugFormat("#####test##### Connecting to '{0}'", Program.Uri);

                HttpResponseMessage postResult = await client.PostAsync("api/BeaconApi",
                    new List<BeaconPacket> { packet }, new JsonMediaTypeFormatter());

                if (postResult.IsSuccessStatusCode)
                {
                    _log.Debug("#####test##### success!");
                }
                else
                {
                    _log.Error("#####test##### fail!");
                    _log.Error("#####test##### " + postResult.StatusCode + " " + postResult.ReasonPhrase);
                }
            }

        }

        internal static async Task<bool> ProcessImprintsAsync(int minuteIndex, List<string> macs)
        {
            try
            {
                _log.DebugFormat("############## Processing {0} imprints for minute {1}...", macs.Count, minuteIndex);
                //return await Task.Run(() => ProcessImprints(minuteIndex, macs));
                bool result = await postBeaconPackets(createPackets(minuteIndex, macs));
                _log.DebugFormat("############## done processing.  Result = {0}...", result ? "ok" : "fail");
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Error Posting.", ex);
                return false;
            }
        }

        private static List<BeaconPacket> createPackets(int mIndex, List<string> macs)
        {
            List<BeaconPacket> ret = new List<BeaconPacket>();
            foreach (string macAddr in macs)
            {
                ret.Add(new BeaconPacket()
                {
                    beaconId = Program.BeaconId,
                    minuteIndex = mIndex,
                    mac = macAddr
                });
            }
            return ret;
        }
    }
}
