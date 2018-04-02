using System;
using System.Collections.Generic;
using System.IO;


namespace OHDMApp
{/// <summary>
/// retrieve offline maps names present on the device
/// </summary>
    class OfflineMaps
    {
        bool exist = false;
        public List<KeyValuePair<string, int>> retrievedOfflineCityName()
        {
            List<KeyValuePair<string, int>> city = new List<KeyValuePair<string, int>>();

            var documentsDirectoryPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDocuments);
            DirectoryInfo directoryOHDM_MAP = new DirectoryInfo(documentsDirectoryPath.AbsolutePath + "//OHDM_MAP//");
            string OHDM_MAP_Path = documentsDirectoryPath.AbsolutePath + "//OHDM_MAP//";

            if (!directoryOHDM_MAP.Exists)
            {
                directoryOHDM_MAP.CreateSubdirectory(OHDM_MAP_Path);
            }
            if (exist = Directory.GetFiles(OHDM_MAP_Path, "*.csv").Length > 0)
            {
                city.Add(new KeyValuePair<string, int>("City", 0));
                foreach (var file in directoryOHDM_MAP.GetFiles())
                {
                    string csvFileName = file.ToString();

                    string name = System.IO.Path.GetFileNameWithoutExtension(csvFileName);

                    string[] cityName = name.Split('-');

                    city.Add(new KeyValuePair<string, int>(cityName[0], Int32.Parse(cityName[1])));
                }
            }
            return city;
        }
    }
}