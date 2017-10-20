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
using Carto.Core;
using Carto.DataSources;
using Carto.Layers;
using Carto.Projections;
using Carto.Ui;
using Carto.VectorElements;

namespace OHDMApp
{
    public delegate void MapEventHandler(object source, MapEventArgs e);

    //EventArgs of the Event, that will be fired to show the status of the drawing of objects
    public class MapEventArgs : EventArgs
    {
        private int active;
        private int max;
        private string message;
        public MapEventArgs(int active, int max, string message)
        {
            this.active = active;
            this.max = max;
            this.message = message;
        }
        public int GetActive()
        {
            return active;
        }

        public int GetMax()
        {
            return max;
        }

        public string GetMessage()
        {
            return message;
        }
    }

    //Class, that is used to draw the objects on the Map
    public static class MapDrawer
    {
        public static event MapEventHandler MapEvent;
        private static readonly DatabaseHelper d = new DatabaseHelper();
        private static DateTime day;
        private static readonly EPSG3857 projection = new EPSG3857();
        private static MapView mapView;
        //Afterwards are all different datasources for the different layers of objects declared
        private static readonly LocalVectorDataSource dataSourcePoints =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourcePointsText =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourceMotorways =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourceMotorways2 =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourceMotorwaysText =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourceTrunks =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourceTrunks2 =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourceTrunksText =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourcePrimaries =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourcePrimariesText =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourceSecondaries =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourceSecondariesText =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourceTertiaries =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourceTertiariesText =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourceUnclassifieds =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourceUnclassifiedsText =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourceRivers =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourceRiversText =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourceLines =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourceLinesText =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourcePolygons =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourcePolygonsText =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourceWaters =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourceWatersText =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourceForests =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourceForestsText =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);

        public static void DrawMap(DateTime day, MapView mapView)
        {
            //The historical day of which the map should be rendered is set
            MapDrawer.day = day;
            //The MapView-Object of the MainActivity
            MapDrawer.mapView = mapView;
            //All classes of objects are drawn
            DrawPolygons();
            DrawPoints();
            DrawWaters();
            DrawForests();
            DrawLines();
            DrawRivers();
            DrawUnclassifieds();
            DrawTertiaries();
            DrawSecondaries();
            DrawPrimaries();
            DrawTrunks();
            DrawMotorways();
        }

        private static void DrawPoints()
        {
            //MessageBox content to be rendered
            MapEvent(null, new MapEventArgs(0, 100, "Frage Punkte ab..."));
            //Objects are pulled from SQLite database
            var points = d.getObjects(day, 1);
            //Layers and the ZoomLevels where they should be visible are created (Zoomlevels range from 0 to 24)
            var overlayLayer = new VectorLayer(dataSourcePoints);
            overlayLayer.VisibleZoomRange = new MapRange(17, 24);
            mapView.Layers.Add(overlayLayer);
            var overlayLayerText = new VectorLayer(dataSourcePointsText);
            overlayLayerText.VisibleZoomRange = new MapRange(17, 24);
            mapView.Layers.Add(overlayLayerText);
            VectorElementVector v = new VectorElementVector();
            VectorElementVector vText = new VectorElementVector();
            int i = 0;
            //Objects are drawn on layer
            foreach (var obj in points)
            {
                var pt = new Point(
                    projection.FromWgs84(new MapPos(obj.pointList.coordinates[0],
                        obj.pointList.coordinates[1])), Styles.GetDefaultPointStyle().BuildStyle());
                //pt.SetMetaDataElement("ClickText", obj.name);

                v.Add(pt);
                if (i == 0)
                    mapView.FocusPos = projection.FromWgs84(new MapPos(obj.pointList.coordinates[0],
                        obj.pointList.coordinates[1]));
                var textpopup1 = new Text(
                    projection.FromWgs84(new MapPos(obj.pointList.coordinates[0], obj.pointList.coordinates[1])),
                    Styles.GetDefaultTextStyle().BuildStyle(),
                    obj.name);
                vText.Add(textpopup1);
                MapEvent(null, new MapEventArgs(i, points.Count, "Zeichne Punkte..."));
                i++;
            }
            //Layers are cleared of old data and written with new data
            dataSourcePoints.RemoveAll(dataSourcePoints.GetAll());
            dataSourcePoints.AddAll(v);
            dataSourcePointsText.RemoveAll(dataSourcePointsText.GetAll());
            dataSourcePointsText.AddAll(vText);
        }

