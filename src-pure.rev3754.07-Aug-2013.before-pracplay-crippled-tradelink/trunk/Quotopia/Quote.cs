using System;
using System.Collections.Generic;
using TradeLink.Common;
using TradeLink.API;
using TradeLink.AppKit;

namespace Quotopia
{
    public class Quote : Model, GotTickIndicator
    {
        public string symbol { get; set; }
        public decimal Bid = 0;
        public decimal Ask = 0;
        public decimal Trade = 0;

        public int Size = 0;
        public int BidSize = 0;
        public int AskSize = 0;

        decimal _close = 0;
        public decimal close { get { if (!isValid) return 0; if (_close == 0) _close = BarListImpl.GetChart(symbol).RecentBar.Close; return _close; } }

        public decimal Change { get { if (!isValid) return 0; if (Trade * close == 0) return 0; return (Trade - close) / close; } }

        public string bidask { get { return Bid.ToString("N2") + "x" + Ask.ToString("N2"); } }

        public bool isValid
        {
            get { return !string.IsNullOrWhiteSpace(symbol); }
        }

        public string[] Value { get; set; }
        public bool isDynamic { get { return false; } }

        public static Quote Get(string sym) { var q = new Quote(); q.symbol = sym; return q; }

        

        public void GotTick(Tick k)
        {
            // ensure it's our symbol
            if (symbol != k.symbol)
                return;
            if (k.hasAsk)
            {
                Ask = k.ask;
                AskSize = k.AskSize;
            }
            if (k.hasBid)
            {
                Bid = k.bid;
                BidSize = k.BidSize;
            }
            if (k.isTrade)
            {
                Trade = k.trade;
                Size = k.size;
            }
        }

        string _owner = string.Empty;
        public string owner
        {
            get
            {
                return _owner;
            }
            set
            {
                _owner = value;
            }
        }


        //static Quote t;
        static GenericViewItem<Quote> _t;
        public static GenericViewItem<Quote> GetGVI()
        {
            if (_t != null)
                return _t;

            var q = new GenericViewItem<Quote>();
            q.Add("symbol");
            q.Add("trade","Trade");
            q.Add("size","Size");
            q.Add("bidask");
            q.Add("bidsize", "BidSize");
            q.Add("asksize","AskSize");
            q.Add("change", "Change", "P1");
            q.Add("close");

            _t = q;
            return _t;
        }
        

        public List<string> ToItem()
        {
            return GetGVI().ToItems(this);
        }

        public List<object> ToData()
        {
            return GetGVI().ToData(this);
        }
    }
}
