using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeometryConversions.SystemSpatial
{
	internal partial class SystemSpatialConverter
	{
		internal System.Spatial.Geometry CreateGeometry(Esri.ArcGISRuntime.Geometry.Geometry geometry)
		{
			if (geometry == null)
				throw new ArgumentNullException("geometry");
			System.Spatial.CoordinateSystem cs = null;
			if (geometry.SpatialReference != null && geometry.SpatialReference.Wkid > 0)
				cs = System.Spatial.CoordinateSystem.Geometry(geometry.SpatialReference.Wkid);
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

		private System.Spatial.GeometryPoint CreateGeometryPoint(Esri.ArcGISRuntime.Geometry.MapPoint mapPoint, System.Spatial.CoordinateSystem cs)
		{
			return System.Spatial.GeometryPoint.Create(cs, mapPoint.X, mapPoint.Y,
				mapPoint.HasZ ? (double?)mapPoint.Z : null,
				mapPoint.HasM ? (double?)mapPoint.M : null);
		}

		private System.Spatial.GeometryMultiPoint CreateGeometryMultipoint(Esri.ArcGISRuntime.Geometry.Multipoint multipoint, System.Spatial.CoordinateSystem cs)
		{
			var b = System.Spatial.SpatialBuilder.Create();
			if (cs != null)
				b.GeometryPipeline.SetCoordinateSystem(cs);
			b.GeometryPipeline.BeginGeometry(System.Spatial.SpatialType.MultiPoint);

			foreach (var p in multipoint.Points)
			{
				b.GeometryPipeline.BeginGeometry(System.Spatial.SpatialType.Point);
				b.GeometryPipeline.BeginFigure(ToGeometryPosition(p));
				b.GeometryPipeline.EndFigure();
				b.GeometryPipeline.EndGeometry();
			}
			b.GeometryPipeline.EndGeometry();
			return (System.Spatial.GeometryMultiPoint)b.ConstructedGeometry;
		}


		private System.Spatial.Geometry CreateGeometryLineString(Esri.ArcGISRuntime.Geometry.Polyline polyline, System.Spatial.CoordinateSystem cs)
		{
			var b = System.Spatial.SpatialBuilder.Create();
			if (cs != null)
				b.GeometryPipeline.SetCoordinateSystem(cs);
			b.GeometryPipeline.BeginGeometry(
				polyline.Parts.Count < 2 ?
				System.Spatial.SpatialType.LineString :
				System.Spatial.SpatialType.MultiLineString);

			CreateGeometryFigures(polyline.Parts, b, false);

			b.GeometryPipeline.EndGeometry();
			return b.ConstructedGeometry;
		}

		private System.Spatial.Geometry CreateGeometryPolygon(Esri.ArcGISRuntime.Geometry.Polygon polygon, System.Spatial.CoordinateSystem cs)
		{
			var b = System.Spatial.SpatialBuilder.Create();
			if (cs != null)
				b.GeometryPipeline.SetCoordinateSystem(cs);

			var rings = Utilities.SplitMultiPolygon(polygon).ToList();

			if (rings.Count > 1)
			{
				b.GeometryPipeline.BeginGeometry(System.Spatial.SpatialType.MultiPolygon);
			}

			foreach (var outerRing in rings)
			{
				b.GeometryPipeline.BeginGeometry(System.Spatial.SpatialType.Polygon);
                var figure = new Esri.ArcGISRuntime.Geometry.ReadOnlyPart[] { outerRing.Item1 }.Union(outerRing.Item2);

                CreateGeometryFigures(figure, b, true);
				b.GeometryPipeline.EndGeometry();
			}

			if (rings.Count > 1)
			{
				b.GeometryPipeline.EndGeometry();
			}
			return b.ConstructedGeometry;
		}


		private static void CreateGeometryFigures(IEnumerable<Esri.ArcGISRuntime.Geometry.ReadOnlyPart> parts,
			System.Spatial.SpatialBuilder b, bool close)
		{
			foreach (var part in parts)
			{
				if (part.Count == 0)
					continue;
				b.GeometryPipeline.BeginFigure(ToGeometryPosition(part.Points[0]));
				for (int i = 1; i < part.Points.Count; i++)
				{
					b.GeometryPipeline.LineTo(ToGeometryPosition(part.Points[i]));
				}
				if(close)
					b.GeometryPipeline.LineTo(ToGeometryPosition(part.Points[0]));
				b.GeometryPipeline.EndFigure();
			}
		}
		private static System.Spatial.GeometryPosition ToGeometryPosition(Esri.ArcGISRuntime.Geometry.MapPoint p)
		{
			return new System.Spatial.GeometryPosition(
					p.X, p.Y,
					p.HasZ ? (double?)p.Z : null,
					p.HasM ? (double?)p.M : null
					);
		}
	}
}
