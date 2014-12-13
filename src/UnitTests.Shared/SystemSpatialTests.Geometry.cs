using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using GeometryConversions.SystemSpatial;

#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace UnitTests
{
	[TestClass]
	public class SystemSpatialGeometryTests
	{
		[TestMethod]
		public void SystemSpatialGeometryConvertMapPointToXY()
		{
			MapPoint mp = new MapPoint(12, 34, SpatialReferences.Wgs84);
			var g = mp.ToSystemSpatialGeometry();
			Assert.IsNotNull(g);
			Assert.IsInstanceOfType(g, typeof(System.Spatial.GeometryPoint));
			var p = (System.Spatial.GeometryPoint)g;
			Assert.AreEqual(12, p.X);
			Assert.AreEqual(34, p.Y);
			Assert.IsNull(p.Z);
			Assert.IsNull(p.M);
			Assert.AreEqual(SpatialReferences.Wgs84.Wkid, p.CoordinateSystem.EpsgId);
		}

		[TestMethod]
		public void SystemSpatialGeometryConvertMultipointToXY()
		{
			var mp = new Multipoint(new MapPoint[] { new MapPoint(12, 34), new MapPoint(12, 34, 56) }, SpatialReferences.WebMercator);
			var g = mp.ToSystemSpatialGeometry();
			Assert.IsNotNull(g);
			Assert.IsInstanceOfType(g, typeof(System.Spatial.GeometryMultiPoint));
			var p = (System.Spatial.GeometryMultiPoint)g;
			Assert.AreEqual(2, p.Points.Count);
			Assert.AreEqual(SpatialReferences.WebMercator.Wkid, p.CoordinateSystem.EpsgId);
		}

		[TestMethod]
		public void SystemSpatialGeometryConvertPolylineToLineString()
		{
			PolylineBuilder pb = new PolylineBuilder(SpatialReferences.Wgs84);
			pb.AddPart(new MapPoint[] { new MapPoint(56, 67), new MapPoint(78, 89), new MapPoint(90, 10) });
			var pl = pb.ToGeometry();
			var g = pl.ToSystemSpatialGeometry();
			Assert.IsNotNull(g);
			Assert.IsInstanceOfType(g, typeof(System.Spatial.GeometryLineString));
			var p = (System.Spatial.GeometryLineString)g;
			Assert.AreEqual(3, p.Points.Count);
			Assert.AreEqual(SpatialReferences.Wgs84.Wkid, p.CoordinateSystem.EpsgId);
		}

		[TestMethod]
		public void SystemSpatialGeometryConvertPolylineToMultiLineString()
		{
			PolylineBuilder pb = new PolylineBuilder(SpatialReferences.Wgs84);
			pb.AddPart(new MapPoint[] { new MapPoint(12, 34), new MapPoint(12, 44) });
			pb.AddPart(new MapPoint[] { new MapPoint(56, 67), new MapPoint(78, 89), new MapPoint(90, 10) });
			var pl = pb.ToGeometry();
			var g = pl.ToSystemSpatialGeometry();
			Assert.IsNotNull(g);
			Assert.IsInstanceOfType(g, typeof(System.Spatial.GeometryMultiLineString));
			var p = (System.Spatial.GeometryMultiLineString)g;
			Assert.AreEqual(2, p.LineStrings.Count);
			Assert.AreEqual(2, p.LineStrings[0].Points.Count);
			Assert.AreEqual(3, p.LineStrings[1].Points.Count);
			Assert.AreEqual(SpatialReferences.Wgs84.Wkid, p.CoordinateSystem.EpsgId);
		}


		[TestMethod]
		public void SystemSpatialGeometryConvertPolygonToPolygon()
		{
			PolygonBuilder pb = new PolygonBuilder(SpatialReferences.Wgs84);
			pb.AddPart(new MapPoint[] { new MapPoint(56, 67), new MapPoint(90, 10), new MapPoint(78, 89) });
			var pl = pb.ToGeometry();
			var g = pl.ToSystemSpatialGeometry();
			Assert.IsNotNull(g);
			Assert.IsInstanceOfType(g, typeof(System.Spatial.GeometryPolygon));
			var p = (System.Spatial.GeometryPolygon)g;
			Assert.AreEqual(1, p.Rings.Count);
			Assert.AreEqual(4, p.Rings[0].Points.Count);
			Assert.AreEqual(SpatialReferences.Wgs84.Wkid, p.CoordinateSystem.EpsgId);
		}

		[TestMethod]
		public void SystemSpatialGeometryConvertPolygonToMultiPolygon()
		{
			PolygonBuilder pb = new PolygonBuilder(SpatialReferences.Wgs84);
			pb.AddPart(new MapPoint[] { new MapPoint(0, 0), new MapPoint(1, 0), new MapPoint(1, 1) });
			pb.AddPart(new MapPoint[] { new MapPoint(10, 10), new MapPoint(11, 10), new MapPoint(11, 11), new MapPoint(10, 11) });
			var pl = pb.ToGeometry();
			var g = pl.ToSystemSpatialGeometry();
			Assert.IsNotNull(g);
			Assert.IsInstanceOfType(g, typeof(System.Spatial.GeometryMultiPolygon));
			var p = (System.Spatial.GeometryMultiPolygon)g;
			Assert.AreEqual(2, p.Polygons.Count);
			Assert.AreEqual(1, p.Polygons[0].Rings.Count);
			Assert.AreEqual(1, p.Polygons[1].Rings.Count);
			Assert.AreEqual(SpatialReferences.Wgs84.Wkid, p.CoordinateSystem.EpsgId);
		}
	}
}
