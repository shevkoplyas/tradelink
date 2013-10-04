using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TradeLink.API;

namespace ASP
{
    public partial class ASPOptions : Form
    {
        public ASPOptions()
        {
            InitializeComponent();
            FormClosing += new FormClosingEventHandler(ASPOptions_FormClosing);
        }

        void ASPOptions_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
            try
            {
                Properties.Settings.Default.Save();
            }
            catch { }
        }
        public event Int32Delegate TimeoutChanged;
        private void _brokertimeout_ValueChanged(object sender, EventArgs e)
        {
            if (TimeoutChanged != null)
                TimeoutChanged((int)_brokertimeout.Value);
        }

        private void _portal_TextChanged(object sender, EventArgs e)
        {
        }

        internal event VoidDelegate MktTimestampChange;

        private void _usemkttime_CheckedChanged(object sender, EventArgs e)
        {
            if (MktTimestampChange != null)
                MktTimestampChange();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            Properties.Settings.Default.Save();
        }

        public event BoolDelegate SendExecSubscribeChangedEvent;
        public int ExecSubscribeWaitMS { get { return (int)execsubscribedelayms.Value; } }

        private void execsubscribes_CheckedChanged(object sender, EventArgs e)
        {
            var status = execsubscribes.Checked;
            execsubscribedelayms.Enabled = status;
            Invalidate(true);
            if (SendExecSubscribeChangedEvent != null)
                SendExecSubscribeChangedEvent(status);
        }


    }
}
