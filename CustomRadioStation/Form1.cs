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
        public static string appName = "Custom Radio Station v1.10 by ArmanIII";
        GameType gameType;
        string m_Path = ""; // Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        readonly List<string> musicFiles = new();
        readonly List<float> musicFilesVolume = new();
        readonly List<int> musicFilesLength = new();
        readonly List<string> musicFilesNames = new();
        readonly List<string> musicFilesCond = new();
        readonly string[] bnkIDs = new string[] {
            "100001",
            "110001",
            "120001",
            "130001",
            "140001",
            "150001",
            "160001",
            "170001",
            "180001",
            "190001",
        };
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
            1678687147, // BNK ID
            624637911, // WEM ID
            3185078914, // Short ID
            106380378,
            320299941,
            630521720,
            1043557427,
            1937429806,
            2187133148,
            1056378868,
        };
        readonly float[] bnkVolumes = new float[] { -8, -10, -11, -13 };
        readonly float[] bnkVolumes_nd = new float[] { -5, -10, 5, -8, -7 };
        readonly float[] bnkVolumes_6 = new float[] { -6 };
        readonly string[] replaceIDRange = new string[] {
            "9017777777770",
            "9017777777771",
            "9017777777772",
            "9017777777773",
            "9017777777774",
            "9017777777775",
            "9017777777776",
            "9017777777777",
            "9017777777778",
            "9017777777779",
        };
        List<uint> selectedBNKIDs = new();
        List<uint> selectedWEMIDs = new();
        List<uint> selectedShortBNKIDs = new();
        readonly List<uint> allBNKIDs = new();
        readonly string outputFileName = "Custom Radio Station.a3";
        readonly string desc = 
            "[img]hdr.jpg[/img]" + Environment.NewLine + Environment.NewLine +
            "Adds custom radio station to the game." + Environment.NewLine + Environment.NewLine +
            "This package has set station to: [color=::main:color::][b]:stationnum:[/b][/color]" + Environment.NewLine + Environment.NewLine + 
            "[size=2][color=::main:color::][b]List of music[/b][/color][/size]" + Environment.NewLine + ":files:" + Environment.NewLine + Environment.NewLine +
            "The radio station is available right after begin of the game, no other actions is required.";

        readonly string[] freqs = new string[] {
            "91.5 MHz",
            "92.7 MHz",
            "94.3 MHz",
            "96.5 MHz",
            "97.8 MHz",
            "99.1 MHz",
            "101.5 MHz",
            "102.3 MHz",
            "104.0 MHz",
            "105.7 MHz",
        };

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

                string d = desc.Replace(":files:", fls);
                d = d.Replace(":stationnum:", numericUpDown1.Value.ToString() + " (" + freqs[(int)numericUpDown1.Value - 1] + ")");

                string game = "";
                if (gameType == GameType.FarCry5) game = "FC5";
                if (gameType == GameType.FarCryNewDawn) game = "FCND";
                if (gameType == GameType.FarCry6) game = "FC6";

                XDocument xInfoXML = new(new XDeclaration("1.0", "utf-8", "yes"));
                XElement xInfo = new("PackageInfo");
                xInfo.Add(new XElement("DefaultInclude", "false"));
                xInfo.Add(new XElement("Games", new XElement("Game", game)));
                xInfo.Add(new XElement("Name", "Custom Radio Station"));
                xInfo.Add(new XElement("Description", d));
                XElement xPairs = new("Pairs");

                for (int i = 0; i < musicFiles.Count; i++)
                {
                    string bnkSel = "";
                    if (gameType == GameType.FarCry5) bnkSel = "sample.bnk";
                    if (gameType == GameType.FarCryNewDawn) bnkSel = "sample_nd.bnk";
                    if (gameType == GameType.FarCry6) bnkSel = "sample_6.bnk";

                    Stream streamBNK = GetFromResourceData(bnkSel);
                    MemoryStream fileStream = new();
                    streamBNK.CopyTo(fileStream);
                    byte[] bytes = fileStream.ToArray();

                    string selectedBNKIDRange = bnkIDs[(int)numericUpDown1.Value - 1];

                    uint[] samplebid = null;
                    if (gameType == GameType.FarCry5) samplebid = sampleBNKIDs;
                    if (gameType == GameType.FarCryNewDawn) samplebid = sampleBNKIDs_ND;
                    if (gameType == GameType.FarCry6) samplebid = sampleBNKIDs_6;

                    for (int a = 0; a < samplebid.Length; a++)
                    {
                        uint newBNKID = TryBNKIDFree(uint.Parse(selectedBNKIDRange + a.ToString("00") + i.ToString("00")));

                        if (a == 0)
                            selectedBNKIDs.Add(newBNKID);

                        if (a == 1)
                            selectedWEMIDs.Add(newBNKID);

                        if (a == 2)
                            selectedShortBNKIDs.Add(newBNKID);

                        byte[] search = BitConverter.GetBytes(samplebid[a]);
                        byte[] replace = BitConverter.GetBytes(newBNKID);

                        int[] poses = SearchBytesMultiple(bytes, search);

                        foreach (int pos in poses)
                        {
                            fileStream.Position = pos;
                            fileStream.Write(replace, 0, replace.Length);
                        }
                    }

                    float[] vols = null;
                    if (gameType == GameType.FarCry5) vols = bnkVolumes;
                    if (gameType == GameType.FarCryNewDawn) vols = bnkVolumes_nd;
                    if (gameType == GameType.FarCry6) vols = bnkVolumes_6;

                    for (int a = 0; a < vols.Length; a++)
                    {
                        byte[] search = BitConverter.GetBytes(vols[a]);
                        byte[] replace = BitConverter.GetBytes(musicFilesVolume[i] + (vols[a] + 8));

                        int[] poses = SearchBytesMultiple(bytes, search);

                        foreach (int pos in poses)
                        {
                            fileStream.Position = pos;
                            fileStream.Write(replace, 0, replace.Length);
                        }
                    }

                    fileStream.Seek(0, SeekOrigin.Begin);

                    XElement xPair = new("Pair");
                    xPair.Add(new XElement("Source", Path.GetFileName(musicFiles[i])));
                    xPair.Add(new XElement("Target", "soundbinary/" + selectedWEMIDs[i] + ".wem"));
                    xPairs.Add(xPair);

                    XElement xPair2 = new("Pair");
                    xPair2.Add(new XElement("Source", Path.GetFileNameWithoutExtension(musicFiles[i]) + ".bnk"));
                    xPair2.Add(new XElement("Target", "soundbinary/" + selectedBNKIDs[i] + ".bnk"));
                    xPairs.Add(xPair2);

                    zip.CreateEntryFromFile(musicFiles[i], Path.GetFileName(musicFiles[i]));

                    ZipArchiveEntry zipBNK = zip.CreateEntry(Path.GetFileNameWithoutExtension(musicFiles[i]) + ".bnk");
                    using (Stream entryStream = zipBNK.Open())
                    {
                        fileStream.CopyTo(entryStream);
                    };
                }

                /*XElement XPairF1 = new("Pair");
                XPairF1.Add(new XElement("FromDatFile", @"patch.fat"));
                XPairF1.Add(new XElement("Source", @"databases\generic\radiostations.ndb"));
                XPairF1.Add(new XElement("Target", @"databases\generic\radiostations.ndb"));
                xPairs.Add(XPairF1);

                XElement XPairF2 = new("Pair");
                XPairF2.Add(new XElement("FromDatFile", @"\worlds\installpkg.fat"));
                XPairF2.Add(new XElement("Source", @"databases\generic\radiotracks.ndb"));
                XPairF2.Add(new XElement("Target", @"databases\generic\radiotracks.ndb"));
                xPairs.Add(XPairF2);*/

                xInfo.Add(xPairs);
                xInfoXML.Add(xInfo);

                MemoryStream ms = new();
                xInfoXML.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);

                ZipArchiveEntry zipInfo = zip.CreateEntry("info.xml");
                using (Stream entryStream = zipInfo.Open())
                {
                    ms.CopyTo(entryStream);
                };


                string selectedID = replaceIDRange[(int)numericUpDown1.Value - 1];

                XElement xReplaces = new("Replaces");
                xReplaces.Add(new XElement("Replace", new XAttribute("RequiredFile", @"databases\generic\radiostations.ndb"), new XElement("object", new XAttribute("hash", "1F027DA1"), new XElement("template", new XAttribute("id", "CFCXRadioStation"), new XAttribute("templateValueID", selectedID + "100"), new XAttribute("templateValueBlockID", selectedID + "200")))));
                xReplaces.Add(new XElement("Replace", new XAttribute("RequiredFile", @"generated\nomadobjecttemplates_rt.fcb"), new XElement("object", new XAttribute("hash", "772EA8B4"), new XElement("template", new XAttribute("id", "Station"), new XAttribute("templateValueID", selectedID + "100")))));

                XElement rBlocks = new("Replace", new XAttribute("RequiredFile", @"databases\generic\radioblocks.ndb"));
                rBlocks.Add(new XElement("object", new XAttribute("hash", "1F027DA1"), new XElement("template", new XAttribute("id", "CFCXRadioBlock"), new XAttribute("templateValueID", selectedID + "200"))));

                string rbID = ByteArrayToString(BitConverter.GetBytes(ulong.Parse(selectedID + "200"))).ToUpper();
                XElement rBlocksTracks = new("object", new XAttribute("hash", "59F2984F"), new XElement("primaryKey", rbID, new XAttribute("hash", "8EDB0295")));
                XElement rBlocksTracksSub = new("object", new XAttribute("hash", "AF3F66FA"));

                XElement rTracks = new("Replace", new XAttribute("RequiredFile", @"databases\generic\radiotracks.ndb"));
                XElement rTracksOb = new("object", new XAttribute("hash", "1F027DA1"));

                XElement rInfo = new("Replace", new XAttribute("RequiredFile", @"soundbinary\soundinfo.bin"));
                XElement rInfoEvents = new("Events");
                XElement rInfoSoundBanks = new("SoundBanks");

                for (int i = 0; i < musicFiles.Count; i++)
                {
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

                    string bnkID = selectedBNKIDs[i].ToString();
                    string bnkShortID = selectedShortBNKIDs[i].ToString();
                    rTracksOb.Add(new XElement("template", new XAttribute("id", "CFCXRadioTrack"), new XAttribute("templateValueID", selectedID + "3" + i.ToString("00")), new XAttribute("templateValueBNK", bnkID), new XAttribute("templateValueMinT", minT), new XAttribute("templateValueMaxT", maxT)));

                    rBlocksTracksSub.Add(new XElement("template", new XAttribute("id", "Track"), new XAttribute("templateValueID", selectedID + "3" + i.ToString("00"))));

                    if (minNT != "")
                    {
                        rTracksOb.Add(new XElement("template", new XAttribute("id", "CFCXRadioTrack"), new XAttribute("templateValueID", selectedID + "4" + i.ToString("00")), new XAttribute("templateValueBNK", bnkID), new XAttribute("templateValueMinT", minNT), new XAttribute("templateValueMaxT", maxNT)));

                        rBlocksTracksSub.Add(new XElement("template", new XAttribute("id", "Track"), new XAttribute("templateValueID", selectedID + "4" + i.ToString("00"))));
                    }

                    if (gameType == GameType.FarCry6)
                    {
                        rInfoEvents.Add(new XElement("Event", new XAttribute("ShortID", bnkShortID), new XAttribute("SoundBankID", bnkID), new XAttribute("Priority", "100"), new XAttribute("MemoryNodeId", "22"), new XAttribute("MaxRadius", "220"), new XAttribute("Unknown", "144"), new XAttribute("Duration", musicFilesLength[i].ToString()), new XAttribute("addNode", "1")));
                        rInfoSoundBanks.Add(new XElement("Bank", new XAttribute("ShortID", bnkID), new XAttribute("Unknown", "2139062143"), new XAttribute("bnkFileName", "soundbinary\\" + bnkID + ".bnk"), new XAttribute("addNode", "1")));
                    
                    
                    
                    
                    
                    }
                    else
                    {
                        rInfoEvents.Add(new XElement("Event", new XAttribute("ShortID", bnkShortID), new XAttribute("SoundBankID", bnkID), new XAttribute("Priority", "100"), new XAttribute("MemoryNodeId", "22"), new XAttribute("MaxRadius", "220"), new XAttribute("Unknown", "144"), new XAttribute("Duration", musicFilesLength[i].ToString()), new XAttribute("addNode", "1")));
                        rInfoSoundBanks.Add(new XElement("Bank", new XAttribute("ShortID", bnkID), new XAttribute("Unknown", "2139062143"), new XAttribute("bnkFileName", "soundbinary\\" + bnkID + ".bnk"), new XAttribute("addNode", "1")));
                    }
                }
                rTracks.Add(rTracksOb);
                xReplaces.Add(rTracks);

                rBlocksTracks.Add(rBlocksTracksSub);
                rBlocks.Add(rBlocksTracks);
                xReplaces.Add(rBlocks);

                rInfo.Add(rInfoEvents);
                rInfo.Add(rInfoSoundBanks);
                xReplaces.Add(rInfo);

                Stream stream = GetFromResourceData("info_replace.xml");
                XDocument xReplaceXML = XDocument.Load(stream);
                xReplaceXML.Element("PackageInfoReplace").Add(xReplaces);

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

                Stream stream2 = GetFromResourceData(pic);
                ZipArchiveEntry zipInfo3 = zip.CreateEntry("hdr.jpg");
                using (Stream entryStream3 = zipInfo3.Open())
                {
                    stream2.CopyTo(entryStream3);
                };

                zip.Dispose();

                selectedBNKIDs = new();
                selectedWEMIDs = new();
                selectedShortBNKIDs = new();

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

        private static int SearchBytes(byte[] haystack, byte[] needle, int start_index)
        {
            int len = needle.Length;
            int limit = haystack.Length - len;
            for (int i = start_index; i <= limit; i++)
            {
                int k = 0;
                for (; k < len; k++)
                {
                    if (needle[k] != haystack[i + k]) break;
                }
                if (k == len) return i;
            }
            return -1;
        }

        private static int[] SearchBytesMultiple(byte[] haystack, byte[] needle)
        {
            int index = 0;

            List<int> results = new();

            while (true)
            {
                index = SearchBytes(haystack, needle, index);

                if (index == -1)
                    break;

                results.Add(index);

                index += needle.Length;
            }

            return results.ToArray();
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
            XElement files = new("Files", new XAttribute("StationNumber", numericUpDown1.Value.ToString()));
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

                XAttribute xAttribute = xSave.Element("Files").Attribute("StationNumber");
                if (xAttribute != null)
                    numericUpDown1.Value = decimal.Parse(xAttribute.Value);
            }

            Form3 form3 = new();
            if (form3.ShowDialog() == DialogResult.OK)
            {
                gameType = form3.GameType;
            }
            else
                Environment.Exit(0);

            Text = (gameType == GameType.FarCryNewDawn ? "FCND" : "FC5") + " " + appName;

            pictureBox1.Image = System.Drawing.Image.FromStream(GetFromResourceData(gameType == GameType.FarCryNewDawn ? "hdr_nd.jpg" : "hdr.jpg"));

            StreamReader streamReader = new(GetFromResourceData(gameType == GameType.FarCryNewDawn ? "bnk_id_list_nd.txt" : "bnk_id_list.txt"));
            do
            {
                allBNKIDs.Add(uint.Parse(streamReader.ReadLine()));
            }
            while (!streamReader.EndOfStream);
            streamReader.Close();
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

        private uint TryBNKIDFree(uint id)
        {
            if (allBNKIDs.Contains(id))
            {
                id += 10000;
                return TryBNKIDFree(id);
            }

            allBNKIDs.Add(id);

            return id;
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
