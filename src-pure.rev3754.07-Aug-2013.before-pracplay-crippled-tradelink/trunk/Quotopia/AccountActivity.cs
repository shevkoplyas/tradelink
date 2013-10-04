using System;
using System.Collections.Generic;
using TradeLink.Common;
using TradeLink.API;
using TradeLink.AppKit;

namespace Quotopia
{
    public class AccountActivity : Model
    {
        string _sym = string.Empty;
        public string symbol { get { return _sym; } set { _sym = value; } }
        public bool isValid { get { return  (type!= ActivityType.None); } }
        public bool isDynamic { get { return false; } }
        public Position CurrentPos = new PositionImpl();

        public bool side = true;
        public long id = 0;
        public int size = 0;
        public decimal price = 0;

        public string pos_nice { get { return CurrentPos.isValid ? CurrentPos.ToString() : string.Empty; } }
        

        public ActivityType type = ActivityType.None;
        public string type_nice { get { return type.ToString(); } }
        public string side_nice { get { return (type != ActivityType.Cancel) ? side ? "buy" : "sell" : string.Empty; } }

        public string[] Value { get; set; }

        public static AccountActivity NewTrade(Trade f, Position p)
        {
            var aa = new AccountActivity();
            aa.CurrentPos = p;
            aa.symbol = f.symbol;
            aa.side = f.xsize > 0;
            aa.id = f.id;
            aa.price = f.xprice;
            aa.size = f.xsize;
            aa.type = ActivityType.Fill;
            return aa;
        }

        public static AccountActivity NewPosition(Position p)
        {
            var aa = new AccountActivity();
            aa.CurrentPos = p;
            aa.symbol = p.symbol;
            aa.side = p.isLong;
            aa.type = ActivityType.Position;

            return aa;
        }

        public static AccountActivity NewOrder(Order o)
        {
            var aa = new AccountActivity();
            aa.symbol = o.symbol;
            aa.side = o.side;
            aa.id = o.id;
            aa.price = o.price;
            aa.size = o.size;
            aa.type = o.isLimit ? ActivityType.Limit : o.isMarket ? ActivityType.Market : o.isStop ? ActivityType.Stop : ActivityType.Order;

            return aa;
        }

        public static AccountActivity NewCancel(long id)
        {
            var aa = new AccountActivity();
            aa.id = id;
            aa.type = ActivityType.Cancel;

            return aa;
        }

        //static AccountActivity t;
        static GenericViewItem<AccountActivity> _t;
        public static GenericViewItem<AccountActivity> GetGVI()
        {
            if (_t != null)
                return _t;

            var q = new GenericViewItem<AccountActivity>();
            q.Add("symbol");
            q.Add("activity", "type_nice");
            q.Add("side", "side_nice");
            q.Add("size");
            q.Add("price");
            q.Add("id","id","F0");
            q.Add("position","pos_nice");
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

        string _owner = string.Empty;
        public string owner
        {
            get { return _owner; } set { _owner  = value; } 
        }

        public override string ToString()
        {
            return isValid ? symbol + " " + type.ToString() + " id: " + id + " pos: " + CurrentPos.ToString() : string.Empty;
        }
    }

    public enum ActivityType
    {
        None,
        Market,
        Limit,
        Stop,
        Order,
        Fill,
        Cancel,
        Position,
    }
}
