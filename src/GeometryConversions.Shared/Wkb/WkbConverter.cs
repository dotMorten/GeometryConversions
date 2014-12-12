using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GeometryConversions.Wkb
{
    internal partial class WkbConverter
    {
		/// <summary>
		/// Byte order enumeration
		/// </summary>
		internal enum WkbByteOrder : byte
		{
			/// <summary>
			/// XDR (Big Endian) Encoding of Numeric Types
			/// </summary>
			Xdr = 0,
			/// <summary>
			/// NDR (Little Endian) Encoding of Numeric Types
			/// </summary>
			Ndr = 1
		}
		private enum WkbGeometryType : uint
		{
			wkbPoint = 1,
			wkbLineString = 2,
			wkbPolygon = 3,
			wkbMultiPoint = 4,
			wkbMultiLineString = 5,
			wkbMultiPolygon = 6,
			wkbGeometryCollection = 7,
			wkbPolyhedralSurface = 15,
			wkbTIN = 16,
			wkbTriangle = 17,

			wkbPointZ = 1001,
			wkbLineStringZ = 1002,
			wkbPolygonZ = 1003,
			wkbTrianglez = 1017,
			wkbMultiPointZ = 1004,
			wkbMultiLineStringZ = 1005,
			wkbMultiPolygonZ = 1006,
			wkbGeometryCollectionZ = 1007,
			wkbPolyhedralSurfaceZ = 1015,
			wkbTINZ = 1016,

			wkbPointM = 2001,
			wkbLineStringM = 2002,
			wkbPolygonM = 2003,
			wkbTriangleM = 2017,
			wkbMultiPointM = 2004,
			wkbMultiLineStringM = 2005,
			wkbMultiPolygonM = 2006,
			wkbGeometryCollectionM = 2007,
			wkbPolyhedralSurfaceM = 2015,
			wkbTINM = 2016,

			wkbPointZM = 3001,
			wkbLineStringZM = 3002,
			wkbPolygonZM = 3003,
			wkbTriangleZM = 3017,
			wkbMultiPointZM = 3004,
			wkbMultiLineStringZM = 3005,
			wkbMultiPolygonZM = 3006,
			wkbGeometryCollectionZM = 3007,
			wkbPolyhedralSurfaceZM = 3015,
			wkbTinZM = 3016
		}

		/// <summary>
		/// Gets the byte order used.
		/// </summary>
		public WkbByteOrder wkbByteOrder { get; private set; }
		/// <summary>Instantiates the Well-Known Binary reader/writer</summary>
		public WkbConverter() : this(WkbByteOrder.Ndr) { }
		/// <summary>Instantiates the Well-Known Binary reader/writer</summary>
		/// <param name="byteOrder">Byte order</param>
		public WkbConverter(WkbByteOrder byteOrder)
		{
			wkbByteOrder = byteOrder;
		}
    }
}
