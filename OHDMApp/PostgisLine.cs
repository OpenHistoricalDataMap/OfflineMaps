using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace OHDMApp
{
    public class PostgisLine
    {
        public string type { get; set; }
        public List<List<double>> coordinates { get; set; }

        public PostgisLine()
        {
        }
    }
}