        private static void DrawMotorways()
        {
            MapEvent(null, new MapEventArgs(0, 100, "Frage Typ highway motorway ab..."));
            var motorways = d.getObjects(day, 2, 556);
            var overlayLayer = new VectorLayer(dataSourceMotorways);
            overlayLayer.VisibleZoomRange = new MapRange(6, 24);
            mapView.Layers.Add(overlayLayer);
            var overlayLayer2 = new VectorLayer(dataSourceMotorways2);
            overlayLayer2.VisibleZoomRange = new MapRange(9, 24);
            mapView.Layers.Add(overlayLayer2);
            var overlayLayerText = new VectorLayer(dataSourceMotorwaysText);
            overlayLayerText.VisibleZoomRange = new MapRange(17, 24);
            mapView.Layers.Add(overlayLayerText);
            VectorElementVector v = new VectorElementVector();
            VectorElementVector v2 = new VectorElementVector();
            VectorElementVector vText = new VectorElementVector();
            int i = 0;
            foreach (var obj in motorways)
            {
                var linePoses = new MapPosVector();
                foreach (var pnt in obj.lineList.coordinates)
                {
                    linePoses.Add(projection.FromWgs84(new MapPos(pnt[0], pnt[1])));
                }
                var line = new Line(linePoses, Styles.GetMotorwayStyle1().BuildStyle());
                v.Add(line);
                var line2 = new Line(linePoses, Styles.GetMotorwayStyle2().BuildStyle());
                v2.Add(line2);
                if (i == 0)
                    mapView.FocusPos = projection.FromWgs84(new MapPos(obj.lineList.coordinates[0][0], obj.lineList.coordinates[0][1]));
                var textpopup1 = new Text(projection.FromWgs84(new MapPos(obj.lineList.coordinates[0][0], obj.lineList.coordinates[0][1])),
                    Styles.GetDefaultTextStyle().BuildStyle(),
                    obj.name);
                vText.Add(textpopup1);
                MapEvent(null, new MapEventArgs(i, motorways.Count, "Zeichne Typ highway motorway..."));
                i++;
            }
            dataSourceMotorways.RemoveAll(dataSourceMotorways.GetAll());
            dataSourceMotorways.AddAll(v);
            dataSourceMotorways2.RemoveAll(dataSourceMotorways2.GetAll());
            dataSourceMotorways2.AddAll(v2);
            dataSourceMotorwaysText.RemoveAll(dataSourceMotorwaysText.GetAll());
            dataSourceMotorwaysText.AddAll(vText);
        }

