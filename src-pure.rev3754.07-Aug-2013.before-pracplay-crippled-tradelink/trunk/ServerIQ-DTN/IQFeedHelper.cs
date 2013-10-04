using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using TradeLink.API;
using TradeLink.Common;
using System.ComponentModel;
using System.Diagnostics;
namespace IQFeedBroker
{
    public class IQFeedHelper 
    {
        #region Variables
        private const string IQ_FEED = "iqconnect";
        private readonly string IQ_FEED_PROGRAM = "IQConnect.exe";
        private const string IQ_FEED_REGISTRY_LOCATION = "SOFTWARE\\DTN\\IQFeed";
        private AsyncCallback m_pfnCallback;
        private Socket m_sockAdmin;
        private Socket m_sockIQConnect;
        private Socket m_hist;
        private byte[] m_szAdminSocketBuffer = new byte[8096];
        private byte[] m_szLevel1SocketBuffer = new byte[8096];
        private byte[] m_buffhist = new byte[8096];
        private Basket _basket;
        private string _user;
        private string _pswd;
        string _product = string.Empty;
        private bool _registered;
        BackgroundWorker _connect;
        #endregion
        #region Properties
        public bool IsConnected { get { return _registered; } }
        private bool HaveUserCredentials
        {
            get
            {
                return !(string.IsNullOrEmpty(_user) && string.IsNullOrEmpty(_pswd));
            }
        }

        #endregion
        #region Constructors
        public string DtnPath = string.Empty;

        int _ignoreaftertime = 250000;
        public int IgnoreAfterTimeWithBefore { get { return _ignoreaftertime; } set { _ignoreaftertime = value; } }

        int _ignoreafterifbeforetime = 0;
        public int IfBeforeTimeUseIgnoreAfter { get { return _ignoreafterifbeforetime; } set { _ignoreafterifbeforetime = value; } }


        bool _papertrade = false;
        public bool isPaperTradeEnabled { get { return _papertrade; } set { _papertrade = value; } }
        PapertradeTracker ptt = new PapertradeTracker();
        bool _papertradebidask = false;
        public bool isPaperTradeUsingBidAsk { get { return _papertradebidask; } set { _papertradebidask = value; } }

        bool _saverawdata = false;
        public bool SaveRawData { get { return _saverawdata; } 
            set { 
                _saverawdata = value; 
            } }

        BackgroundWorker saveraw = new BackgroundWorker();

        bool _reportlatency = false;
        public bool ReportLatency { get { return _reportlatency; } 
            set 
            { 
                _reportlatency = value; 
            } }

        public IQFeedHelper(TLServer tls)
        {
            _basket = new BasketImpl();
            _connect = new BackgroundWorker();
            _connect.DoWork += new DoWorkEventHandler(bw_DoWork);
            tl = tls;
            tl.newProviderName = Providers.IQFeed;
            tl.newRegisterSymbols += new SymbolRegisterDel(tl_newRegisterSymbols);
            tl.newFeatureRequest += new MessageArrayDelegate(IQFeedHelper_newFeatureRequest);
            tl.newUnknownRequest += new UnknownMessageDelegate(IQFeedHelper_newUnknownRequest);
            tl.newSendOrderRequest += new OrderDelegateStatus(tl_newSendOrderRequest);
            tl.newOrderCancelRequest+=new LongDelegate(ptt.sendcancel);
            _cb_hist = new AsyncCallback(OnReceiveHist);
            saveraw.WorkerSupportsCancellation = true;
            saveraw.DoWork += new DoWorkEventHandler(saveraw_DoWork);
        }

        RingBuffer<string> rawdatabuf = new RingBuffer<string>(1000000);
        System.IO.StreamWriter rawfile;

        void saveraw_DoWork(object sender, DoWorkEventArgs e)
        {
            string rawfn= Util.ProgramData(IQFeedFrm.PROGRAM) + "\\Iqfeedraw." + Util.ToTLDate() + ".txt";
            bool fileok = false;
            try
            {
                rawfile = new System.IO.StreamWriter(rawfn, true);
                rawfile.AutoFlush = true;
                debug("started rawfeed archive at: " + rawfn);
                fileok = true;
            }
            catch (Exception ex)
            {
                debug("Raw saving failed from error writing to raw file: " + rawfn + " err: " + ex.Message + ex.StackTrace);
                
            }
            if (fileok)
            {
                while (_go)
                {
                    while (rawdatabuf.hasItems)
                    {
                        if (e.Cancel || !_go)
                            break;
                        string data = rawdatabuf.Read();
                        if (data!=string.Empty)
                            rawfile.Write(data);
                    }
                    Util.sleep(100);
                }
            }
            debug("Ended raw file write: " + rawfn);
            try
            {
                if (rawfile!=null)
                    rawfile.Close();
            }
            catch { }
        }

        long tl_newSendOrderRequest(Order o)
        {
            if (isPaperTradeEnabled)
            {
                ptt.sendorder(o);
            }
            else
            {
                debug("paper trade disabled, ignoring: " + o);
            }
            return 0;
        }

