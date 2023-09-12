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

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;

namespace CustomRadioStation
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            AddHandler(DragDrop.DropEvent, listView1_DragDrop);
            AddHandler(DragDrop.DragEnterEvent, listView1_DragEnter);
        }

        private void Animation(bool fadeInOut, Grid grid)
        {
            foreach (var ch in grid.GetVisualDescendants().OfType<Button>())
            {
                if (fadeInOut)
                    (ch as Button).IsEnabled = true;
                else
                    (ch as Button).IsEnabled = false;
            }

            if (fadeInOut)
            {
                grid.IsVisible = true;
            }

            grid.Opacity = fadeInOut ? 1 : 0;

            if (!fadeInOut)
            {
                Timer aTimer = new Timer(300);
                aTimer.Enabled = true;
                aTimer.Elapsed += (object source, ElapsedEventArgs e) =>
                {
                    aTimer.Stop();
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        grid.IsVisible = false;
                    });
                };
            }
        }

        private void OpenInfoDialog(string name, string val)
        {
            dialogInfoName.Content = name;
            dialogInfoLabel.Text = val;
            Animation(true, gridDialogInfo);
        }

        private void ShowNotice(string text)
        {
            noticeNote.Content = text;

            gridNoticeNote.Opacity = 1;

            Timer aTimer = new Timer(5000);
            aTimer.Enabled = true;
            aTimer.Elapsed += (object source, ElapsedEventArgs e) =>
            {
                aTimer.Stop();
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    gridNoticeNote.Opacity = 0;
                });
            };
        }

        private void ButtonChrome_Click(object sender, RoutedEventArgs e)
        {
            string tag = (string)((Button)sender).Tag;

            if (tag == "0")
            {
                Close();
            }
            if (tag == "2")
            {
                WindowState = WindowState.Minimized;
            }
        }

        private void ButtonDialogAskClose_Click(object sender, RoutedEventArgs e)
        {
            string tag = (string)((Button)sender).Tag;

            if (tag == "0")
            {
            }

            if (tag == "1")
            {
            }

            Animation(false, gridDialogAsk);
        }

        private void ButtonDialogInfoClose_Click(object sender, RoutedEventArgs e)
        {
            Animation(false, gridDialogInfo);
        }

        private void MoveWnd(object sender, PointerPressedEventArgs e)
        {
            var props = e.GetCurrentPoint(this).Properties;
            if (props.IsLeftButtonPressed && e.ClickCount == 1)
            {
                Cursor = new Cursor(StandardCursorType.SizeAll);
                BeginMoveDrag(e);
                Cursor = new Cursor(StandardCursorType.Arrow);
            }
        }

        // *************************************************************************************

        public static char DS = Path.DirectorySeparatorChar;

        public static string version = "v3.00";
        public static string appName = "Custom Radio Station " + version + " by ArmanIII";
        public static string createdBy = "Package created using " + appName;
        GameType gameType = GameType.Invalid;
        string m_Path = ""; // Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        /*readonly List<string> musicFiles = new();
        readonly List<float> musicFilesVolume = new();
        readonly List<int> musicFilesLength = new();
        readonly List<string> musicFilesNames = new();
        readonly List<string> musicFilesCond = new();*/
        readonly ObservableCollection<ListEntry> entries = new();
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            /*List<ListEntry> list = new List<ListEntry>();
            for (int i = 0; i < 100; i++)
                list.Add(new() { FileName = "File Name", Volume = "-5", Duration = "158", Condition = "0" });
            mainList.ItemsSource = list;*/
            
            m_Path = Path.GetDirectoryName(AppContext.BaseDirectory);
            
            wndTitle.Content = Title = appName;
            verLabel.Content = version;

            mainList.ItemsSource = entries;
        }

        private void Window_Closing(object sender, WindowClosingEventArgs e)
        {
            if (gameType != GameType.Invalid)
            {
                XDocument xSave = new(new XDeclaration("1.0", "utf-8", "yes"));
                XElement files = new("Files");
                for (int i = 0; i < entries.Count; i++)
                {
                    files.Add(new XElement("Music", new XAttribute("FileName", entries[i].FileName), new XAttribute("Name", entries[i].Name), new XAttribute("Volume", entries[i].Volume.ToString()), new XAttribute("Duration", entries[i].Duration.ToString()), new XAttribute("Condition", entries[i].Condition)));
                }
                xSave.Add(files);
                xSave.Save(m_Path + DS + "CustomRadioStation.xml");
            }
        }

        private void buttonSelGame_Click(object sender, RoutedEventArgs e)
        {
            string tag = (string)((Button)sender).Tag;

            string picName = "";

            if (tag == "0")
            {
                gameType = GameType.FarCry5;
                picName = "hdr";
                Title = "FC5" + " " + appName;
            }

            if (tag == "1")
            {
                gameType = GameType.FarCryNewDawn;
                picName = "hdr_nd";
                Title = "FCND" + " " + appName;
            }

            if (tag == "2")
            {
                gameType = GameType.FarCry6;
                picName = "hdr_6";
                Title = "FC6" + " " + appName;
            }

            Stream stream = AssetLoader.Open(new Uri("avares://" + GetType().Assembly.GetName().Name + "/Resources/" + picName + ".jpg"));
            var bitmap = new Bitmap(stream);
            headerPic.Source = bitmap;
            
            wndTitle.Content = Title;

            if (File.Exists(m_Path + DS + "CustomRadioStation.xml"))
            {
                XDocument xSave = XDocument.Load(m_Path + DS + "CustomRadioStation.xml");
                IEnumerable<XElement> files = xSave.Element("Files").Elements("Music");
                foreach (XElement music in files)
                {
                    AddMusic(music.Attribute("FileName").Value, float.Parse(music.Attribute("Volume").Value), music.Attribute("Name")?.Value, music.Attribute("Condition")?.Value, int.Parse(music.Attribute("Duration").Value));
                }
            }

            Animation(false, selGameGrid);
            Animation(true, mainGrid);
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            if (entries.Count == 0)
            {
                OpenInfoDialog(this.Title, "There are no music files, can't create package!");
                return;
            }

            if (entries.Count > 900)
            {
                OpenInfoDialog(this.Title, "You have too much music files, remove some of them. Maximum count of musics per station is 900.");
                return;
            }

            FilePickerSaveOptions opts = new();
            opts.FileTypeChoices = new FilePickerFileType[] { new("Mod Installer package") { Patterns = new[] { "*.a3" } } };
            opts.SuggestedFileName = outputFileName;
            opts.Title = Title;
            
            IStorageFolder? folder = await StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Desktop);
            opts.SuggestedStartLocation = folder;

            var d = await StorageProvider.SaveFilePickerAsync(opts);

            if (d != null)
            {
                if (File.Exists(d.Path.LocalPath))
                    File.Delete(d.Path.LocalPath);

                ZipArchive zip = ZipFile.Open(d.Path.LocalPath, ZipArchiveMode.Create);

                string fls = "";
                foreach (var mf in entries)
                    fls += mf.Name + Environment.NewLine;

                string game = "";
                if (gameType == GameType.FarCry5) game = "FC5";
                if (gameType == GameType.FarCryNewDawn) game = "FCND";
                if (gameType == GameType.FarCry6) game = "FC6";

                Stream stream = GetFromResourceData("info.xml");
                XDocument xInfo = XDocument.Load(stream);
                xInfo.Element("PackageInfo").Element("Games").Element("Game").Value = game;
                string desc = xInfo.Element("PackageInfo").Element("Description").Value;
                desc = desc.Replace(":files:", fls);
                desc = desc.Replace(":cred:", createdBy);
                xInfo.Element("PackageInfo").Element("Description").Value = desc;

                //string baseID = "1{RadioStation}";
                string baseCID = "90177777777{RadioStation}";

                XElement xInfoComputeData = new("ComputeData");
                xInfo.Root.Add(xInfoComputeData);

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

                for (int i = 0; i < entries.Count; i++)
                {
                    string trackIDStr = i.ToString("000");
                    string cpBnkName = "{" + $"Track{trackIDStr}_BNK_Name" + "}";
                    //string cpInt1 = $"Track{trackIDStr}_BNK_Internal_1";
                    //string cpInt2 = $"Track{trackIDStr}_BNK_Internal_2";
                    string cpWemName = "{" + $"Track{trackIDStr}_WEM_Name" + "}";
                    string cpID = "{" + $"Track{trackIDStr}_ID" + "}";
                    List<string> cpIntIds = new();

                    uint[] samplebid = null;
                    if (gameType == GameType.FarCry5) samplebid = sampleBNKIDs;
                    if (gameType == GameType.FarCryNewDawn) samplebid = sampleBNKIDs_ND;
                    if (gameType == GameType.FarCry6) samplebid = sampleBNKIDs_6;

                    XElement xBNKReplace = new("Replace");

                    for (int a = 0; a < samplebid.Length; a++)
                    {
                        //string newID = baseID + a.ToString("00") + i.ToString("00");
                        string aa = "";

                        if (a == 0)
                            aa = cpBnkName;
                        else if (a == 1)
                            aa = cpWemName;
                        else if (a == 2)
                            aa = cpID;
                        else
                        {
                            aa = "{" + $"Track{trackIDStr}_BNK_Internal_{a}" + "}";
                            cpIntIds.Add(aa);
                        }

                        xBNKReplace.Add(new XElement("Replace", new XAttribute("find", samplebid[a].ToString()), new XAttribute("replace", aa), new XAttribute("type", "UInt32")));
                    }

                    xBNKReplace.Add(new XAttribute("RequiredFile", "soundbinary\\" + cpBnkName + ".bnk"));

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
                        string replace = (entries[i].Volume + (vols[a] + volDes)).ToString();

                        xBNKReplace.Add(new XElement("Replace", new XAttribute("find", search), new XAttribute("replace", replace), new XAttribute("type", "Float32")));
                    }

                    xInfoReplaceReplaces.Add(xBNKReplace);


                    string minT;
                    string maxT;
                    string minNT = "";
                    string maxNT = "";

                    if (entries[i].Condition == "night")
                    {
                        minT = "19";
                        maxT = "23";
                        minNT = "0";
                        maxNT = "5";
                    }
                    else
                    {
                        string[] aa = entries[i].Condition.Split('-');
                        minT = aa[0];
                        maxT = aa[1];
                    }

                    xInfoReplaceTracks.Add(new XElement("template", new XAttribute("id", "CFCXRadioTrack"), new XAttribute("templateValueID", baseCID + "3" + i.ToString("00")), new XAttribute("templateValueBNK", cpBnkName), new XAttribute("templateValueMinT", minT), new XAttribute("templateValueMaxT", maxT)));

                    xInfoReplaceBlockTracks.Add(new XElement("template", new XAttribute("id", "Track"), new XAttribute("templateValueID", baseCID + "3" + i.ToString("00"))));

                    if (minNT != "")
                    {
                        xInfoReplaceTracks.Add(new XElement("template", new XAttribute("id", "CFCXRadioTrack"), new XAttribute("templateValueID", baseCID + "4" + i.ToString("00")), new XAttribute("templateValueBNK", cpBnkName), new XAttribute("templateValueMinT", minNT), new XAttribute("templateValueMaxT", maxNT)));

                        xInfoReplaceBlockTracks.Add(new XElement("template", new XAttribute("id", "Track"), new XAttribute("templateValueID", baseCID + "4" + i.ToString("00"))));
                    }

                    if (gameType == GameType.FarCry6)
                    {
                        XElement infoEvent = new XElement("Event", new XAttribute("ShortID", cpID), new XAttribute("SoundBankID", cpBnkName), new XAttribute("Priority", "0"), new XAttribute("MemoryNodeId", "0"), new XAttribute("MaxRadius", "0"), new XAttribute("Unknown", "0"), new XAttribute("Duration", entries[i].Duration.ToString()), new XAttribute("Unknown2", "153"), new XAttribute("addNode", "1"));
                        infoEvent.Add(new XElement("Streams", new XElement("Stream", "soundbinary\\" + cpWemName + ".wem")));

                        xInfoReplaceSoundInfoEvents.Add(infoEvent);
                        xInfoReplaceSoundInfoSoundBanks.Add(new XElement("Bank", new XAttribute("ShortID", cpBnkName), new XAttribute("Unknown", "2139062143"), new XAttribute("bnkFileName", "soundbinary\\" + cpBnkName + ".bnk"), new XAttribute("addNode", "1")));
                        xInfoReplaceSoundInfoMem.Add(new XElement("MemoryNodeAssociation", new XAttribute("SoundBankID", cpBnkName), new XAttribute("MemoryNodeID", "5"), new XAttribute("addNode", "1")));
                    }
                    else
                    {
                        xInfoReplaceSoundInfoEvents.Add(new XElement("Event", new XAttribute("ShortID", cpID), new XAttribute("SoundBankID", cpBnkName), new XAttribute("Priority", "100"), new XAttribute("MemoryNodeId", "22"), new XAttribute("MaxRadius", "220"), new XAttribute("Unknown", "144"), new XAttribute("Duration", entries[i].Duration.ToString()), new XAttribute("addNode", "1")));
                        xInfoReplaceSoundInfoSoundBanks.Add(new XElement("Bank", new XAttribute("ShortID", cpBnkName), new XAttribute("Unknown", "2139062143"), new XAttribute("bnkFileName", "soundbinary\\" + cpBnkName + ".bnk"), new XAttribute("addNode", "1")));
                    }


                    XElement xPair = new("Pair");
                    xPair.Add(new XElement("Source", Path.GetFileName(entries[i].FileName)));
                    xPair.Add(new XElement("Target", "soundbinary/" + cpWemName + ".wem"));
                    xInfoPairs.Add(xPair);

                    XElement xPair2 = new("Pair");
                    xPair2.Add(new XElement("Source", "source.bnk"));
                    xPair2.Add(new XElement("Target", "soundbinary/" + cpBnkName + ".bnk"));
                    xInfoPairs.Add(xPair2);

                    xInfoComputeData.Add(new XElement("Compute", new XAttribute("ID", cpBnkName.Replace("{", "").Replace("}", "")), new XAttribute("Value", "CRS_Track_" + trackIDStr + "_Station_{RadioStation}_Soundbank"), new XAttribute("Process", "WwiseFNV")));
                    xInfoComputeData.Add(new XElement("Compute", new XAttribute("ID", cpWemName.Replace("{", "").Replace("}", "")), new XAttribute("Value", "CRS_Track_" + trackIDStr + "_Station_{RadioStation}_Audio"), new XAttribute("Process", "WwiseMediaID")));
                    xInfoComputeData.Add(new XElement("Compute", new XAttribute("ID", cpID.Replace("{", "").Replace("}", "")), new XAttribute("Value", "CRS_Track_" + trackIDStr + "_Station_{RadioStation}"), new XAttribute("Process", "WwiseFNV")));
                    for (int k = 0; k < cpIntIds.Count; k++)
                        xInfoComputeData.Add(new XElement("Compute", new XAttribute("ID", cpIntIds[k].Replace("{", "").Replace("}", "")), new XAttribute("Value", "CRS_Track_" + trackIDStr + "_Station_{RadioStation}_Internal_" + k.ToString()), new XAttribute("Process", "WwiseFNV")));

                    zip.CreateEntryFromFile(entries[i].FileName, Path.GetFileName(entries[i].FileName));
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

                OpenInfoDialog(this.Title, "Successfuly saved!");
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (mainList.SelectedIndex != -1 && mainList.SelectedIndex < entries.Count)
                entries.RemoveAt(mainList.SelectedIndex);
        }

        private async void button3_Click(object sender, RoutedEventArgs e)
        {
            FilePickerOpenOptions opts = new();
            opts.AllowMultiple = true;
            opts.FileTypeFilter = new FilePickerFileType[] { new("Wwise Encoded Media file") { Patterns = new[] { "*.wem" } } };
            opts.Title = "Select music in WEM format";

            var d = await StorageProvider.OpenFilePickerAsync(opts);
            if (d != null && d.Count > 0)
            {
                foreach (var file in d)
                {
                    AddMusic(file.Path.LocalPath);
                }
            }
        }

        private void AddMusic(string file, float volume = -8, string name = null, string cond = null, int duration = -1)
        {
            if (!File.Exists(file))
            {
                OpenInfoDialog(this.Title, "File " + file + " doesn't exist.");
                return;
            }

            /*if (length == 1)
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
            }*/

            WEMFile wf = new();
            if (!wf.LoadWEM(file))
            {
                OpenInfoDialog(this.Title, $"Error occurred during loading WEM file:{file}{Environment.NewLine}{wf.ParseErrorStr}{Environment.NewLine}{Environment.NewLine}The error can be caused by wrong WEM encoding. Please convert WEM to correct encoding or select different WEM.");
                return;
            }

            if (wf.Channels > 1)
            {
                OpenInfoDialog(this.Title, $"Selected WEM {file} contains more than 1 channel. Recommended is to use single channel audio for better 3D sound.");
            }

            string fname = name ?? Path.GetFileNameWithoutExtension(file);

            if (cond == null) cond = "0-23";

            ListEntry listViewItem = new();
            listViewItem.Tooltip = fname + Environment.NewLine + wf.PrintInfo();
            listViewItem.FileName = file;
            listViewItem.Name = fname;
            listViewItem.Volume = volume;
            listViewItem.Duration = duration > 0 ? duration : wf.AudioLength;
            listViewItem.Condition = cond;
            listViewItem.ConditionR = TextCond(cond);
            entries.Add(listViewItem);
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

        private Stream GetFromResourceData(string file)
        {
            Stream stream = AssetLoader.Open(new Uri("avares://" + GetType().Assembly.GetName().Name + "/Resources/" + file));
            return stream;
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
            if (e.Data.Contains(DataFormats.Files))
            {
                var files = e.Data.GetFiles();

                foreach (var file in files)
                {
                    if (file is IStorageFile sf)
                    {
                        var ppp = sf.Path.LocalPath;
                        if (ppp != null)
                        {
                            AddMusic(ppp);
                        }
                    }
                }
            }
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.Files))
                e.DragEffects = DragDropEffects.Copy;
        }

        private void listView1_MouseDoubleClick(object sender, PointerPressedEventArgs e)
        {
            var props = e.GetCurrentPoint(mainList).Properties;

            if (e.ClickCount == 2 && props.IsLeftButtonPressed)
            {
                //var senderList = (ListBox)sender;
                //var clickedItem = senderList.InputHitTest(e.GetPosition(senderList));
                if (mainList.SelectedIndex != -1 && mainList.SelectedIndex < entries.Count)
                {
                    var entry = entries[mainList.SelectedIndex];

                    propName.Text = entry.Name;
                    propVolume.Value = (decimal)entry.Volume;
                    propLength.Value = entry.Duration;

                    if (entry.Condition == "night")
                        propOnlyNight.IsChecked = true;
                    else
                    {
                        propOnlyNight.IsChecked = false;

                        string[] aa = entry.Condition.Split('-');
                        propMinHour.Value = int.Parse(aa[0]);
                        propMaxHour.Value = int.Parse(aa[1]);
                    }

                    Animation(true, gridDialogProps);
                }
            }

            if (e.ClickCount == 1 && props.IsRightButtonPressed)
            {
                if (mainList.SelectedIndex != -1 && mainList.SelectedIndex < entries.Count)
                {
                    //contextMenuStrip1.Show(Cursor.Position);
                }
            }
        }

        private void ButtonDialogProps_Click(object sender, RoutedEventArgs e)
        {
            string tag = (string)((Button)sender).Tag;

            if (tag == "0")
            {
                var entry = entries[mainList.SelectedIndex];
                entry.Name = propName.Text;
                entry.Volume = (float)propVolume.Value;
                entry.Duration = (int)propLength.Value;

                if (propOnlyNight.IsChecked == true)
                    entry.Condition = "night";
                else
                {
                    entry.Condition = propMinHour.Value.ToString() + "-" + propMaxHour.Value.ToString();
                }

                entry.ConditionR = TextCond(entry.Condition);

                entries[mainList.SelectedIndex] = entry;
            }

            Animation(false, gridDialogProps);
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A && e.KeyModifiers == KeyModifiers.Control)
            {
                mainList.SelectAll();
            }
        }

        /*private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var ps = new ProcessStartInfo("https://youtu.be/O5UxULDT8_4")
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);
        }*/

        /*private void copyVolumeToAllMusicFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems[0] != null)
            {
                for (int i = 0; i < musicFilesVolume.Count; i++)
                {
                    musicFilesVolume[i] = musicFilesVolume[listView1.SelectedItems[0].Index];
                    listView1.Items[i].SubItems[1].Text = musicFilesVolume[i].ToString();
                }
            }
        }*/
    }
}