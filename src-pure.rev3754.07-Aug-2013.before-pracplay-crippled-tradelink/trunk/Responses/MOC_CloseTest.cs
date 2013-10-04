using TradeLink.Common;
using TradeLink.API;

namespace Responses
{
    /// <summary>
    /// Response Name  :
    /// Last Modified  :
    /// Parameter Notes:
    /// Synopsis       :
    /// </summary>

    public class MOC_CloseTest : ResponseTemplate
    {
        ///////////////////////////////////////////////////////////////////////
        // Initializers
        ///////////////////////////////////////////////////////////////////////
        public override void Reset()
        {
            Name = "MOC CloseTest";
            RequestedForSymbol = new GenericTracker<bool>();
        }

        ///////////////////////////////////////////////////////////////////////
        // TradeLink Hooks
        ///////////////////////////////////////////////////////////////////////
        public override void GotTick(Tick tick)
        {
            if (RequestedForSymbol.getindex(tick.symbol) == GenericTracker.UNKNOWN
                || RequestedForSymbol[tick.symbol] == false)
            {
                RequestedForSymbol.addindex(tick.symbol, true);

                // send opening order
                Order Opener = new OrderImpl(tick.symbol, true, 100);
                sendorder(Opener);

                // send closing order
                Order Closer = new MOCOrder(tick.symbol, false, 100);
                sendorder(Closer);
                senddebug("Sent Order: " + Closer);
            }
        }

        public override void GotFill(Trade fill)
        {
            senddebug("Fill for " + fill.symbol + "  time: " + fill.xtime + "  price: " + fill.xprice);
        }


        ///////////////////////////////////////////////////////////////////////
        // Member Objects
        ///////////////////////////////////////////////////////////////////////
        GenericTracker<bool> RequestedForSymbol = new GenericTracker<bool>();
    }
}