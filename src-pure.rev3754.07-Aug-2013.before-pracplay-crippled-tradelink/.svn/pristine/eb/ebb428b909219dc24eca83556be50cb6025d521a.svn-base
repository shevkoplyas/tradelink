using System;
using System.Collections.Generic;
using TradeLink.Common;
using TradeLink.AppKit;
using TradeLink.API;

namespace Quotopia
{
    public class AccountActivityView : GenericView<AccountActivity>, GotFillIndicator, GotOrderIndicator, GotCancelIndicator, GotPositionIndicator
    {
        public AccountActivityView()
            : base(AccountActivity.GetGVI())
        {
            ViewName = "AccountActivity";
            DoubleClick += new EventHandler(AccountActivityView_DoubleClick);
            
        }

        

        public override void CreateRightClick()
        {
            clearcontextmenu();
            rightaddviewsall();
            rightadd();
            rightadddebugtogrequest();

        }

        void AccountActivityView_DoubleClick(object sender, EventArgs e)
        {
            // get selected model
            var mods = getselectedmods();
            if (mods.Count == 0)
                return;
            var mod = mods[0];
            // if order, and is pending, open cancel confirm
            if (ord.isPending(mod.id))
                cancelorders(string.Empty);
            // otherwise 
            else
                rightaddticket(string.Empty);

        }

        public event LongDelegate SendCancelEvent;
        void cancel(params long[] ids)
        {
            foreach (var id in ids)
                cancel(id);
        }

        void cancel(long id)
        {
            if (SendCancelEvent != null)
                SendCancelEvent(id);
            else
                debug("Cancels are not enabled for " + ViewName+", ignoring: "+id);
        }

        protected override void grid_doubleclick(int r, int c)
        {
            // get model
            var mod = models[r];
            // if it's pending, cancel it otherwise open ticket
            if (ord.isPending(mod.id))
                cancelorders(string.Empty);
            else
                rightaddticket(string.Empty);

        }



        PositionTracker pt = new PositionTracker();
        OrderTracker ord = new OrderTracker();

        protected void cancelorders(string click)
        {
            // get models
            var mods = getselectedmods();
            if (mods.Count == 0)
            {
                status("Select an account activity to cancel.");
                return;
            }
            List<long> cancel = new List<long>();
            // process any that are pending orders
            for (int i = 0; i < mods.Count; i++)
            {
                var mod = mods[i];
                var id = mod.id;
                if (ord.isPending(id))
                    cancel.Add(id);
                else
                    v(mod.symbol + " can no longer cancel: " + id);
            }
            if (cancel.Count == 0)
                return;
            // confirm
            if (System.Windows.Forms.MessageBox.Show("Cancel " + cancel.Count + " pending orders?", "Confirm Order Cancel", System.Windows.Forms.MessageBoxButtons.YesNoCancel) != System.Windows.Forms.DialogResult.Yes)
                return;


        }


        public void GotFill(Trade t)
        {
            
            ord.GotFill(t);
            pt.GotFill(t);
            ShowItems(AccountActivity.NewTrade(t,pt[t.symbol]));
            
        }

        public void GotPosition(Position p)
        {
            pt.GotPosition(p);
            ShowItems(AccountActivity.NewPosition(p));

        }

        public void GotOrder(Order o)
        {
            ord.GotOrder(o);
            ShowItems(AccountActivity.NewOrder(o));
        }

        public void GotCancel(long id)
        {
            ord.GotCancel(id);
            ShowItems(AccountActivity.NewCancel(id));
        }
    }
}
