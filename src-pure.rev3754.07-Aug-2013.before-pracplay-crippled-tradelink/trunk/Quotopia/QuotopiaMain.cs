using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TradeLink.API;
using TradeLink.AppKit;
using TradeLink.Common;

namespace Quotopia
{
    [System.ComponentModel.DesignerCategory("")] 
    public partial class QuotopiaMain
    {
        public const string PROGRAM = "Quotopia";
        public Log log = new Log(PROGRAM);
        SymbolKeyUpBuilder skub = new SymbolKeyUpBuilder();

        public QuotopiaMain()
        {
            InitializeComponent();
            if (!isquotopiacountok())
                return;
            // handle close smoothly
            FormClosing += new FormClosingEventHandler(QuotopiaMain_FormClosing);

            // initialization
            initquotopiachrome();
            initgvs();
            initfeeds();
            split.SplitterMoved += new SplitterEventHandler(split_SplitterMoved);
            Resize += new EventHandler(QuotopiaMain_Resize);
            

            restoresettings();
            setsplitter();
            RunHelper.run(refreshviews, completenone, debug, "refreshview");
        }

        void completenone()
        {
        }




        // view states
        int curviewidx = 0;
        List<GenericViewI> gvs = new List<GenericViewI>();
        List<string> viewnames = new List<string>();
        GotTickIndicator gti;
        GotPositionIndicator actpos;
        GotFillIndicator actfill;
        GotCancelIndicator actcancel;
        GotOrderIndicator actord;

        ResponseModelView rmv;
        GotTickIndicator rtick;
        GotPositionIndicator rpos;
        GotFillIndicator rfill;
        GotCancelIndicator rcancel;
        GotOrderIndicator rorder;
        GotMessageIndicator rmsg;
        

        string rname = Properties.Settings.Default.LastResponseName;
        string rdll = Properties.Settings.Default.LastResponseDll;
        double DEBUGWINDOWSIZEPCT = Properties.Settings.Default.DebugWindowSizePct;

        void initgvs()
        {
            
            // setting up all the model views supported by this quotopia instance

            // quotes
            QuoteView qv = new QuoteView();
            // custom bindings
            qv.SubscribeEvent+=new BasketDelegate(subscribe);
            qv.GetPositionsEvent += new PositionArrayDelegate(qv_GetPositionsEvent);
            qv.GetAllParentSymbols += new StringDelegate(qv_GetAllParentSymbols);
            qv.SendReconnectRequestEvent += new VoidDelegate(qv_SendReconnectRequestEvent);
            qv.Parent = split.Panel1;
            qv.KeyUp+=new KeyEventHandler(skub.KeyUp);
            gti = (GotTickIndicator)qv;
            bindaddview(qv);
            

            // account activity
            AccountActivityView av = new AccountActivityView();

            av.SendCancelEvent += new LongDelegate(av_SendCancelEvent);
            av.Parent = split.Panel1;
            actpos = (GotPositionIndicator)av;
            actord = (GotOrderIndicator)av;
            actfill = (GotFillIndicator)av;
            actcancel = (GotCancelIndicator)av;
            bindaddview(av);

            loadresponseview(rdll, rname);
            if (rmv!=null)
                bindaddview(rmv);

            updaterightclick();


        }

        void qv_SendReconnectRequestEvent()
        {
            initfeeds();
        }

        string qv_GetAllParentSymbols()
        {
            return Util.cjoin(allbaskets.ToSymArray());
        }

        void updaterightclick()
        {
            // recreate right click menus (to get automatic view switching)
            for (int i = 0; i < gvs.Count; i++)
                gvs[i].CreateRightClick();

        }

