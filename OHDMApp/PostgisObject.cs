using System;
using System.Collections.Generic;
using Android.Media;
using Newtonsoft.Json;
using SQLite;

namespace OHDMApp
{
    //The object structure that contains all necessary data from database to fully draw and categorize it
    public class PostgisObject
    {
        [PrimaryKey]
        public int id { get; set; }
        public string name { get; set; }
        public int type_target { get; set; }
        public int classification_id { get; set; }
        public string classname { get; set; }
        public string subclassname { get; set; }
        public DateTime valid_since { get; set; }
        public DateTime valid_until { get; set; }
        public string point { get; set; }
        public string line { get; set; }
        public string polygon { get; set; }
        [Ignore]
        public PostgisPoint pointList {
            get
            {
                var pl = JsonConvert.DeserializeObject<PostgisPoint>(point, new JsonSerializerSettings
                {
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                });
                return pl;
            }
        }

        [Ignore]
        public PostgisLine lineList
        {
            get
            {
                var ll = JsonConvert.DeserializeObject<PostgisLine>(line, new JsonSerializerSettings
                {
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                });
                return ll;
            }
        }

        [Ignore]
        public PostgisPolygon polygonList
        {
            get
            {
                var pl = JsonConvert.DeserializeObject<PostgisPolygon>(polygon, new JsonSerializerSettings
                {
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                });
                return pl;
            }
        }

        public PostgisObject()
        {
        }

        public PostgisObject(int id, string name, int type_target, int classification_id, string classname, string subclassname, DateTime valid_since, DateTime valid_until, string point, string line, string polygon)
        {
            this.id = id;
            this.name = name;
            this.type_target = type_target;
            this.classification_id = classification_id;
            this.classname = classname;
            this.subclassname = subclassname;
            this.valid_since = valid_since;
            this.valid_until = valid_until;
            this.point = point;
            this.line = line;
            this.polygon = polygon;
        }

        public override string ToString()
        {
            return string.Format("[PostGISObject: ID={0}, Name={1}, type_target={2}, classification_id={3}, classname={4}, subclassname={5}, valid_since={6}, valid_until={7}, point={8}, line={9}, polygon={10}, pointList={11}, lineList={12}, polygonList={13}]", 
                id, name, type_target, classification_id, classname, subclassname, valid_since, valid_until, point, line, polygon, pointList, lineList, polygonList);
        }
    }
}