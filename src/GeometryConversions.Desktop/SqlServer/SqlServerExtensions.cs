using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GeometryConversions.SqlServer
{
	/// <summary>
	/// Converter for converting ESRI Runtime Geometry to/from SQL Server Spatial geometry and geography types
	/// </summary>
	public static class SqlServerExtensions
	{
		static SqlServerExtensions()
		{
			SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);
		}
		public static Microsoft.SqlServer.Types.SqlGeometry ToSqlSpatialGeometry(this Geometry geometry)
		{
			return SqlServerConverter.CreateGeometry(geometry);
		}

		public static Microsoft.SqlServer.Types.SqlGeography ToSqlSpatialGeography(this Geometry geometry)
		{
			return SqlServerConverter.CreateGeography(geometry);
		}

		public static Geometry FromSqlSpatialGeometry(this Microsoft.SqlServer.Types.SqlGeometry geometry)
		{
			return SqlServerConverter.ReadGeometry(geometry);
		}

		public static Geometry FromSqlSpatialGeography(this Microsoft.SqlServer.Types.SqlGeography geography)
		{
			return SqlServerConverter.ReadGeography(geography);
		}
	}
}
