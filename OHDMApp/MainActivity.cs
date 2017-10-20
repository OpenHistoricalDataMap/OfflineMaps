using System;
using Android.App;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Android.Content;
using Android.Views;
using Newtonsoft.Json;
using Carto.Core;
using Carto.DataSources;
using Carto.Layers;
using Carto.Projections;
using Carto.Styles;
using Carto.Ui;
using Carto.VectorElements;
using Point = Carto.VectorElements.Point;

namespace OHDMApp
{
    [Activity(Label = "OHDM", MainLauncher = true, Icon = "@drawable/map")]
    public class MainActivity : Activity
    {
        //Here needs to be the personal API-Key from the own Carto-Account, see https://carto.com/docs/carto-engine/mobile-sdk/getting-started/#registering-your-app
        const string LICENSE = "XXX";
        private List<PostgisObject> csvContent;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            //ActionBar.SetDisplayUseLogoEnabled(true);
            //ActionBar.SetDisplayShowHomeEnabled(true);
            ActionBar.SetIcon(Resource.Drawable.map);
            ActionBar.SetDisplayShowTitleEnabled(true);
            //ActionBar.SetDisplayHomeAsUpEnabled(true);

            Window window = this.Window;
            window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.KeepScreenOn);

            Button csvButton = FindViewById<Button>(Resource.Id.csvButton);

            //databasepath is set
            var docsFolder = this.GetDir("databases",0).AbsolutePath;
            var pathToDatabase = Path.Combine(docsFolder, "db_ohdm.db");
            GlobalData.dbpath = pathToDatabase;
            DatabaseHelper d = new DatabaseHelper();
            d.createDatabase();
            int count = d.findNumberRecords();

            var mapView = FindViewById<MapView>(Resource.Id.mapView);
            MapView.RegisterLicense(LICENSE, this);
            mapView.SetZoom(18f, 0f);
            
            //if own SQLite database is empty fill it from CSV in assets
            if (count < 100000)
            {
                processCsv();
                count = d.findNumberRecords();
            }

            //create Datepicker-Dialog
            var datePickerText = FindViewById<EditText>(Resource.Id.editText1);
            datePickerText.Focusable = false;
            datePickerText.Click += delegate
            {
                var dialog = new DatePickerDialog(this);
                dialog.DateSet += (sender, args) =>
                {
                    datePickerText.Text = args.Date.ToShortDateString();
                    DateTime day = args.Date;
                    var progressDialog2 = new ProgressDialog(this);
                    progressDialog2.SetCancelable(false);
                    progressDialog2.SetCanceledOnTouchOutside(false);
                    progressDialog2.Indeterminate = false;
                    progressDialog2.SetProgressStyle(ProgressDialogStyle.Horizontal);
                    progressDialog2.SetTitle("Bitte warten...");
                    progressDialog2.SetMessage("Die Karte wird aus dem Kartenstand des gewählten Datums aufgebaut...");
                    progressDialog2.Show();
                    MapDrawer.MapEvent += (source, args2) =>
                    {
                        RunOnUiThread(() =>
                        {
                            progressDialog2.SetMessage(args2.GetMessage());
                            progressDialog2.Progress =
                                (int) (((double) args2.GetActive() / (double) args2.GetMax()) * 100);
                        });
                    };
                    new Thread(new ThreadStart(delegate
                    {
                        MapDrawer.DrawMap(day, mapView);
                        RunOnUiThread(() =>
                        {
                            progressDialog2.Hide();
                        });
                    })).Start();
                };
                dialog.Show();
            };
        }

        //read CSV to database
        private void processCsv()
        {
            var progressDialog = new ProgressDialog(this);
            progressDialog.SetCancelable(false);
            progressDialog.SetCanceledOnTouchOutside(false);
            progressDialog.Indeterminate = false;
            progressDialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
            progressDialog.SetTitle("Bitte warten...");
            progressDialog.SetMessage("Kartenmaterial wird aus CSV eingelesen...");
            progressDialog.Show();
            CsvUtils.CsvStatus += (source, args) =>
            {
                progressDialog.Progress = (int)(((double)args.GetActive() / (double)args.GetMax()) * 100);
            };
            new Thread(new ThreadStart(delegate {
                Stream input = Assets.Open("outputnew.csv");
                csvContent = CsvUtils.readInCSV(input);
                RunOnUiThread(() =>
                {
                    //csvButton.Text = "CSV wurde erfolgreich eingelesen";
                    //csvButton.Enabled = false;
                    progressDialog.Hide();
                });
            })).Start();
        }
    }
}