        void tl_newRegisterSymbols(string client, string symbols)
        {
            // get existing basket
            Basket old = new BasketImpl(_basket);
            // update new basket
            AddBasket(BasketImpl.FromString(symbols));
            // remove unused symbols
            if (ReleaseDeadSymbols && (old.Count > 0))
            {
                Basket rem = BasketImpl.Subtract(old, tl.AllClientBasket);
                foreach (Security s in rem)
                    unsubscribe(s.symbol);
            }

        }
        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            // start iqfeed and try to connect in background so it doesn't delay UI
            Connect();
            ConnectToAdmin();
            ConnectToLevelOne();
            ConnectHist();
        }
        long IQFeedHelper_newUnknownRequest(MessageTypes t, string msg)
        {
            switch (t)
            {
                case MessageTypes.DAYHIGH:
                    {
                        // get index for request
                        int idx = _highs.getindex(msg);
                        // ignore if no index
                        if (idx == GenericTracker.UNKNOWN) return 0;
                        decimal v = _highs[idx];
                        // ensure we have a high
                        if (v == decimal.MinValue) return 0;
                        return WMUtil.pack(v);
                    }
                case MessageTypes.DAYLOW:
                    {
                        // get index for request
                        int idx = _highs.getindex(msg);
                        // ignore if no index
                        if (idx == GenericTracker.UNKNOWN) return 0;
                        decimal v = _highs[idx];
                        // ensure we have a high
                        if (v == decimal.MaxValue) return 0;
                        return WMUtil.pack(v);
                    }
                case MessageTypes.BARREQUEST:
                    {
                        BarRequest br = BarRequest.Deserialize(msg);
                        if (!br.isIdValid)
                            br.ID = _idt.AssignId;
                        if (br.isValid)
                        {
                            v(br.symbol + " queuing bar request: " + br.ToString());
                            barrequestbuf.Write(br);
                        }
                        else
                            debug("got invalid bar request: " + msg);
                        return (long)MessageTypes.OK;
                    }
            }
            return (long)MessageTypes.FEATURE_NOT_IMPLEMENTED;
        }

        void processbarrequests()
        {
            debug("started bar request monitoring.");
            while (_go)
            {
                while (barrequestbuf.hasItems)
                {
                    if (!_go)
                        break;
                    var br = barrequestbuf.Read();
                    if (br.isValid)
                    {
                        RequestBars(br);
                    }
                }
                Util.sleep(25);
            }
            debug("ended bar request monitoring.");
        }


        RingBuffer<BarRequest> barrequestbuf = new RingBuffer<BarRequest>(1000);

        string PROTOCOL = "5.0";
        string PROTOCOL_HIST = "5.0";

        string HistoricalDataDirection = "1";
        int HistoricalDataPointPerSec = 2500;

