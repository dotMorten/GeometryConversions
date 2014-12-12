using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeometryConversions.SqlServer
{
	internal static partial class SqlServerConverter
	{
		internal static Microsoft.SqlServer.Types.SqlGeometry CreateGeometry(Esri.ArcGISRuntime.Geometry.Geometry geometry)
		{
			if (geometry == null)
				throw new ArgumentNullException("geometry");
			int cs = 0;
			if (geometry.SpatialReference != null && geometry.SpatialReference.Wkid > 0)
				cs = geometry.SpatialReference.Wkid;
			if (geometry is Esri.ArcGISRuntime.Geometry.MapPoint)
				return CreateGeometryPoint((Esri.ArcGISRuntime.Geometry.MapPoint)geometry, cs);
			if (geometry is Esri.ArcGISRuntime.Geometry.Multipoint)
				return CreateGeometryMultipoint((Esri.ArcGISRuntime.Geometry.Multipoint)geometry, cs);
			if (geometry is Esri.ArcGISRuntime.Geometry.Polyline)
				return CreateGeometryLineString((Esri.ArcGISRuntime.Geometry.Polyline)geometry, cs);
			if (geometry is Esri.ArcGISRuntime.Geometry.Envelope) //Convert to polygon
			{
				var env = ((Esri.ArcGISRuntime.Geometry.Envelope)geometry);
				geometry = new Esri.ArcGISRuntime.Geometry.Polygon(new Esri.ArcGISRuntime.Geometry.MapPoint[] {
					new Esri.ArcGISRuntime.Geometry.MapPoint(env.XMin, env.YMax),
					new Esri.ArcGISRuntime.Geometry.MapPoint(env.XMin, env.YMin),
					new Esri.ArcGISRuntime.Geometry.MapPoint(env.XMax, env.YMin),
					new Esri.ArcGISRuntime.Geometry.MapPoint(env.XMax, env.YMax),
					new Esri.ArcGISRuntime.Geometry.MapPoint(env.XMin, env.YMax)
				}, geometry.SpatialReference);
			}
			if (geometry is Esri.ArcGISRuntime.Geometry.Polygon)
				return CreateGeometryPolygon((Esri.ArcGISRuntime.Geometry.Polygon)geometry, cs);
			
			throw new NotImplementedException();
		}

		private static Microsoft.SqlServer.Types.SqlGeometry CreateGeometryPoint(Esri.ArcGISRuntime.Geometry.MapPoint p, int cs)
		{
			if(!p.HasZ && !p.HasM)
				return Microsoft.SqlServer.Types.SqlGeometry.Point(p.X, p.Y, cs);

			var b = new Microsoft.SqlServer.Types.SqlGeometryBuilder();
			if(cs > 0)
				b.SetSrid(cs);
			b.BeginGeometry(Microsoft.SqlServer.Types.OpenGisGeometryType.Point);
			b.BeginFigure(p.X, p.Y, p.HasZ ? (double?)p.Z : null,
					p.HasM ? (double?)p.M : null);
			b.EndGeometry();
			return b.ConstructedGeometry;
		}

		private static Microsoft.SqlServer.Types.SqlGeometry CreateGeometryMultipoint(Esri.ArcGISRuntime.Geometry.Multipoint multipoint, int cs)
		{
			var b = new Microsoft.SqlServer.Types.SqlGeometryBuilder();
			if (cs > 0)
				b.SetSrid(cs);
			b.BeginGeometry(Microsoft.SqlServer.Types.OpenGisGeometryType.MultiPoint);

			foreach (var p in multipoint.Points)
			{
				b.BeginGeometry(Microsoft.SqlServer.Types.OpenGisGeometryType.Point);
				b.BeginFigure(p.X, p.Y, p.HasZ ? (double?)p.Z : null,
					p.HasM ? (double?)p.M : null);
				b.EndFigure();
				b.EndGeometry();
			}
			b.EndGeometry();
			return b.ConstructedGeometry;
		}
		
		private static Microsoft.SqlServer.Types.SqlGeometry CreateGeometryLineString(Esri.ArcGISRuntime.Geometry.Polyline polyline, int cs)
		{
			var b = new Microsoft.SqlServer.Types.SqlGeometryBuilder();
			if (cs > 0)
				b.SetSrid(cs);
			b.BeginGeometry(
				polyline.Parts.Count <= 1 ?
				Microsoft.SqlServer.Types.OpenGisGeometryType.LineString :
				Microsoft.SqlServer.Types.OpenGisGeometryType.MultiLineString);

			CreateGeometryFigures(polyline.Parts, b, false);

			b.EndGeometry();
			return b.ConstructedGeometry;
		}

		private static Microsoft.SqlServer.Types.SqlGeometry CreateGeometryPolygon(Esri.ArcGISRuntime.Geometry.Polygon polygon, int cs)
		{
			var b = new Microsoft.SqlServer.Types.SqlGeometryBuilder();
			if (cs > 0)
				b.SetSrid(cs);

			var rings = Utilities.SplitMultiPolygon(polygon).ToList();

			if (rings.Count > 1)
			{
				b.BeginGeometry(Microsoft.SqlServer.Types.OpenGisGeometryType.MultiPolygon);
			}

			foreach (var outerRing in rings)
			{
				b.BeginGeometry(Microsoft.SqlServer.Types.OpenGisGeometryType.Polygon);
				CreateGeometryFigures(new Esri.ArcGISRuntime.Geometry.ReadOnlySegmentCollection[] { outerRing.Item1 }.Union(outerRing.Item2), b, true);
				b.EndGeometry();
			}

			if (rings.Count > 1)
			{
				b.EndGeometry();
			}
			return b.ConstructedGeometry;
		}
		
		private static void CreateGeometryFigures(IEnumerable<Esri.ArcGISRuntime.Geometry.ReadOnlySegmentCollection> parts,
			Microsoft.SqlServer.Types.SqlGeometryBuilder b, bool close)
		{
			foreach (var part in parts)
			{
				if (part.Count == 0)
					continue;
				var p = part.GetPoint(0);
				b.BeginFigure(p.X, p.Y, p.HasZ ? (double?)p.Z : null,
					p.HasM ? (double?)p.M : null);
				for (int i = 1; i <= part.Count; i++)
				{
					p = part.GetPoint(i);
					b.AddLine(p.X, p.Y, p.HasZ ? (double?)p.Z : null,
					p.HasM ? (double?)p.M : null);
				}
				b.EndFigure();
			}
		}
	}
}
