using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GeometryConversions.Wkb
{
    internal partial class WkbConverter
    {
		internal Geometry Read(byte[] bytes, SpatialReference spatialReference)
		{
			// Create a memory inputStream using the supplied byte array.
			using (MemoryStream ms = new MemoryStream(bytes))
			{
				return Read(ms, spatialReference);
			}
		}

		/// <summary>
		/// Parses a Well-Known Binary inputStream to a <see cref="Geometry"/>.
		/// </summary>
		/// <param name="reader">BinaryReader</param>
		/// <returns>A <see cref="Geometry"/> based on the Well-known binary representation.</returns>
		internal Geometry Read(Stream stream, SpatialReference spatialReference)
		{
			// Create a new binary reader
			using (BinaryReader reader = new BinaryReader(stream))
			{
				// Call the main create function.
				return Read(reader, spatialReference);
			}
		}

		private Geometry Read(BinaryReader reader, SpatialReference spatialReference)
		{
			// Get the first byte in the array.  This specifies if the WKB is in
			// XDR (big-endian) format of NDR (little-endian) format.
			byte byteOrder = reader.ReadByte();

			if (!Enum.IsDefined(typeof(WkbByteOrder), byteOrder))
			{
				throw new ArgumentException("Byte order not recognized");
			}

			// Get the type of this geometry.
			uint type = (uint)readUInt32(reader, (WkbByteOrder)byteOrder);

			if (!Enum.IsDefined(typeof(WkbGeometryType), type))
				throw new ArgumentException("Geometry type not recognized");

			WkbGeometryType wkbtype = (WkbGeometryType)type;

			switch ((WkbGeometryType)type)
			{
				//XY / YX formats
				case WkbGeometryType.wkbPoint:
				case WkbGeometryType.wkbPointZ:
				case WkbGeometryType.wkbPointM:
				case WkbGeometryType.wkbPointZM:
					return ReadWkbPoint(reader, (WkbByteOrder)byteOrder, wkbtype, spatialReference);

				case WkbGeometryType.wkbLineString:
				case WkbGeometryType.wkbLineStringZ:
				case WkbGeometryType.wkbLineStringM:
				case WkbGeometryType.wkbLineStringZM:
					return ReadWkbLineString(reader, (WkbByteOrder)byteOrder, wkbtype, spatialReference);

				case WkbGeometryType.wkbPolygon:
				case WkbGeometryType.wkbPolygonZ:
				case WkbGeometryType.wkbPolygonM:
				case WkbGeometryType.wkbPolygonZM:
					return ReadWkbPolygon(reader, (WkbByteOrder)byteOrder, wkbtype, spatialReference);

				case WkbGeometryType.wkbMultiPoint:
				case WkbGeometryType.wkbMultiPointZ:
				case WkbGeometryType.wkbMultiPointM:
				case WkbGeometryType.wkbMultiPointZM:
					return ReadWkbMultiPoint(reader, (WkbByteOrder)byteOrder, wkbtype, spatialReference);

				case WkbGeometryType.wkbMultiLineString:
				case WkbGeometryType.wkbMultiLineStringZ:
				case WkbGeometryType.wkbMultiLineStringM:
				case WkbGeometryType.wkbMultiLineStringZM:
					return ReadWkbMultiLineString(reader, (WkbByteOrder)byteOrder, wkbtype, spatialReference);

				case WkbGeometryType.wkbMultiPolygon:
				case WkbGeometryType.wkbMultiPolygonZ:
				case WkbGeometryType.wkbMultiPolygonM:
				case WkbGeometryType.wkbMultiPolygonZM:
					return ReadWkbMultiPolygon(reader, (WkbByteOrder)byteOrder, wkbtype, spatialReference);

				case WkbGeometryType.wkbGeometryCollection:
				case WkbGeometryType.wkbGeometryCollectionZ:
				case WkbGeometryType.wkbGeometryCollectionM:
				case WkbGeometryType.wkbGeometryCollectionZM:
					throw new NotSupportedException("GeometryCollection");
					//return ReadWkbGeometryCollection(reader, (WkbByteOrder)byteOrder, wkbtype);

				default:
					throw new NotSupportedException("Geometry type '" + type.ToString() + "' not supported");
			}
		}

		
		private MapPoint ReadWkbPoint(BinaryReader reader, WkbByteOrder byteOrder, WkbGeometryType type, SpatialReference spatialReference)
		{
			// Create and return the point.
			return ReadCoordinate(reader, byteOrder, type, spatialReference);
		}

		private IEnumerable<MapPoint> ReadCoordinates(BinaryReader reader, WkbByteOrder byteOrder, WkbGeometryType type, SpatialReference spatialReference)
		{
			// Get the number of points in this linestring.
			int numPoints = (int)readUInt32(reader, byteOrder);

			// Loop on the number of points in the ring.
			for (int i = 0; i < numPoints; i++)
			{
				yield return ReadCoordinate(reader, byteOrder, type, null);
			}
		}

		private MapPoint ReadCoordinate(BinaryReader reader, WkbByteOrder byteOrder, WkbGeometryType type, SpatialReference spatialReference)
		{
			double X = readDouble(reader, byteOrder);
			double Y = readDouble(reader, byteOrder);
			double Z = ((uint)type > 1000 && (uint)type < 2000 || (int)type > 3000) ? readDouble(reader, byteOrder) : double.NaN;
			double M = ((uint)type > 2000) ? readDouble(reader, byteOrder) : double.NaN;
            bool hasZ = ((uint)type > 1000 && (uint)type < 2000 || (int)type > 3000);
            if ((uint)type > 2000) // HasM
            {
                if (hasZ)
                    return Esri.ArcGISRuntime.Geometry.MapPoint.CreateWithM(X, Y, Z, M, spatialReference);
                else
                    return Esri.ArcGISRuntime.Geometry.MapPoint.CreateWithM(X, Y, M, spatialReference);
            }
            if (hasZ)
                return new Esri.ArcGISRuntime.Geometry.MapPoint(X, Y, Z, spatialReference);
            else
                return new Esri.ArcGISRuntime.Geometry.MapPoint(X, Y, spatialReference);
		}

		private Polyline ReadWkbLineString(BinaryReader reader, WkbByteOrder byteOrder, WkbGeometryType type, SpatialReference spatialReference)
		{
			return new Polyline(ReadCoordinates(reader, byteOrder, type, null), spatialReference);
		}

		private Polygon ReadWkbPolygon(BinaryReader reader, WkbByteOrder byteOrder, WkbGeometryType type, SpatialReference spatialReference)
		{
			// Get the Number of rings in this Polygon.
			int numRings = (int)readUInt32(reader, byteOrder);
		    List<IEnumerable<MapPoint>> rings = new List<IEnumerable<MapPoint>>();
		    foreach (var ring in CoordinateCollectionEnumerator(numRings, reader, byteOrder, type, null))
		    {
		        rings.Add(new List<MapPoint>(ring));
		    }
			return new Polygon(rings, spatialReference);
			
		}

		IEnumerable<IEnumerable<MapPoint>> CoordinateCollectionEnumerator(int count, BinaryReader reader, WkbByteOrder byteOrder, WkbGeometryType type, SpatialReference spatialReference)
		{
			for(int i=0;i<count;i++)
				yield return ReadCoordinates(reader, byteOrder, type, spatialReference);
		}

		private Multipoint ReadWkbMultiPoint(BinaryReader reader, WkbByteOrder byteOrder, WkbGeometryType type, SpatialReference spatialReference)
		{
			// Get the number of points in this multipoint.
			int numPoints = (int)readUInt32(reader, byteOrder);

			// Create a new array for the points.
			List<MapPoint> points = new List<MapPoint>(numPoints);
			// Loop on the number of points.
			for (int i = 0; i < numPoints; i++)
			{
				// ReadGeometry point header
				reader.BaseStream.Seek(5, SeekOrigin.Current);
				//reader.ReadByte();
				//readUInt32(reader, byteOrder);

				// TODO: Validate type

				// Create the next point and add it to the point array.
				points.Add(ReadWkbPoint(reader, byteOrder, type, null));
			}
			return new Multipoint(points, spatialReference);
			
		}

		private Polyline ReadWkbMultiLineString(BinaryReader reader, WkbByteOrder byteOrder, WkbGeometryType type, SpatialReference spatialReference)
		{
			// Get the number of linestrings in this multilinestring.
			int numLineStrings = (int)readUInt32(reader, byteOrder);

			List<IEnumerable<MapPoint>> mline = new List<IEnumerable<MapPoint>>(numLineStrings);
			// Create a new array for the linestrings .
			
			// Loop on the number of linestrings.
			for (int i = 0; i < numLineStrings; i++)
			{
				// ReadGeometry linestring header
				reader.BaseStream.Seek(5, SeekOrigin.Current);
				//reader.ReadByte();
				//readUInt32(reader, byteOrder);

				// Create the next linestring and add it to the array.
				mline.Add(ReadCoordinates(reader, byteOrder, type, null));
			}

			// Create and return the MultiLineString.
			return new Polyline(mline, spatialReference);
			
		}

		private Polygon ReadWkbMultiPolygon(BinaryReader reader, WkbByteOrder byteOrder, WkbGeometryType type, SpatialReference spatialReference)
		{
			// Get the number of Polygons.
			int numPolygons = (int)readUInt32(reader, byteOrder);

			// Create a new array for the Polygons.
			List<IEnumerable<MapPoint>> rings = new List<IEnumerable<MapPoint>>();
			// Loop on the number of polygons.
			for (int i = 0; i < numPolygons; i++)
			{
				// read polygon header
				reader.BaseStream.Seek(5, SeekOrigin.Current);
				//reader.ReadByte();
				//readUInt32(reader, byteOrder);

				// TODO: Validate type

				int numRings = (int)readUInt32(reader, byteOrder);
			    foreach (var ring in CoordinateCollectionEnumerator(numRings, reader, byteOrder, type, spatialReference))
			    {
			        rings.Add(new List<MapPoint>(ring));
			    }
			}

			//Create and return the MultiPolygon.
			return new Polygon(rings);
		}

		/*private GeometryCollection ReadWkbGeometryCollection(BinaryReader reader, WkbByteOrder byteOrder, WkbGeometryType type)
		{
			// The next byte in the array tells the number of geometries in this collection.
			int numGeometries = (int)readUInt32(reader, byteOrder);

			// Create a new array for the geometries.
			GeometryCollection geometries = new GeometryCollection(numGeometries);

			// Loop on the number of geometries.
			for (int i = 0; i < numGeometries; i++)
			{
				// Call the main create function with the next geometry.
				geometries.Geometries.Add(ReadGeometry(reader));
			}

			// Create and return the next geometry.
			return geometries;
		}*/


			
		private static uint readUInt32(BinaryReader reader, WkbByteOrder byteOrder)
		{
			if (byteOrder == WkbByteOrder.Xdr)
			{
				byte[] bytes = BitConverter.GetBytes(reader.ReadUInt32());
				Array.Reverse(bytes);
				return BitConverter.ToUInt32(bytes, 0);
			}
			else
				return reader.ReadUInt32();
		}

		private static double readDouble(BinaryReader reader, WkbByteOrder byteOrder)
		{
			if (byteOrder == WkbByteOrder.Xdr)
			{
				byte[] bytes = BitConverter.GetBytes(reader.ReadDouble());
				Array.Reverse(bytes);
				return BitConverter.ToDouble(bytes, 0);
			}
			else
				return reader.ReadDouble();
		}
    }
}
