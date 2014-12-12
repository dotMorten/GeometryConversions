using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeometryConversions.SqlServer
{
	internal static partial class SqlServerConverter
	{
		internal static Esri.ArcGISRuntime.Geometry.Geometry ReadGeography(Microsoft.SqlServer.Types.SqlGeography geography)
		{
			if (geography == null)
				throw new ArgumentNullException("geography");

			Esri.ArcGISRuntime.Geometry.SpatialReference sr = null;
			if (!geography.STSrid.IsNull && geography.STSrid.Value > 0)
				sr = Esri.ArcGISRuntime.Geometry.SpatialReference.Create(geography.STSrid.Value);
			switch (geography.STGeometryType().Value)
			{
				case "Point":
					return ReadGeographyPoint(geography, sr);
				case "MultiPoint":
					return ReadGeographyMultiPoint(geography, sr);
				case "LineString":
					return ReadGeographyLineString(geography, sr);
				case "MultiLineString":
					return ReadGeographyMultiLineString(geography, sr);
				case "Polygon":
					return ReadGeographyPolygon(geography, sr);
				case "MultiPolygon":
					return ReadGeographyMultiPolygon(geography, sr);
				default:
					throw new NotSupportedException(geography.STGeometryType().Value);
			}			
		}
		
		private static Esri.ArcGISRuntime.Geometry.MapPoint ReadGeographyPoint(Microsoft.SqlServer.Types.SqlGeography p, Esri.ArcGISRuntime.Geometry.SpatialReference sr)
		{
			return new Esri.ArcGISRuntime.Geometry.MapPoint(p.Long.Value, p.Lat.Value,
				!p.Z.IsNull ? p.Z.Value : double.NaN,
				!p.M.IsNull ? p.M.Value : double.NaN, sr);
		}
		
		private static Esri.ArcGISRuntime.Geometry.Geometry ReadGeographyMultiPoint(Microsoft.SqlServer.Types.SqlGeography mpoint, Esri.ArcGISRuntime.Geometry.SpatialReference sr)
		{
			return new Esri.ArcGISRuntime.Geometry.Multipoint(
				Utilities.CountEnumerator(mpoint.STNumPoints().Value).Select(i => mpoint.STPointN(i))
					.Select(p => new Esri.ArcGISRuntime.Geometry.MapPoint(p.Long.Value, p.Lat.Value,
					!p.Z.IsNull ? p.Z.Value : double.NaN,
					!p.M.IsNull ? p.M.Value : double.NaN))
			);
		}

		private static Esri.ArcGISRuntime.Geometry.Polyline ReadGeographyLineString(Microsoft.SqlServer.Types.SqlGeography line, Esri.ArcGISRuntime.Geometry.SpatialReference sr)
		{
			return new Esri.ArcGISRuntime.Geometry.Polyline(
				Utilities.CountEnumerator(line.STNumPoints().Value)
					.Select(i => line.STPointN(i))
					.Select(p => new Esri.ArcGISRuntime.Geometry.MapPoint(p.Long.Value, p.Lat.Value,
						!p.Z.IsNull ? p.Z.Value : double.NaN,
						!p.M.IsNull ? p.M.Value : double.NaN))
			);
		}

		private static Esri.ArcGISRuntime.Geometry.Geometry ReadGeographyMultiLineString(Microsoft.SqlServer.Types.SqlGeography mline, Esri.ArcGISRuntime.Geometry.SpatialReference sr)
		{
			return new Esri.ArcGISRuntime.Geometry.Polyline(
				Utilities.CountEnumerator(mline.STNumGeometries().Value)
					.Select(i => mline.STGeometryN(i))
					.SelectMany(line => Utilities.CountEnumerator(line.STNumPoints().Value)
						.Select(i => line.STPointN(i))
						.Select(p => new Esri.ArcGISRuntime.Geometry.MapPoint(p.Long.Value, p.Lat.Value,
							!p.Z.IsNull ? p.Z.Value : double.NaN,
							!p.M.IsNull ? p.M.Value : double.NaN))));
		}

		private static Esri.ArcGISRuntime.Geometry.Geometry ReadGeographyPolygon(Microsoft.SqlServer.Types.SqlGeography poly, Esri.ArcGISRuntime.Geometry.SpatialReference sr)
		{
			var rings = Utilities.CountEnumerator(poly.NumRings().Value)
				.Select(i => poly.RingN(i))
				.Select(t => Utilities.CountEnumerator(t.STNumPoints().Value)
					.Select(r => t.STPointN(r))
					.Select(p => new Esri.ArcGISRuntime.Geometry.MapPoint(p.Long.Value, p.Lat.Value,
							!p.Z.IsNull ? p.Z.Value : double.NaN,
							!p.M.IsNull ? p.M.Value : double.NaN)));


			return new Esri.ArcGISRuntime.Geometry.Polygon(rings);
		}

		private static Esri.ArcGISRuntime.Geometry.Geometry ReadGeographyMultiPolygon(Microsoft.SqlServer.Types.SqlGeography mpoly, Esri.ArcGISRuntime.Geometry.SpatialReference sr)
		{
			var rings = new List<IEnumerable<Esri.ArcGISRuntime.Geometry.MapPoint>>();
			for (int j = 0; j < mpoly.STNumGeometries().Value; j++)
			{
				var poly = mpoly.STGeometryN(j);
				var innerRings = Utilities.CountEnumerator(poly.NumRings().Value)
					.Select(i => poly.RingN(i))
					.Select(t => Utilities.CountEnumerator(t.STNumPoints().Value)
						.Select(r => t.STPointN(r))
						.Select(p => new Esri.ArcGISRuntime.Geometry.MapPoint(p.Long.Value, p.Lat.Value,
								!p.Z.IsNull ? p.Z.Value : double.NaN,
								!p.M.IsNull ? p.M.Value : double.NaN)));
				rings.AddRange(innerRings);
			}

			return new Esri.ArcGISRuntime.Geometry.Polygon(rings);

		}
	}
}
