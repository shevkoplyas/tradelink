using System;
using System.Collections.Generic;
using TradeLink.Common;
using TradeLink.AppKit;
using TradeLink.API;

namespace Quotopia
{
    public delegate void ResponseLoadDel(string dll, string rname);
    public class ResponseModelView : GenericView<ResponseModel>, GotTickIndicator, GotCancelIndicator, GotMessageIndicator, GotOrderIndicator, GotFillIndicator, GotPositionIndicator,
        SendOrderIndicator, SendMessage, SendCancelIndicator, SendBasketIndicator, SendDebugIndicator
    {
        public ResponseModelView(string[] indicators)
            : base(ResponseModel.getgvi(indicators))
        {
      
            ViewName = "Response Viewer";
        }
        public ResponseModelView()
            : base(ResponseModel.getgvi())
        {
            ViewName = "Response Viewer";
        }

        List<string> settablenames = new List<string>();

        Basket currentbasket = new BasketImpl();

        protected override void addsymsnow_post()
        {
            if (symbols2add.Length > 0)
            {
                currentbasket.Add(symbols2add);
                if (SendBasketEvent != null)
                    SendBasketEvent(currentbasket, id);
            }
        }

        Response response = null;
        protected bool isResponseEnabled = false;
        public bool isReady { get { return isValid && isResponseEnabled; } }
        public bool isValid { get { return (response != null) && response.isValid && isbound && issymbolpresent ; } }

        GenericTracker<int> sym2modidx = new GenericTracker<int>();
        GenericTracker<int> sym2lastupdatetime = new GenericTracker<int>();

        protected override List<ResponseModel> getmodels()
        {
            List<ResponseModel> mods = new List<ResponseModel>();
            if (!isValid)
            {
                debug("Can't add models when response is not loaded.");
                symbols2add = new string[0];
                return mods;
            }
            var indcount = mygvi.Count;
            foreach (var sym in symbols2add)
            {
                // get index
                var idx = sym2modidx.getindex(sym);
                // skip if already have it
                if (idx >= 0)
                    continue;
                dynamic mod = new ResponseModel(indcount);
                mod.symbol = sym;
                mod.responseowner = ResponseName;
                mods.Add(mod);
                idx = sym2modidx.addindex(sym, models.Count);
                sym2lastupdatetime.addindex(sym, -1);
            }
            return mods;

        }

        
        public string ResponseName { get { return isValid ? response.FullName : string.Empty; } }
        string _rdll = string.Empty;
        public string ResponseDll { get { return _rdll; } }

        static DebugDelegate _d = null;
        static void sdebug(string msg)
        {
            if (_d != null)
                _d(msg);
        }

        public static ResponseModelView LoadResponseView(string name, string dll, DebugDelegate d)
        {
            _d = d;
            var rmv = new ResponseModelView();
            // verify file exists
            if (string.IsNullOrWhiteSpace(dll))
            {
                sdebug("No dll provided, user can specifiy new dll in response view.");
                return rmv;
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                sdebug("Responses not being used.");
                return rmv;
            }
            if (!System.IO.File.Exists(dll))
            {
                sdebug("Dll no longer exists at: " + dll + ", user can specifiy new dll in response view.");
                return rmv;
            }
            // load response
            var r = ResponseLoader.FromDLL(name, dll, d);
            if ((r == null) || !r.isValid)
            {
                sdebug("Unable to load response: " + name + " from: " + dll);
                return rmv;
            }
            var inds = ResponseLoader.GetIndicators(dll, name, d);
            rmv = new ResponseModelView(inds);
            rmv.response = r;
            // update it
            rmv._rdll = dll;
            // bind it
            rmv.bind(inds);
            // get any user settable values
            rmv.settablenames = ResponseLoader.GetSettableList(dll, name, d);

            if (rmv.isValid)
            {
                sdebug("Loaded response: " + name + " from: " + dll);

            }
            else
            {
                sdebug("Response missing sym/symbol indicator: " + name + " in: " + dll);
                rmv = new ResponseModelView();
            }
            return rmv;

        }

        const string SETON = " = on";
        const string SETOFF = " = OFF";

        void changeresponse(string click)
        {
            // get dll
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "Responses (*.dll)|*.dll|All Files (*.*)|*.*";
            ofd.Multiselect = false;
            if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                status("User canceled dll change.");
                return;
            }
            var dll = ofd.FileName;
            // choose response
            var name = ResponseList.GetUserResponseName(dll, debug);
            if (string.IsNullOrWhiteSpace(name))
            {
                status("User canceled response name selection.");
                return;
            }
            // send request to load
            if (ResponseLoadRequestEvent != null)
                ResponseLoadRequestEvent(dll, name);
            else
            {
                status("This instance does not allow response loading.");
                debug("This view has no response load request handler defined.");
            }
        }

