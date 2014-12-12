using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeometryConversions.SystemSpatial
{
    internal partial class SystemSpatialConverter
    {
		internal Esri.ArcGISRuntime.Geometry.Geometry ReadGeometry(System.Spatial.Geometry geometry)
		{
			if (geometry == null)
				throw new ArgumentNullException("geometry");
			Esri.ArcGISRuntime.Geometry.SpatialReference sr = null;
			if (geometry.CoordinateSystem.EpsgId.HasValue && geometry.CoordinateSystem.EpsgId.Value > 0)
				sr = Esri.ArcGISRuntime.Geometry.SpatialReference.Create(geometry.CoordinateSystem.EpsgId.Value);
			if(geometry is System.Spatial.GeometryPoint)
			{
				return ReadGeometryPoint((System.Spatial.GeometryPoint)geometry, sr);
			}
			if(geometry is System.Spatial.GeometryMultiPoint)
			{
				return ReadGeometryMultiPoint((System.Spatial.GeometryMultiPoint)geometry, sr);
			}
			if (geometry is System.Spatial.GeometryLineString)
			{
				return ReadGeometryLineString((System.Spatial.GeometryLineString)geometry, sr);
			}
			if (geometry is System.Spatial.GeometryMultiLineString)
			{
				return ReadGeometryMultiLineString((System.Spatial.GeometryMultiLineString)geometry, sr);
			}
			
			throw new NotImplementedException();
		}
		private Esri.ArcGISRuntime.Geometry.MapPoint ReadGeometryPoint(System.Spatial.GeometryPoint p, Esri.ArcGISRuntime.Geometry.SpatialReference sr)
		{
			return new Esri.ArcGISRuntime.Geometry.MapPoint(p.X, p.Y,
				p.Z.HasValue ? p.Z.Value : double.NaN,
				p.M.HasValue ? p.M.Value : double.NaN, sr);
		}

		private Esri.ArcGISRuntime.Geometry.Geometry ReadGeometryMultiPoint(System.Spatial.GeometryMultiPoint mpoint, Esri.ArcGISRuntime.Geometry.SpatialReference sr)
		{
			return new Esri.ArcGISRuntime.Geometry.Multipoint(mpoint.Points.Select(p => ReadGeometryPoint(p, null)), sr);
		}

		private Esri.ArcGISRuntime.Geometry.Geometry ReadGeometryLineString(System.Spatial.GeometryLineString line, Esri.ArcGISRuntime.Geometry.SpatialReference sr)
		{
			return new Esri.ArcGISRuntime.Geometry.Polyline(line.Points.Select(p => ReadGeometryPoint(p, null)), sr);
		}

		private Esri.ArcGISRuntime.Geometry.Geometry ReadGeometryMultiLineString(System.Spatial.GeometryMultiLineString mline, Esri.ArcGISRuntime.Geometry.SpatialReference sr)
		{
			return new Esri.ArcGISRuntime.Geometry.Polyline(mline.LineStrings.Select(line=>line.Points.Select(p => ReadGeometryPoint(p, null))), sr);
		}

		private Esri.ArcGISRuntime.Geometry.Geometry ReadGeometryPolygon(System.Spatial.GeometryPolygon poly, Esri.ArcGISRuntime.Geometry.SpatialReference sr)
		{
			return new Esri.ArcGISRuntime.Geometry.Polygon(poly.Rings.Select(ring => ring.Points.Select(p => ReadGeometryPoint(p, null))), sr);
		}

		private Esri.ArcGISRuntime.Geometry.Geometry ReadGeometryMultiPolygon(System.Spatial.GeometryMultiPolygon mpoly, Esri.ArcGISRuntime.Geometry.SpatialReference sr)
		{
			var rings = mpoly.Polygons.SelectMany(line => line.Rings.Select(ring => ring.Points.Select(p => ReadGeometryPoint(p, null))));
			return new Esri.ArcGISRuntime.Geometry.Polygon( rings, sr);
		}
	}
}
