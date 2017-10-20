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
using CsvHelper.Configuration;

namespace OHDMApp
{
    public sealed class PostgisObjectMap : CsvClassMap<PostgisObject>
    {
        public PostgisObjectMap()
        {
            Map(m => m.id).Name("id");
            Map(m => m.name).Name("name");
            Map(m => m.valid_since).Name("valid_since");
            Map(m => m.valid_until).Name("valid_until");
            Map(m => m.type_target).Name("type_target");
            Map(m => m.classification_id).Name("classification_id");
            Map(m => m.classname).Name("classname");
            Map(m => m.subclassname).Name("subclassname");
            Map(m => m.point).Name("point");
            Map(m => m.line).Name("line");
            Map(m => m.polygon).Name("polygon");
        }
    }
}