        void loadresponseview(string dll, string name)
        {
            if (string.IsNullOrWhiteSpace(dll) || string.IsNullOrWhiteSpace(name))
            {
                debug("User can provide responses to trade in response view (right click).");
                return;
            }
            debug(name + " load requested in: " + dll);
            var tmp = ResponseModelView.LoadResponseView(name, dll, debug);
            if (tmp.isValid)
            {
                rdll = dll;
                rname = name;
                
            }
            else
            {
                status(name + " did not load, see debug for details.");
            }
            loadresponseview(tmp);
        }
        void loadresponseview(ResponseModelView myrmv)
        {
            // response view
            if (rmv != null)
            {
                try
                {
                    rmv.Dispose();
                    rmv = null;
                }
                catch { }
            }
            rmv = myrmv;
            rmv.KeyUp+=new KeyEventHandler(skub.KeyUp);
            rmv.ResponseLoadRequestEvent += new ResponseLoadDel(rmv_ResponseLoadRequestEvent);
            rmv.Parent = split.Panel1;
            if (rmv.isValid)
            {
                rmv.SendBasketEvent += new BasketDelegate(subscribe);
                rmv.SendCancelEvent += new LongDelegate(av_SendCancelEvent);
                rmv.SendMessageEvent += new MessageDelegate(rmv_SendMessageEvent);
                rmsg = (GotMessageIndicator)rmv;
                rtick = (GotTickIndicator)rmv;
                rfill = (GotFillIndicator)rmv;
                rpos = (GotPositionIndicator)rmv;
                rcancel = (GotCancelIndicator)rmv;
                rorder = (GotOrderIndicator)rmv;
                status(rname + " loaded.  Right click on " + rmv.ViewName + " and turn on.");
            }
            


            
        }

        void rmv_ResponseLoadRequestEvent(string dll, string rname)
        {
            loadresponseview(dll, rname);
            bindaddview(rmv);
            if (rmv!=null)
                rmv.CreateRightClick();
        }

        void rmv_SendMessageEvent(MessageTypes type, long source, long dest, long msgid, string request, ref string response)
        {
            _bf.TLSend(type, source, dest, msgid, request, ref response);
        }

        void av_SendCancelEvent(long val)
        {
            if (_bf!=null)
                _bf.CancelOrder(val);
        }




        void bindaddview(GenericViewI gv)
        {
            if (gv == null)
            {
                debug("ignoring invalid view.");
                return;
            }
            var gvidx = viewnames.IndexOf(gv.ViewName);
            gv.isColoringEnabled = Properties.Settings.Default.ColoringEnabled;
            gv.SendDebugEvent += new DebugDelegate(debug);
            gv.SendDebugVisibleToggleEvent += new VoidDelegate(SendDebugVisibleToggleEvent);
            gv.SendViewRequestEvent += new DebugDelegate(gv_SendViewRequestEvent);
            gv.GetAvailViewsEvent += new GetAvailableGenericViewNamesDel(gv_GetAvailViewsEvent);
            gv.SendStatusEvent+=new DebugDelegate(status);
            gv.SendOrderEvent += new OrderDelegate(gv_SendOrderEvent);



            if (gvidx < 0)
            {
                gv.id = gvs.Count;
                viewnames.Add(gv.ViewName);
                gvs.Add(gv);

                if (gv.id == 0)
                    gv.Show();
                else
                    gv.Hide();
            }
            else
            {
                gv.id = gvidx;
                gvs[gvidx] = gv;
                gv.Show();
            }

            


            debug("loaded view: " + gv.ViewName);
            return;
            

            
            
        }

        void gv_SendOrderEvent(Order o)
        {
            if (_bf != null)
                _bf.SendOrder(o);
        }





        PositionTracker pt = new PositionTracker();
        Basket mb = new BasketImpl();

        AsyncResponse _ar = new AsyncResponse();
        BrokerFeed _bf;
        TLTracker _tlt;
        void initfeeds()
        {
            string[] servers = Properties.Settings.Default.ServerIpAddresses.Split(',');
            _bf = new BrokerFeed(Properties.Settings.Default.PreferredQuote, Properties.Settings.Default.PreferredExec, Properties.Settings.Default.FallbackToAnyProvider, false, PROGRAM, servers, Properties.Settings.Default.ServerPort);
            _bf.SendDebugEvent += new DebugDelegate(debug);
            _bf.Reset();

            // if our machine is multi-core we use seperate thread to process ticks
            if (Environment.ProcessorCount == 1)
                _bf.gotTick += new TickDelegate(tl_gotTick);
            else
            {
                _bf.gotTick += new TickDelegate(tl_gotTickasync);
                _ar.GotTick += new TickDelegate(tl_gotTick);
            }

            _bf.gotFill += new FillDelegate(_bf_gotFill);
            _bf.gotOrder += new OrderDelegate(_bf_gotOrder);
            _bf.gotOrderCancel += new LongDelegate(_bf_gotOrderCancel);
            _bf.gotPosition += new PositionDelegate(tl_gotPosition);
            _bf.gotUnknownMessage += new MessageDelegate(_bf_gotUnknownMessage);
            // monitor quote feed
            if (_bf.isFeedConnected)
            {
                int poll = (int)((double)Properties.Settings.Default.brokertimeoutsec * 1000 / 2);
                debug(poll == 0 ? "connection timeout disabled." : "using connection timeout: " + poll);
                _tlt = new TLTracker(poll, (int)Properties.Settings.Default.brokertimeoutsec, _bf.FeedClient, Providers.Unknown, true);
                _tlt.GotConnectFail += new VoidDelegate(_tlt_GotConnectFail);
                _tlt.GotConnect += new VoidDelegate(_tlt_GotConnect);
                _tlt.GotDebug += new DebugDelegate(_tlt_GotDebug);
                status("Connected: " + _bf.Feed);
                debug("Connected: " + _bf.Feed);
            }
            else
            {
                status("No feed available, did you start a tradelink connector?");
                debug("No feed available, did you start a tradelink connector?");
            }
        }