        IdTracker _idt = new IdTracker();
        Dictionary<long, BarRequest> reqid2req = new Dictionary<long, BarRequest>();
        void RequestBars(BarRequest br)
        {
            // for command docs see http://www.iqfeed.net/dev/api/docs/HistoricalviaTCPIP.cfm
            string command = string.Empty;
            // ensure we're connected
            if (m_hist == null)
            {
                ConnectHist();
            }
            
            if (br.BarInterval == BarInterval.Day)
            {
                var previousdays = br.isExplictBarsBack ? br.BarsBackExplicit : BarImpl.BarsBackFromDate(br.BarInterval, br.StartDateTime, br.EndDateTime);
                // HDX,[Symbol],[MaxDatapoints],[DataDirection],[RequestID],[DatapointsPerSend]
                if (br.isExplictBarsBack)
                {
                    br.Tag = "HDX";
                    command = String.Format("HDX,{0},{1},{2},{3},{4}", br.symbol, previousdays, HistoricalDataDirection, br.ID, HistoricalDataPointPerSec);
                }
                else if (br.isExplicitStart)
                {
                    // HDT,[Symbol],[BeginDate],[EndDate],[MaxDatapoints],[DataDirection],[RequestID],[DatapointsPerSend]
                    var start = br.StartDateTime.ToString("yyyyMMdd");
                    var end = br.isExplicitDate ? br.EndDateTime.ToString("yyyyMMdd") : string.Empty;
                    var maxdata = br.isExplicitDate ? (int)br.EndDateTime.Subtract(br.StartDateTime).TotalDays : (int)DateTime.Now.Subtract(br.StartDateTime).TotalDays;
                    br.Tag = "HDT";
                    command = string.Format("HDT,{0},{1},{2},{3},{4},{5},{6}", br.symbol, start, end, maxdata, HistoricalDataDirection, br.ID, HistoricalDataPointPerSec);
                }
            }
            else
            {
                // validate interval type
                string sIntervalType = "s";
                int bint = br.CustomInterval;
                if (br.BarInterval== BarInterval.CustomVol)
                {
                    sIntervalType = "v";
                    bint = br.CustomInterval;
                }
                else if (br.BarInterval== BarInterval.CustomTicks)
                {
                    sIntervalType = "t";
                    bint = br.CustomInterval;
                }
                if (br.isExplictBarsBack && !br.isExplicitEnd && !br.isExplicitStart)
                {
                    br.Tag = "HIX";
                    //HIX,[Symbol],[Interval],[MaxDatapoints],[DataDirection],[RequestID],[DatapointsPerSend],[IntervalType]
                    command = String.Format("HIX,{0},{1},{2},{3},{4},{5},{6}", br.symbol, bint, br.BarsBackExplicit, HistoricalDataDirection, br.ID, HistoricalDataPointPerSec, sIntervalType);
                }
                else
                {
                    br.Tag = "HIT";
                    // request in the format:
                    //HIT,[Symbol],[Interval],[BeginDate BeginTime],[EndDate EndTime],[MaxDatapoints],[BeginFilterTime],[EndFilterTime],[DataDirection],[RequestID],[DatapointsPerSend],[IntervalType]<CR><LF> 
                    // HIT,SYMBOL,INTERVAL,BEGINDATE BEGINTIME,ENDDATE ENDTIME,MaxDataPoints,BEGINFILTERTIME,ENDFILTERTIME,DIRECTION,REQUESTID,DATAPOINTSPERSEND,INTERVALTYPE<CR><LF>
                    
                    var startdate = br.isExplicitStart ? br.StartDateTime.ToString("yyyyMMdd") : string.Empty;
                    var starttime = br.isExplicitStart ? br.StartDateTime.ToString("HHmmss") : string.Empty;
                    var enddate = br.isExplicitEnd ? br.EndDateTime.ToString("yyyyMMdd") : string.Empty;
                    var endtime = br.isExplicitEnd ? br.EndDateTime.ToString("HHmmss") : string.Empty;
                    var maxpoints = br.isExplictBarsBack ? br.BarsBackExplicit.ToString("F0") : string.Empty;
                    command = String.Format("HIT,{0},{1},{2} {3},{4} {5},{6},,,{7},{8},{9},{10}", br.symbol, bint, startdate, starttime,enddate ,endtime , maxpoints, HistoricalDataDirection, br.ID, HistoricalDataPointPerSec, sIntervalType);
                }
            }
            reqid2req.Add(br.ID, br);
            byte[] watchCommand = getcmddata(command);
            try
            {
                if (m_hist != null)
                {
                    m_hist.Send(watchCommand, watchCommand.Length, SocketFlags.None);
                    if (VerboseDebugging)
                        v("hist requested: " + br.symbol + " watch: " + command+" for: "+br.ToString());
                    else
                        debug("Requested historical bars for: " + br.ToString());

                }
                else
                {
                    debug("No historical connection available to request bars: " + br.ToString());
                }
            }
            catch (Exception ex)
            {
                debug("Exception sending barrequest: " + br.ToString());
                debug(ex.Message + ex.StackTrace);
            }
        }
        public delegate void booldel(bool v);
        public event booldel Connected;
        MessageTypes[] IQFeedHelper_newFeatureRequest()
        {
            List<MessageTypes> f = new List<MessageTypes>();
            // add features supported by this connector
            f.Add(MessageTypes.LIVEDATA);
            f.Add(MessageTypes.TICKNOTIFY);
            f.Add(MessageTypes.BROKERNAME);
            f.Add(MessageTypes.REGISTERSTOCK);
            f.Add(MessageTypes.VERSION);
            f.Add(MessageTypes.BARREQUEST);
            f.Add(MessageTypes.BARRESPONSE);
            f.Add(MessageTypes.BARRESPONSE_FINAL);
            f.Add(MessageTypes.DAYHIGH);
            f.Add(MessageTypes.DAYLOW);
            
            if (isPaperTradeEnabled)
            {
                f.Add(MessageTypes.SIMTRADING);
                f.Add(MessageTypes.SENDORDER);
                f.Add(MessageTypes.SENDORDERLIMIT);
                f.Add(MessageTypes.SENDORDERMARKET);
                f.Add(MessageTypes.SENDORDERSTOP);
                f.Add(MessageTypes.ORDERNOTIFY);
                f.Add(MessageTypes.ORDERCANCELREQUEST);
                f.Add(MessageTypes.ORDERCANCELRESPONSE);
                f.Add(MessageTypes.EXECUTENOTIFY);
            }
            return f.ToArray();
        }


        #endregion
        #region IQFeedHelper Members
        public void Stop()
        {
            // stop threads
            _go = false;
            if (SaveRawData)
            {
                try
                {
                    saveraw.CancelAsync();
                }
                catch { }
                if (rawfile != null)
                {
                    try
                    {
                        rawfile.Close();
                    }
                    catch { }
                }
            }
            Array.ForEach(System.Diagnostics.Process.GetProcessesByName(IQ_FEED), iqProcess => iqProcess.Kill());
            debug("iqfeed stopped.");
        }
        internal void ConnectToAdmin()
        {
            // Establish a connection to the admin socket in IQ Feed
            //Thread.Sleep(2000);
            try
            {
                m_sockAdmin = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipLocalHost = IPAddress.Parse(Properties.Settings.Default.IP_LOCAL_HOST);
                RegistryKey key = Registry.CurrentUser.OpenSubKey(String.Format("{0}\\Startup", IQ_FEED_REGISTRY_LOCATION));
                int port = int.Parse(key.GetValue(String.Format("{0}Port", Properties.Settings.Default.ADMINISTRATION_SOCKET_NAME), "9300").ToString());
                IPEndPoint endPoint = new IPEndPoint(ipLocalHost, port);
                v("connecting to admin at: " + endPoint);
                m_sockAdmin.Connect(endPoint);
                if (m_sockAdmin.Connected)
                    v("admin connected.");
                
                WaitForData(Properties.Settings.Default.ADMINISTRATION_SOCKET_NAME);
            }
            catch (Exception ex)
            {
                debug(String.Format("ADMIN SOCKET ERROR: {0}", ex.Message));
            }
        }
        internal void ConnectToLevelOne()
        {
            try
            {
                Thread.Sleep(1000);
                try
                {
                    m_sockIQConnect = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPAddress ipLocalHost = IPAddress.Parse(Properties.Settings.Default.IP_LOCAL_HOST);
                    RegistryKey key = Registry.CurrentUser.OpenSubKey(String.Format("{0}\\Startup", IQ_FEED_REGISTRY_LOCATION));
                    int port = int.Parse(key.GetValue(String.Format("{0}Port", Properties.Settings.Default.LEVEL_ONE_SOCKET_NAME), "5009").ToString());
                    IPEndPoint endPoint = new IPEndPoint(ipLocalHost, port);
                    v("connecting to l1 at: " + endPoint);
                    m_sockIQConnect.Connect(endPoint);
                    if (m_sockIQConnect.Connected)
                        v("l1 connected.");
                    WaitForData(Properties.Settings.Default.LEVEL_ONE_SOCKET_NAME);
                }
                catch (Exception ex)
                {
                    debug(String.Format("LEVEL ONE ERROR: {0}", ex.Message));
                    throw ex;
                }
                //Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                debug(ex.ToString());
            }
        }
        /// <summary>
        /// Subscribe to securities in the basket supplied as long as they aren't already in the underlying basket.
        /// </summary>
        /// <param name="basket"></param>
        internal void AddBasket(Basket basket)
        {
            foreach (Security security in basket)
            {
                AddSecurity(security);
            }
        }
        /// <summary>
        /// Subscribe to the security supplied as long as its not in the underlying basket
        /// </summary>
        /// <param name="security"></param>
        internal void AddSecurity(Security security)
        {
            if (!havesymbol(security.symbol))
            {
                _highs.addindex(security.symbol,decimal.MinValue);
                _lows.addindex(security.symbol,decimal.MaxValue);
                _basket.Add(security);
                AddSecurityToBasket(security);
                debug("Added subscription: " + security.symbol);
            }
        }
        /// <summary>
        /// Physically adds the security to the underlying basket and connects to the socket passing
        /// this security
        /// </summary>
        /// <param name="security"></param>
        private void AddSecurityToBasket(Security security)
        {
            // we form a watch command in the form of wSYMBOL\r\n
            string command = String.Format("w{0}\r\n", security.symbol);
            // and we send it to the feed via the socket
            byte[] watchCommand = new byte[command.Length];
            watchCommand = Encoding.ASCII.GetBytes(command);
            m_sockIQConnect.Send(watchCommand, watchCommand.Length, SocketFlags.None);
        }