        private static void DrawTrunks()
        {
            MapEvent(null, new MapEventArgs(0, 100, "Frage Typ highway trunk ab..."));
            var trunks = d.getObjects(day, 2, 557);
            var overlayLayer = new VectorLayer(dataSourceTrunks);
            overlayLayer.VisibleZoomRange = new MapRange(6, 24);
            mapView.Layers.Add(overlayLayer);
            var overlayLayer2 = new VectorLayer(dataSourceTrunks2);
            overlayLayer2.VisibleZoomRange = new MapRange(9, 24);
            mapView.Layers.Add(overlayLayer2);
            var overlayLayerText = new VectorLayer(dataSourceTrunksText);
            overlayLayerText.VisibleZoomRange = new MapRange(17, 24);
            mapView.Layers.Add(overlayLayerText);
            VectorElementVector v = new VectorElementVector();
            VectorElementVector v2 = new VectorElementVector();
            VectorElementVector vText = new VectorElementVector();
            int i = 0;
            foreach (var obj in trunks)
            {
                var linePoses = new MapPosVector();
                foreach (var pnt in obj.lineList.coordinates)
                {
                    linePoses.Add(projection.FromWgs84(new MapPos(pnt[0], pnt[1])));
                }
                var line = new Line(linePoses, Styles.GetTrunkStyle1().BuildStyle());
                v.Add(line);
                var line2 = new Line(linePoses, Styles.GetTrunkStyle2().BuildStyle());
                v2.Add(line2);
                if (i == 0)
                    mapView.FocusPos = projection.FromWgs84(new MapPos(obj.lineList.coordinates[0][0], obj.lineList.coordinates[0][1]));
                var textpopup1 = new Text(projection.FromWgs84(new MapPos(obj.lineList.coordinates[0][0], obj.lineList.coordinates[0][1])),
                    Styles.GetDefaultTextStyle().BuildStyle(),
                    obj.name);
                vText.Add(textpopup1);
                MapEvent(null, new MapEventArgs(i, trunks.Count, "Zeichne Typ highway trunk..."));
                i++;
            }
            dataSourceTrunks.RemoveAll(dataSourceTrunks.GetAll());
            dataSourceTrunks.AddAll(v);
            dataSourceTrunks2.RemoveAll(dataSourceTrunks2.GetAll());
            dataSourceTrunks2.AddAll(v2);
            dataSourceTrunksText.RemoveAll(dataSourceTrunksText.GetAll());
            dataSourceTrunksText.AddAll(vText);
        }

        private static void DrawPrimaries()
        {
            MapEvent(null, new MapEventArgs(0, 100, "Frage Typ highway primary ab..."));
            var primaries = d.getObjects(day, 2, 558);
            var overlayLayer = new VectorLayer(dataSourcePrimaries);
            overlayLayer.VisibleZoomRange = new MapRange(6, 24);
            mapView.Layers.Add(overlayLayer);
            var overlayLayerText = new VectorLayer(dataSourcePrimariesText);
            overlayLayerText.VisibleZoomRange = new MapRange(17, 24);
            mapView.Layers.Add(overlayLayerText);
            VectorElementVector v = new VectorElementVector();
            VectorElementVector vText = new VectorElementVector();
            int i = 0;
            foreach (var obj in primaries)
            {
                var linePoses = new MapPosVector();
                foreach (var pnt in obj.lineList.coordinates)
                {
                    linePoses.Add(projection.FromWgs84(new MapPos(pnt[0], pnt[1])));
                }
                var line = new Line(linePoses, Styles.GetPrimaryStyle().BuildStyle());
                v.Add(line);
                if (i == 0)
                    mapView.FocusPos = projection.FromWgs84(new MapPos(obj.lineList.coordinates[0][0], obj.lineList.coordinates[0][1]));
                var textpopup1 = new Text(projection.FromWgs84(new MapPos(obj.lineList.coordinates[0][0], obj.lineList.coordinates[0][1])),
                    Styles.GetDefaultTextStyle().BuildStyle(),
                    obj.name);
                vText.Add(textpopup1);
                MapEvent(null, new MapEventArgs(i, primaries.Count, "Zeichne Typ highway primary..."));
                i++;
            }
            dataSourcePrimaries.RemoveAll(dataSourcePrimaries.GetAll());
            dataSourcePrimaries.AddAll(v);
            dataSourcePrimariesText.RemoveAll(dataSourcePrimariesText.GetAll());
            dataSourcePrimariesText.AddAll(vText);
        }