        void enableresponse(string click)
        {
            if (!isValid)
                return;
            // make sure we get add some symbols
            if (sym2lastupdatetime.Count == 0)
            {
                rightaddsym_user(string.Empty);
            }
            isResponseEnabled = true;
            status("Response: " + response.FullName + " is on.");
            debug("Response: " + response.FullName + " turned on by user.");
        }

        public event ResponseLoadDel ResponseLoadRequestEvent;

        void disableresponse(string click)
        {
            if (!isValid)
                return;
            isResponseEnabled = false;
            status("Response: " + response.FullName + " turned off.");
            debug("Response: " + response.FullName + " turned off by user.");
        }



        public override void CreateRightClick()
        {
            // reset everything
            clearcontextmenu();
            // add views
            rightaddviewsall();

            // add symbols to response
            rightadd("add symbols");
            rightaddsym_user();
            rightaddsym_user_raw();
            rightaddsym_clip();
            rightaddsym_file();
            rightadd();
            // add defaults
            rightadd_defaults();
            rightadd();
            if (isValid)
            {
                rightadd(response.Name);
                // change response
                rightadd("change response", changeresponse);
                rightadd("turn on", enableresponse);
                rightadd("turn off", disableresponse);
                rightadd("show status", respstatus);
                rightadd("reset", resetresponse);
                
                // add response menu
                if (settablenames.Count > 0)
                {
                    rightaddsep();
                    foreach (var sn in settablenames)
                    {
                        rightadd(sn + SETON, seton);
                        rightadd(sn + SETOFF, setoff);
                    }
                }
                rightadd();
            }
            else
            {
                rightadd("add response",changeresponse);

            }
            
            rightadddebugtogrequest();

        }

        void resetresponse(string click)
        {
            if (!isValid)
            {
                // should never happen
                return;
            }
            // confirm
            string extra = isResponseEnabled ? Environment.NewLine + "(Recommend turning off response first, currently ON)" : string.Empty;
            if (System.Windows.Forms.MessageBox.Show("Are you sure you want to reset: " + response.Name + "?" +extra, "Confirm " + response.Name + " Reset", System.Windows.Forms.MessageBoxButtons.YesNoCancel) != System.Windows.Forms.DialogResult.Yes)
            {
                status("User canceled " + response.Name + " reset.");
                return;
            }
            try
            {
                ResponseLoader.SendSimulationHints(ref response, Util.ToTLDate(), Util.ToTLTime(), getallsymbols().ToArray(), debug);
                response.Reset();
                status(response.Name + " was reset by user.");
                debug(response.FullName+ " was reset by user.");
            }
            catch (Exception ex)
            {
                status(response.Name + " error during Reset, see debugs.");
                debug(response.FullName + " got an error in user code on Reset, err: " + ex.Message + ex.StackTrace);
            }
        }

        string rstat { get 
        {
            return isResponseEnabled ? "ON." : "OFF.";
        }
        }

        void respstatus(string click)
        {
            status(response.Name+" is "+rstat);
        }

        public bool isUserSetsConfirmed = true;

