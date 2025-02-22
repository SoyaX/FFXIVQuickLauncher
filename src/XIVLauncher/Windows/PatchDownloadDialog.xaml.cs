﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using Serilog;
using XIVLauncher.Game.Patch;
using XIVLauncher.Game.Patch.Acquisition;
using XIVLauncher.Http;
using XIVLauncher.Windows.ViewModel;
using Brushes = System.Windows.Media.Brushes;

namespace XIVLauncher.Windows
{
    /// <summary>
    /// Interaction logic for PatchDownloadDialog.xaml
    /// </summary>
    public partial class PatchDownloadDialog : Window
    {
        private readonly PatchManager _manager;

        public PatchDownloadDialogViewModel ViewModel => DataContext as PatchDownloadDialogViewModel;

        public PatchDownloadDialog(PatchManager manager)
        {
            _manager = manager;
            InitializeComponent();
            this.DataContext = new PatchDownloadDialogViewModel();

            var viewUpdateTimer = new Timer();
            viewUpdateTimer.Elapsed += ViewUpdateTimerOnElapsed;
            viewUpdateTimer.AutoReset = true;
            viewUpdateTimer.Interval = 200;
            viewUpdateTimer.Enabled = true;
            viewUpdateTimer.Start();
        }

        private void ViewUpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                SetGeneralProgress(_manager.CurrentInstallIndex, _manager.Downloads.Count);

                for (var i = 0; i < PatchManager.MAX_DOWNLOADS_AT_ONCE; i++)
                {
                    var activePatch = _manager.Actives[i];

                    if (_manager.Slots[i] == PatchManager.SlotState.Done || activePatch == null)
                    {
                        SetPatchProgress(i, ViewModel.PatchDoneLoc, 100f, false);
                        continue;
                    }

                    if (_manager.Slots[i] == PatchManager.SlotState.Checking)
                    {
                        SetPatchProgress(i,
                            $"{activePatch.Patch} ({ViewModel.PatchCheckingLoc})", 100f, false);
                    }
                    else
                    {
                        var pct = Math.Round((double) (100 * _manager.Progresses[i]) / activePatch.Patch.Length, 2);
                        SetPatchProgress(i,
                            $"{activePatch.Patch} ({pct:#0.0}%, {Util.BytesToString(_manager.Speeds[i])}/s)",
                            pct, this._manager.DownloadServices[i] is TorrentPatchAcquisition);
                    }
                }

                if (_manager.DownloadsDone)
                {
                    SetLeft(0, 0);
                    SetDownloadDone();
                }
                else
                {
                    SetLeft(_manager.AllDownloadsLength, _manager.Speeds.Sum());
                }
            });
        }

        public void SetGeneralProgress(int curr, int final)
        {
            PatchProgressText.Text = string.Format(ViewModel.PatchGeneralStatusLoc,
                $"{curr}/{final}");

            InstallingText.Text = string.Format(ViewModel.PatchInstallingFormattedLoc, curr);
        }

        public void SetLeft(long left, double rate)
        {
            BytesLeftText.Text = string.Format(ViewModel.PatchEtaLoc, Util.BytesToString(left), Util.BytesToString(rate));
        }

        public void SetPatchProgress(int index, string patchName, double pct, bool torrent)
        {
            switch (index)
            {
                case 0:
                    SetProgressBar1Progress(patchName, pct, torrent);
                    break;
                case 1:
                    SetProgressBar2Progress(patchName, pct, torrent);
                    break;
                case 2:
                    SetProgressBar3Progress(patchName, pct, torrent);
                    break;
                case 3:
                    SetProgressBar4Progress(patchName, pct, torrent);
                    break;
            }
        }

        public void SetProgressBar1Progress(string patchName, double percentage, bool torrent)
        {
            Progress1.Value = percentage;
            Progress1Text.Text = patchName;

            if (torrent)
            {
                this.Progress1.Foreground = Brushes.DarkGreen;
                this.Progress1.Background = Brushes.LightGreen;
                this.Progress1.BorderBrush = Brushes.LightGreen;
            }
            else
            {
                this.Progress1.Foreground = Brushes.DodgerBlue;
                this.Progress1.Background = Brushes.LightSkyBlue;
                this.Progress1.BorderBrush = Brushes.LightSkyBlue;
            }
        }

        public void SetProgressBar2Progress(string patchName, double percentage, bool torrent)
        {
            Progress2.Value = percentage;
            Progress2Text.Text = patchName;

            if (torrent)
            {
                this.Progress2.Foreground = Brushes.DarkGreen;
                this.Progress2.Background = Brushes.LightGreen;
                this.Progress2.BorderBrush = Brushes.LightGreen;
            }
            else
            {
                this.Progress2.Foreground = Brushes.DodgerBlue;
                this.Progress2.Background = Brushes.LightSkyBlue;
                this.Progress2.BorderBrush = Brushes.LightSkyBlue;
            }
        }

        public void SetProgressBar3Progress(string patchName, double percentage, bool torrent)
        {
            Progress3.Value = percentage;
            Progress3Text.Text = patchName;

            if (torrent)
            {
                this.Progress3.Foreground = Brushes.DarkGreen;
                this.Progress3.Background = Brushes.LightGreen;
                this.Progress3.BorderBrush = Brushes.LightGreen;
            }
            else
            {
                this.Progress3.Foreground = Brushes.DodgerBlue;
                this.Progress3.Background = Brushes.LightSkyBlue;
                this.Progress3.BorderBrush = Brushes.LightSkyBlue;
            }
        }

        public void SetProgressBar4Progress(string patchName, double percentage, bool torrent)
        {
            Progress4.Value = percentage;
            Progress4Text.Text = patchName;

            if (torrent)
            {
                this.Progress4.Foreground = Brushes.DarkGreen;
                this.Progress4.Background = Brushes.LightGreen;
                this.Progress4.BorderBrush = Brushes.LightGreen;
            }
            else
            {
                this.Progress4.Foreground = Brushes.DodgerBlue;
                this.Progress4.Background = Brushes.LightSkyBlue;
                this.Progress4.BorderBrush = Brushes.LightSkyBlue;
            }
        }

        public void SetDownloadDone()
        {
            Progress1.Visibility = Visibility.Collapsed;
            Progress1Text.Visibility = Visibility.Collapsed;

            Progress2.Visibility = Visibility.Collapsed;
            Progress2Text.Visibility = Visibility.Collapsed;

            Progress3.Visibility = Visibility.Collapsed;
            Progress3Text.Visibility = Visibility.Collapsed;

            Progress4.Visibility = Visibility.Collapsed;
            Progress4Text.Visibility = Visibility.Collapsed;
        }

        private void PatchDownloadDialog_OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true; // We can't cancel patching yet, big TODO
        }

        private void BytesLeftText_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
#if DEBUG
            _manager.CancelAllDownloads();
#endif
        }
    }
}