        private static void DrawSecondaries()
        {
            MapEvent(null, new MapEventArgs(0, 100, "Frage Typ highway secondary ab..."));
            var secondaries = d.getObjects(day, 2, 559);
            var overlayLayer = new VectorLayer(dataSourceSecondaries);
            overlayLayer.VisibleZoomRange = new MapRange(9, 24);
            mapView.Layers.Add(overlayLayer);
            var overlayLayerText = new VectorLayer(dataSourceSecondariesText);
            overlayLayerText.VisibleZoomRange = new MapRange(17, 24);
            mapView.Layers.Add(overlayLayerText);
            VectorElementVector v = new VectorElementVector();
            VectorElementVector vText = new VectorElementVector();
            int i = 0;
            foreach (var obj in secondaries)
            {
                var linePoses = new MapPosVector();
                foreach (var pnt in obj.lineList.coordinates)
                {
                    linePoses.Add(projection.FromWgs84(new MapPos(pnt[0], pnt[1])));
                }
                var line = new Line(linePoses, Styles.GetSecondaryStyle().BuildStyle());
                v.Add(line);
                if (i == 0)
                    mapView.FocusPos = projection.FromWgs84(new MapPos(obj.lineList.coordinates[0][0], obj.lineList.coordinates[0][1]));
                var textpopup1 = new Text(projection.FromWgs84(new MapPos(obj.lineList.coordinates[0][0], obj.lineList.coordinates[0][1])),
                    Styles.GetDefaultTextStyle().BuildStyle(),
                    obj.name);
                vText.Add(textpopup1);
                MapEvent(null, new MapEventArgs(i, secondaries.Count, "Zeichne Typ highway secondary..."));
                i++;
            }
            dataSourceSecondaries.RemoveAll(dataSourceSecondaries.GetAll());
            dataSourceSecondaries.AddAll(v);
            dataSourceSecondariesText.RemoveAll(dataSourceSecondariesText.GetAll());
            dataSourceSecondariesText.AddAll(vText);
        }

        private static void DrawTertiaries()
        {
            MapEvent(null, new MapEventArgs(0, 100, "Frage Typ highway tertiary ab..."));
            var tertiaries = d.getObjects(day, 2, 560);
            var overlayLayer = new VectorLayer(dataSourceTertiaries);
            overlayLayer.VisibleZoomRange = new MapRange(11, 24);
            mapView.Layers.Add(overlayLayer);
            var overlayLayerText = new VectorLayer(dataSourceTertiariesText);
            overlayLayerText.VisibleZoomRange = new MapRange(17, 24);
            mapView.Layers.Add(overlayLayerText);
            VectorElementVector v = new VectorElementVector();
            VectorElementVector vText = new VectorElementVector();
            int i = 0;
            foreach (var obj in tertiaries)
            {
                var linePoses = new MapPosVector();
                foreach (var pnt in obj.lineList.coordinates)
                {
                    linePoses.Add(projection.FromWgs84(new MapPos(pnt[0], pnt[1])));
                }
                var line = new Line(linePoses, Styles.GetTertiaryStyle().BuildStyle());
                v.Add(line);
                if (i == 0)
                    mapView.FocusPos = projection.FromWgs84(new MapPos(obj.lineList.coordinates[0][0], obj.lineList.coordinates[0][1]));
                var textpopup1 = new Text(projection.FromWgs84(new MapPos(obj.lineList.coordinates[0][0], obj.lineList.coordinates[0][1])),
                    Styles.GetDefaultTextStyle().BuildStyle(),
                    obj.name);
                vText.Add(textpopup1);
                MapEvent(null, new MapEventArgs(i, tertiaries.Count, "Zeichne Typ highway tertiary..."));
                i++;
            }
            dataSourceTertiaries.RemoveAll(dataSourceTertiaries.GetAll());
            dataSourceTertiaries.AddAll(v);
            dataSourceTertiariesText.RemoveAll(dataSourceTertiariesText.GetAll());
            dataSourceTertiariesText.AddAll(vText);
        }

