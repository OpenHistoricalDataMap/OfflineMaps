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

        private static readonly LocalVectorDataSource dataSourceBuildings =
           new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourceBuildingsText =
            new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourceShops =
         new LocalVectorDataSource(projection, LocalSpatialIndexType.LocalSpatialIndexTypeKdtree);
        private static readonly LocalVectorDataSource dataSourceShopsText =
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
            DrawBuildings();
            EndMap();
        }
        private static void EndMap()
        {
            //MessageBox content to be rendered
            MapEvent(null, new MapEventArgs(0, 0, "End"));
        }

        private static void DrawPoints()
        {
            //MessageBox content to be rendered
            MapEvent(null, new MapEventArgs(0, 100, "Calling points"));
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
            mapView.FocusPos = projection.FromWgs84(new MapPos(points[0].pointList.coordinates[0],
                        points[0].pointList.coordinates[1]));
            int i = 0;
            //Objects are drawn on layer
            foreach (var obj in points)
            {
                if (i > 500) continue;
                var pt = new Point(
                projection.FromWgs84(new MapPos(obj.pointList.coordinates[0],
                obj.pointList.coordinates[1])), Styles.GetDefaultPointStyle().BuildStyle());
                v.Add(pt);
                var textpopup1 = new Text(
                    projection.FromWgs84(new MapPos(obj.pointList.coordinates[0], obj.pointList.coordinates[1])),
                    Styles.GetDefaultTextStyle().BuildStyle(),
                    obj.name);
                vText.Add(textpopup1);
                MapEvent(null, new MapEventArgs(i, points.Count, "Drawing points"));
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
            MapEvent(null, new MapEventArgs(0, 100, "Calling type motorway.."));
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
            mapView.FocusPos = projection.FromWgs84(new MapPos(motorways[0].lineList.coordinates[0][0], motorways[0].lineList.coordinates[0][1]));
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
                var textpopup1 = new Text(projection.FromWgs84(new MapPos(obj.lineList.coordinates[0][0], obj.lineList.coordinates[0][1])),
                    Styles.GetDefaultTextStyle().BuildStyle(),
                    obj.name);
                vText.Add(textpopup1);
                MapEvent(null, new MapEventArgs(i, motorways.Count, "Drawing type motorway..."));
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
            MapEvent(null, new MapEventArgs(0, 100, "Calling type trunk..."));
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
            mapView.FocusPos = projection.FromWgs84(new MapPos(trunks[0].lineList.coordinates[0][0], trunks[0].lineList.coordinates[0][1]));
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
                var textpopup1 = new Text(projection.FromWgs84(new MapPos(obj.lineList.coordinates[0][0], obj.lineList.coordinates[0][1])),
                    Styles.GetDefaultTextStyle().BuildStyle(),
                    obj.name);
                vText.Add(textpopup1);
                MapEvent(null, new MapEventArgs(i, trunks.Count, "Drawing type trunk..."));
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
            MapEvent(null, new MapEventArgs(0, 100, "Calling type primary highway..."));
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
                MapEvent(null, new MapEventArgs(i, primaries.Count, "Drawing type primary highway..."));
                i++;
            }
            dataSourcePrimaries.RemoveAll(dataSourcePrimaries.GetAll());
            dataSourcePrimaries.AddAll(v);
            dataSourcePrimariesText.RemoveAll(dataSourcePrimariesText.GetAll());
            dataSourcePrimariesText.AddAll(vText);
        }

        private static void DrawSecondaries()
        {
            MapEvent(null, new MapEventArgs(0, 100, "Calling type secondary highway..."));
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
                MapEvent(null, new MapEventArgs(i, secondaries.Count, "Drawing type secondary highway..."));
                i++;
            }
            dataSourceSecondaries.RemoveAll(dataSourceSecondaries.GetAll());
            dataSourceSecondaries.AddAll(v);
            dataSourceSecondariesText.RemoveAll(dataSourceSecondariesText.GetAll());
            dataSourceSecondariesText.AddAll(vText);
        }

        private static void DrawTertiaries()
        {
            MapEvent(null, new MapEventArgs(0, 100, "Calling type tertiary highway..."));
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
                MapEvent(null, new MapEventArgs(i, tertiaries.Count, "Drawing type tertiary highway..."));
                i++;
            }
            dataSourceTertiaries.RemoveAll(dataSourceTertiaries.GetAll());
            dataSourceTertiaries.AddAll(v);
            dataSourceTertiariesText.RemoveAll(dataSourceTertiariesText.GetAll());
            dataSourceTertiariesText.AddAll(vText);
        }

        private static void DrawUnclassifieds()
        {
            MapEvent(null, new MapEventArgs(0, 100, "Calling type unclassified highway..."));
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
                MapEvent(null, new MapEventArgs(i, unclassifieds.Count, "Drawing type unclassified highway..."));
                i++;
            }
            dataSourceUnclassifieds.RemoveAll(dataSourceUnclassifieds.GetAll());
            dataSourceUnclassifieds.AddAll(v);
            dataSourceUnclassifiedsText.RemoveAll(dataSourceUnclassifiedsText.GetAll());
            dataSourceUnclassifiedsText.AddAll(vText);
        }

        private static void DrawRivers()
        {
            MapEvent(null, new MapEventArgs(0, 100, "Calling type waterway river..."));
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
                MapEvent(null, new MapEventArgs(i, rivers.Count, "Drawing type waterway river..."));
                i++;
            }
            dataSourceRivers.RemoveAll(dataSourceRivers.GetAll());
            dataSourceRivers.AddAll(v);
            dataSourceRiversText.RemoveAll(dataSourceRiversText.GetAll());
            dataSourceRiversText.AddAll(vText);
        }

        private static void DrawLines()
        {
            MapEvent(null, new MapEventArgs(0, 100, "Calling lines..."));
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
                MapEvent(null, new MapEventArgs(i, lines.Count, "Drawing lines..."));
                i++;
            }
            dataSourceLines.RemoveAll(dataSourceLines.GetAll());
            dataSourceLines.AddAll(v);
            dataSourceLinesText.RemoveAll(dataSourceLinesText.GetAll());
            dataSourceLinesText.AddAll(vText);
        }

        private static void DrawPolygons()
        {
            MapEvent(null, new MapEventArgs(0, 100, "Calling polygons..."));
            var polygons = d.getObjects(day, 3);
            var overlayLayer = new VectorLayer(dataSourcePolygons);
            overlayLayer.VisibleZoomRange = new MapRange(16, 24);
            mapView.Layers.Add(overlayLayer);
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
                v.Add(polygon);
                if (i == 0)
                    mapView.FocusPos = projection.FromWgs84(new MapPos(obj.polygonList.coordinates[0][0][0], obj.polygonList.coordinates[0][0][1]));
                MapEvent(null, new MapEventArgs(i, polygons.Count, "Drawing polygons..."));
                i++;
            }
            dataSourcePolygons.RemoveAll(dataSourcePolygons.GetAll());
            dataSourcePolygons.AddAll(v);
            }

        private static void DrawWaters()
        {
            MapEvent(null, new MapEventArgs(0, 100, "Calling type natural water..."));
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
                v.Add(polygon);
                if (i == 0)
                    mapView.FocusPos = projection.FromWgs84(new MapPos(obj.polygonList.coordinates[0][0][0], obj.polygonList.coordinates[0][0][1]));
                var textpopup1 = new Text(projection.FromWgs84(new MapPos(obj.polygonList.coordinates[0][0][0], obj.polygonList.coordinates[0][0][1])),
                    Styles.GetDefaultTextStyle().BuildStyle(),
                    obj.name);
                vText.Add(textpopup1);
                MapEvent(null, new MapEventArgs(i, waters.Count, "Drawing type natural water..."));
                i++;
            }
            dataSourceWaters.RemoveAll(dataSourceWaters.GetAll());
            dataSourceWaters.AddAll(v);
            dataSourceWatersText.RemoveAll(dataSourceWatersText.GetAll());
            dataSourceWatersText.AddAll(vText);
        }

        private static void DrawForests()
        {
            MapEvent(null, new MapEventArgs(0, 100, "Calling type landuse forest ..."));
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
                MapEvent(null, new MapEventArgs(i, forests.Count, "Drawing Type landuse forest..."));
                i++;
            }
            dataSourceForests.RemoveAll(dataSourceForests.GetAll());
            dataSourceForests.AddAll(v);
            dataSourceForestsText.RemoveAll(dataSourceForestsText.GetAll());
            dataSourceForestsText.AddAll(vText);
        }

        private static void DrawBuildings()
        {
            MapEvent(null, new MapEventArgs(0, 100, "Calling type building.."));
            var Buildings = d.getObjectsByClasseName(day, 3, "building");
            var overlayLayer = new VectorLayer(dataSourceBuildings);
            overlayLayer.VisibleZoomRange = new MapRange(16, 24);
            mapView.Layers.Add(overlayLayer);
            var overlayLayerText = new VectorLayer(dataSourceBuildingsText);
            overlayLayerText.VisibleZoomRange = new MapRange(10, 24);
            mapView.Layers.Add(overlayLayerText);
            VectorElementVector v = new VectorElementVector();
            VectorElementVector vText = new VectorElementVector();
            int i = 0;
            foreach (var obj in Buildings)
            {
                //Define coordinates of outer ring
                MapPosVector polygonPoses = new MapPosVector();
                foreach (var pnt in obj.polygonList.coordinates[0])
                {
                    polygonPoses.Add(projection.FromWgs84(new MapPos(pnt[0], pnt[1])));
                }
                Polygon polygon = new Polygon(polygonPoses, Styles.GetBuildingsStyle().BuildStyle());
                v.Add(polygon);
                if (i == 0)
                    mapView.FocusPos = projection.FromWgs84(new MapPos(obj.polygonList.coordinates[0][0][0], obj.polygonList.coordinates[0][0][1]));
                var textpopup1 = new Text(projection.FromWgs84(new MapPos(obj.polygonList.coordinates[0][0][0], obj.polygonList.coordinates[0][0][1])),
                    Styles.GetDefaultTextStyle().BuildStyle(),
                    obj.name);
                vText.Add(textpopup1);
                MapEvent(null, new MapEventArgs(i, Buildings.Count, "Drawing Type Buildings..."));
                i++;
            }
            dataSourceForests.RemoveAll(dataSourceBuildings.GetAll());
            dataSourceForests.AddAll(v);
            dataSourceForestsText.RemoveAll(dataSourceBuildingsText.GetAll());
            dataSourceForestsText.AddAll(vText);
        }

        private static void DrawShops()
        {
            MapEvent(null, new MapEventArgs(0, 100, "Calling Type Shops ..."));
            var Shops = d.getObjectsByClasseName(day, 3, "shop");
            var overlayLayer = new VectorLayer(dataSourceShops);
            overlayLayer.VisibleZoomRange = new MapRange(16, 24);
            mapView.Layers.Add(overlayLayer);
            var overlayLayerText = new VectorLayer(dataSourceShopsText);
            overlayLayerText.VisibleZoomRange = new MapRange(10, 24);
            mapView.Layers.Add(overlayLayerText);
            VectorElementVector v = new VectorElementVector();
            VectorElementVector vText = new VectorElementVector();
            int i = 0;
            foreach (var obj in Shops)
            {
                //Define coordinates of outer ring
                MapPosVector polygonPoses = new MapPosVector();
                foreach (var pnt in obj.polygonList.coordinates[0])
                {
                    polygonPoses.Add(projection.FromWgs84(new MapPos(pnt[0], pnt[1])));
                }
                Polygon polygon = new Polygon(polygonPoses, Styles.GetShopsStyle().BuildStyle());
                v.Add(polygon);
                if (i == 0)
                    mapView.FocusPos = projection.FromWgs84(new MapPos(obj.polygonList.coordinates[0][0][0], obj.polygonList.coordinates[0][0][1]));
                var textpopup1 = new Text(projection.FromWgs84(new MapPos(obj.polygonList.coordinates[0][0][0], obj.polygonList.coordinates[0][0][1])),
                    Styles.GetDefaultTextStyle().BuildStyle(),
                    obj.name);
                vText.Add(textpopup1);
                MapEvent(null, new MapEventArgs(i, Shops.Count, "Drawing Type Shops..."));
                i++;
            }
            dataSourceForests.RemoveAll(dataSourceShops.GetAll());
            dataSourceForests.AddAll(v);
            dataSourceForestsText.RemoveAll(dataSourceShopsText.GetAll());
            dataSourceForestsText.AddAll(vText);
        }

    }
}
    