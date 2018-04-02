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
    //Databasehelper class to write data to SQLite database and retrieve from it
    public class DatabaseHelper
    {
        //initialisation of an empty database
        private SQLiteConnection db = null;
        /// <summary>
        /// database creation
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// deleting the database
        /// </summary>
        /// <returns></returns>
        public string deleteDatabase()
        {
            try
            {
                var connection = new SQLiteConnection(GlobalData.dbpath);
                connection.DeleteAll<PostgisObject>();
                return "Database deleted";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        /// <summary>
        /// update or insert in database
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
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
        /// <summary>
        /// load objects in database
        /// </summary>
        public void commit()
        {
            db.Commit();
        }
        /// <summary>
        /// recover objects from database in accordance with the specified date, type target and the classification 
        /// </summary>
        /// <param name="day"></param>
        /// <param name="type_target"></param>
        /// <param name="classification"></param>
        /// <returns></returns>
        public List<PostgisObject> getObjects(DateTime day, int type_target, int classification = -1)
        {
            var db = new SQLiteConnection(GlobalData.dbpath);
            if (classification == -1) return db.Query<PostgisObject>("SELECT * FROM PostgisObject WHERE type_target = ? AND valid_since <= ? AND valid_until >= ?", type_target, day, day);
            else return db.Query<PostgisObject>("SELECT * FROM PostgisObject WHERE type_target = ? AND valid_since <= ? AND valid_until >= ? AND classification_id = ?", type_target, day, day, classification);
        }
        /// <summary>
        ///  recover objects from database in accordance with the specified date, type target and the classname
        /// </summary>
        /// <param name="day"></param>
        /// <param name="type_target"></param>
        /// <param name="classname"></param>
        /// <returns></returns>
        public List<PostgisObject> getObjectsByClasseName(DateTime day, int type_target, string classname)
        {
            var db = new SQLiteConnection(GlobalData.dbpath);
            return db.Query<PostgisObject>("SELECT * FROM PostgisObject WHERE type_target = ? AND valid_since <= ? AND valid_until >= ? AND classname = ?", type_target, day, day, classname);
        }
        /// <summary>
        /// recover informations from database
        /// </summary>
        /// <returns></returns>
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