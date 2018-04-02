using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;

using Renci.SshNet;
using Renci.SshNet.Sftp;

using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;

using Carto.Ui;



namespace OHDMApp
{
    [Activity(Label = "OHDM", MainLauncher = true, Icon = "@drawable/map", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]

    public class MainActivity : Activity
    {
        //Here needs to be the personal API-Key from the own Carto-Account, see https://carto.com/docs/carto-engine/mobile-sdk/getting-started/#registering-your-app
        const string LICENSE = "XTUN3Q0ZIYnpMSkdOQWRVUnhLUEtPSDM3ZlM4NU9aSDVBaFFETldxUURyOExEV1NNZUljdklKdlNISEtzb2c9PQoKYXBwVG9rZW49NjliMmRhYTgtOWM3Mi00OGFiLTk1ZDYtZTIxOWVmMThkMjA0CnBhY2thZ2VOYW1lPWNvbS5pbHllcy5PSERNQXBwCm9ubGluZUxpY2Vuc2U9MQpwcm9kdWN0cz1zZGsteGFtYXJpbi1hbmRyb2lkLTQuKgp3YXRlcm1hcms9Y2FydG9kYgo=";

        //FTP server connection credentials
        private string serverName = "";                                                   //server adress
        private string login = "";                                                                       // login   
        private string password = "";                                                                    // password    
        private string directory = "";                                                   // Directory containing the locations


        //Definition of global variables
        private List<PostgisObject> csvContent;
        private EditText datePickerText;
        public List<KeyValuePair<string, int>> city = new List<KeyValuePair<string, int>>();
        public List<int> cityValue = new List<int>();                                                             //Index of cities
        public List<string> cityNames = new List<string>();                                                       //List of cities
        public bool citynamedownloaded = false;                                                                   //Check if the city name is downloaded
        string cityName = "City";
        private int cityselected = 0;
        public string csvLocalPath = "";
        //public string databasePath = "";
        public bool downloading = false;
        public bool connectedMode = false;
        public int numberOfOnlineCity = 0;
        public string nameOfTheCurrentMap = "";                                                                    //Name of the current map selected

        //Classes instantiation
        OfflineMaps offlineMaps;

        MapView mapView;

        //Global views instantiation
        ProgressBar progress;
        Spinner spinner;
        Switch switchSlider;

        /// <summary>
        /// First method called on application started
        /// </summary>
        /// <param name="bundle"></param>
        protected override void OnCreate(Bundle bundle)
        {                                                                  //List of cities
            base.OnCreate(bundle);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            ActionBar.SetIcon(Resource.Drawable.map);
            ActionBar.SetDisplayShowTitleEnabled(true);

            Window window = this.Window;
            window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.KeepScreenOn);


            //Recovery of the progressbar, csvbutton and switchslider
            progress = FindViewById<ProgressBar>(Resource.Id.progressBar1);
            Button csvButton = FindViewById<Button>(Resource.Id.csvButton);
            switchSlider = FindViewById<Switch>(Resource.Id.switch1);
            CrossConnectivity.Current.ConnectivityChanged += HandleConnectivityChanged;                         //check if connectivity is changed
            spinner = FindViewById<Spinner>(Resource.Id.spinner);
            datePickerText = FindViewById<EditText>(Resource.Id.editText1);                                     //create Datepicker-Dialog


            //Sqlite database creation
            var docsFolder = this.GetDir("databases", 0).AbsolutePath;
            var pathToDatabase = Path.Combine(docsFolder, "db_ohdm.db");
            GlobalData.dbpath = pathToDatabase;


            if (CrossConnectivity.Current.IsConnected)                                                          //on launch check if the device is connected
            {
                Toast.MakeText(this, "Internet connection established", ToastLength.Long).Show();
                switchSlider.Visibility = ViewStates.Visible;
            }
            else
            {
                //if while downloading the connection fails, the damaged file is deleted
                if (downloading)
                {
                    if (!csvLocalPath.Contains("") || File.Exists(csvLocalPath))
                    {
                        File.Delete(csvLocalPath);
                    }
                }
                Toast.MakeText(this, "Internet connection failed", ToastLength.Long).Show();
                switchSlider.Visibility = ViewStates.Invisible;
            }


            //creation of new thread to recover the stored map
            ThreadPool.QueueUserWorkItem(state =>
            {
                if (city.Count > 0)                                                                             // check if city list is empty or not
                {
                    city.Clear();
                }
                offlineMaps = new OfflineMaps();                                                                //initialize OfflineMaps class
                city = offlineMaps.retrievedOfflineCityName();                                                  // cities names recovery
                if (cityNames.Count > 0)                                                                        // check if city list is empty or not
                {
                    cityNames.Clear();
                    cityValue.Clear();
                }
                foreach (var item in city)
                {
                    cityNames.Add(item.Key);                                                                   // add location names to the list
                    cityValue.Add(item.Value);                                                                 // add number of lines in the csv file
                }

                //Display cities names in the spinner
                RunOnUiThread(() =>
                {
                    spinner = FindViewById<Spinner>(Resource.Id.spinner);
                    var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, cityNames);
                    spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);
                    adapter = new ArrayAdapter<string>(this,
                            Android.Resource.Layout.SimpleSpinnerItem, cityNames);

                    adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                    spinner.Adapter = adapter;

                });
            });

