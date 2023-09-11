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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace CustomRadioStation
{
    public partial class MainWindow : Window
    {
        GameType gameType;

        public MainWindow()
        {
            InitializeComponent();
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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<ListEntry> list = new List<ListEntry>();
            for (int i = 0; i < 100; i++)
                list.Add(new() { FileName = "File Name", Volume = "-5", Duration = "158", Condition = "0" });
            mainList.ItemsSource = list;
        }

        private void Window_Closing(object sender, WindowClosingEventArgs e)
        {
        }

        private void buttonSelGame_Click(object sender, RoutedEventArgs e)
        {
            string tag = (string)((Button)sender).Tag;

            string picName = "";

            if (tag == "0")
            {
                gameType = GameType.FarCry5;
                picName = "hdr";
            }

            if (tag == "1")
            {
                gameType = GameType.FarCryNewDawn;
                picName = "hdr_nd";
            }

            if (tag == "2")
            {
                gameType = GameType.FarCry6;
                picName = "hdr_6";
            }

            Stream stream = AssetLoader.Open(new Uri("avares://" + GetType().Assembly.GetName().Name + "/Resources/" + picName + ".jpg"));
            var bitmap = new Bitmap(stream);
            headerPic.Source = bitmap;

            Animation(false, selGameGrid);
            Animation(true, mainGrid);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}