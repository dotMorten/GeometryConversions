using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeometryConversions.SystemSpatial
{
    internal partial class SystemSpatialConverter
    {
		internal Esri.ArcGISRuntime.Geometry.Geometry ReadGeography(System.Spatial.Geography geography)
		{
			if (geography == null)
				throw new ArgumentNullException("geography");

			Esri.ArcGISRuntime.Geometry.SpatialReference sr = null;
			if (geography.CoordinateSystem.EpsgId.HasValue && geography.CoordinateSystem.EpsgId.Value > 0)
				sr = Esri.ArcGISRuntime.Geometry.SpatialReference.Create(geography.CoordinateSystem.EpsgId.Value);
			if (geography is System.Spatial.GeographyPoint)
			{
				return ReadGeographyPoint((System.Spatial.GeographyPoint)geography, sr);
			}
			if (geography is System.Spatial.GeographyMultiPoint)
			{
				return ReadGeographyMultiPoint((System.Spatial.GeographyMultiPoint)geography, sr);
			}
			if (geography is System.Spatial.GeographyLineString)
			{
				return ReadGeographyLineString((System.Spatial.GeographyLineString)geography, sr);
			}
			if (geography is System.Spatial.GeographyMultiLineString)
			{
				return ReadGeographyMultiLineString((System.Spatial.GeographyMultiLineString)geography, sr);
			}
			if (geography is System.Spatial.GeographyPolygon)
			{
				return ReadGeographyPolygon((System.Spatial.GeographyPolygon)geography, sr);
			}
			if (geography is System.Spatial.GeographyMultiPolygon)
			{
				return ReadGeographyMultiPolygon((System.Spatial.GeographyMultiPolygon)geography, sr);
			}
			throw new NotImplementedException();
		}

		private Esri.ArcGISRuntime.Geometry.MapPoint ReadGeographyPoint(System.Spatial.GeographyPoint p, Esri.ArcGISRuntime.Geometry.SpatialReference sr)
        {
            if (p.M.HasValue)
            {
                if (p.Z.HasValue)
                    return Esri.ArcGISRuntime.Geometry.MapPoint.CreateWithM(p.Longitude, p.Latitude, p.Z.Value, p.M.Value, sr);
                else
                    return Esri.ArcGISRuntime.Geometry.MapPoint.CreateWithM(p.Longitude, p.Latitude, p.M.Value, sr);
            }
            if (p.Z.HasValue)
                return new Esri.ArcGISRuntime.Geometry.MapPoint(p.Longitude, p.Latitude, p.Z.Value, sr);
            else
                return new Esri.ArcGISRuntime.Geometry.MapPoint(p.Longitude, p.Latitude, sr);
		}

		private Esri.ArcGISRuntime.Geometry.Geometry ReadGeographyMultiPoint(System.Spatial.GeographyMultiPoint mpoint, Esri.ArcGISRuntime.Geometry.SpatialReference sr)
		{
			return new Esri.ArcGISRuntime.Geometry.Multipoint(mpoint.Points.Select(p => ReadGeographyPoint(p, null)), sr);
		}

		private Esri.ArcGISRuntime.Geometry.Geometry ReadGeographyLineString(System.Spatial.GeographyLineString line, Esri.ArcGISRuntime.Geometry.SpatialReference sr)
		{
			return new Esri.ArcGISRuntime.Geometry.Polyline(line.Points.Select(p => ReadGeographyPoint(p, null)), sr);
		}

		private Esri.ArcGISRuntime.Geometry.Geometry ReadGeographyMultiLineString(System.Spatial.GeographyMultiLineString mline, Esri.ArcGISRuntime.Geometry.SpatialReference sr)
		{
			return new Esri.ArcGISRuntime.Geometry.Polyline(mline.LineStrings.Select(line => line.Points.Select(p => ReadGeographyPoint(p, null))), sr);
		}

		private Esri.ArcGISRuntime.Geometry.Geometry ReadGeographyPolygon(System.Spatial.GeographyPolygon poly, Esri.ArcGISRuntime.Geometry.SpatialReference sr)
		{
			return new Esri.ArcGISRuntime.Geometry.Polygon(poly.Rings.Select(ring => ring.Points.Select(p => ReadGeographyPoint(p, null))), sr);
		}

		private Esri.ArcGISRuntime.Geometry.Geometry ReadGeographyMultiPolygon(System.Spatial.GeographyMultiPolygon mpoly, Esri.ArcGISRuntime.Geometry.SpatialReference sr)
		{
			var rings = mpoly.Polygons.SelectMany(line => line.Rings.Select(ring => ring.Points.Select(p => ReadGeographyPoint(p, null))));
			return new Esri.ArcGISRuntime.Geometry.Polygon(rings, sr);
		}
	}
}