            //Check if switch state is changed
            switchSlider.CheckedChange += delegate (object sender, CompoundButton.CheckedChangeEventArgs e)
            {
                if (e.IsChecked)
                {
                    datePickerText.Text = "Pick the date...";
                    datePickerText.Focusable = false;
                    citynamedownloaded = true;

                    //disable widgets
                    spinner.Enabled = false;
                    switchSlider.Enabled = false;
                    datePickerText.Enabled = false;

                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        if (city.Count > 0)
                        {
                            city.Clear();
                        }

                        city = retrievedOnlineCityName();                                                            // online cities names recovery
                        if (cityNames.Count > 0)
                        {
                            cityNames.Clear();
                            cityValue.Clear();
                        }
                        foreach (var item in city)
                        {
                            cityNames.Add(item.Key);
                            cityValue.Add(item.Value);
                        }

                        RunOnUiThread(() =>
                        {
                            spinner = FindViewById<Spinner>(Resource.Id.spinner);

                            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);
                            var adapter = new ArrayAdapter<string>(this,
                                    Android.Resource.Layout.SimpleSpinnerItem, cityNames);

                            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                            spinner.Adapter = adapter;
                            //enable/disable  widgets
                            spinner.Enabled = true;
                            switchSlider.Enabled = true;
                            datePickerText.Enabled = false;
                        });

                    });
                }
                else
                {

                    datePickerText.Text = "Pick the date...";
                    datePickerText.Focusable = false;
                    connectedMode = false;
                    citynamedownloaded = false;

                    //enable/disable  widgets
                    spinner.Enabled = false;
                    switchSlider.Enabled = false;
                    datePickerText.Enabled = false;

                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        if (city.Count > 0)
                        {
                            city.Clear();
                        }
                        offlineMaps = new OfflineMaps();
                        city = offlineMaps.retrievedOfflineCityName();                                              // offline cities names recovery
                        if (cityNames.Count > 0)
                        {
                            cityNames.Clear();
                            cityValue.Clear();
                        }
                        foreach (var item in city)
                        {
                            cityNames.Add(item.Key);
                            cityValue.Add(item.Value);
                        }

                        RunOnUiThread(() =>
                        {
                            spinner = FindViewById<Spinner>(Resource.Id.spinner);

                            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);
                            var adapter = new ArrayAdapter<string>(this,
                                    Android.Resource.Layout.SimpleSpinnerItem, cityNames);

                            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                            spinner.Adapter = adapter;

                            // enable/disable  widgets
                            spinner.Enabled = true;
                            switchSlider.Enabled = true;
                            datePickerText.Enabled = false;

                        });
                    });
                }
            };


            datePickerText.Focusable = false;
            datePickerText.Click += delegate
            {
                var dialog = new DatePickerDialog(this, dateset, 2017, 0, 1);
                dialog.Show();
            };
            //enable/disable  widgets
            spinner.Enabled = true;
            switchSlider.Enabled = true;
            datePickerText.Enabled = false;
        }

        /// <summary>
        /// Events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //method to check the connectivity
        void HandleConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            Switch switchSlider = FindViewById<Switch>(Resource.Id.switch1);                                    //Switch view recovery
            if (CrossConnectivity.Current.IsConnected)                                                          //Check if the deviced is connected to internet
            {
                Toast.MakeText(this, "Internet connection established", ToastLength.Long).Show();
                switchSlider.Visibility = ViewStates.Visible;

            }
            else
            {
                if (downloading)
                {
                    if (!csvLocalPath.Contains("") || File.Exists(csvLocalPath))
                    {
                        File.Delete(csvLocalPath);
                    }
                }

                Toast.MakeText(this, "Internet connection failed", ToastLength.Long).Show();
                switchSlider.Visibility = ViewStates.Visible;

            }
        }

        /// <summary>
        /// Recover the spinner selected item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            spinner = (Spinner)sender;


            if (!spinner.GetItemAtPosition(e.Position).ToString().Contains("City"))                                            //Check if the selected item is not the first in the list
            {

                string spinnerValue = string.Format("{0}", spinner.GetItemAtPosition(e.Position), city[e.Position].Key);          // Get the string of the spinner selected item
                cityName = spinnerValue + "-" + city[e.Position].Value + ".csv";
                bool cityExists = false;

                //enable/disable  widgets
                spinner.Enabled = false;
                switchSlider.Enabled = false;
                datePickerText.Enabled = false;

                cityselected = e.Position;

                var deviceDocumentsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments);//Get the Document directory path of the android device
                string cityPath = deviceDocumentsPath.AbsolutePath + "/OHDM_MAP/" + cityName;                                                //Add selected cityname to the path
                DirectoryInfo OHDM_MAP_Directory = new DirectoryInfo(deviceDocumentsPath.AbsolutePath + "/OHDM_MAP/");                                       //Create the OHDM_MAP directory
                csvLocalPath = cityPath;



                if (connectedMode)                                                                                                           //check if connected to server
                {
                    if (OHDM_MAP_Directory.Exists)                                                                                           //check if the directory exists
                    {
                        foreach (var file in OHDM_MAP_Directory.GetFiles())                                                                  //Check if the selected cityname is on the device
                        {
                            string csvFile = file.ToString();
                            if (csvFile.Contains(cityName))
                            {
                                //enable/disable  widgets
                                spinner.Enabled = false;
                                switchSlider.Enabled = false;
                                datePickerText.Enabled = false;
                                cityExists = true;
                            }
                        }
                    }
                    if (cityExists == false)                                                                                              //if cityname doesn't exist and device is connected
                    {
                        downloading = true;
                        ThreadPool.QueueUserWorkItem(state =>                                                                             //add thread  into the queue to be executed 
                        {
                            RunOnUiThread(() =>
                            {
                                displayMessage(4);
                            });
                            string nameOfCitySelected = city[e.Position].Key.ToString() + "-" + city[e.Position].Value.ToString();
                            DownloadOnlineMaps(nameOfCitySelected);    // download the city map fron ftp server to device 

                        });

                    }

                    if (cityExists)                                                                                                        // if connected to server but location already exists on the device, load location on device
                    {
                        ThreadPool.QueueUserWorkItem(state =>
                        {
                            processCsv(csvLocalPath);                                                                                      // load csv file to sqlite database
                        });
                        mapView = FindViewById<MapView>(Resource.Id.mapView);
                        MapView.RegisterLicense(LICENSE, this);
                        mapView.SetZoom(18f, 0f);
                    }
                }
                else                                                                                                                       // if in offline mode, load location on device
                {
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        processCsv(csvLocalPath);
                    });
                    mapView = FindViewById<MapView>(Resource.Id.mapView);
                    MapView.RegisterLicense(LICENSE, this);
                    mapView.SetZoom(18f, 0f);
                }

            }

        }

        /// <summary>
        /// Functions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        //Datetime management method
        public void dateset(object sender, DatePickerDialog.DateSetEventArgs args)
        {



            datePickerText.Text = args.Date.ToShortDateString();
            DateTime day = args.Date;                                                                           //Date recovery

            TextView label_dl = FindViewById<TextView>(Resource.Id.label_Progressbar);                          //Labelview recovery
            label_dl.Visibility = ViewStates.Visible;

            //Recovery of MapDrawer Class events
            MapDrawer.MapEvent += (source, args2) =>
            {
                progress.Max = 100;
                // Draw view on user interface thread
                RunOnUiThread(() =>
                {
                    label_dl.Visibility = ViewStates.Visible;
                    if ((((double)args2.GetActive() / (double)args2.GetMax()) * 100) == 99)               //Check the end of the progress bar status to display messages on the screen
                    {
                        label_dl.SetText(args2.GetMessage().ToString() + " charged ....", TextView.BufferType.Normal);
                        downloadProgressBar(0);
                    }
                    if (args2.GetMessage().Contains("End"))
                    {
                        //enable/disable  widgets
                        spinner.Enabled = true;
                        switchSlider.Enabled = true;
                        datePickerText.Enabled = true;

                        displayMessage(6);
                        downloadProgressBar(0);
                    }
                    else
                    {
                        label_dl.SetText(args2.GetMessage(), TextView.BufferType.Normal);                           //Display messages on the screen
                    }

                });
                downloadProgressBar((ulong)(((double)args2.GetActive() / (double)args2.GetMax()) * 100));        //Method to set the progress of the progressbar
            };
            //Creation of a thread to draw maps
            ThreadPool.QueueUserWorkItem(state =>
            {
                RunOnUiThread(() =>
                {
                    //enable/disable  widgets
                    spinner.Enabled = false;
                    switchSlider.Enabled = false;
                    datePickerText.Enabled = false;
                });
                MapDrawer.DrawMap(day, mapView);                                                                //Method of MapDrawer class to load a map

            });

        }
        private bool fileIsOpen(string file)                                                                    // check if the file is not already used by a thread
        {
            try
            {
                FileStream fs;
                fs = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                if (fs != null)
                {
                    fs.Close();
                    return false;
                }
                else
                    return true;

            }
            catch (IOException)
            {
                return true;
            }
        }
        /// <summary>
        /// method to display messages
        /// </summary>
        /// <param name="value"></param>
        private void displayMessage(int value)
        {
            TextView label_dl = FindViewById<TextView>(Resource.Id.label_Progressbar);
            switch (value)
            {
                case 0:
                    label_dl.Visibility = ViewStates.Visible;
                    label_dl.SetText("Please wait...\nMap material is imported from CSV", TextView.BufferType.Normal);     //Display message on the screen
                    break;
                case 1:
                    label_dl.Visibility = ViewStates.Visible;
                    label_dl.SetText("CSV charged to database ....\nYou can pick the date...", TextView.BufferType.Normal);//Display message on the screen
                    break;
                case 2:
                    Toast.MakeText(this, "Connection to server established...", ToastLength.Short).Show();
                    break;
                case 3:
                    Toast.MakeText(this, "Map Downloaded ...\nNow loading into the Database.....", ToastLength.Long).Show();
                    break;
                case 4:
                    label_dl.Visibility = ViewStates.Visible;
                    label_dl.SetText("Downloading ...", TextView.BufferType.Normal);
                    break;
                case 5:
                    Toast.MakeText(this, "Connection to the server Failed ...", ToastLength.Long).Show();
                    break;
                case 6:
                    label_dl.SetText("End of loading Map", TextView.BufferType.Normal);                                   //Display messages on the screen
                    Toast.MakeText(this, "Map is load", ToastLength.Long).Show();
                    break;
                case 7:
                    label_dl.SetText("Error in the downloaded file\nPlease download it again", TextView.BufferType.Normal);                      //Display messages on the screen
                    Toast.MakeText(this, "Map was not downloaded", ToastLength.Long).Show();
                    break;
                default:

                    break;
            }
        }
        /// <summary>
        /// read CSV to database
        /// </summary>
        /// <param name="_csvLocalPath"></param>
        public void processCsv(string _csvLocalPath)
        {
            bool failed = false;
            bool finish = false;
            do
            {

                if ((!fileIsOpen(csvLocalPath)) && (nameOfTheCurrentMap != csvLocalPath) && finish == false)   // check if file is available and if the name is correct and the loading of the file in the database is not finished
                {
                    if (downloading)                                                                           // if we are in download mode we inform that the map has been downloaded from the FTP server
                    {
                        RunOnUiThread(() =>
                        {
                            displayMessage(3);
                        });

                    }


                    progress.Max = 100;
                    CsvUtils.CsvStatus += (source, args) =>
                    {

                        RunOnUiThread(() =>
                        {
                            displayMessage(0);
                            if ((int)(((double)args.GetActive() / (double)args.GetMax()) * 100) == 99)
                            {
                                //displayMessage(1);
                                //spinner.Enabled = true;
                                //switchSlider.Enabled = true;
                                //datePickerText.Enabled = true;
                                nameOfTheCurrentMap = csvLocalPath;
                            }
                        });
                        downloadProgressBar((ulong)(((double)args.GetActive() / (double)args.GetMax()) * 100));                          //show csv loading to sql-lite database progress
                    };

                    Stream input = File.OpenRead(_csvLocalPath) as Stream;


                    int numberOfLineInDevice = CsvUtils.countInCSV(input);                                                               //first reading of the csv file to count the number of line (that allows to adapt the querry to any csv file) and return number of line
                    input.Close();
                    if (cityValue[cityselected] != numberOfLineInDevice)                                                                //Check if the number of line is correct
                    {
                        failed = true;
                        if (!csvLocalPath.Contains("") || File.Exists(_csvLocalPath))                                                   // check if the csv file are selected and if it exist in the device 
                        {
                            File.Delete(_csvLocalPath);                                                                                 //delete the csv file downloaded because they don't  have the goot number of line so the download failed
                        }
                        RunOnUiThread(() =>
                        {
                            //enable/disable  widgets
                            spinner.Enabled = true;
                            switchSlider.Enabled = true;
                            datePickerText.Enabled = false;
                            displayMessage(7);
                        });
                    }
                    else
                    {
                        Stream input2 = File.OpenRead(_csvLocalPath) as Stream;

                        //load csv file to database
                        csvContent = CsvUtils.readInCSV(input2, cityValue[cityselected]);
                        input2.Close();
                        failed = false;
                        finish = true;
                    }
                }
                if (nameOfTheCurrentMap == csvLocalPath)
                {
                    RunOnUiThread(() =>
                    {
                        //enable/disable  widgets
                        spinner.Enabled = true;
                        switchSlider.Enabled = true;
                        datePickerText.Enabled = true;
                        displayMessage(1);
                    });
                    failed = false;
                }
            } while (((fileIsOpen(csvLocalPath)) && (nameOfTheCurrentMap != csvLocalPath)) && failed == false && finish == false); //Wait that the file is not open and if the write of the csv file into the sqlite database is successful or if it is failed

            downloading = false;
        }
        /// <summary>
        /// retrieve online cities names
        /// </summary>
        /// <returns></returns>
        public List<KeyValuePair<string, int>> retrievedOnlineCityName()
        {
            List<KeyValuePair<string, int>> city = new List<KeyValuePair<string, int>>();
            connectedMode = true;
            citynamedownloaded = true;
            using (SftpClient sftp = new SftpClient(@serverName, login, password))
            {
                try
                {
                    sftp.Connect();                                                                             //Connection to the FTP server
                    int count = 0;
                    var files = sftp.ListDirectory(directory);

                    foreach (var file in files)                                                                 //count the number of cities names in the ftp server to know the maximum progressbar size 
                    {
                        if (file.Name.Contains(".csv"))
                        {
                            count++;
                        }
                    }
                    progress.Max = count;
                    city.Add(new KeyValuePair<string, int>("City", 0));
                    int nbFile = 0;
                    foreach (var file in files)
                    {
                        if (file.Name.Contains(".csv"))
                        {
                            string name = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                            string[] cityName = name.Split('-');                                                                        // remove the "-" to insert the number of line contained in the csv file
                            city.Add(new KeyValuePair<string, int>(cityName[0], Int32.Parse(cityName[1])));                             //Add the cities in the city List and their value 
                            nbFile++;
                            downloadProgressBar((ulong)nbFile);
                        }
                    }
                    sftp.Disconnect();                                                                          // Disconnect from FTP server
                }
                catch (Exception er)
                {
                    Console.WriteLine("Error " + er);
                    if (!sftp.IsConnected)
                    {
                        connectedMode = false;
                        RunOnUiThread(() =>
                        {
                            //enable/disable  widgets
                            spinner.Enabled = true;
                            switchSlider.Enabled = true;
                            datePickerText.Enabled = false;

                            displayMessage(5);                                                                 // Display message if the connection to the FTP server Failed
                        });
                    }
                }
            }
            return city;
        }
        ///// <summary>
        ///// download online maps
        ///// </summary>
        ///// <param name="_selectedCityName"></param>
        public void DownloadOnlineMaps(string _selectedCityName)
        {
            string cityName = _selectedCityName + ".csv";
            var deviceDocumentsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments);
            string cityPath = deviceDocumentsPath.AbsolutePath + "/OHDM_MAP/" + cityName;



            using (SftpClient sftp = new SftpClient(@serverName, login, password))
            {
                try
                {
                    sftp.Connect();
                    using (Stream fileStream = File.OpenWrite(cityPath))
                    {
                        SftpFileAttributes attributes = sftp.GetAttributes(directory + cityName);                             //recover the file size
                        progress.Max = (int)attributes.Size;                                                                  // Set progress bar maximum on foreground thread 
                        sftp.DownloadFile(directory + cityName, fileStream, downloadProgressBar);
                        fileStream.Close();
                    }
                    sftp.Disconnect();
                }
                catch (Exception er)
                {
                    Console.WriteLine("Error " + er);
                    if (!sftp.IsConnected)
                    {
                        RunOnUiThread(() =>
                        {
                            //enable/disable  widgets
                            spinner.Enabled = true;
                            switchSlider.Enabled = true;
                            datePickerText.Enabled = false;

                            displayMessage(5);   // Display message if the connection to the FTP server Failed
                        });
                    }
                }
            }

            processCsv(csvLocalPath);
            mapView = FindViewById<MapView>(Resource.Id.mapView);
            MapView.RegisterLicense(LICENSE, this);
            mapView.SetZoom(18f, 0f);

        }
        /// <summary>
        /// Method for the progression of the progressbar
        /// </summary>
        /// <param name="uploaded"></param>
        public void downloadProgressBar(ulong uploaded)
        {
            if (citynamedownloaded && (int)uploaded == progress.Max - 1)                                                     //Check if the file is completely downloaded
            {
                RunOnUiThread(() =>
                {
                    displayMessage(2);
                    progress.Max = 0;
                    citynamedownloaded = false;
                });
            }
            progress.SetProgress((int)uploaded, true);
        }

    }
}
