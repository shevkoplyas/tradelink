﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TradeLink.API
{
    public interface Response
    {
        // Response input
        void GotTick(Tick tick);
        void GotOrder(Order order);
        void GotFill(Trade fill);
        void GotOrderCancel(uint orderid);
        void GotPosition(Position pos);
        void GotMessage(MessageTypes type, uint id, string data);

        // Response output
        event StringParamDelegate SendIndicators;
        event OrderDelegate SendOrder;
        event UIntDelegate SendCancel;
        event DebugFullDelegate SendDebug;
        event MessageDelegate SendMessage;
        event BasketDelegate SendBasket;
        event ChartLabelDelegate SendChartLabel;

        // response control
        void Reset();

        // Response Information
        bool isValid { get; set; }
        string Name { get; set; }
        string FullName { get; set; }
        string[] Indicators { get; set; }
        int ID { get; set; }
    }
}