        void seton(string click)
        {
            if (!isValid)
                return;
            var syms = getselectedsymbols();
            if (syms.Count<1)
            {
                status(click + " requires you to select a symbol from grid.");
                return;
            }

            // get symbol
            var sym = syms[0];

            // confirm
            if (isUserSetsConfirmed && ( System.Windows.Forms.MessageBox.Show("Do you want to '" + click + "' for " + sym + " on " + response.FullName + "?", "Confirm User Set", System.Windows.Forms.MessageBoxButtons.YesNoCancel) != System.Windows.Forms.DialogResult.Yes))
            {
                status("User canceled: " + click);
                return;
            }
            // get name
            click = click.Replace(SETON, string.Empty);
            // get message
            var set = new MessageSETUSER(sym,click,true);
            // send on message
            string tmp = string.Empty;
            response.GotMessage(MessageTypes.SENDUSERSET, 0, 0, 0, Util.Serialize<MessageSETUSER>(set, debug), ref tmp);
            status("Sent: " + set.ToString()+" to "+ResponseName);
            debug("User Sent: " + set.ToString() + " to " + ResponseName);
        }

        void setoff(string click)
        {
            if (!isValid)
                return;
            var syms = getselectedsymbols();
            if (syms.Count < 1)
            {
                status(click + " requires you to select a symbol from grid.");
                return;
            }
            // get symbol
            var sym = syms[0];
            // get name
            click = click.Replace(SETOFF, string.Empty);
            // get message
            var set = new MessageSETUSER(sym, click, false);
            // send on message
            string tmp = string.Empty;
            response.GotMessage(MessageTypes.SENDUSERSET, 0, 0, 0, Util.Serialize<MessageSETUSER>(set, debug), ref tmp);
            status("Sent: " + set.ToString() + " to " + ResponseName);
            debug("User Sent: " + set.ToString() + " to " + ResponseName);
        }
        

        void response_SendIndicatorsEvent(int idx, string data)
        {
            // can only process if we have symbol indicator
            if (issymbolmissing)
                return;
            // split data
            var inds = data.Split(',');
            // ensure looks good
            if (inds.Length != response.Indicators.Length)
                return;
            // get symbol
            var sym = inds[symbolidx];
            // get model index
            var modidx = sym2modidx.getindex(sym);
            // skip if invalid
            if (modidx<0)
            {
                debug(sym+" unknown symbol in "+response.FullName);
                return;
            }
            // grab model
            var mod = models[modidx];
            // update it
            mod.Value = inds;
            // see if we need to update screen
            if (_lastticktime > sym2lastupdatetime[modidx])
            {
                if (updaterow(modidx,mod))
                    sym2lastupdatetime[modidx] = _lastticktime;
            }
        }


        string[] Indicators = new string[0];

        int symbolidx = -1;
        bool issymbolmissing { get { return (symbolidx < 0) || (symbolidx >= Indicators.Length); } }
        bool issymbolpresent { get { return !issymbolmissing; } }



        int _lastticktime = -1;

        public void GotTick(Tick k)
        {
            // skip if disabled
            if (!isReady)
                return;
            // ensure tick is desired by this response
            var idx = sym2lastupdatetime.getindex(k.symbol);
            if (idx < 0)
                return;
            // update time
            _lastticktime = k.time;
            // pass tick
            response.GotTick(k);
        }



        void response_SendTicketEvent(string space, string user, string password, string summary, string description, Priority pri, TicketStatus stat)
        {
            // not implemented
        }

        void response_SendOrderEvent(Order o, int source)
        {
            sendorder(o);
        }

        void response_SendMessageEvent(MessageTypes type, long source, long dest, long msgid, string request, ref string response)
        {
            if (SendMessageEvent != null)
                SendMessageEvent(type, source, dest, msgid, request, ref response);
        }

        void response_SendDebugEvent(string msg)
        {
            debug(msg);
        }

        void response_SendChartLabelEvent(decimal price, int time, string label, System.Drawing.Color c)
        {
            // not implemented
        }

        void response_SendCancelEvent(long val, int source)
        {
            if (SendCancelEvent != null)
                SendCancelEvent(val);
        }

        void response_SendBasketEvent(Basket b, int id)
        {
            // create models if we don't have them
            bool create = false;
            for (int i = 0; i<b.Count; i++)
            {
                if (sym2modidx.getindex(b[i].FullName) < 0)
                {
                    create = true;
                    break;
                }
            }
            if (create)
            {
                symbols2add = b.ToSymArrayFull();
                addsymsnow();
            }

        }








        static void r_SendOrderEvent(Order o, int source)
        {

        }

        static void r_SendMessageEvent(MessageTypes type, long source, long dest, long msgid, string request, ref string response)
        {

        }