        void _bf_gotUnknownMessage(MessageTypes type, long source, long dest, long msgid, string request, ref string response)
        {
            if (rmsg != null)
            {
                try
                {
                    rmsg.GotMessage(type, source, dest, msgid, request, ref response);
                }
                catch (Exception ex)
                {
                    debug(rname + " error processing message: " + type + " err: " + ex.Message + ex.StackTrace);
                    status(rname + " error on got message, see debugs for details.");
                }
            }
        }

        void _bf_gotOrder(Order o)
        {
            // use full symbols if required
            if (RewriteSecuritySymbols)
            {
                Security sec;
                if (allbaskets.TryGetSecurityAnySymbol(o.symbol, out sec))
                {
                    o.symbol = sec.FullName;
                }
            }

            if (actord!= null)
                actord.GotOrder(o);
            if (rorder != null)
            {
                try
                {
                    rorder.GotOrder(o);
                }
                catch (Exception ex)
                {
                    debug(rname + " got order error: " + ex.Message + ex.StackTrace + " on order: " + o.ToString());
                    status(rname + " order error, see debug for details.");
                }
            }
        }

        void _bf_gotOrderCancel(long val)
        {
            if (actcancel!= null)
                actcancel.GotCancel(val);
            if (rcancel != null)
            {
                try
                {
                    rcancel.GotCancel(val);
                }
                catch (Exception ex)
                {
                    debug(rname + " got cancel error: " + ex.Message + ex.StackTrace + " on cancel : " + val.ToString());
                    status(rname + " cancel error, see debug for details.");
                }
            }

        }

        void _bf_gotFill(Trade t)
        {
            if (RewriteSecuritySymbols)
            {
                Security sec;
                if (allbaskets.TryGetSecurityAnySymbol(t.symbol, out sec))
                {
                    t.symbol = sec.FullName;
                }
            }
            debug(t.symbol+ " fill: " + t.ToString());
            if (actfill!= null)
                actfill.GotFill(t);
            if (rfill != null)
            {
                try
                {
                    rfill.GotFill(t);
                }
                catch (Exception ex)
                {
                    debug(rname + " got fill error: " + ex.Message + ex.StackTrace + " on fill : " + t.ToString());
                    status(rname + " fill  error, see debug for details.");
                }
            }

        }

        bool RewriteSecuritySymbols = Properties.Settings.Default.RewriteSecuritySymbols;

        void tl_gotTick(Tick t)
        {
            // heartbeat
            _tlt.newTick(t);
            // use full symbols if required
            if (RewriteSecuritySymbols)
            {
                Security sec;
                if (allbaskets.TryGetSecurityAnySymbol(t.symbol, out sec))
                {
                    t.symbol = sec.FullName;
                }
            }
            // update quotes
            if (gti!=null)
                gti.GotTick(t);
            // update response
            if (rtick != null)
            {
                try
                {
                    rtick.GotTick(t);
                }
                catch (Exception ex)
                {
                    debug(rname + " got tick error: " + ex.Message + ex.StackTrace + " on tick: " + t.ToString());
                    status(rname + " tick error, see debug for details.");
                }
            }
        }

        void _tlt_GotDebug(string msg)
        {
            //debug(msg);
        }

        void _tlt_GotConnect()
        {
            try
            {
                if (_tlt.tw.RecentTime != 0)
                {
                    debug(_bf.BrokerName + " " + _bf.ServerVersion + " refreshed.");
                    status(_bf.BrokerName + " connected.");
                }
                // if we have a quote provider
                if ((_bf.ProvidersAvailable.Length > 0))
                {
                    // don't track tradelink
                    if (_bf.BrokerName == Providers.TradeLink)
                    {
                        _tlt.Stop();
                    }

                    // if we have a quote provid
                    if (mb.Count > 0)
                        subscribe(mb);
                }
            }
            catch { }
        }