        bool _releasedeadsymbols = true;
        public bool ReleaseDeadSymbols { get { return _releasedeadsymbols; } set { _releasedeadsymbols = value; } }

        void unsubscribe(string sym)
        {
            debug(sym + " removing.");
            _basket.Remove(sym);
            string command = String.Format("r{0}\r\n", sym);
            byte[] clearcommand = new byte[command.Length];
            clearcommand = Encoding.ASCII.GetBytes(command);
            m_sockIQConnect.Send(clearcommand, clearcommand.Length, SocketFlags.None);
        }
        bool havesymbol(string sym)
        {
            for (int i = 0; i < _basket.Count; i++)
                if (_basket[i].symbol == sym) return true;
            return false;
        }
	public TLServer tl;



    bool _go = true;

    
    int build = 0;

        public void Start(string username, string password, string data1, int data2)
        {
            
            build = data2;
            // paper trading
            ptt.GotCancelEvent += new LongDelegate(tl.newCancel);
            ptt.GotFillEvent += new FillDelegate(tl.newFill);
            ptt.GotOrderEvent += new OrderDelegate(tl.newOrder);
            ptt.SendDebugEvent += new DebugDelegate(ptt_SendDebugEvent);
            ptt.UseBidAskFills = isPaperTradeUsingBidAsk;
            _registered = false;
            if (port == 0)
            {
                port = CheckDefaultPort();
                if (port == 0)
                    debug("Dtn port not set in config file, auto-set port to: " + port);
            }

            _user = username;
            _pswd = password;
            _product = data1;
            usebeforeafterignoretime = IfBeforeTimeUseIgnoreAfter != 0;
            _go = true;
            _connect.RunWorkerAsync();
            if (SaveRawData)
            {
                if (saveraw.IsBusy)
                    debug("raw data save thread was already running.");
                else
                {
                    debug("Starting thread to save raw data.");
                    saveraw.RunWorkerAsync();
                }
            }
            TradeLink.AppKit.RunHelper.run(processbarrequests, null, debug, "processbarrequests");
        }

