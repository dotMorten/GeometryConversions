using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeometryConversions.SystemSpatial
{
	internal partial class SystemSpatialConverter
	{
		internal System.Spatial.Geography CreateGeography(Esri.ArcGISRuntime.Geometry.Geometry geometry)
		{
			if (geometry == null)
				throw new ArgumentNullException("geometry");
			System.Spatial.CoordinateSystem cs = null;
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
					cs = System.Spatial.CoordinateSystem.Geometry(geometry.SpatialReference.Wkid);
				}
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

		private System.Spatial.GeographyPoint CreateGeographyPoint(Esri.ArcGISRuntime.Geometry.MapPoint mapPoint, System.Spatial.CoordinateSystem cs)
		{
			return System.Spatial.GeographyPoint.Create(cs, mapPoint.Y, mapPoint.X,
				mapPoint.HasZ ? (double?)mapPoint.Z : null,
				mapPoint.HasM ? (double?)mapPoint.M : null);
		}

		private System.Spatial.GeographyMultiPoint CreateGeographyMultipoint(Esri.ArcGISRuntime.Geometry.Multipoint multipoint, System.Spatial.CoordinateSystem cs)
		{
			var b = System.Spatial.SpatialBuilder.Create();
			if (cs != null)
				b.GeographyPipeline.SetCoordinateSystem(cs);
			b.GeographyPipeline.BeginGeography(System.Spatial.SpatialType.MultiPoint);

			foreach (var p in multipoint.Points)
			{
				b.GeographyPipeline.BeginGeography(System.Spatial.SpatialType.Point);
				b.GeographyPipeline.BeginFigure(ToGeographyPosition(p));
				b.GeographyPipeline.EndFigure();
				b.GeographyPipeline.EndGeography();
			}
			b.GeographyPipeline.EndGeography();
			return (System.Spatial.GeographyMultiPoint)b.ConstructedGeography;
		}


		private System.Spatial.Geography CreateGeographyLineString(Esri.ArcGISRuntime.Geometry.Polyline polyline, System.Spatial.CoordinateSystem cs)
		{
			var b = System.Spatial.SpatialBuilder.Create();
			if (cs != null)
				b.GeographyPipeline.SetCoordinateSystem(cs);
			b.GeographyPipeline.BeginGeography(
				polyline.Parts.Count < 2 ?
				System.Spatial.SpatialType.LineString :
				System.Spatial.SpatialType.MultiLineString);

			CreateGeographyFigures(polyline.Parts, b, false);

			b.GeographyPipeline.EndGeography();
			return b.ConstructedGeography;
		}

		private System.Spatial.Geography CreateGeographyPolygon(Esri.ArcGISRuntime.Geometry.Polygon polygon, System.Spatial.CoordinateSystem cs)
		{
			var b = System.Spatial.SpatialBuilder.Create();
			if (cs != null)
				b.GeographyPipeline.SetCoordinateSystem(cs);

			var rings = Utilities.SplitMultiPolygon(polygon).ToList();

			if (rings.Count > 1)
			{
				b.GeographyPipeline.BeginGeography(System.Spatial.SpatialType.MultiPolygon);
			}

			foreach (var outerRing in rings)
			{
				b.GeographyPipeline.BeginGeography(System.Spatial.SpatialType.Polygon);
				CreateGeographyFigures(new Esri.ArcGISRuntime.Geometry.ReadOnlyPart[] { outerRing.Item1 }.Union(outerRing.Item2), b, true);
				b.GeographyPipeline.EndGeography();
			}

			if (rings.Count > 1)
			{
				b.GeographyPipeline.EndGeography();
			}
			return b.ConstructedGeography;
		}


		private static void CreateGeographyFigures(IEnumerable<Esri.ArcGISRuntime.Geometry.ReadOnlyPart> parts,
			System.Spatial.SpatialBuilder b, bool close)
		{
			foreach (var part in parts)
			{
				if (part.Count == 0)
					continue;
				b.GeographyPipeline.BeginFigure(ToGeographyPosition(part.Points[0]));
				for (int i = 1; i < part.Points.Count; i++)
				{
					b.GeographyPipeline.LineTo(ToGeographyPosition(part.Points[i]));
				}
                if (close)
                {
                    b.GeographyPipeline.LineTo(ToGeographyPosition(part.Points[0]));
                }
                b.GeographyPipeline.EndFigure();
			}
		}
		private static System.Spatial.GeographyPosition ToGeographyPosition(Esri.ArcGISRuntime.Geometry.MapPoint p)
		{
			return new System.Spatial.GeographyPosition(
					p.Y, p.X,
					p.HasZ ? (double?)p.Z : null,
					p.HasM ? (double?)p.M : null
					);
		}
	}
}