        void _tlt_GotConnectFail()
        {
            if (_tlt.tw.RecentTime != 0)
            {
            }
        }


        void Quote_Resize(object sender, EventArgs e)
        {
            Quotopia.Properties.Settings.Default.wsize = Size;

        }






        void tl_gotTickasync(Tick t)
        {
            // on multi-core machines, this will be invoked to write ticks
            // to a cache where they will be processed by a seperate thread
            // asynchronously
            _ar.newTick(t);
        }

        void tl_gotPosition(Position pos)
        {
            if (RewriteSecuritySymbols)
            {
                Security sec;
                if (allbaskets.TryGetSecurityAnySymbol(pos.symbol, out sec))
                {
                    pos = new PositionImpl(sec.FullName, pos.AvgPrice, pos.Size, pos.ClosedPL, pos.Account);
                }
            }
            debug(pos.symbol + " new position: " + pos.ToString());
            pt.Adjust(pos);
            if (actpos!=null)
                actpos.GotPosition(pos);
            if (rpos != null)
            {
                try
                {
                    rpos.GotPosition(pos);
                }
                catch (Exception ex)
                {
                    debug(rname + " got position error: " + ex.Message + ex.StackTrace + " on position : " + pos.ToString());
                    status(rname + " position  error, see debug for details.");
                }
            }
        }

        Basket allbaskets = new BasketImpl();

        void subscribe(Basket mb, int id) { subscribe(mb); }
        bool subscribe(Basket mb)
        {
            try
            {
                allbaskets.Add(mb);
                _bf.Subscribe(allbaskets);
                return true;
            }
            catch (TLServerNotFound) 
            { 
                debug("No broker/feed connector is running. Start one from Start -> Programs -> Tradelink Connectors");
                debug("See http://faq.tradelink.org for common connectors setup instructions and questions.");
            }
            return false;
        }

        Position[] qv_GetPositionsEvent()
        {
            return pt.ToArray();
        }

        void initquotopiachrome()
        {
            Text = PROGRAM;
            Text += " " + Util.TLVersion();
            var f = new Font(statusStrip1.Font.FontFamily, 10);
            statusStrip1.Font = f;
            statusStrip1.BackColor = Color.White;
            KeyPreview = true;
            KeyUp+=new KeyEventHandler(skub.KeyUp);
            skub.SendDebugEvent+=new DebugDelegate(debug);
            skub.SendStatusEvent+=new DebugDelegate(status);
            skub.SendNewBasketEvent += new BasketDelegate(skub_SendNewBasketEvent);
        }

        void skub_SendNewBasketEvent(Basket b, int id)
        {
            if ((curviewidx>=0) && (curviewidx<gvs.Count))
            {
                var view = gvs[curviewidx];
                debug(view.ViewName + " symbol add model request for: " + Util.cjoin(b.ToSymArrayFull()));
                view.addsymbols(b.ToSymArrayFull());

            
            }
            subscribe(b);
        }

        bool isquotopiacountok()
        {
            // count instances of program
            _QuotopiaINSTANCE = Util.ProgramCount(PROGRAM) - 1;
            // ensure have not exceeded maximum
            if ((_QuotopiaINSTANCE + 1) > _MAXQuotopiaINSTANCE)
            {
                MessageBox.Show("You have exceeded maximum # of running Quotopias (" + _MAXQuotopiaINSTANCE + ")." + Environment.NewLine + "Please close some.", "too many Quotopias");
                status("Too many Quotopias.  Disabled.");
                debug("Too many Quotopias.  Disabled.");
                return false;
            }
            else
            {
                status("Quotopia " + (_QuotopiaINSTANCE + 1) + "/" + _MAXQuotopiaINSTANCE);
                debug("Quotopia " + (_QuotopiaINSTANCE + 1) + "/" + _MAXQuotopiaINSTANCE);
            }
            return true;
        }

        int _QuotopiaINSTANCE = -1;
        int _MAXQuotopiaINSTANCE = 4;


        void setsplitter()
        {
            var newsize = (int)(split.Height * DEBUGWINDOWSIZEPCT);
            if (newsize>0)
                split.SplitterDistance = newsize;
        }

        int ForceRefreshInterval = Properties.Settings.Default.ForceRefreshIntervalMS;

