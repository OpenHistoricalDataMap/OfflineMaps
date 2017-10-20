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
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

namespace OHDMApp
{
    public delegate void CsvEventHandler(object source, CsvEventArgs e);

    public class CsvEventArgs : EventArgs
    {
        private int active;
        private int max;
        public CsvEventArgs(int active, int max)
        {
            this.active = active;
            this.max = max;
        }
        public int GetActive()
        {
            return active;
        }

        public int GetMax()
        {
            return max;
        }
    }

    //Csv is parsed and read
    public class CsvUtils
    {
        public static event CsvEventHandler CsvStatus;

        public static List<PostgisObject> readInCSV(Stream stream)
        {
            DatabaseHelper d = new DatabaseHelper();
            //d.createDatabase(pathToDatabase);
            List<PostgisObject> result = new List<PostgisObject>();
            PostgisObject record;
            using (TextReader fileReader = new StreamReader(stream))
            {
                CsvReader csv = new CsvReader(fileReader);
                csv.Configuration.HasHeaderRecord = true;
                csv.Configuration.IgnoreHeaderWhiteSpace = true;
                csv.Configuration.RegisterClassMap<PostgisObjectMap>();
                csv.Configuration.Delimiter = ";";
                while (csv.Read())
                {
                    record = csv.GetRecord<PostgisObject>();
                    d.insertUpdateData(record);
                    result.Add(record);
                    CsvStatus(null, new CsvEventArgs(csv.Row,722517));
                }
                d.commit();
            }
            return result;
        }
    }
}