using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Database.Sqlite;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLite;

namespace OHDMApp
{
    //Databasehelper class to write data to SQLite database and retrieve it from it
    public class DatabaseHelper
    {
        private SQLiteConnection db = null;

        public string createDatabase()
        {
            try
            {
                var connection = new SQLiteConnection(GlobalData.dbpath);
                connection.CreateTable<PostgisObject>();
                return "Database created";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string insertUpdateData(PostgisObject data)
        {
            try
            {
                if (db == null)
                {
                    db = new SQLiteConnection(GlobalData.dbpath);
                    db.DeleteAll<PostgisObject>();
                    db.BeginTransaction();
                }
                db.InsertOrReplace(data);
                return "Single data file inserted or updated";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public void commit()
        {
            db.Commit();
        }

        public List<PostgisObject> getObjects(DateTime day, int type_target, int classification = -1)
        {
            var db = new SQLiteConnection(GlobalData.dbpath);
            if (classification==-1) return db.Query<PostgisObject>("SELECT * FROM PostgisObject WHERE type_target = ? AND valid_since <= ? AND valid_until >= ?", type_target, day, day);
            else return db.Query<PostgisObject>("SELECT * FROM PostgisObject WHERE type_target = ? AND valid_since <= ? AND valid_until >= ? AND classification_id = ?", type_target, day, day, classification);
        }

        public int findNumberRecords()
        {
            try
            {
                var db = new SQLiteConnection(GlobalData.dbpath);
                return db.Table<PostgisObject>().Count();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return -1;
            }
        }
    }
}