        private static void DrawUnclassifieds()
        {
            MapEvent(null, new MapEventArgs(0, 100, "Frage Typ highway unclassified ab..."));
            var unclassifieds = d.getObjects(day, 2, 561);
            var overlayLayer = new VectorLayer(dataSourceUnclassifieds);
            overlayLayer.VisibleZoomRange = new MapRange(12, 24);
            mapView.Layers.Add(overlayLayer);
            var overlayLayerText = new VectorLayer(dataSourceUnclassifiedsText);
            overlayLayerText.VisibleZoomRange = new MapRange(17, 24);
            mapView.Layers.Add(overlayLayerText);
            VectorElementVector v = new VectorElementVector();
            VectorElementVector vText = new VectorElementVector();
            int i = 0;
            foreach (var obj in unclassifieds)
            {
                var linePoses = new MapPosVector();
                foreach (var pnt in obj.lineList.coordinates)
                {
                    linePoses.Add(projection.FromWgs84(new MapPos(pnt[0], pnt[1])));
                }
                var line = new Line(linePoses, Styles.GetUnclassifiedStyle().BuildStyle());
                v.Add(line);
                if (i == 0)
                    mapView.FocusPos = projection.FromWgs84(new MapPos(obj.lineList.coordinates[0][0], obj.lineList.coordinates[0][1]));
                var textpopup1 = new Text(projection.FromWgs84(new MapPos(obj.lineList.coordinates[0][0], obj.lineList.coordinates[0][1])),
                    Styles.GetDefaultTextStyle().BuildStyle(),
                    obj.name);
                vText.Add(textpopup1);
                MapEvent(null, new MapEventArgs(i, unclassifieds.Count, "Zeichne Typ highway unclassified..."));
                i++;
            }
            dataSourceUnclassifieds.RemoveAll(dataSourceUnclassifieds.GetAll());
            dataSourceUnclassifieds.AddAll(v);
            dataSourceUnclassifiedsText.RemoveAll(dataSourceUnclassifiedsText.GetAll());
            dataSourceUnclassifiedsText.AddAll(vText);
        }

        private static void DrawRivers()
        {
            MapEvent(null, new MapEventArgs(0, 100, "Frage Typ waterway river ab..."));
            var rivers = d.getObjects(day, 2, 744);
            var overlayLayer = new VectorLayer(dataSourceRivers);
            overlayLayer.VisibleZoomRange = new MapRange(9, 24);
            mapView.Layers.Add(overlayLayer);
            var overlayLayerText = new VectorLayer(dataSourceRiversText);
            overlayLayerText.VisibleZoomRange = new MapRange(17, 24);
            mapView.Layers.Add(overlayLayerText);
            VectorElementVector v = new VectorElementVector();
            VectorElementVector vText = new VectorElementVector();
            int i = 0;
            foreach (var obj in rivers)
            {
                var linePoses = new MapPosVector();
                foreach (var pnt in obj.lineList.coordinates)
                {
                    linePoses.Add(projection.FromWgs84(new MapPos(pnt[0], pnt[1])));
                }
                var line = new Line(linePoses, Styles.GetRiverStyle().BuildStyle());
                v.Add(line);
                if (i == 0)
                    mapView.FocusPos = projection.FromWgs84(new MapPos(obj.lineList.coordinates[0][0], obj.lineList.coordinates[0][1]));
                var textpopup1 = new Text(projection.FromWgs84(new MapPos(obj.lineList.coordinates[0][0], obj.lineList.coordinates[0][1])),
                    Styles.GetDefaultTextStyle().BuildStyle(),
                    obj.name);
                vText.Add(textpopup1);
                MapEvent(null, new MapEventArgs(i, rivers.Count, "Zeichne Typ waterway river..."));
                i++;
            }
            dataSourceRivers.RemoveAll(dataSourceRivers.GetAll());
            dataSourceRivers.AddAll(v);
            dataSourceRiversText.RemoveAll(dataSourceRiversText.GetAll());
            dataSourceRiversText.AddAll(vText);
        }

