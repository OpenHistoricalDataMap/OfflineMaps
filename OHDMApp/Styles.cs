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
using Carto.Styles;

namespace OHDMApp
{
    //This class contains all style definitions of different types of points, lines and polygons
    public static class Styles
    {
        public static PointStyleBuilder GetDefaultPointStyle()
        {
            /// Create Marker style
            var builder = new PointStyleBuilder();
            builder.Size = 2;
            builder.Color = new Carto.Graphics.Color(0, 0, 0, 255);
            builder.ClickSize = 10;
            //var s= new StyleSet<PointStyle>(builder);
            return builder;
        }

        public static TextStyleBuilder GetDefaultTextStyle()
        {
            var textStyleBuilder = new TextStyleBuilder();
            textStyleBuilder.Color = new Carto.Graphics.Color(255, 0, 0, 255);
            textStyleBuilder.OrientationMode = BillboardOrientation.BillboardOrientationFaceCamera;
            return textStyleBuilder;
        }

        public static LineStyleBuilder GetDefaultLineStyle()
        {
            var lineStyleBuilder = new LineStyleBuilder();
            lineStyleBuilder.LineJoinType = LineJoinType.LineJoinTypeNone;
            lineStyleBuilder.Width = 1f;
            return lineStyleBuilder;
        }

        public static LineStyleBuilder GetDefaultLineTransparentStyle()
        {
            var lineStyleBuilder = new LineStyleBuilder();
            lineStyleBuilder.LineJoinType = LineJoinType.LineJoinTypeNone;
            lineStyleBuilder.Color = new Carto.Graphics.Color(255, 255, 255, 0);
            lineStyleBuilder.Width = 1f;
            return lineStyleBuilder;
        }

        public static PolygonStyleBuilder GetDefaultPolygonStyle()
        {
            // Create polygon style and poses
            PolygonStyleBuilder polygonStyleBuilder = new PolygonStyleBuilder();
            polygonStyleBuilder.Color = new Carto.Graphics.Color(255, 0, 0, 255); // red
            polygonStyleBuilder.LineStyle = GetDefaultLineStyle().BuildStyle();
            return polygonStyleBuilder;
        }

        public static PolygonStyleBuilder GetDefaultForestStyle()
        {
            // Create polygon style and poses
            PolygonStyleBuilder polygonStyleBuilder = new PolygonStyleBuilder();
            polygonStyleBuilder.Color = new Carto.Graphics.Color(173, 209, 158, 255);
            polygonStyleBuilder.LineStyle = GetDefaultLineTransparentStyle().BuildStyle();
            return polygonStyleBuilder;
        }

        public static PolygonStyleBuilder GetDefaultWaterStyle()
        {
            // Create polygon style and poses
            PolygonStyleBuilder polygonStyleBuilder = new PolygonStyleBuilder();
            polygonStyleBuilder.Color = new Carto.Graphics.Color(181, 208, 208, 255); 
            polygonStyleBuilder.LineStyle = GetDefaultLineTransparentStyle().BuildStyle();
            return polygonStyleBuilder;
        }

        public static LineStyleBuilder GetMotorwayStyle1()
        {
            var lineStyleBuilder = new LineStyleBuilder();
            lineStyleBuilder.LineJoinType = LineJoinType.LineJoinTypeNone;
            lineStyleBuilder.Width = 4f;
            lineStyleBuilder.Color = new Carto.Graphics.Color(235, 125, 84, 255);
            return lineStyleBuilder;
        }

        public static LineStyleBuilder GetMotorwayStyle2()
        {
            var lineStyleBuilder = new LineStyleBuilder();
            lineStyleBuilder.LineJoinType = LineJoinType.LineJoinTypeNone;
            lineStyleBuilder.Width = 1f;
            lineStyleBuilder.Color = new Carto.Graphics.Color(246, 236, 86, 255);
            return lineStyleBuilder;
        }

        public static LineStyleBuilder GetTrunkStyle1()
        {
            var lineStyleBuilder = new LineStyleBuilder();
            lineStyleBuilder.LineJoinType = LineJoinType.LineJoinTypeNone;
            lineStyleBuilder.Width = 4f;
            lineStyleBuilder.Color = new Carto.Graphics.Color(235, 156, 156, 255);
            return lineStyleBuilder;
        }

        public static LineStyleBuilder GetTrunkStyle2()
        {
            var lineStyleBuilder = new LineStyleBuilder();
            lineStyleBuilder.LineJoinType = LineJoinType.LineJoinTypeNone;
            lineStyleBuilder.Width = 1f;
            lineStyleBuilder.Color = new Carto.Graphics.Color(244, 242, 241, 255);
            return lineStyleBuilder;
        }

        public static LineStyleBuilder GetPrimaryStyle()
        {
            var lineStyleBuilder = new LineStyleBuilder();
            lineStyleBuilder.LineJoinType = LineJoinType.LineJoinTypeNone;
            lineStyleBuilder.Width = 4f;
            lineStyleBuilder.Color = new Carto.Graphics.Color(226, 114, 114, 255);
            return lineStyleBuilder;
        }

        public static LineStyleBuilder GetSecondaryStyle()
        {
            var lineStyleBuilder = new LineStyleBuilder();
            lineStyleBuilder.LineJoinType = LineJoinType.LineJoinTypeNone;
            lineStyleBuilder.Width = 4f;
            lineStyleBuilder.Color = new Carto.Graphics.Color(246, 232, 86, 255);
            return lineStyleBuilder;
        }

        public static LineStyleBuilder GetTertiaryStyle()
        {
            var lineStyleBuilder = new LineStyleBuilder();
            lineStyleBuilder.LineJoinType = LineJoinType.LineJoinTypeNone;
            lineStyleBuilder.Width = 3f;
            lineStyleBuilder.Color = new Carto.Graphics.Color(255, 255, 179, 255);
            return lineStyleBuilder;
        }

        public static LineStyleBuilder GetUnclassifiedStyle()
        {
            var lineStyleBuilder = new LineStyleBuilder();
            lineStyleBuilder.LineJoinType = LineJoinType.LineJoinTypeNone;
            lineStyleBuilder.Width = 2f;
            lineStyleBuilder.Color = new Carto.Graphics.Color(255, 255, 255, 255);
            return lineStyleBuilder;
        }

        public static LineStyleBuilder GetRiverStyle()
        {
            var lineStyleBuilder = new LineStyleBuilder();
            lineStyleBuilder.LineJoinType = LineJoinType.LineJoinTypeNone;
            lineStyleBuilder.Width = 2f;
            lineStyleBuilder.Color = new Carto.Graphics.Color(181, 208, 208, 255);
            return lineStyleBuilder;
        }
    }
}