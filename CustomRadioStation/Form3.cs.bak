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
    public partial class Form3 : Form
    {
        public GameType GameType
        {
            set;
            get;
        }

        public Form3()
        {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            Text = Form1.appName;

            var sii = new NativeMethods.SHSTOCKICONINFO
            {
                cbSize = (UInt32)Marshal.SizeOf(typeof(NativeMethods.SHSTOCKICONINFO))
            };
            _ = NativeMethods.SHGetStockIconInfo(NativeMethods.SHSTOCKICONID.SIID_INFO, NativeMethods.SHGSI.SHGSI_ICON, ref sii);
            pictureBox1.Image = Icon.FromHandle(sii.hIcon).ToBitmap();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GameType = GameType.FarCry5;
            DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            GameType = GameType.FarCryNewDawn;
            DialogResult = DialogResult.OK;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            GameType = GameType.FarCry6;
            DialogResult = DialogResult.OK;
        }
    }
}
