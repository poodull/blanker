using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace blanker
{
    class Proc
    {
        private static readonly ILog _log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string _filename;
        private string _args;
        private Process _process;
        private Dictionary<int, List<string>> _macsPerMinute;
        private int _lastminute = -1;
        private List<string> _ignoreList;

        public bool IsRunning
        { get; private set; }

        public Proc(string filename, string args)
        {
            _filename = filename;
            _args = args;
            _ignoreList = Util.ShowNetworkInterfaces();
            if (_ignoreList == null)
                throw new Exception("No Network Interfaces!");
        }

        public bool Start()
        {
            if (_process != null)
            {
                _process.Kill();
                _process.Dispose();
            }
            _macsPerMinute = new Dictionary<int, List<string>>();
            _process = new Process();
            _process.StartInfo.FileName = _filename;
            _process.StartInfo.Arguments = _args;
            _process.OutputDataReceived += _process_OutputDataReceived;
            _process.ErrorDataReceived += _process_ErrorDataReceived;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.UseShellExecute = false;
            _process.Exited += _process_Exited;

            _log.InfoFormat("Starting process {0} '{1}'...", _filename, _args);
            _process.Start();
            _log.InfoFormat("Started process ID={0}", _process.Id);
            IsRunning = true;

            Task readAll = new Task(() =>
            {
                reader();
            }, TaskCreationOptions.LongRunning);
            readAll.Start();

            //_process.WaitForExit(); //wtf is this then?
            return true;
        }

        void _process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            _log.ErrorFormat("Error Data Received: {0}", e.Data);
        }
        private async void reader()
        {
            using (StreamReader reader = _process.StandardOutput)
            {
                string strOut;
                string[] cols;
                DateTime time;
                int minuteIndex;
                _log.InfoFormat("Reader is reading...");
                while (IsRunning)
                {
                    strOut = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(strOut))
                    {
                        //Console.WriteLine(strOut);
                        cols = strOut.Split(Program.SEPARATOR);
                        if (tryParseUTC(cols[0], out time))
                        {
                            if (!string.IsNullOrWhiteSpace(cols[1]))
                            {
                                cols[1] = cols[1].Trim().ToLower();
                                minuteIndex = (int)(DateTime.Now - DateTime.Today).TotalMinutes;
                                if (!_ignoreList.Contains(cols[1]))
                                {
                                    if (!_macsPerMinute.ContainsKey(minuteIndex))
                                    {
                                        _macsPerMinute.Add(minuteIndex, new List<string>() { cols[1] });
                                        _log.DebugFormat("First MAC for Minute {0}: {1} ",
                                            minuteIndex.ToString(), cols[1]);
                                        if (_lastminute > -1)
                                        {
                                            var uploadResult = await WebAccess.ProcessImprintsAsync(
                                                _lastminute, _macsPerMinute[_lastminute]);
                                            //if (uploadResult) //when the internet goes down?
                                            {
                                                _macsPerMinute.Remove(_lastminute);
                                            }
                                        }
                                        _lastminute = minuteIndex;
                                    }
                                    else
                                    {
                                        if (!_macsPerMinute[minuteIndex].Contains(cols[1]))
                                        {
                                            _macsPerMinute[minuteIndex].Add(cols[1]);
                                            _log.DebugFormat("New MAC for Minute {0}: {1}   count is now {2}",
                                                minuteIndex.ToString(), cols[1], _macsPerMinute[minuteIndex].Count.ToString());
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //no datetime component?
                            _log.WarnFormat("Bad Input Stream: '{0}'", strOut);
                        }
                    }
                }
            }
            _log.InfoFormat("Reader is quitting.");
        }

        private bool tryParseUTC(string utcTime, out DateTime localTime)
        {
            double val;
            if (!double.TryParse(utcTime, out val))
            {
                localTime = DateTime.MinValue;
                return false;
            }
            localTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(val).ToLocalTime();
            return true;
        }

        void _process_Exited(object sender, EventArgs e)
        {
            IsRunning = false;
        }

        void _process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string srt = e.Data;
            _log.WarnFormat("Output Data Stream Read: '{0}'", srt);
        }
    }
}
