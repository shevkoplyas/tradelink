using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TradeLink.Common;


namespace TestDOM
{
    public partial class Form1 : Form
    {

        private TLClient_WM tc;

        public Form1()
        {
            InitializeComponent();
            tc = new TLClient_WM();
            tc.gotTick += gotTick;
        }

        public void gotTick(TradeLink.API.Tick tick)
        {
            textBox1.Text += "DEPTH=" + tick.depth + " SYM=" + tick.symbol + " BID=" + tick.bid +
                " BIDSIZE=" + tick.BidSize + " ASK=" + tick.ask + " ASKSIZE=" + tick.AskSize + "/n";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tc.RequestDOM();
            
            String[] symlist = new String[2];
            symlist[0] = textBox2.Text;
            symlist[1] = textBox3.Text;
            TradeLink.API.Basket b = new TradeLink.Common.BasketImpl(symlist);

            tc.Subscribe(b);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
