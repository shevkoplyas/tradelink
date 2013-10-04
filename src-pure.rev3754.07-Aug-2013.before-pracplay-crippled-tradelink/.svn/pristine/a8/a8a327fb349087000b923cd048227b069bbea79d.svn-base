using System;
using System.Collections.Generic;
using TradeLink.Common;
using TradeLink.AppKit;
using TradeLink.API;

namespace Quotopia
{
    public class QuoteView :GenericView<Quote>, GotTickIndicator
    {
        public QuoteView() : base(Quote.GetGVI()) 
        {
            ViewName = "Quotes";
        }

        public void GotTick(Tick k)
        {
            var idx = symidx.getindex(k.symbol);
            // ensure model is valid
            if ((idx < 0) || (idx>=models.Count))
                return;
            // ensure it's a new time
            if (k.time <= sym2lasttime[idx])
                return;
            // update model
            var mod = models[idx];
            mod.GotTick(k);
            // ensure table exists
            if (updaterow(idx, mod))
            {
                // update time
                sym2lasttime[idx] = k.time;

            }

        }

        public event BasketDelegate SubscribeEvent;

        GenericTracker<int> symidx = new GenericTracker<int>();
        GenericTracker<int> sym2lasttime = new GenericTracker<int>();

        protected override List<Quote> getmodels()
        {

            // add them
            var mods = new List<Quote>();
            foreach (var sym in symbols2add)
            {
                // skip if we already have the symbol
                var idx = symidx.getindex(sym);
                if (idx < 0)
                {
                    Quote q = new Quote();
                    q.symbol = sym;
                    mods.Add(q);
                    idx = symidx.addindex(sym, mods.Count - 1);
                    sym2lasttime.addindex(sym, -1);
                    debug(sym + " added quote.");
                }
            }



            return mods;
            
        }

        public event StringDelegate GetAllParentSymbols;
        
        protected override void addsymsnow_post()
        {
            // subscribe to symbols after they've been added to grid
            if (SubscribeEvent != null)
            {
                var b = new BasketImpl(symbols2add);
                debug("Subscribed: " + Util.join(symbols2add));
                SubscribeEvent(b, id);
                
            }
        }

        protected override void grid_doubleclick(int r, int c)
        {
            openchartselected(string.Empty);
        }

        void addparentsymbols(string click)
        {
            // get symbols
            if (GetAllParentSymbols != null)
            {
                var symtext = GetAllParentSymbols();
                addsymbols(parsesymbols(symtext));
            }
            else
            {
                debug("This application does not support quoting all view symbols.");
                status("Quoting all view symbols is unsupported.");
            }
        }

        public override void CreateRightClick()
        {
            clearcontextmenu();
            rightaddviewsall();
            
            rightadd("add symbols");
            
            rightaddsym_user();
            rightaddsym_user_raw();
            rightaddsym_clip();
            rightaddpos();
            rightaddsym_file();
            rightadd("from all views", addparentsymbols);

            rightadd("lookup");
            rightaddgooglesym();
            rightaddyahoosym();
            rightaddopenchart();
            
            rightadd_defaults();

            rightaddremove_both();

            rightaddorders_all();

            rightadd("debugging");
            rightadd("show/hide", toggledebugrequest);
            rightadd("VerboseDebugging on/off", toggleverboseclick);
            rightadd("reconnect to broker/feed", forcereconnect);
            rightadd();

            
        }

        public event VoidDelegate SendReconnectRequestEvent;

        void forcereconnect(string click)
        {
            // confirm
            if (System.Windows.Forms.MessageBox.Show("Are you sure you want to reset all connections?", "Confirm Connection Reset", System.Windows.Forms.MessageBoxButtons.YesNoCancel) != System.Windows.Forms.DialogResult.Yes)
            {
                status("Connect reset canceled.");
                debug("Connect reset canceled by user.");
                return;
            }
            debug("Connection reset requested by user.");
            if (SendReconnectRequestEvent != null)
                SendReconnectRequestEvent();
            else
                debug("No reconnection binding is available, reconnect failed.");
            debug("Connection reset completed.");
        }

    }
}