        DateTime lastrefresh = DateTime.MinValue;
        void refreshviews()
        {
            // don't run if disabled
            if (ForceRefreshInterval == 0)
            {
                debug("Auto refresh enabled.");
                return;
            }
            debug("Refreshing every: " + ForceRefreshInterval + "ms");
            int sleep = (int)((double)ForceRefreshInterval / 2);
            // otherwise check periodically
            while (_go)
            {
                var now = DateTime.Now;
                // refresh current view
                if (_go && (now.Subtract(lastrefresh).TotalMilliseconds > ForceRefreshInterval))
                {
                    refreshview(curviewidx);
                    lastrefresh = now;
                }

                Util.sleep(sleep);
            }
        }

        void refreshview(int idx)
        {
            if ((idx < 0) || (idx >= gvs.Count))
                gvs[idx].refreshnow();
        }





        void gv_SendViewRequestEvent(string view)
        {
            // ensure we have view
            int newviewidx = viewnames.IndexOf(view);
            if (newviewidx < 0)
                return;
            // hide current
            gvs[curviewidx].Hide();
            // show new
            gvs[newviewidx].Show();
            // update current
            curviewidx = newviewidx;
            lastview = view;
            refreshview(curviewidx);
            debug("View switched to: " + view);
            Text = PROGRAM + " " + Util.TLVersion() + " : " + view;
        }

        List<string> gv_GetAvailViewsEvent()
        {
            return viewnames;
        }



        void SendDebugVisibleToggleEvent()
        {
            split.Panel2Collapsed = !split.Panel2Collapsed;

            Invalidate();
            Properties.Settings.Default.ShowDebugs = !split.Panel2Collapsed;
            Properties.Settings.Default.Save();
            status("Debugging: "+(!split.Panel2Collapsed ? "ON." : "hidden."));
                
        }

        void QuotopiaMain_Resize(object sender, EventArgs e)
        {
            setsplitter();
            if (Size.Height * Size.Width != 0)
            {
                Properties.Settings.Default.wsize = Size;
                Properties.Settings.Default.Save();
            }
        }

        void status(string msg)
        {
            if (InvokeRequired)
                Invoke(new DebugDelegate(status), new object[] { msg });
            else
            {
                _status.Text = msg;
                _status.Invalidate();
            }
        }

        void debug(string msg)
        {
            debugControl1.GotDebug(msg);
            log.GotDebug(msg);
        }

        void split_SplitterMoved(object sender, SplitterEventArgs e)
        {
            var tsd = split.Height == 0 ? 0 : (double)split.SplitterDistance / split.Height;
            if (tsd != 0)
            {
                DEBUGWINDOWSIZEPCT = tsd;
                Properties.Settings.Default.DebugWindowSizePct = DEBUGWINDOWSIZEPCT;
                Properties.Settings.Default.Save();
            }
        }

        string defaultexchange = Properties.Settings.Default.exchangedest;
        string defaultaccount = Properties.Settings.Default.accountname;
        string lastview = Properties.Settings.Default.LastViewName;

        void restoresettings()
        {
            // window size and location
            StartPosition = FormStartPosition.Manual;
            Location = Properties.Settings.Default.location; 
            Size = Properties.Settings.Default.wsize;
            // safety for saving too small window
            if (Size.Width == 0)
                Size = new Size(600, Size.Height);
            if (Size.Height == 0)
                Size = new Size(Size.Width, 600);

            // restore debug
            var showdebug = Properties.Settings.Default.ShowDebugs;
            split.Panel2Collapsed = !showdebug;

            // load saved view
            if (!string.IsNullOrWhiteSpace(lastview) && viewnames.Contains(lastview))
            {
                gv_SendViewRequestEvent(lastview);
            }

            // refresh
            Invalidate();
        }


        bool _go = true;
        void QuotopiaMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                _go = false;
                if ((rmv != null) && !string.IsNullOrWhiteSpace(rdll) && !string.IsNullOrWhiteSpace(rname))
                {
                    Properties.Settings.Default.LastResponseDll = rdll;
                    Properties.Settings.Default.LastResponseName = rname;
                }
                Properties.Settings.Default.LastViewName = lastview;
                Properties.Settings.Default.location = Location;
                Properties.Settings.Default.Save();
                log.Stop();
                _ar.Stop();
                _tlt.Stop();
                _bf.Unsubscribe();
                _bf.Disconnect();
            }
            catch { }
        }



    }
}
