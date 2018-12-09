using System;
using System.Windows.Forms;
using FameBot.Core;

namespace FameBot.UserInterface
{
    public partial class HealthBarGUI : Form
    {
        private Plugin.HealthEventHandler healthChangedHandler;
        public HealthBarGUI()
        {
            InitializeComponent();

            healthChangedHandler = new Plugin.HealthEventHandler((s, e) =>
            {
                int hP = (int)Math.Floor(e.Health);
                healthBar.Value = hP;
            });

            Plugin.healthChanged += healthChangedHandler;

            this.FormClosing += (s, e) =>
            {
                Plugin.healthChanged -= healthChangedHandler;
            };
        }

        private void onTopCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            TopMost = onTopCheckBox.Checked;
        }
    }
}
