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
    /// <summary>
    /// this class is used for the communication between csvutils class and the user interface 
    /// </summary>
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

    /// <summary>
    /// Csv is parsed and read
    /// </summary>
    public class CsvUtils
    {
        public static event CsvEventHandler CsvStatus;
        public static int count = 0;
        /// <summary>
        /// count number of lines present in the csv file
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static int countInCSV(Stream stream)
        {
            count = 0;
            using (TextReader fileReader1 = new StreamReader(stream))
            {
                string line;
                while ((line = fileReader1.ReadLine()) != null)
                {
                    count++;
                }
            }
            return count;
        }
        /// <summary>
        /// load csv file data into the database
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="numberofLine"></param>
        /// <returns></returns>
        public static List<PostgisObject> readInCSV(Stream stream, int numberofLine)
        {
            DatabaseHelper d = new DatabaseHelper();
            d.deleteDatabase();
            d.createDatabase();
            List<PostgisObject> result = new List<PostgisObject>();
            PostgisObject record;
            using (TextReader fileReader = new StreamReader(stream))
            {

                CsvReader csv = new CsvReader(fileReader);
                csv.Configuration.HasHeaderRecord = true;
                csv.Configuration.IgnoreHeaderWhiteSpace = true;
                csv.Configuration.RegisterClassMap<PostgisObjectMap>();
                csv.Configuration.Delimiter = ",";
                while (csv.Read())
                {
                    record = csv.GetRecord<PostgisObject>();
                    d.insertUpdateData(record);
                    result.Add(record);

                    CsvStatus(null, new CsvEventArgs(csv.Row, numberofLine));
                }
                d.commit();
            }
            return result;
        }
    }
}