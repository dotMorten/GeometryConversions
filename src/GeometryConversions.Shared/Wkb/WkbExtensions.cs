using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GeometryConversions.Wkb
{
	/// <summary>
	/// Converter for converting ESRI Runtime Geometry to/from Well-known binary
	/// </summary>
	public static class WkbExtensions
    {
		public static byte[] ToWellKnownBinary(this Geometry geometry)
		{
			MemoryStream ms = new MemoryStream();
			var bytes = WriteWellKnownBinary(geometry, ms);
			return ms.ToArray();
		}

		public static int WriteWellKnownBinary(this Geometry geometry, Stream outputStream)
		{
			return new WkbConverter().WriteWellKnownBinary(geometry, outputStream);
		}

		public static Geometry FromWellKnownBinary(this byte[] data, SpatialReference spatialReference = null)
		{
			return new WkbConverter().Read(data, spatialReference);
		}

		public static Geometry ReadWellKnownBinary(this Stream inputStream, SpatialReference spatialReference = null)
		{
			return new WkbConverter().Read(inputStream, spatialReference);
		}
    }
}
