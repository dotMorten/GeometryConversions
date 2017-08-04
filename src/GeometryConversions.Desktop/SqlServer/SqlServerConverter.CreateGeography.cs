using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeometryConversions.SqlServer
{
	internal static partial class SqlServerConverter
	{
		internal static Microsoft.SqlServer.Types.SqlGeography CreateGeography(Esri.ArcGISRuntime.Geometry.Geometry geometry)
		{
			if (geometry == null)
				throw new ArgumentNullException("geometry");
			if (geometry.SpatialReference == null)
				throw new ArgumentException("Spatial reference cannot be null when converting to Geography types");
			int cs = 0;
			if (geometry.SpatialReference != null)
			{
				if (!geometry.SpatialReference.IsGeographic && 
					geometry.SpatialReference.BaseGeographic != null && 
					geometry.SpatialReference.BaseGeographic.Wkid > 0)
					geometry = Esri.ArcGISRuntime.Geometry.GeometryEngine.Project(geometry, geometry.SpatialReference.BaseGeographic);
				if (!geometry.SpatialReference.IsGeographic)
					throw new ArgumentException("Can't convert geometry spatial reference to a supported geographic coordinate system");
				if (geometry.SpatialReference.Wkid > 0)
				{
					cs = geometry.SpatialReference.Wkid;
				}
				else
					throw new ArgumentException("Unsupported geographic coordinate system");
			}
			if (geometry is Esri.ArcGISRuntime.Geometry.MapPoint)
				return CreateGeographyPoint((Esri.ArcGISRuntime.Geometry.MapPoint)geometry, cs);
			if (geometry is Esri.ArcGISRuntime.Geometry.Multipoint)
				return CreateGeographyMultipoint((Esri.ArcGISRuntime.Geometry.Multipoint)geometry, cs);
			if (geometry is Esri.ArcGISRuntime.Geometry.Polyline)
				return CreateGeographyLineString((Esri.ArcGISRuntime.Geometry.Polyline)geometry, cs);
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
				return CreateGeographyPolygon((Esri.ArcGISRuntime.Geometry.Polygon)geometry, cs);
			
			throw new NotImplementedException();
		}

		private static Microsoft.SqlServer.Types.SqlGeography CreateGeographyPoint(Esri.ArcGISRuntime.Geometry.MapPoint p, int cs)
		{
			if (!p.HasZ && !p.HasM)
				return Microsoft.SqlServer.Types.SqlGeography.Point(p.Y, p.X, cs);

			var b = new Microsoft.SqlServer.Types.SqlGeographyBuilder();
			b.SetSrid(cs);
			b.BeginGeography(Microsoft.SqlServer.Types.OpenGisGeographyType.Point);
			b.BeginFigure(p.Y, p.X, p.HasZ ? (double?)p.Z : null,
					p.HasM ? (double?)p.M : null);
			b.EndFigure();
			b.EndGeography();
			return b.ConstructedGeography;
		}

		private static Microsoft.SqlServer.Types.SqlGeography CreateGeographyMultipoint(Esri.ArcGISRuntime.Geometry.Multipoint multipoint, int cs)
		{
			var b = new Microsoft.SqlServer.Types.SqlGeographyBuilder();
			b.SetSrid(cs);
			b.BeginGeography(Microsoft.SqlServer.Types.OpenGisGeographyType.MultiPoint);

			foreach (var p in multipoint.Points)
			{
				b.BeginGeography(Microsoft.SqlServer.Types.OpenGisGeographyType.Point);
				b.BeginFigure(p.Y, p.X,
					p.HasZ ? (double?)p.Z : null,
					p.HasM ? (double?)p.M : null);
				b.EndFigure();
				b.EndGeography();
			}
			b.EndGeography();
			return b.ConstructedGeography;
		}
		
		private static Microsoft.SqlServer.Types.SqlGeography CreateGeographyLineString(Esri.ArcGISRuntime.Geometry.Polyline polyline, int cs)
		{
			var b = new Microsoft.SqlServer.Types.SqlGeographyBuilder();
			b.SetSrid(cs);
			b.BeginGeography(
				polyline.Parts.Count <= 1 ?
				Microsoft.SqlServer.Types.OpenGisGeographyType.LineString :
				Microsoft.SqlServer.Types.OpenGisGeographyType.MultiLineString);

			CreateGeographyFigures(polyline.Parts, b, false);

			b.EndGeography();
			return b.ConstructedGeography;
		}

		private static Microsoft.SqlServer.Types.SqlGeography CreateGeographyPolygon(Esri.ArcGISRuntime.Geometry.Polygon polygon, int cs)
		{
			var b = new Microsoft.SqlServer.Types.SqlGeographyBuilder();
			b.SetSrid(cs);

			var rings = Utilities.SplitMultiPolygon(polygon).ToList();

			if (rings.Count > 1)
			{
				b.BeginGeography(Microsoft.SqlServer.Types.OpenGisGeographyType.MultiPolygon);
			}

			foreach (var outerRing in rings)
			{
				b.BeginGeography(Microsoft.SqlServer.Types.OpenGisGeographyType.Polygon);
				CreateGeographyFigures(new Esri.ArcGISRuntime.Geometry.ReadOnlyPart[] { outerRing.Item1 }.Union(outerRing.Item2), b, true);
				b.EndGeography();
			}

			if (rings.Count > 1)
			{
				b.EndGeography();
			}
			return b.ConstructedGeography;
		}
		
		private static void CreateGeographyFigures(IEnumerable<Esri.ArcGISRuntime.Geometry.ReadOnlyPart> parts,
			Microsoft.SqlServer.Types.SqlGeographyBuilder b, bool close)
		{
			foreach (var part in parts)
			{
				if (part.Count == 0)
					continue;
				var p = part.Points[0];
				b.BeginFigure(p.Y, p.X, p.HasZ ? (double?)p.Z : null, p.HasM ? (double?)p.M : null);
				for (int i = 1; i < part.Count; i++)
				{
					p = part.Points[i];
					b.AddLine(p.Y, p.X, p.HasZ ? (double?)p.Z : null, p.HasM ? (double?)p.M : null);
				}
                if(close)
                {
                    p = part.Points[0];
                    b.AddLine(p.X, p.Y, p.HasZ ? (double?)p.Z : null, p.HasM ? (double?)p.M : null);
                }
				b.EndFigure();
			}
		}
	}
}
