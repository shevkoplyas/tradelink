using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using TradeLink.Common;
using TradeLink.API;
using System.Diagnostics;


namespace IQFeedBroker
{
    [TestFixture]
    public class TestIQFeed
    {

        [Test]
        public void HistoricalBarRequest()
        {
            
            Stopwatch sw = new Stopwatch();
            // verify we are ready
            Assert.IsTrue(isconok, "connection not ready");
            // prep test case data
            msgok &= true;
            recvbarcount = 0;
            reccomplete = 0;
            recvsyms.Clear();
            symintcount.Clear();
            lastbarraw = string.Empty;
            var syms = new string[] { "IBM", "GOOG", "QQQ" };
            //var syms = new string[] { "IBM"};

            var ints = new int[] { (int)BarInterval.Day, (int)BarInterval.FiveMin, (int)BarInterval.Hour };
            var barsback = new int[] { 100, 100, 20 };
            
            // verify we have no data
            foreach (var sym in syms)
            {
                foreach (var bi in ints)
                {
                    Assert.AreEqual(0, barcount(sym, bi), "had bar data: " + sym + " " + bi);
                }
            }
            // request a bunch of data
            sw.Start();
            foreach (var sym in syms)
            {
                for (int i = 0; i < ints.Length; i++)
                {
                    var bint = ints[i];
                    var bintsize = bint;
                    var bb = barsback[i];
                    // build request
                    if ((bint < (int)BarInterval.Day) && (bint > 0))
                        bint = (int)BarInterval.CustomTime;
                    var br = BarRequest.BuildBarRequestBarsBack(sym, bb, bint,bintsize,client.Name);
                    var ok = client.TLSend(MessageTypes.BARREQUEST, br);
                    Assert.AreEqual((int)MessageTypes.OK, ok, "failed bar request: " + br);
                }
            }
            // wait for it
            g.d("waiting for bars...");
            int att = 0;
            bool stillwaiting = true;
            var expectbars = Calc.Sum(barsback) * syms.Length;
            var expectcomplete = (syms.Length * barsback.Length);
#if DEBUG
            const int waitseconds = 60;
#else
            const int waitseconds = 15;
#endif
            while (stillwaiting && (att++ < (waitseconds * 200)))
            {
                Util.sleep(5);
                
                System.Windows.Forms.Application.DoEvents();
                stillwaiting = (recvbarcount < expectbars) && (reccomplete<expectcomplete);
            }
            if (!stillwaiting)
            {
                sw.Stop();
                var elap = sw.Elapsed.TotalSeconds;
                var responsepersec = elap == 0 ? 0 : expectcomplete/elap;
                var barssec = recvbarcount == 0 ? 0 : recvbarcount / elap;

                g.d("received expected # of bars: " + recvbarcount + "/" + expectbars + "\t" + "rate (req/sec): " + responsepersec.ToString("N2")+" bars/sec: "+barssec.ToString("N2"));
            }
            else
                g.d("wait timeout, got " + recvbarcount + "/" + expectbars);
            //Assert.GreaterOrEqual(recvbarcount, expect, "did not receive enough bars");
            Assert.IsTrue(msgok, "errors processing messages");
            Assert.AreEqual(syms.Length, recvsyms.Count, "unexpected bar symbols: " + Util.join(recvsyms)+Environment.NewLine+lastbar.ToString());
            // verify we got it all
            foreach (var sym in syms)
            {
                for (int i = 0; i<ints.Length; i++)
                {
                    var bi = ints[i];
                    var bisize = bi;
                    if ((bi < (int)BarInterval.Day) && (bi > 0))
                        bi = (int)BarInterval.CustomTime;
                    var count = barsback[i];
                    var symcount= barcount(sym, bi);
                    var bars = symintcount[sym+bi+bisize];
                    if (g.ta(bars.Count == count, "error: " + sym + bi + " expected: " + count + " got: " + bars.Count + " raw bars: " + string.Join(Environment.NewLine, bars)))
                        g.d(sym + " tracking " + bi + " bars, count: " + bars.Count);
                    else
                        Assert.AreEqual(count, bars.Count, "wrong bar count: " + sym + bi);
                }
            }
            
            
            


        }

        public const string loginfile = "iqlogininfo.txt";
        const int userr = 0;
        const int pwr = 1;
        const int prodr = 2;

