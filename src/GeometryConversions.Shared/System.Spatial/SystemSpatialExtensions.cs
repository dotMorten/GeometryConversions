using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GeometryConversions.SystemSpatial
{
	/// <summary>
	/// Converter for converting ESRI Runtime Geometry to/from System.Spatial geometry and geography types
	/// </summary>
	public static class SystemSpatialExtensions
	{
		public static System.Spatial.Geometry ToSystemSpatialGeometry(this Geometry geometry)
		{
			return new SystemSpatialConverter().CreateGeometry(geometry);
		}

		public static System.Spatial.Geography ToSystemSpatialGeography(this Geometry geometry)
		{
			return new SystemSpatialConverter().CreateGeography(geometry);
		}

		public static Geometry FromSystemSpatialGeometry(this System.Spatial.Geometry geometry)
		{
			return new SystemSpatialConverter().ReadGeometry(geometry);
		}

		public static Geometry FromSystemSpatialGeography(this System.Spatial.Geography geography)
		{
			return new SystemSpatialConverter().ReadGeography(geography);
		}
	}
}
