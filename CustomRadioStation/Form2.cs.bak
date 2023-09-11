/* 
 * Custom Radio Station
 * Copyright (C) 2021  Jakub Mareček (info@jakubmarecek.cz)
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with Custom Radio Station.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CustomRadioStation
{
    public partial class Form2 : Form
    {
        public int MusicVolume
        {
            get { return (int)numericUpDown1.Value; }
            set { numericUpDown1.Value = value; }
        }

        public int MusicLength
        {
            get { return (int)numericUpDown2.Value; }
            set { numericUpDown2.Value = value; }
        }

        public string MusicName
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }

        public bool MusicCondNight
        {
            get { return checkBox1.Checked; }
            set { checkBox1.Checked = value; }
        }

        public int MusicCondMin
        {
            get { return (int)numericUpDown3.Value; }
            set { numericUpDown3.Value = value; }
        }

        public int MusicCondMax
        {
            get { return (int)numericUpDown4.Value; }
            set { numericUpDown4.Value = value; }
        }

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            var sii = new NativeMethods.SHSTOCKICONINFO
            {
                cbSize = (UInt32)Marshal.SizeOf(typeof(NativeMethods.SHSTOCKICONINFO))
            };
            _ = NativeMethods.SHGetStockIconInfo(NativeMethods.SHSTOCKICONID.SIID_HELP, NativeMethods.SHGSI.SHGSI_ICON, ref sii);
            pictureBox1.Image = Icon.FromHandle(sii.hIcon).ToBitmap();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (numericUpDown3.Value >= numericUpDown4.Value)
            {
                MessageBox.Show("Minimum hour can't be same or higher than maximum hour!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult = DialogResult.OK;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                numericUpDown3.Enabled = false;
                numericUpDown4.Enabled = false;
            }
            else
            {
                numericUpDown3.Enabled = true;
                numericUpDown4.Enabled = true;
            }
        }
    }
}