        void ptt_SendDebugEvent(string msg)
        {
            if (!isPaperTradeEnabled)
                return;
            debug("papertrade: " + msg);
        }
        void Connect()
        {
            try
            {
                int iqConnectProcessCount = System.Diagnostics.Process.GetProcessesByName(IQ_FEED).Length;
                switch (iqConnectProcessCount)
                {
                    case 1:
                        _registered = true;
                        debug("IQ Connect price feed is already running");
                        break;
                    case 0:
                        debug("IQFeed not running, attempting to start...");
                        string args = string.Empty;
                        if (HaveUserCredentials)
                        {
                            args += String.Format("-product {0} -version {1} -login {2} -password {3} -savelogininfo -autoconnect", _product,Util.TLVersion(), _user, _pswd);
                            Process p = Process.Start(IQ_FEED_PROGRAM, args);
                        }
                        break;
                    default:
                        throw new ApplicationException(string.Format("IQ Connect Feed has {0} instances currently running", iqConnectProcessCount));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        const string HISTSOCKET = "HISTSOCK";
        public int port = 0;
        public static int CheckDefaultPort()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(IQ_FEED_REGISTRY_LOCATION + "\\Startup");
            int p = 0;
            if (!int.TryParse(rk.GetValue("LookupPort").ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out p))
            {
                return 0;
            }
            return p;
        }

        const string CMDEND = "\r\n";
        byte[] getcmddata(params string[] commands)
        {
            // terminate commands
            var cmddata = string.Join(CMDEND, commands)+CMDEND;
            byte[] data = new byte[cmddata.Length];
            data = Encoding.ASCII.GetBytes(cmddata);
            return data;
        }

        int lookup_port = 9100;
        void ConnectHist()
        {
            try
            {
                
                m_hist = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress local = IPAddress.Loopback;
                var endpoint = new IPEndPoint(local, lookup_port);
                debug("Attempting to connect to historical feed at: "+endpoint);
                m_hist.Connect(endpoint);
                if (m_hist.Connected)
                {
                    checkhistproto = true;
                    var cmd = getcmddata("S,SET PROTOCOL," + PROTOCOL_HIST);
                    debug("historical connected: " + endpoint);
                    m_hist.Send(cmd,cmd.Length, SocketFlags.None);
                    WaitForData(HISTSOCKET);
                }
                
            }
            catch (Exception ex)
            {
                debug("historical connect failed.");
                debug(ex.Message + ex.StackTrace);
            }
        }
        AsyncCallback _cb_hist = null;
        private void WaitForData(string socketName)
        {
            try
            {
                if (m_pfnCallback == null)
                    m_pfnCallback = new AsyncCallback(OnReceiveData);
                if (socketName == Properties.Settings.Default.LEVEL_ONE_SOCKET_NAME)
                {
                    if (m_sockIQConnect != null)
                        m_sockIQConnect.BeginReceive(m_szLevel1SocketBuffer, 0, m_szLevel1SocketBuffer.Length, SocketFlags.None, m_pfnCallback, socketName);
                }
                else if (socketName == Properties.Settings.Default.ADMINISTRATION_SOCKET_NAME)
                    m_sockAdmin.BeginReceive(m_szAdminSocketBuffer, 0, m_szAdminSocketBuffer.Length, SocketFlags.None, m_pfnCallback, socketName);
                else if (socketName == HISTSOCKET)
                {
                    m_hist.BeginReceive(m_buffhist, 0, m_buffhist.Length, SocketFlags.None, _cb_hist, socketName);
                }
            }
            catch (Exception ex)
            {
                debug("socket error on: " + socketName);
                debug(ex.Message + ex.StackTrace);
            }
            
        }
        string _histbuff = string.Empty;
        const string HISTEND = "!ENDMSG!";

        bool _verb = false;
        public bool VerboseDebugging { get { return _verb; } set { var v = value; if (v == _verb) return; _verb = v; debug("Verbose: " + (_verb ? "ON." : "disabled.")); } }

        void v(string msg)
        {
            if (VerboseDebugging)
            {
                debug(msg);
            }
        }

        bool checkhistproto = false;
        string expecthistproto { get { return "S,CURRENT PROTOCOL," + PROTOCOL_HIST; } }

        public bool isBarRawDataDebugged = false;

        string[] getrecords(byte[] buffer, int bytes)
        {
            string recvdata = Encoding.ASCII.GetString(m_buffhist, 0, bytes);
            string rawData = recvdata;
            if (!string.IsNullOrWhiteSpace(_histbuff))
            {
                rawData = _histbuff + recvdata;
                if (isBarRawDataDebugged)
                    debug("hist post buffer: " + rawData);
                _histbuff = string.Empty;
            }
            string[] records = rawData.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (VerboseDebugging)
            {
                if (isBarRawDataDebugged)
                    debug("hist recv: " + bytes + " bytes " + records.Length + " records from: " + recvdata);
                else
                    debug("hist recv: " + bytes + " bytes " + records.Length);
            }
            return records;
        }

        bool isrecorddone(string rec, string [] r, string prev, out BarRequest br)
        {
            long reqid = 0;
            br = new BarRequest();
            if (r.Length > 0)
            {
                if (long.TryParse(r[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out reqid))
                {
                    // make request invalid if can't be found
                    if (!reqid2req.TryGetValue(reqid, out br))
                    {
                        v("hist no request id found: " + rec);
                        br = new BarRequest();
                    }
                    else
                    {
                        v("hist response to request: " + reqid + " from: " + br.Client);
                    }
                }

            }

            var minexpectrecordcount = br.BarInterval == BarInterval.Day ? 7 : 8;

            // this should get hit on ENDMSG
            if (r.Length < minexpectrecordcount)
            {
                if (rec.Contains(HISTEND))
                {
                    if (br.isIdValid)
                    {
                        debug("hist completed request " + br.ID);
                        tl.TLSend(BarRequest.Serialize(br), MessageTypes.BARRESPONSE_FINAL, br.Client);
                        
                    }
                    else
                        debug("hist unknown request completed: " + rec);
                    return true;
                }
                else // could be a partial record  (that needs buffering) or an error/unknown state
                {
                    v("hist unknown data, id: " + br.ID + " data: " + rec);
                }
                return false;
            }
            if (!br.isValid)
            {
                if (string.IsNullOrWhiteSpace(prev))
                    debug("hist unknown bar request data " + rec);
                else
                    debug("hist unknown bar request data:" + rec + " prev: " + prev + " buf: " + _histbuff);
                return true;
            }
            return false;
        }

        void OnReceiveHist(IAsyncResult result)
        {
            try
            {
                int bytesReceived = m_hist.EndReceive(result);
                var records = getrecords(m_buffhist, bytesReceived);

                // mark record processing status
                var lastrecordprocessed = false;
                var lastrecordidx = records.Length - 1;

                // process records
                for (int i = 0; i<records.Length; i++)
                {
                    // get record
                    var rec = records[i];
                    // mark it's status
                    lastrecordprocessed = false;
                    // skip empty data
                    if (string.IsNullOrWhiteSpace(rec))
                        continue;
                    // look for misc responses
                    if (checkhistproto && rec.Contains(expecthistproto))
                    {
                        checkhistproto = false;
                        rec = rec.Replace(expecthistproto, string.Empty);
                        records[i] = rec;
                        lastrecordprocessed = i == lastrecordidx;
                        v("hist protocol ack:" + expecthistproto);
                    }
                    string[] r = rec.Split(',');
                    // test for request id and response type
                    BarRequest br = new BarRequest();
                    var prev = (i == 0) ? string.Empty : records[i - 1];
                    if (isrecorddone(rec, r, prev, out br))
                    {
                        v("hist processed: " + rec);
                        lastrecordprocessed = i == lastrecordidx;
                        continue;
                    }
                    else if (i==lastrecordidx)
                    {
                        lastrecordprocessed = false;
                    }

                    Bar b = new BarImpl();

                    try
                    {
                        if (br.isValid)
                        {
                            if (br.BarInterval == BarInterval.Day)
                            {
                                DateTime dt = DateTime.MinValue;
                                if (!DateTime.TryParse(r[1], System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.NoCurrentDateDefault, out dt))
                                {
                                    v("hist can't parse date: " + r[1] + " data: " + rec);
                                    continue;
                                }
                                decimal o, h, l, c;
                                long vol;
                                if (!decimal.TryParse(r[2], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out h))
                                {
                                    v("hist can't parse high: " + r[2] + " data: " + rec);
                                    continue;
                                }
                                else if (!decimal.TryParse(r[3], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out l))
                                {
                                    v("hist can't parse low: " + r[3] + " data: " + rec);
                                    continue;
                                }
                                else if (!decimal.TryParse(r[4], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out o))
                                {
                                    v("hist can't parse open: " + r[4] + " data: " + rec);
                                    continue;
                                }
                                else if (!decimal.TryParse(r[5], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out c))
                                {
                                    v("hist can't parse close: " + r[5] + " data: " + rec);
                                    continue;
                                }
                                else if (!long.TryParse(r[6], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out vol))
                                {
                                    v("hist can't parse vol: " + r[6] + " data: " + rec);
                                    continue;
                                }
                                // mark iqfeed processing status
                                lastrecordprocessed = i == lastrecordidx;
                                // build bar
                                b = new BarImpl(o, h, l, c, vol, Util.ToTLDate(dt), Util.ToTLTime(dt), br.symbol, br.Interval,br.CustomInterval,br.ID);
                                if (VerboseDebugging)
                                    debug("hist got bar: " + b);
                            }
                            else
                            {
                                DateTime dt = DateTime.MinValue;
                                if (!DateTime.TryParse(r[1], System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.NoCurrentDateDefault, out dt))
                                {
                                    v("hist can't parse date: " + r[1] + " data: " + rec);
                                    continue;
                                }
                                decimal o, h, l, c;
                                long vol;
                                if (!decimal.TryParse(r[2], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out h))
                                {
                                    v("hist can't parse high: " + r[2] + " data: " + rec);
                                    continue;
                                }
                                else if (!decimal.TryParse(r[3], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out l))
                                {
                                    v("hist can't parse low: " + r[3] + " data: " + rec);
                                    continue;
                                }
                                else if (!decimal.TryParse(r[4], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out o))
                                {
                                    v("hist can't parse open: " + r[4] + " data: " + rec);
                                    continue;
                                }
                                else if (!decimal.TryParse(r[5], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out c))
                                {
                                    v("hist can't parse close: " + r[5] + " data: " + rec);
                                    continue;
                                }
                                else if (!long.TryParse(r[7], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out vol))
                                {
                                    v("hist can't parse vol: " + r[7] + " data: " + rec);
                                    continue;
                                }
                                // mark iqfeed processing status
                                lastrecordprocessed = i == lastrecordidx;
                                // build bar
                                b = new BarImpl(o, h, l, c, vol, Util.ToTLDate(dt), Util.ToTLTime(dt), br.symbol, br.Interval,br.CustomInterval,br.ID);
                                if (VerboseDebugging)
                                    debug("hist got bar: " + b);

                            }
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        v("hist ohlc parse err: " + ex.Message + ex.StackTrace + " data: " + rec);
                        b = new BarImpl();
                    }
                    // send it
                    if (b.isValid)
                    {
                        var barserial = BarImpl.Serialize(b);
                        if (VerboseDebugging)
                            debug("hist sending response to: "+br.ID+" int: "+ b.Interval + " bar: " + b.ToString() + " to: " + br.Client + " using data: " + rec);
                        try
                        {
                            tl.TLSend(barserial, MessageTypes.BARRESPONSE, br.Client);
                            if (VerboseDebugging)
                                debug("hist sent " + b.Symbol + " bar in response to: " + br.ID);
                        }
                        catch (Exception ex)
                        {
                            debug("hist send error: " + ex.Message + ex.StackTrace + " on: " + barserial + " to: " + br.Client);
                        }
                    }
                    
                }
                if (!lastrecordprocessed)
                {
                    string lastrecord = records[lastrecordidx];
                    var endidx = lastrecord.IndexOf(HISTEND);
                    if (endidx >= 0)
                    {
                        lastrecord = lastrecord.Substring(endidx, HISTEND.Length);
                        v("hist got " + HISTEND);
                    }
                    if (!string.IsNullOrWhiteSpace(lastrecord))
                    {
                        _histbuff = lastrecord;
                        v("hist saving: " + _histbuff + " for more data.");
                    }
                }
                // wait for more historical data
                WaitForData(HISTSOCKET);
            }
            catch (SocketException)
            {
                v("hist connection closed.");
            }
            catch (Exception ex)
            {
                debug(ex.Message + ex.StackTrace);
            }
        }

        bool _showstats = false;
        public bool isStatsShown { get { return _showstats; } set { if (value == _showstats) return; _showstats = value; debug("Show Stats: " + (_showstats ? "ON." : "disabled.")); } }

        string laststats = string.Empty;
        public string LastStats { get { return laststats; } }
        DateTime laststat_time = DateTime.MinValue;
        public DateTime LastStatTime { get { return laststat_time; } }

        public event VoidDelegate SendHeartBeatEvent;

        void beatheart()
        {
            if (SendHeartBeatEvent != null)
                SendHeartBeatEvent();
        }

        /// <summary>
        /// This is our callback that gets called by the .NET socket class when new data arrives on the socket
        /// </summary>
        /// <param name="asyn"></param>
        private void OnReceiveData(IAsyncResult result)
        {
            // first verify we received data from the correct socket.
            if (result.AsyncState.ToString().Equals(Properties.Settings.Default.ADMINISTRATION_SOCKET_NAME))
            {
                try
                {
                    int bytesReceived = m_sockAdmin.EndReceive(result);
                    string rawData = Encoding.ASCII.GetString(m_szAdminSocketBuffer, 0, bytesReceived);
                    bool connectToLevelOne = _registered;
                    while (rawData.Length > 0)
                    {
                        string data = rawData.Substring(0, rawData.IndexOf("\n"));
                        if (data.StartsWith("S,STATS,"))
                        {
                            laststats = data.TrimEnd('\r', '\n');
                            laststat_time = DateTime.Now;
                            if (isStatsShown)
                                debug(laststats);
                            beatheart();
                            //v("admin stats: " + laststats);
                            if (!_registered)
                            {
                                string command = String.Format("S,REGISTER CLIENT APP,{0},{1}\r\n", _product, Util.TLVersion());
                                byte[] size = new byte[command.Length];
                                int bytesToSend = size.Length;
                                size = Encoding.ASCII.GetBytes(command);
                                v("admin registering: " + command.Replace("\r\n", string.Empty));
                                m_sockAdmin.Send(size, bytesToSend, SocketFlags.None);
                                _registered = true;
                                if (Connected != null)
                                    Connected(_registered);
                                if (!VerboseDebugging)
                                    debug("Connected.");
                            }
                        }
                        else if (data.StartsWith("S,REGISTER CLIENT APP COMPLETED"))
                        {
                            v("admin recv: " + data.TrimEnd('\r', '\n'));
                            string command = string.Format("S,SET PROTOCOL,{0}\r\nS,SET LOGINID,{1}\r\nS,SET PASSWORD,{2}\r\nS,SET SAVE LOGIN INFO,On\r\nS,SET AUTOCONNECT,On\r\nS,CONNECT\r\n",PROTOCOL, _user,_pswd);

                            byte[] size = new byte[command.Length];
                            size = Encoding.ASCII.GetBytes(command);
                            int bytesToSend = size.Length;
                            v("admin setattributes: " + command.Replace("\r\n", string.Empty));
                            m_sockAdmin.Send(size, bytesToSend, SocketFlags.None);
                             
                            _registered = true;
                        }
                        else
                        {
                            v("admin received: " + data);
                            beatheart();
                        }
                        rawData = rawData.Substring(data.Length + 1);
                    }
                    WaitForData(Properties.Settings.Default.ADMINISTRATION_SOCKET_NAME);
                }
                catch (SocketException ex)
                {
                    debug(ex.Message+ex.StackTrace);
                }
                catch (Exception ex)
                {
                    debug(ex.Message + ex.StackTrace);
                }
            }
            else if (result.AsyncState.ToString().Equals(Properties.Settings.Default.LEVEL_ONE_SOCKET_NAME))
            {
                try
                {
                    beatheart();
                    // only ticks should be handled on this socket...
                    // create a new socket for other types of data
                    int bytesReceived = 0;
                    bytesReceived = m_sockIQConnect.EndReceive(result);
                    string rawData = Encoding.ASCII.GetString(m_szLevel1SocketBuffer, 0, bytesReceived);
                    if (SaveRawData)
                        rawdatabuf.Write(rawData);
                    string[] msgs = rawData.Split('\n');
                    Array.Reverse(msgs);
                    
                    for (int i = 0; i<msgs.Length; i++)
                    {
                        string msg = msgs[i];
                        string[] actualData = msg.Split(',');

                        if ((actualData.Length >= 15))
                        {
                            if ((actualData[0] == "Q"))
                            {
                                FireTick(actualData);
                            }
                        }
                        else if (actualData.Length == 0)
                        {
                            if (VerboseDebugging && !string.IsNullOrWhiteSpace(msg))
                                v("l1 received: " + msg.TrimEnd('\r', '\n'));
                        }

                        else if (actualData[0] == "T")
                            beatheart();
                        else if (actualData[0] == "S")
                        {
                            if (VerboseDebugging && !string.IsNullOrWhiteSpace(msg))
                                v("l1 system: "+msg.TrimEnd('\r', '\n'));
                        }
                        else
                        {
                            if (VerboseDebugging && !string.IsNullOrWhiteSpace(msg))
                                v("l1 received: " + msg.TrimEnd('\r', '\n'));
                        }
                    }
                }
                catch (SocketException ex)
                {
                    debug("l1 socket: "+ex.Message);
                }
                catch (Exception ex)
                {
                    var ine = ex.InnerException==null ? string.Empty : ex.InnerException.Message+ex.InnerException.StackTrace;
                    debug("l1 error: " + ex.Message + ex.StackTrace + ine);
                }
                WaitForData(Properties.Settings.Default.LEVEL_ONE_SOCKET_NAME);
            }
            else
            {
                debug(result.AsyncState.ToString());
            }
        
        }

        bool usebeforeafterignoretime = false;

        long lastticktime = 0;
        double latencyavg = 0;
        double peaklatency = 0;
        int tickssincelastlatencyreport = 0;

        int lasttickordertime = 0;
        int lasttickdate = 0;
        bool ignoreoutoforder = false;
        public bool IgnoreOutOfOrderTick { get { return ignoreoutoforder; } set { ignoreoutoforder = value; } }

        GenericTracker<decimal> _highs = new GenericTracker<decimal>();
        GenericTracker<decimal> _lows = new GenericTracker<decimal>();
        public void FireTick(string[] actualData)
        {
            if (actualData.Length < 66)
                return;
                try
                {
                    if (actualData[0] == "F") 
                        return;

                        Tick tick = new TickImpl();
                        tick.date = Util.ToTLDate();
                    DateTime now;
                    int local = Util.ToTLTime();
                    if (DateTime.TryParse(actualData[65],out now))
                    {
                        tick.time = Util.DT2FT(now);
                        
                        if (ReportLatency)
                        {
                            Int64 latency = 0;
                            if (lastticktime!=0)
                                latency = DateTime.Now.Ticks - lastticktime;
                            lastticktime = now.Ticks;
                            
                            latencyavg = (latency + latencyavg) / 2;
                            tickssincelastlatencyreport++;
                            // test for peak
                            if (latency > peaklatency)
                                peaklatency = latency;
                            // test for report
                            if (tickssincelastlatencyreport > 500000)
                            {
                                // convert to ms
                                double latencyms = latencyavg / 10000;
                                double peakms = peaklatency / 10000;
                                debug(string.Format("latency (ms) avg {0:N1} peak: {0:N1}",latencyms,peaklatency));
                                tickssincelastlatencyreport = 0;
                                latencyavg = 0;
                            }
                        }
                    }
                    else
                        tick.time = local;
                    // see if user has tick ignoring enabled 
                    //(this is because in early AM iqfeed will frequently send ticks with yesterday time stamps
                    if (usebeforeafterignoretime)
                    {
                        // ensure that ordering works across multiple days
                        if (lasttickdate != tick.date)
                        {
                            lasttickordertime = 0;
                            lasttickdate = tick.date;
                        }

                        // see if tick should be ignored
                        if ((local < IfBeforeTimeUseIgnoreAfter)
                            && (tick.time > IgnoreAfterTimeWithBefore))
                            return;
                            // allow ignoring for ignoring other ticks that are out of order
                        else if (ignoreoutoforder && (tick.time < lasttickordertime))
                            return;
                        lasttickordertime = tick.time;
                    }

                        int v = 0;
                        if (int.TryParse(actualData[64], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out v))
                            tick.oe = getmarket(v);
                        if (int.TryParse(actualData[63], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out v))
                            tick.be = getmarket(v);
                        if (int.TryParse(actualData[62], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out v))
                            tick.ex = getmarket(v);
                        tick.symbol = actualData[1];
                        tick.bid = Convert.ToDecimal(actualData[10], System.Globalization.CultureInfo.InvariantCulture);
                        tick.ask = Convert.ToDecimal(actualData[11], System.Globalization.CultureInfo.InvariantCulture);
                        tick.trade = Convert.ToDecimal(actualData[3], System.Globalization.CultureInfo.InvariantCulture);
                        tick.size = Convert.ToInt32(actualData[7], System.Globalization.CultureInfo.InvariantCulture);
                        tick.BidSize = Convert.ToInt32(actualData[12], System.Globalization.CultureInfo.InvariantCulture);
                        tick.AskSize = Convert.ToInt32(actualData[13], System.Globalization.CultureInfo.InvariantCulture);
                        // get symbol index for custom data requests
                        int idx = _highs.getindex(tick.symbol);
                        // update custom data (tryparse is faster than convert)
                        decimal d = 0;
                        if (decimal.TryParse(actualData[8], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out d))
                            _highs[idx] = d;
                        if (decimal.TryParse(actualData[9], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out d))
                            _lows[idx] = d;
                        if (isPaperTradeEnabled)
                            ptt.newTick(tick);
                        tl.newTick(tick);
                }
                catch (Exception ex)
                {
                    debug("Tick error: " + string.Join(",", actualData));
                    debug(ex.Message+ex.StackTrace);
                }
                        
        }
        Dictionary<int, string> _code2mkt = new Dictionary<int, string>();
        public Dictionary<int, string> MktCodes { get { return _code2mkt; } set { _code2mkt = value; } }
        string getmarket(int code)
        {
            string mkt = string.Empty;
            if (!_code2mkt.TryGetValue(code,out mkt)) return string.Empty;
            return mkt;
        }
        void debug(string msg)
        {
            if (SendDebug != null)
                SendDebug(msg);
        }
        public event TradeLink.API.DebugDelegate SendDebug;
        
        #endregion
    }
}