        private static void DrawLines()
        {
            MapEvent(null, new MapEventArgs(0, 100, "Frage Lines ab..."));
            var lines = d.getObjects(day, 2);
            var overlayLayer = new VectorLayer(dataSourceLines);
            overlayLayer.VisibleZoomRange = new MapRange(13, 24);
            mapView.Layers.Add(overlayLayer);
            var overlayLayerText = new VectorLayer(dataSourceLinesText);
            overlayLayerText.VisibleZoomRange = new MapRange(17, 24);
            mapView.Layers.Add(overlayLayerText);
            VectorElementVector v = new VectorElementVector();
            VectorElementVector vText = new VectorElementVector();
            int i = 0;
            foreach (var obj in lines)
            {
                var linePoses = new MapPosVector();
                foreach (var pnt in obj.lineList.coordinates)
                {
                    linePoses.Add(projection.FromWgs84(new MapPos(pnt[0], pnt[1])));
                }
                var line = new Line(linePoses, Styles.GetDefaultLineStyle().BuildStyle());
                v.Add(line);
                if (i == 0)
                    mapView.FocusPos = projection.FromWgs84(new MapPos(obj.lineList.coordinates[0][0], obj.lineList.coordinates[0][1]));
                var textpopup1 = new Text(projection.FromWgs84(new MapPos(obj.lineList.coordinates[0][0], obj.lineList.coordinates[0][1])),
                    Styles.GetDefaultTextStyle().BuildStyle(),
                    obj.name);
                vText.Add(textpopup1);
                MapEvent(null, new MapEventArgs(i, lines.Count, "Zeichne Lines..."));
                i++;
            }
            dataSourceLines.RemoveAll(dataSourceLines.GetAll());
            dataSourceLines.AddAll(v);
            dataSourceLinesText.RemoveAll(dataSourceLinesText.GetAll());
            dataSourceLinesText.AddAll(vText);
        }

        private static void DrawPolygons()
        {
            MapEvent(null, new MapEventArgs(0, 100, "Frage Polygone ab..."));
            var polygons = d.getObjects(day, 3);
            var overlayLayer = new VectorLayer(dataSourcePolygons);
            overlayLayer.VisibleZoomRange = new MapRange(16, 24);
            mapView.Layers.Add(overlayLayer);
            //var overlayLayerText = new VectorLayer(dataSourcePolygonsText);
            //overlayLayerText.VisibleZoomRange = new MapRange(17, 24);
            //mapView.Layers.Add(overlayLayerText);
            VectorElementVector v = new VectorElementVector();
            VectorElementVector vText = new VectorElementVector();
            int i = 0;
            foreach (var obj in polygons)
            {
               //Define coordinates of outer ring
               MapPosVector polygonPoses = new MapPosVector();
                foreach (var pnt in obj.polygonList.coordinates[0])
                {
                    polygonPoses.Add(projection.FromWgs84(new MapPos(pnt[0], pnt[1])));
                }
                Polygon polygon = new Polygon(polygonPoses, Styles.GetDefaultPolygonStyle().BuildStyle());
                //polygon.SetMetaDataElement("ClickText", obj.name);
                v.Add(polygon);
                if (i == 0)
                    mapView.FocusPos = projection.FromWgs84(new MapPos(obj.polygonList.coordinates[0][0][0], obj.polygonList.coordinates[0][0][1]));
                //var textpopup1 = new Text(projection.FromWgs84(new MapPos(obj.polygonList.coordinates[0][0][0], obj.polygonList.coordinates[0][0][1])),
                //    Styles.GetDefaultTextStyle().BuildStyle(),
                //    obj.name);
                //vText.Add(textpopup1);
                MapEvent(null, new MapEventArgs(i, polygons.Count, "Zeichne Polygone..."));
                i++;
            }
            dataSourcePolygons.RemoveAll(dataSourcePolygons.GetAll());
            dataSourcePolygons.AddAll(v);
            //dataSourcePolygonsText.RemoveAll(dataSourcePolygonsText.GetAll());
            //dataSourcePolygonsText.AddAll(vText);
        }

