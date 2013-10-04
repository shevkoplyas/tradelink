using System;
using System.Collections.Generic;
using TradeLink.Common;
using TradeLink.AppKit;
using TradeLink.API;

namespace Quotopia
{
    public class ResponseModel : System.Dynamic.DynamicObject, 
        Model
    {
        public ResponseModel() : this(0) { }
        public ResponseModel(int vals)
        {
            _val = new string[vals];
        }
        string _sym = string.Empty;
        public string symbol { get { return _sym; } set { _sym = value; } }

        public string responseowner = string.Empty;

        public bool isValid { get { return !string.IsNullOrWhiteSpace(_sym) && !string.IsNullOrWhiteSpace(responseowner); } }

        public bool isDynamic { get { return true; } }

        string[] _val = new string[0];

        
        public string[] Value { get { return _val; } set { _val = value; } }

        GenericTracker<int> iname2validx = new GenericTracker<int>();

        int getindex(string iname)
        {
            var idx = iname2validx.getindex(iname);
            // build index first time
            if (idx < 0)
            {
                var inds = getgvi().itemnames;
                for (int i = 0; i < inds.Count; i++)
                {
                    var tmpiname = inds[i];
                    if (tmpiname == iname)
                        idx = i;
                    iname2validx.addindex(tmpiname, i);
                }

            }
            return idx;

        }

        

        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
        {
            // get index of column value trying to be retrieved
            var idx = getindex(binder.Name);
            // default to empty
            result = string.Empty;
            // return error if we can't find
            if (idx < 0)
            {
                return base.TryGetMember(binder, out result);
            }
            // get result
            result = Value[idx];
            return true;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return iname2validx.ToLabelArray();
        }



        string _owner = string.Empty;
        public string owner { get { return _owner; } set { _owner = value; } }


        //static ResponseModel t;
        static GenericViewItem<ResponseModel> _t;

        public static void cleargvi() { _t = null; }

        public static GenericViewItem<ResponseModel> getgvi() { return getgvi(new string[0]); }
        public static GenericViewItem<ResponseModel> getgvi(string[] indicatornames)
        {
            if ((_t != null))
                return _t;


            var gvi = new GenericViewItem<ResponseModel>();

            // create string column for each indicator value
            foreach (var i in indicatornames)
            {
                gvi.Add(i,i,string.Empty,string.Empty,typeof(string));
            }
            if (indicatornames.Length>0)
                _t = gvi;
            return gvi;
        }

        public List<string> ToItem()
        {
            return getgvi().ToItems(this);
        }

        public List<object> ToData()
        {
            return getgvi().ToData(this);
        }

        public override string ToString()
        {
            return !isValid ? "Invalid Response Model" : symbol + " running under: " + responseowner;
        }
        
    }
}
