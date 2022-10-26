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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace CustomRadioStation
{
    public partial class Form1 : Form
    {
        public static string appName = "Custom Radio Station v2.00 by ArmanIII";
        GameType gameType;
        string m_Path = ""; // Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        readonly List<string> musicFiles = new();
        readonly List<float> musicFilesVolume = new();
        readonly List<int> musicFilesLength = new();
        readonly List<string> musicFilesNames = new();
        readonly List<string> musicFilesCond = new();
        readonly uint[] sampleBNKIDs = new uint[] {
            164370702, // BNK ID
            888652588, // WEM ID
            1188643307, // Short ID
            134173468,
            417585942,
            528275598,
            627107392,
            658292691,
            771727209,
            1008334217,
            56356790,
            48516712,
            133802278,
            223443951,
            744122392,
            765935597,
            1064289555,
            851527417,
            974014491,
            659444010,
            478050401,
            1783355234,
            2712157172,
            749600624,
            764018471
        };
        readonly uint[] sampleBNKIDs_ND = new uint[] {
            1218334055, // BNK ID
            25667423, // WEM ID
            973244057, // Short ID
            178391155,
            394206044,
            417585942,
            501295279,
            981314518,
            56356790,
            117281576,
            133366811,
            333086244,
            475473390,
            561749661,
            448846712,
            860823947,
            950266236,
            181380205,
            331618101,
            659444010,
            478050401,
            1783355234,
            2712157172,
            206596153,
        };
        readonly uint[] sampleBNKIDs_6 = new uint[] {
            1174781907, // BNK ID
            723150784, // WEM ID
            2228076101, // Short ID
            881797481,
            // 870504206,
            // 630521720,
            // 1043557427,
            // 1937429806,
            // 2187133148,
            524492719
        };
        readonly float[] bnkVolumes = new float[] { -8, -10, -11, -13 };
        readonly float[] bnkVolumes_nd = new float[] { -5, -10, 5, -8, -7 };
        readonly float[] bnkVolumes_6 = new float[] { -6 };
        readonly string outputFileName = "Custom Radio Station.a3";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (musicFiles.Count == 0)
            {
                MessageBox.Show("There are no music files, can't create package!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (musicFiles.Count > 90)
            {
                MessageBox.Show("You have too much music files, remove some of them. Maximum count of musics per station is 90.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new();
            saveFileDialog.Filter = "Mod Installer package|*.a3";
            saveFileDialog.Title = "Select an output location";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            saveFileDialog.FileName = outputFileName;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(saveFileDialog.FileName))
                    File.Delete(saveFileDialog.FileName);

                ZipArchive zip = ZipFile.Open(saveFileDialog.FileName, ZipArchiveMode.Create);

                string fls = "";
                foreach (string mf in musicFilesNames)
                    fls += mf + Environment.NewLine;

                string game = "";
                if (gameType == GameType.FarCry5) game = "FC5";
                if (gameType == GameType.FarCryNewDawn) game = "FCND";
                if (gameType == GameType.FarCry6) game = "FC6";

                Stream stream = GetFromResourceData("info.xml");
                XDocument xInfo = XDocument.Load(stream);
                xInfo.Element("PackageInfo").Element("Games").Element("Game").Value = game;
                string desc = xInfo.Element("PackageInfo").Element("Description").Value;
                desc = desc.Replace(":files:", fls);
                xInfo.Element("PackageInfo").Element("Description").Value = desc;

                string baseID = "1{RadioStation}";
                string baseCID = "901777777777{RadioStation}";

                XElement xInfoPairs = new("Pairs");
                xInfo.Root.Add(xInfoPairs);

                string ir = "";
                if (gameType == GameType.FarCry5) ir = "info_replace.xml";
                if (gameType == GameType.FarCryNewDawn) ir = "info_replace.xml";
                if (gameType == GameType.FarCry6) ir = "info_replace_6.xml";

                Stream stream2 = GetFromResourceData(ir);
                XDocument xReplaceXML = XDocument.Load(stream2);
                XElement xInfoReplaceReplaces = xReplaceXML.Element("PackageInfoReplace").Element("Replaces");

                XElement xInfoReplaceCFCXRadioStation = xInfoReplaceReplaces.Descendants().Where(d => d.Attribute("CRS")?.Value == "1").Single();
                xInfoReplaceCFCXRadioStation.Attribute("templateValueID").Value = baseCID + "100";
                xInfoReplaceCFCXRadioStation.Attribute("templateValueBlockID").Value = baseCID + "200";

                XElement xInfoReplaceStation = xInfoReplaceReplaces.Descendants().Where(d => d.Attribute("CRS")?.Value == "2").Single();
                xInfoReplaceStation.Attribute("templateValueID").Value = baseCID + "100";

                XElement xInfoReplaceTracks = xInfoReplaceReplaces.Descendants().Where(d => d.Attribute("CRS")?.Value == "3").Single();

                XElement xInfoReplaceBlockID = xInfoReplaceReplaces.Descendants().Where(d => d.Attribute("CRS")?.Value == "4").Single();
                xInfoReplaceBlockID.Value = baseCID + "200";

                XElement xInfoReplaceBlockTracks = xInfoReplaceReplaces.Descendants().Where(d => d.Attribute("CRS")?.Value == "5").Single();
                
                XElement xInfoReplaceSoundInfoEvents = xInfoReplaceReplaces.Descendants().Where(d => d.Attribute("CRS")?.Value == "6").Single();
                XElement xInfoReplaceSoundInfoSoundBanks = xInfoReplaceReplaces.Descendants().Where(d => d.Attribute("CRS")?.Value == "7").Single();
                XElement xInfoReplaceSoundInfoMem = xInfoReplaceReplaces.Descendants().Where(d => d.Attribute("CRS")?.Value == "8").Single();

                for (int i = 0; i < musicFiles.Count; i++)
                {
                    string bnkID = "";
                    string wemID = "";
                    string shortID = "";

                    uint[] samplebid = null;
                    if (gameType == GameType.FarCry5) samplebid = sampleBNKIDs;
                    if (gameType == GameType.FarCryNewDawn) samplebid = sampleBNKIDs_ND;
                    if (gameType == GameType.FarCry6) samplebid = sampleBNKIDs_6;

                    XElement xBNKReplace = new("Replace");

                    for (int a = 0; a < samplebid.Length; a++)
                    {
                        string newID = baseID + a.ToString("00") + i.ToString("00");

                        if (a == 0)
                            bnkID = newID;

                        if (a == 1)
                            wemID = newID;

                        if (a == 2)
                            shortID = newID;

                        string search = samplebid[a].ToString();
                        string replace = newID.ToString();

                        xBNKReplace.Add(new XElement("Replace", new XAttribute("find", search), new XAttribute("replace", replace), new XAttribute("type", "UInt32")));
                    }

                    xBNKReplace.Add(new XAttribute("RequiredFile", "soundbinary\\" + bnkID + ".bnk"));

                    float[] vols = null;
                    if (gameType == GameType.FarCry5) vols = bnkVolumes;
                    if (gameType == GameType.FarCryNewDawn) vols = bnkVolumes_nd;
                    if (gameType == GameType.FarCry6) vols = bnkVolumes_6;

                    int volDes = 0;
                    if (gameType == GameType.FarCry5) volDes = 8;
                    if (gameType == GameType.FarCryNewDawn) volDes = 8;
                    if (gameType == GameType.FarCry6) volDes = 6;

                    for (int a = 0; a < vols.Length; a++)
                    {
                        string search = vols[a].ToString();
                        string replace = (musicFilesVolume[i] + (vols[a] + volDes)).ToString();

                        xBNKReplace.Add(new XElement("Replace", new XAttribute("find", search), new XAttribute("replace", replace), new XAttribute("type", "Float32")));
                    }

                    xInfoReplaceReplaces.Add(xBNKReplace);


                    string minT;
                    string maxT;
                    string minNT = "";
                    string maxNT = "";

                    if (musicFilesCond[i] == "night")
                    {
                        minT = "19";
                        maxT = "23";
                        minNT = "0";
                        maxNT = "5";
                    }
                    else
                    {
                        string[] aa = musicFilesCond[i].Split('-');
                        minT = aa[0];
                        maxT = aa[1];
                    }

                    xInfoReplaceTracks.Add(new XElement("template", new XAttribute("id", "CFCXRadioTrack"), new XAttribute("templateValueID", baseCID + "3" + i.ToString("00")), new XAttribute("templateValueBNK", bnkID), new XAttribute("templateValueMinT", minT), new XAttribute("templateValueMaxT", maxT)));

                    xInfoReplaceBlockTracks.Add(new XElement("template", new XAttribute("id", "Track"), new XAttribute("templateValueID", baseCID + "3" + i.ToString("00"))));

                    if (minNT != "")
                    {
                        xInfoReplaceTracks.Add(new XElement("template", new XAttribute("id", "CFCXRadioTrack"), new XAttribute("templateValueID", baseCID + "4" + i.ToString("00")), new XAttribute("templateValueBNK", bnkID), new XAttribute("templateValueMinT", minNT), new XAttribute("templateValueMaxT", maxNT)));

                        xInfoReplaceBlockTracks.Add(new XElement("template", new XAttribute("id", "Track"), new XAttribute("templateValueID", baseCID + "4" + i.ToString("00"))));
                    }

                    if (gameType == GameType.FarCry6)
                    {
                        XElement infoEvent = new XElement("Event", new XAttribute("ShortID", shortID), new XAttribute("SoundBankID", bnkID), new XAttribute("Priority", "0"), new XAttribute("MemoryNodeId", "0"), new XAttribute("MaxRadius", "0"), new XAttribute("Unknown", "0"), new XAttribute("Duration", musicFilesLength[i].ToString()), new XAttribute("Unknown2", "153"), new XAttribute("addNode", "1"));
                        infoEvent.Add(new XElement("Streams", new XElement("Stream", "soundbinary\\" + wemID + ".wem")));

                        xInfoReplaceSoundInfoEvents.Add(infoEvent);
                        xInfoReplaceSoundInfoSoundBanks.Add(new XElement("Bank", new XAttribute("ShortID", bnkID), new XAttribute("Unknown", "2139062143"), new XAttribute("bnkFileName", "soundbinary\\" + bnkID + ".bnk"), new XAttribute("addNode", "1")));
                        xInfoReplaceSoundInfoMem.Add(new XElement("MemoryNodeAssociation", new XAttribute("SoundBankID", bnkID), new XAttribute("MemoryNodeID", "5"), new XAttribute("addNode", "1")));
                    }
                    else
                    {
                        xInfoReplaceSoundInfoEvents.Add(new XElement("Event", new XAttribute("ShortID", shortID), new XAttribute("SoundBankID", bnkID), new XAttribute("Priority", "100"), new XAttribute("MemoryNodeId", "22"), new XAttribute("MaxRadius", "220"), new XAttribute("Unknown", "144"), new XAttribute("Duration", musicFilesLength[i].ToString()), new XAttribute("addNode", "1")));
                        xInfoReplaceSoundInfoSoundBanks.Add(new XElement("Bank", new XAttribute("ShortID", bnkID), new XAttribute("Unknown", "2139062143"), new XAttribute("bnkFileName", "soundbinary\\" + bnkID + ".bnk"), new XAttribute("addNode", "1")));
                    }


                    XElement xPair = new("Pair");
                    xPair.Add(new XElement("Source", Path.GetFileName(musicFiles[i])));
                    xPair.Add(new XElement("Target", "soundbinary/" + wemID + ".wem"));
                    xInfoPairs.Add(xPair);

                    XElement xPair2 = new("Pair");
                    xPair2.Add(new XElement("Source", "source.bnk"));
                    xPair2.Add(new XElement("Target", "soundbinary/" + bnkID + ".bnk"));
                    xInfoPairs.Add(xPair2);

                    zip.CreateEntryFromFile(musicFiles[i], Path.GetFileName(musicFiles[i]));
                }

                string b = "";
                if (gameType == GameType.FarCry5) b = "sample.bnk";
                if (gameType == GameType.FarCryNewDawn) b = "sample_nd.bnk";
                if (gameType == GameType.FarCry6) b = "sample_6.bnk";

                Stream streamBNK = GetFromResourceData(b);
                ZipArchiveEntry zipBNK = zip.CreateEntry("source.bnk");
                using (Stream entryBNK = zipBNK.Open())
                {
                    streamBNK.CopyTo(entryBNK);
                };

                MemoryStream ms = new();
                xInfo.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);

                ZipArchiveEntry zipInfo = zip.CreateEntry("info.xml");
                using (Stream entryStream = zipInfo.Open())
                {
                    ms.CopyTo(entryStream);
                };

                MemoryStream ms2 = new();
                xReplaceXML.Save(ms2);
                ms2.Seek(0, SeekOrigin.Begin);

                ZipArchiveEntry zipInfo2 = zip.CreateEntry("info_replace.xml");
                using (Stream entryStream2 = zipInfo2.Open())
                {
                    ms2.CopyTo(entryStream2);
                };

                string pic = "";
                if (gameType == GameType.FarCry5) pic = "hdr.jpg";
                if (gameType == GameType.FarCryNewDawn) pic = "hdr_nd.jpg";
                if (gameType == GameType.FarCry6) pic = "hdr_6.jpg";

                Stream stream3 = GetFromResourceData(pic);
                ZipArchiveEntry zipInfo3 = zip.CreateEntry("hdr.jpg");
                using (Stream entryStream3 = zipInfo3.Open())
                {
                    stream3.CopyTo(entryStream3);
                };

                zip.Dispose();

                MessageBox.Show("Successfuly saved!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem eachItem in listView1.SelectedItems)
            {
                musicFiles.RemoveAt(eachItem.Index);
                musicFilesVolume.RemoveAt(eachItem.Index);
                musicFilesLength.RemoveAt(eachItem.Index);
                musicFilesNames.RemoveAt(eachItem.Index);
                listView1.Items.Remove(eachItem);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Title = "Select music in WEM format";
            openFileDialog.Filter = "Wwise Encoded Media file|*.wem";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (string file in openFileDialog.FileNames)
                {
                    AddMusic(file);
                }
            }
        }

        private void AddMusic(string file, float volume = -8, int length = 1, string name = null, string cond = null)
        {
            if (!File.Exists(file))
            {
                MessageBox.Show("File " + file + " doesn't exist.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (length == 1)
            {
                byte[] bytes = File.ReadAllBytes(file);
                MemoryStream memoryStream = new(bytes);

                byte[] buffer = new byte[sizeof(uint)];
                memoryStream.Read(buffer, 0, sizeof(uint));
                uint grp = BitConverter.ToUInt32(buffer);

                if (grp != 0x46464952)
                {
                    MessageBox.Show("Selected file is not format of Wwise Encoded Media!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                memoryStream.Seek(28, SeekOrigin.Begin);

                buffer = new byte[sizeof(uint)];
                memoryStream.Read(buffer, 0, sizeof(uint));
                uint bytesPerSec = BitConverter.ToUInt32(buffer);

                length = (int)Math.Floor((double)(bytes.Length / bytesPerSec));
            }

            if (!file.EndsWith(".wem"))
            {
                MessageBox.Show("Selected file is not format of Wwise Encoded Media!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string fname = name ?? Path.GetFileNameWithoutExtension(file);

            if (cond == null) cond = "0-23";

            ListViewItem listViewItem = new();
            listViewItem.Text = fname;
            listViewItem.SubItems.Add(volume.ToString());
            listViewItem.SubItems.Add(length.ToString());
            listViewItem.SubItems.Add(TextCond(cond));
            listView1.Items.Add(listViewItem);

            musicFiles.Add(file);
            musicFilesVolume.Add(volume);
            musicFilesLength.Add(length);
            musicFilesNames.Add(fname);
            musicFilesCond.Add(cond);
        }

        private string TextCond(string cond)
        {
            if (cond == "night")
            {
                return "Only during night time";
            }
            else
            {
                string[] aa = cond.Split('-');
                return "From " + aa[0] + "h to " + aa[1] + "h";
            }
        }

        private static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                AddMusic(file);
            }
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var senderList = (ListView)sender;
            var clickedItem = senderList.HitTest(e.Location).Item;
            if (clickedItem != null)
            {
                Form2 form2 = new();
                form2.MusicLength = musicFilesLength[clickedItem.Index];
                form2.MusicVolume = (int)musicFilesVolume[clickedItem.Index];
                form2.MusicName = musicFilesNames[clickedItem.Index];

                if (musicFilesCond[clickedItem.Index] == "night")
                    form2.MusicCondNight = true;
                else
                {
                    form2.MusicCondNight = false;

                    string[] aa = musicFilesCond[clickedItem.Index].Split('-');
                    form2.MusicCondMin = int.Parse(aa[0]);
                    form2.MusicCondMax = int.Parse(aa[1]);
                }

                if (form2.ShowDialog() == DialogResult.OK)
                {
                    int volume = form2.MusicVolume;
                    int length = form2.MusicLength;
                    string name = form2.MusicName;
                    string cond;

                    if (form2.MusicCondNight)
                        cond = "night";
                    else
                    {
                        cond = form2.MusicCondMin + "-" + form2.MusicCondMax;
                    }

                    clickedItem.SubItems[0].Text = name;
                    clickedItem.SubItems[1].Text = volume.ToString();
                    clickedItem.SubItems[2].Text = length.ToString();
                    clickedItem.SubItems[3].Text = TextCond(cond);

                    musicFilesLength[clickedItem.Index] = length;
                    musicFilesVolume[clickedItem.Index] = volume;
                    musicFilesNames[clickedItem.Index] = name;
                    musicFilesCond[clickedItem.Index] = cond;
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            /*if (musicFiles.Count > 0)
            {
                if (MessageBox.Show("Close the app? All set values will be lost. If you want change params of music files in future, you need to edit the package directly.", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                }
                else
                {
                    e.Cancel = true;
                    this.Activate();
                }
            }*/

            XDocument xSave = new(new XDeclaration("1.0", "utf-8", "yes"));
            XElement files = new("Files");
            for (int i = 0; i < musicFiles.Count; i++)
            {
                files.Add(new XElement("Music", new XAttribute("FileName", musicFiles[i]), new XAttribute("Name", musicFilesNames[i]), new XAttribute("Volume", musicFilesVolume[i].ToString()), new XAttribute("Length", musicFilesLength[i].ToString()), new XAttribute("Condition", musicFilesCond[i])));
            }
            xSave.Add(files);
            xSave.Save(m_Path + "\\CustomRadioStation.xml");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            using var processModule = Process.GetCurrentProcess().MainModule;
            m_Path = Path.GetDirectoryName(processModule?.FileName);

            if (File.Exists(m_Path + "\\CustomRadioStation.xml"))
            {
                XDocument xSave = XDocument.Load(m_Path + "\\CustomRadioStation.xml");
                IEnumerable<XElement> files = xSave.Element("Files").Elements("Music");
                foreach (XElement music in files)
                {
                    AddMusic(music.Attribute("FileName").Value, float.Parse(music.Attribute("Volume").Value), int.Parse(music.Attribute("Length").Value), music.Attribute("Name")?.Value, music.Attribute("Condition")?.Value);
                }
            }

            Form3 form3 = new();
            if (form3.ShowDialog() == DialogResult.OK)
            {
                gameType = form3.GameType;
            }
            else
                Environment.Exit(0);

            if (gameType == GameType.FarCry5) Text = "FC5" + " " + appName;
            if (gameType == GameType.FarCryNewDawn) Text = "FCND" + " " + appName;
            if (gameType == GameType.FarCry6) Text = "FC6" + " " + appName;

            string pic = "";
            if (gameType == GameType.FarCry5) pic = "hdr.jpg";
            if (gameType == GameType.FarCryNewDawn) pic = "hdr_nd.jpg";
            if (gameType == GameType.FarCry6) pic = "hdr_6.jpg";

            pictureBox1.Image = System.Drawing.Image.FromStream(GetFromResourceData(pic));
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Control)
            {
                listView1.MultiSelect = true;
                foreach (ListViewItem item in listView1.Items)
                {
                    item.Selected = true;
                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var ps = new ProcessStartInfo("https://youtu.be/O5UxULDT8_4")
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);
        }

        private Stream GetFromResourceData(string file)
        {
            Assembly assembly = GetType().Assembly;
            Stream stream = assembly.GetManifestResourceStream(assembly.GetName().Name + ".data.zip");

            ZipArchive zipArchive = new(stream);
            ZipArchiveEntry zipArchiveEntry = zipArchive.Entries.Where(a => a.FullName == file).FirstOrDefault();

            return zipArchiveEntry.Open();
        }
    }
}
