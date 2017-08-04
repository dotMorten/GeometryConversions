using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeometryConversions.SqlServer
{
	internal static partial class SqlServerConverter
    {
		internal static Esri.ArcGISRuntime.Geometry.Geometry ReadGeometry(Microsoft.SqlServer.Types.SqlGeometry geometry)
		{
			if (geometry == null)
				throw new ArgumentNullException("geometry");
			Esri.ArcGISRuntime.Geometry.SpatialReference sr = null;
			if (!geometry.STSrid.IsNull && geometry.STSrid.Value > 0)
				sr = Esri.ArcGISRuntime.Geometry.SpatialReference.Create(geometry.STSrid.Value);
			switch (geometry.STGeometryType().Value)
			{
				case "Point":
					return ReadGeometryPoint(geometry, sr);
				case "MultiPoint":
					return ReadGeometryMultiPoint(geometry, sr);
				case "LineString":
					return ReadGeometryLineString(geometry, sr);
				case "MultiLineString":
					return ReadGeometryMultiLineString(geometry, sr);
				case "Polygon":
					return ReadGeometryPolygon(geometry, sr);
				case "MultiPolygon":
					return ReadGeometryMultiPolygon(geometry, sr);
				default:
					throw new NotSupportedException(geometry.STGeometryType().Value);
			}			
		}
		
		private static Esri.ArcGISRuntime.Geometry.MapPoint ReadGeometryPoint(Microsoft.SqlServer.Types.SqlGeometry p, Esri.ArcGISRuntime.Geometry.SpatialReference sr)
        {
            if (!p.M.IsNull)
            {
                if (!p.Z.IsNull)
                    return Esri.ArcGISRuntime.Geometry.MapPoint.CreateWithM(p.STX.Value, p.STY.Value, p.Z.Value, p.M.Value, sr);
                else
                    return Esri.ArcGISRuntime.Geometry.MapPoint.CreateWithM(p.STX.Value, p.STY.Value, p.M.Value, sr);
            }
            if (!p.Z.IsNull)
                return new Esri.ArcGISRuntime.Geometry.MapPoint(p.STX.Value, p.STY.Value, p.Z.Value, sr);
            else
                return new Esri.ArcGISRuntime.Geometry.MapPoint(p.STX.Value, p.STY.Value, sr);
		}
		
		private static Esri.ArcGISRuntime.Geometry.Geometry ReadGeometryMultiPoint(Microsoft.SqlServer.Types.SqlGeometry mpoint, Esri.ArcGISRuntime.Geometry.SpatialReference sr)
		{
			return new Esri.ArcGISRuntime.Geometry.Multipoint(
				Utilities.CountEnumerator(mpoint.STNumPoints().Value).Select(i => mpoint.STPointN(i))
					.Select(p => ReadGeometryPoint(p, sr))
			);
		}

		private static Esri.ArcGISRuntime.Geometry.Polyline ReadGeometryLineString(Microsoft.SqlServer.Types.SqlGeometry line, Esri.ArcGISRuntime.Geometry.SpatialReference sr)
		{
			return new Esri.ArcGISRuntime.Geometry.Polyline(
				Utilities.CountEnumerator(line.STNumPoints().Value)
					.Select(i => line.STPointN(i))
					.Select(p => ReadGeometryPoint(p, sr))
			);
		}

		private static Esri.ArcGISRuntime.Geometry.Geometry ReadGeometryMultiLineString(Microsoft.SqlServer.Types.SqlGeometry mline, Esri.ArcGISRuntime.Geometry.SpatialReference sr)
		{
			return new Esri.ArcGISRuntime.Geometry.Polyline(
				Utilities.CountEnumerator(mline.STNumGeometries().Value)
					.Select(i => mline.STGeometryN(i))
					.SelectMany(line => Utilities.CountEnumerator(line.STNumPoints().Value)
						.Select(i => line.STPointN(i))
						.Select(p => ReadGeometryPoint(p, sr))));
		}

		private static Esri.ArcGISRuntime.Geometry.Geometry ReadGeometryPolygon(Microsoft.SqlServer.Types.SqlGeometry poly, Esri.ArcGISRuntime.Geometry.SpatialReference sr)
		{
			var outerRing = Utilities.CountEnumerator(poly.STExteriorRing().STNumPoints().Value)
				.Select(i => poly.STExteriorRing().STPointN(i))
				.Select(p => ReadGeometryPoint(p, sr));
			var innerRings = Utilities.CountEnumerator(poly.STNumInteriorRing().Value)
				.Select(i => poly.STInteriorRingN(i))
				.Select(t => Utilities.CountEnumerator(t.STNumPoints().Value)
					.Select(r => t.STPointN(r))
					.Select(p => ReadGeometryPoint(p, sr)));


			return new Esri.ArcGISRuntime.Geometry.Polygon( new List<IEnumerable<Esri.ArcGISRuntime.Geometry.MapPoint>>() { outerRing }.Union(innerRings));
		}

		private static Esri.ArcGISRuntime.Geometry.Geometry ReadGeometryMultiPolygon(Microsoft.SqlServer.Types.SqlGeometry mpoly, Esri.ArcGISRuntime.Geometry.SpatialReference sr)
		{
			var rings = new List<IEnumerable<Esri.ArcGISRuntime.Geometry.MapPoint>>();
			for (int j = 0; j < mpoly.STNumGeometries().Value; j++)
			{
				var poly = mpoly.STGeometryN(j);
				var outerRing = Utilities.CountEnumerator(poly.STExteriorRing().STNumPoints().Value)
				.Select(i => poly.STExteriorRing().STPointN(i))
				.Select(p => ReadGeometryPoint(p, sr));
				rings.AddRange(rings);
				var innerRings = Utilities.CountEnumerator(poly.STNumInteriorRing().Value)
					.Select(i => poly.STInteriorRingN(i))
					.Select(t => Utilities.CountEnumerator(t.STNumPoints().Value)
						.Select(r => t.STPointN(r))
						.Select(p => ReadGeometryPoint(p, sr)));
				rings.AddRange(innerRings);
			}

			return new Esri.ArcGISRuntime.Geometry.Polygon(rings);

		}
	}
}