        static void r_SendIndicatorsEvent(int idx, string data)
        {

        }

        static void r_SendDebugEvent(string msg)
        {

        }

        static void r_SendChartLabelEvent(decimal price, int time, string label, System.Drawing.Color c)
        {

        }

        static void r_SendBasketEvent(Basket b, int id)
        {

        }

        static void r_SendCancelEvent(long val, int source)
        {

        }

        static void r_SendTicketEvent(string space, string user, string password, string summary, string description, Priority pri, TicketStatus stat)
        {

        }



        public void GotCancel(long id)
        {
            if (!isValid)
                return;
            response.GotOrderCancel(id);
        }

        public void GotMessage(MessageTypes type, long source, long dest, long msgid, string request, ref string result)
        {
            if (!isValid)
                return;
            response.GotMessage(type, source, dest, msgid, request, ref result);
        }

        public void GotOrder(Order o)
        {
            if (!isValid)
                return;
            response.GotOrder(o);
        }

        public void GotFill(Trade f)
        {
            if (!isValid)
                return;
            response.GotFill(f);
        }

        public void GotPosition(Position p)
        {
            if (!isValid)
                return;
            response.GotPosition(p);
        }

        

        public event MessageDelegate SendMessageEvent;

        public event LongDelegate SendCancelEvent;

        public event BasketDelegate SendBasketEvent;

        


        protected override void Dispose(bool disposing)
        {
            unbind();
            base.Dispose(disposing);

        }

        bool isbound = false;
        public bool bind(string[] myrinds)
        {
            unbind();
            isbound = false;
            if (response == null)
                return isbound;
            if (isbound)
                return isbound;
            Indicators = myrinds;
            if (!response.isValid)
            {
                debug("invalid response: " + response.FullName+", check your response code.");
                return isbound;
            }
            // locate primary index
            for (int i = 0; i < Indicators.Length; i++)
            {
                var iname = Indicators[i].ToUpper();
                if ((iname == "SYM") || (iname == "SYMBOL"))
                    symbolidx = i;
            }
            if (!issymbolmissing)
            {
                response.SendIndicatorsEvent += new ResponseStringDel(response_SendIndicatorsEvent);
                response.SendBasketEvent += new BasketDelegate(response_SendBasketEvent);
                response.SendCancelEvent += new LongSourceDelegate(response_SendCancelEvent);
                response.SendChartLabelEvent += new ChartLabelDelegate(response_SendChartLabelEvent);
                response.SendDebugEvent += new DebugDelegate(response_SendDebugEvent);
                response.SendMessageEvent += new MessageDelegate(response_SendMessageEvent);
                response.SendOrderEvent += new OrderSourceDelegate(response_SendOrderEvent);
                response.SendTicketEvent += new TicketDelegate(response_SendTicketEvent);
                try
                {
                    response.ID = 0;
                    response.Reset();
                    isbound = true;
                }
                catch (Exception ex)
                {
                    debug("Error in Reset method of response: " + response.FullName + ", check your response code, err: " + ex.Message + ex.StackTrace);
                }
                
            }
            else
                debug("Response cannot be processed in view, missing symbol/sym column: " + response.FullName+" contains only: "+Util.join(response.Indicators));
            return isbound;
        }



        public void unbind()
        {
            if (!isbound)
                return;
            symbolidx = -1;
            isbound = false;
            if (response == null)
                return;
            
            try
            {
                response.SendIndicatorsEvent -= new ResponseStringDel(response_SendIndicatorsEvent);
                response.SendBasketEvent -= new BasketDelegate(response_SendBasketEvent);
                response.SendCancelEvent -= new LongSourceDelegate(response_SendCancelEvent);
                response.SendChartLabelEvent -= new ChartLabelDelegate(response_SendChartLabelEvent);
                response.SendDebugEvent -= new DebugDelegate(response_SendDebugEvent);
                response.SendMessageEvent -= new MessageDelegate(response_SendMessageEvent);
                response.SendOrderEvent -= new OrderSourceDelegate(response_SendOrderEvent);
                response.SendTicketEvent -= new TicketDelegate(response_SendTicketEvent);
            }
            catch { }
            
        }
    }


}