        private static void DrawWaters()
        {
            MapEvent(null, new MapEventArgs(0, 100, "Frage Typ natural water ab..."));
            var waters = d.getObjects(day, 3, 672);
            var overlayLayer = new VectorLayer(dataSourceWaters);
            overlayLayer.VisibleZoomRange = new MapRange(6, 24);
            mapView.Layers.Add(overlayLayer);
            var overlayLayerText = new VectorLayer(dataSourceWatersText);
            overlayLayerText.VisibleZoomRange = new MapRange(17, 24);
            mapView.Layers.Add(overlayLayerText);
            VectorElementVector v = new VectorElementVector();
            VectorElementVector vText = new VectorElementVector();
            int i = 0;
            foreach (var obj in waters)
            {
                //Define coordinates of outer ring
                MapPosVector polygonPoses = new MapPosVector();
                foreach (var pnt in obj.polygonList.coordinates[0])
                {
                    polygonPoses.Add(projection.FromWgs84(new MapPos(pnt[0], pnt[1])));
                }
                Polygon polygon = new Polygon(polygonPoses, Styles.GetDefaultWaterStyle().BuildStyle());
                //polygon.SetMetaDataElement("ClickText", obj.name);
                v.Add(polygon);
                if (i == 0)
                    mapView.FocusPos = projection.FromWgs84(new MapPos(obj.polygonList.coordinates[0][0][0], obj.polygonList.coordinates[0][0][1]));
                var textpopup1 = new Text(projection.FromWgs84(new MapPos(obj.polygonList.coordinates[0][0][0], obj.polygonList.coordinates[0][0][1])),
                    Styles.GetDefaultTextStyle().BuildStyle(),
                    obj.name);
                vText.Add(textpopup1);
                MapEvent(null, new MapEventArgs(i, waters.Count, "Zeichne Typ natural water..."));
                i++;
            }
            dataSourceWaters.RemoveAll(dataSourceWaters.GetAll());
            dataSourceWaters.AddAll(v);
            dataSourceWatersText.RemoveAll(dataSourceWatersText.GetAll());
            dataSourceWatersText.AddAll(vText);
        }

        private static void DrawForests()
        {
            MapEvent(null, new MapEventArgs(0, 100, "Frage Typ landuse forest ab..."));
            var forests = d.getObjects(day, 3, 472);
            var overlayLayer = new VectorLayer(dataSourceForests);
            overlayLayer.VisibleZoomRange = new MapRange(6, 24);
            mapView.Layers.Add(overlayLayer);
            var overlayLayerText = new VectorLayer(dataSourceForestsText);
            overlayLayerText.VisibleZoomRange = new MapRange(17, 24);
            mapView.Layers.Add(overlayLayerText);
            VectorElementVector v = new VectorElementVector();
            VectorElementVector vText = new VectorElementVector();
            int i = 0;
            foreach (var obj in forests)
            {
                //Define coordinates of outer ring
                MapPosVector polygonPoses = new MapPosVector();
                foreach (var pnt in obj.polygonList.coordinates[0])
                {
                    polygonPoses.Add(projection.FromWgs84(new MapPos(pnt[0], pnt[1])));
                }
                Polygon polygon = new Polygon(polygonPoses, Styles.GetDefaultForestStyle().BuildStyle());
                //polygon.SetMetaDataElement("ClickText", obj.name);
                v.Add(polygon);
                if (i == 0)
                    mapView.FocusPos = projection.FromWgs84(new MapPos(obj.polygonList.coordinates[0][0][0], obj.polygonList.coordinates[0][0][1]));
                var textpopup1 = new Text(projection.FromWgs84(new MapPos(obj.polygonList.coordinates[0][0][0], obj.polygonList.coordinates[0][0][1])),
                    Styles.GetDefaultTextStyle().BuildStyle(),
                    obj.name);
                vText.Add(textpopup1);
                MapEvent(null, new MapEventArgs(i, forests.Count, "Zeichne Typ landuse forest..."));
                i++;
            }
            dataSourceForests.RemoveAll(dataSourceForests.GetAll());
            dataSourceForests.AddAll(v);
            dataSourceForestsText.RemoveAll(dataSourceForestsText.GetAll());
            dataSourceForestsText.AddAll(vText);
        }
    }
}
    