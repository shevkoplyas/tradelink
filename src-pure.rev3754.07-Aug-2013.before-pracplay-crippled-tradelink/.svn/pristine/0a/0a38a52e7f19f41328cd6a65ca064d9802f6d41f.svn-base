using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TradeLink.Common;
using TradeLink.API;
using TradeLink.AppKit;

namespace IQFeedBroker
{
    public partial class IQFeedFrm : AppTracker
    {
        public const string PROGRAM = "IQFeedConnector";

        private IQFeedHelper _helper;
        DebugWindow _dw = new DebugWindow();
        Log _log = new Log(PROGRAM);

        public IQFeedFrm()
        {

            TradeLink.API.TLServer tls;
            if (Properties.Settings.Default.TLClientAddress == string.Empty)
                tls = new TradeLink.Common.TLServer_WM();
            else
                tls = new TradeLink.Common.TLServer_IP(Properties.Settings.Default.TLClientAddress, Properties.Settings.Default.TLClientPort);
            
            TrackEnabled = Util.TrackUsage();
            Program = PROGRAM;
            InitializeComponent();
            if (haspch)
            {
                _prod.Visible = false;
                prodlabel.Visible = false;
            }
            this.FormClosing += IQFeedFrm_FormClosing;
            _helper = new IQFeedHelper(tls);
            _helper.VerboseDebugging = Properties.Settings.Default.VerboseDebugging;
            _helper.IgnoreAfterTimeWithBefore = Properties.Settings.Default.IgnoreAfterTimeWithBefore;
            _helper.IfBeforeTimeUseIgnoreAfter = Properties.Settings.Default.IfBeforeTimeUseIgnoreAfter;
            _helper.isPaperTradeEnabled = Properties.Settings.Default.PaperTrade;
            _helper.isPaperTradeUsingBidAsk = Properties.Settings.Default.PaperTradeUsesBidAsk;
            _helper.DtnPath = Properties.Settings.Default.DtnPath;
            _helper.port = Properties.Settings.Default.Port;
            _helper.ReleaseDeadSymbols = Properties.Settings.Default.ReleaseDeadSymbols;
            _helper.MktCodes = parsemkts(Properties.Resources.marketcenters);
            _helper.SendDebug += new DebugDelegate(_helper_SendDebug);
            _helper.Connected += new IQFeedHelper.booldel(_helper_Connected);
            _helper.SaveRawData = Properties.Settings.Default.SaveRawFeed;
            _helper.ReportLatency = Properties.Settings.Default.ReportLatency;
            _helper.IgnoreOutOfOrderTick = Properties.Settings.Default.IgnoreOutofOrderTicks;
            _helper.SendHeartBeatEvent += new VoidDelegate(_helper_SendHeartBeatEvent);
            _dw.Width = Width * 3;
            _dw.Height = Height;
            _dw.Top = Bottom;
            _dw.Left = Left;
        }

        DateTime LASTstattime = DateTime.MinValue;

        void _helper_SendHeartBeatEvent()
        {
            var now = DateTime.Now;
            var elap = now.Subtract(LASTstattime);

            if (elap.TotalSeconds > 15)
            {
                status("Connected (" + now.ToLongTimeString() + ")");
                LASTstattime = now;
            }


        }

        

        void status(string msg)
        {
            if (coninfo.InvokeRequired)
                coninfo.Invoke(new DebugDelegate(status),new object[] { msg });
            else
            {
                coninfo.Text = msg;
                coninfo.Invalidate();
            }
        }

        void _helper_SendDebug(string msg)
        {
            _dw.GotDebug(msg);
            _log.GotDebug(msg);
        }

        public static Dictionary<int, string> parsemkts(string data)
        {
            string[] lines = data.Split(Environment.NewLine.ToCharArray(),  StringSplitOptions.RemoveEmptyEntries);
            Dictionary<int, string> c2m = new Dictionary<int, string>();
            foreach (string line in lines)
            {
                string [] r = line.Split('\t');
                int c = 0;
                if (int.TryParse(r[0], out c))
                    c2m.Add(c, r[1]);
            }
            return c2m;
        }

        delegate void booldel(bool v);
        void _helper_Connected(bool v)
        {
            if (InvokeRequired)
                Invoke(new booldel(_helper_Connected), new object[] { v });
            else
            {
                BackColor = v ? Color.Green : Color.Red;
                Invalidate(true);
            }
        }



        private void IQFeedFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
            _log.Stop();
            _helper.Stop();
        }



        private void IQFeedFrm_Load(object sender, EventArgs e)
        {
            // auto login if login data present are already present
            if ((_user.Text != string.Empty) && (_pass.Text != string.Empty) && (_prod.Text!=string.Empty))
                _login_Click(null, null);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            // toggle log message viewing
            _dw.Toggle();
        }

        private void _report_Click(object sender, EventArgs e)
        {
            HelpReportCommunity.Help(PROGRAM);
        }

        bool userpurchasedowndeveloperaccount = Properties.Settings.Default.UserPurchasedOwnDeveloperAccount;

        IQFPCI pch = IQFPCIHelper.get();
        bool haspch { get { return !userpurchasedowndeveloperaccount && (pch != null) && !string.IsNullOrWhiteSpace(pch.PC); } }
        string pc { get { return pch != null ? pch.PC : Properties.Settings.Default.PROGRAM_NAME; } }
        

        private void _login_Click(object sender, EventArgs e)
        {
            _helper.Start(_user.Text, _pass.Text, pc, Util.TLBuild());            
        }

        private void _prod_TextChanged(object sender, EventArgs e)
        {

        }

        private void verbdebugs_CheckedChanged(object sender, EventArgs e)
        {
            _helper.VerboseDebugging = verbdebugs.Checked;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Util.openurl("https://code.google.com/p/tradelink/wiki/DtnConfig");
        }


    }
}