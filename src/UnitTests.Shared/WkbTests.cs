using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Esri.ArcGISRuntime.Geometry;
using GeometryConversions.Wkb;
using System.Reflection;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace UnitTests
{
    [TestClass]
    public class WkbTests
    {
		[TestMethod]
		public void WkbConvertMapPointToFromXY()
		{
			MapPoint mp = new MapPoint(12, 34);
			byte[] bytes = mp.ToWellKnownBinary();
			var geom = bytes.FromWellKnownBinary();
			Assert.IsNotNull(geom);
			Assert.IsInstanceOfType(geom, typeof(MapPoint));
			var mp2 = (MapPoint)geom;
			Assert.AreEqual(12, mp2.X);
			Assert.AreEqual(34, mp2.Y);
			Assert.IsFalse(mp2.HasZ);
			Assert.IsFalse(mp2.HasM);
		}
		[TestMethod]
		public void WkbConvertMapPointToFromXYZ()
		{
			MapPoint mp = new MapPoint(12, 34, 56);
			byte[] bytes = mp.ToWellKnownBinary();
			var geom = bytes.FromWellKnownBinary();
			Assert.IsNotNull(geom);
			Assert.IsInstanceOfType(geom, typeof(MapPoint));
			var mp2 = (MapPoint)geom;
			Assert.AreEqual(12, mp2.X);
			Assert.AreEqual(34, mp2.Y);
			Assert.AreEqual(56, mp2.Z);
            Assert.IsFalse(mp2.HasM);
        }
		[TestMethod]
		public void WkbConvertMapPointToFromXYM()
		{
			MapPoint mp = MapPoint.CreateWithM(12, 34, 56, SpatialReferences.Wgs84);
			byte[] bytes = mp.ToWellKnownBinary();
			var geom = bytes.FromWellKnownBinary(mp.SpatialReference);
			Assert.IsNotNull(geom);
			Assert.AreEqual(SpatialReferences.Wgs84, geom.SpatialReference);
			Assert.IsInstanceOfType(geom, typeof(MapPoint));
			var mp2 = (MapPoint)geom;
			Assert.AreEqual(12, mp2.X);
			Assert.AreEqual(34, mp2.Y);
            Assert.IsFalse(mp2.HasZ);
            Assert.AreEqual(56, mp2.M);
		}

        [TestMethod]
		public void WkbConvertMapPointToFromXYZM()
        {
			MapPoint mp = MapPoint.CreateWithM(12, 34, 56, 78);
			byte[] bytes = mp.ToWellKnownBinary();
			var geom = bytes.FromWellKnownBinary();
			Assert.IsNotNull(geom);
			Assert.IsInstanceOfType(geom, typeof(MapPoint));
			var mp2 = (MapPoint)geom;
			Assert.AreEqual(12, mp2.X);
			Assert.AreEqual(34, mp2.Y);
			Assert.AreEqual(56, mp2.Z);
			Assert.AreEqual(78, mp2.M);
        }

        [TestMethod]
        public void WkbConvertPolygonToFromXY()
        {
            MapPoint mp1 = new MapPoint(11, 12);
            MapPoint mp2 = new MapPoint(21, 22);
            MapPoint mp3 = new MapPoint(31, 32);
            Polygon poly = new Polygon(new [] {mp1, mp2, mp3});
            byte[] bytes = poly.ToWellKnownBinary();
            var geom = bytes.FromWellKnownBinary();
            Assert.IsNotNull(geom);
            Assert.IsInstanceOfType(geom, typeof(Polygon));
            Polygon poly2 = (Polygon)geom;
            Assert.IsFalse(poly2.HasZ);
            Assert.IsFalse(poly2.HasM);
            Assert.AreEqual(1, poly2.Parts.Count);
            Assert.AreEqual(3, poly2.Parts[0].Points.Count);
            Assert.AreEqual(31, poly2.Parts[0].Points[2].X);
            Assert.AreEqual(32, poly2.Parts[0].Points[2].Y);
        }

        [TestMethod]
        public void WkbConvertMultiPolygonToFromXY()
        {
            // Poly with 1 outer and 1 inner ring
            MapPoint mp1o = new MapPoint(1, 2);
            MapPoint mp2o = new MapPoint(1, 200);
            MapPoint mp3o = new MapPoint(100, 200);
            MapPoint mp4o = new MapPoint(100, 2);
            MapPoint mp1i = new MapPoint(3, 40);
            MapPoint mp2i = new MapPoint(30, 40);
            MapPoint mp3i = new MapPoint(30, 4);
            MapPoint mp4i = new MapPoint(3, 4);
            Polygon poly = new Polygon(new[]
                {
                    new[] { mp1o, mp2o, mp3o, mp4o },
                    new[] { mp1i, mp2i, mp3i, mp4i }
                }
            );

            byte[] bytes = poly.ToWellKnownBinary();
            var geom = bytes.FromWellKnownBinary();
            Assert.IsNotNull(geom);
            Assert.IsInstanceOfType(geom, typeof(Polygon));
            Polygon poly2 = (Polygon)geom;
            Assert.IsFalse(poly2.HasZ);
            Assert.IsFalse(poly2.HasM);
            Assert.AreEqual(2, poly2.Parts.Count);
            Assert.AreEqual(4, poly2.Parts[0].Points.Count);
            Assert.AreEqual(4, poly2.Parts[1].Points.Count);
            Assert.AreEqual(1, poly2.Parts[0].Points[0].X);
            Assert.AreEqual(2, poly2.Parts[0].Points[0].Y);
            Assert.AreEqual(3, poly2.Parts[1].Points[3].X);
            Assert.AreEqual(4, poly2.Parts[1].Points[3].Y);
        }
    }
}
