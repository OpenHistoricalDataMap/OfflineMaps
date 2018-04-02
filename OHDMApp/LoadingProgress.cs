using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace OHDMApp
{
    public delegate void LoadingEventHandler(object source, MapEventArgs e);

    //EventArgs of the Event, that will be fired to show the status of the drawing of objects
    public class LoadingEventArgs : EventArgs
    {
        private int active;
        private int max;
        private string message;

        public LoadingEventArgs(int active, int max, string message)
        {
            this.active = active;
            this.max = max;
            this.message = message;
        }
        public int GetActive()
        {
            return active;
        }

        public int GetMax()
        {
            return max;
        }

        public string GetMessage()
        {
            return message;
        }
    }
    public class LoadingProgress
    {
        public static event LoadingEventHandler LoadingEvent;

        ProgressBar progress;
        public void setProgressBar(ProgressBar p)
        {
            progress = p;
        }
        public void DownloadProgresBar(ulong uploaded)
        {
            Console.WriteLine("LoadingProgress => downloadProgressBar : " + (int)uploaded);
            progress.SetProgress((int)uploaded, true);
        }
    }
}