        IQFeedHelper con = null;
        TLClient client = null;
        bool isserverconok { get { if (con == null) { g.d("no server"); return false; } else if (!con.IsConnected) { g.d("server conn down"); return false; } g.d("server ok"); return true; } }
        bool isclientconok { get { if (client == null) { g.d("no client"); return false; } else if (client.ProvidersAvailable.Length == 0) { g.d("no servers found"); return false; } else if (client.BrokerName != Providers.IQFeed) { g.d("wrong server"); return false; } g.d("client ok"); return true; } }
        bool isconok { get { if (!isserverconok) { g.d("server down"); return false; } else if (!isclientconok) { g.d("client down"); return false; } g.d("connection ok"); return true; } }
        TLServer server = null;
        Random r = new Random();

#if DEBUG
        bool verbosesetting = true;
#else
        bool verbosesetting = false;
#endif

        [TestFixtureSetUp]
        public void setup()
        {
            
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            
            if (!isserverconok)
            {
                server = new TradeLink.Common.TLServer_WM();
                con = new IQFeedHelper(server);
                con.VerboseDebugging = verbosesetting;
                con.SendDebug += new DebugDelegate(g.d);
                // get login information
                var data = Util.getfile(loginfile, g.d);
                Assert.IsFalse(string.IsNullOrWhiteSpace(data), "no login info");
                var li = data.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(3, li.Length, "missing login info");
                // attempt to startup connection
                con.Start(li[userr], li[pwr], li[prodr], Util.TLBuild());
                // wait a moment
                Util.sleep(4000);
                Assert.IsTrue(isserverconok, "server connection failed");
            }

            if (isserverconok && !isclientconok)
            {
                var c = new TLClient_WM("tempiqclient", false);
                c.VerboseDebugging = verbosesetting;
                c.SendDebugEvent += new DebugDelegate(g.d);

                if (c.ProvidersAvailable[0]!= Providers.IQFeed)
                    throw new Exception("unable to find test server");
                c.Mode(0, false);
                client = c;


                client.gotUnknownMessage += new MessageDelegate(client_gotUnknownMessage);
               
                // verify
                Assert.IsTrue(isclientconok, "client connection failed");
            }
            // reset everything
            mt = new MessageTracker();
            mt.VerboseDebugging = verbosesetting; 
            blt = new BarListTracker();
            mt.BLT = blt;
            mt.SendDebug += new DebugDelegate(g.d);
            mt.GotNewBar += new SymBarIntervalDelegate(mt_GotNewBar);
            recvbarcount = 0;
            msgok = true;
            g.d("iqfeed started.");
            // wait a moment
            Util.sleep(1000);
        }

        


        void mt_GotNewBar(string symbol, int interval)
        {
            if (mt.VerboseDebugging)
                g.d("mt new bar: " + symbol+" interval: "+interval);
        }

        

        MessageTracker mt = new MessageTracker();
        BarListTracker blt = new BarListTracker();
        int barcount(string sym, int bint) { return blt[sym, bint].Count; }
        

        bool msgok = true;
        int recvbarcount = 0;
        List<string> recvsyms = new List<string>();
        GenericTracker<List<Bar>> symintcount = new GenericTracker<List<Bar>>();
        string lastbarraw = string.Empty;
        Bar lastbar { get { return string.IsNullOrWhiteSpace(lastbarraw) ? new BarImpl() : BarImpl.Deserialize(lastbarraw); } }
        int reccomplete = 0;
        void client_gotUnknownMessage(MessageTypes type, long source, long dest, long msgid, string request, ref string response)
        {

            if (type == MessageTypes.BARRESPONSE)
            {
                lastbarraw = response;
                var b = BarImpl.Deserialize(response);
                var label = b.Symbol + b.Interval.ToString() + b.CustomInterval.ToString();
                var symintidx = symintcount.getindex(label);
                if (symintidx < 0)
                {
                    symintidx = symintcount.addindex(label, new List<Bar>());
                    g.d("got new symbol/interval: " + b.Symbol + " " + b.Interval+ b.CustomInterval+" bar:"+b.ToString());
                }

                symintcount[symintidx].Add(b);
                if (!recvsyms.Contains(b.Symbol))
                    recvsyms.Add(b.Symbol);
                recvbarcount++;
            }
            else if (type == MessageTypes.BARRESPONSE_FINAL)
            {
                var br = BarRequest.Deserialize(response);
                reccomplete++;
                g.d("completed: " + br.symbol + " " + br.Interval + " " + br.ID);
            }
            else
                g.d("got unknown message: " + type + request + response);
            var ok = mt.GotMessage(type, source, dest, msgid, request, ref response);
            if (!ok)
                g.d("error processing: " + type + " request: " + request + " response: " + response);
            msgok &= ok;
        }


        //[TestFixtureTearDown]
        public void Stop()
        {
            // stop client
            if (client != null)
            {

                client.Disconnect();
            }
            client = null;
            // stop server
            if (con != null)
                con.Stop();
            con = null;
            g.d("iqfeed teardown");
        }


    }


}
