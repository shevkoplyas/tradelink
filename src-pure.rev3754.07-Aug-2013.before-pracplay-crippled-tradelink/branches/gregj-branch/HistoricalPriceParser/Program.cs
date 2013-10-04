using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine.Utility;
using TradeLink.API;
using System.Globalization;

namespace HistoricalPriceParser
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Test if input arguments were supplied:

            Arguments CommandLine = new Arguments(args);
            if (CommandLine["source"] != null)
            {
                Console.WriteLine("DataSource is " + CommandLine["source"] + "\n");
            } else {
                Console.WriteLine("Source was not defined");            
            }
           // DateTime s = new DateTime(2009,11,12);
           // DateTime e = new DateTime(2009, 11, 13);
           // BarList bar = TradeLink.Common.BarListImpl.DayFromYahoo("GOOG",s,e);            
            BarList bar = TradeLink.Common.BarListImpl.DayFromYahoo("GOOG");
            TradeLink.Common.TikWriter tw = new TradeLink.Common.TikWriter("GOOG");
            Tick t = null;
            
            CultureInfo provider = CultureInfo.InvariantCulture;
            for (int i = bar.Count; i >= 0; i--)           
            {
                string[] bardata = new string[] { bar[i].Open.ToString(), bar[i].High.ToString(), bar[i].Low.ToString(), bar[i].Close.ToString() };
                string mydate = bar[i].Bardate.ToString().Replace("-", "");
                mydate = mydate.Replace("/", "");
               
                
                for (int n = 0; n <= 3; n++)                     
                {
                    t = new TradeLink.Common.TickImpl("GOOG");
                    t.time = 160000;
                    t.size = Convert.ToInt32(bar[i].Volume / 4);
                    t.date = int.Parse(mydate);
                    t.trade = decimal.Parse(bardata[n]);
                    
                    Console.WriteLine(n + " " + mydate + " " + t.trade);
                    tw.newTick(t);
                }
             
            }
            tw.Close();
            

        }

        static int ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date - origin;
            return System.Convert.ToInt32(Math.Floor(diff.TotalSeconds));
        }


    }
}
