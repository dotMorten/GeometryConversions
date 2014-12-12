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
		public byte[] ToWellKnownBinary(Geometry geometry)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				var bytes = WriteWellKnownBinary(geometry, ms);
				return ms.ToArray();
			}
		}
		public int WriteWellKnownBinary(Geometry geometry, Stream stream)
		{
			return WriteWellKnownBinary(geometry, stream, geometry.HasZ, geometry.HasM);
		}

		private int WriteWellKnownBinary(Geometry geometry, Stream stream, bool includeZ, bool includeM)
		{
			BinaryWriter bw = new BinaryWriter(stream);
			var start = stream.Position;
			//Write the byteorder format.
			bw.Write((byte)wkbByteOrder);
			//Write the type of this geometry
			WriteGeometry(geometry, bw, includeZ, includeM);
			return (int)(stream.Position - start);
		}

		private void WriteGeometry(Geometry geometry, BinaryWriter bWriter, bool includeZ, bool includeM)
		{
			//Write the type of the geometry.
			if (geometry is MapPoint)
			{
				WriteUInt32((uint)WkbGeometryType.wkbPoint + (includeZ ? (uint)1000 : 0) + (includeM ? (uint)2000 : 0), bWriter);
				WritePoint((MapPoint)geometry, bWriter, includeZ, includeM);
			}
			else if (geometry is Polyline)
			{
				var pl = (Polyline)geometry;
				if (pl.Parts.Count == 1) //LineString 
				{
					WriteUInt32((uint)WkbGeometryType.wkbLineString + (includeZ ? (uint)1000 : 0) + (includeM ? (uint)2000 : 0), bWriter);
					WriteLineString(pl, bWriter, includeZ, includeM);
				}
				else //MultiLineString
				{
					WriteUInt32((uint)WkbGeometryType.wkbMultiLineString + (includeZ ? (uint)1000 : 0) + (includeM ? (uint)2000 : 0), bWriter);
					WriteMultiLineString(pl, bWriter, includeZ, includeM);
				}
			}
			else if (geometry is Polygon)
			{
				var pg = (Polygon)geometry;
				if (pg.Parts.Count == 1) //Polygon
				{
					WriteUInt32((uint)WkbGeometryType.wkbPolygon + (includeZ ? (uint)1000 : 0) + (includeM ? (uint)2000 : 0), bWriter);
					WritePolygon(pg, bWriter, includeZ, includeM);
				}
				else // MultiPolygon)
				{
					WriteUInt32((uint)WkbGeometryType.wkbMultiPolygon + (includeZ ? (uint)1000 : 0) + (includeM ? (uint)2000 : 0), bWriter);
					WriteMultiPolygon(pg, bWriter, includeZ, includeM);
				}
			}
			else if (geometry is Multipoint)
			{
				WriteUInt32((uint)WkbGeometryType.wkbMultiPoint + (includeZ ? (uint)1000 : 0) + (includeM ? (uint)2000 : 0), bWriter);
				WriteMultiPoint((Multipoint)geometry, bWriter, includeZ, includeM);
			}
			//else if (geometry is GeometryCollection)
			//	WriteUInt32((uint)WkbGeometryType.wkbGeometryCollection + (includeZ ? (uint)1000 : 0) + (includeM ? (uint)2000 : 0), bWriter);
			else	//If the type is not of the above 7 throw an exception.
				throw new ArgumentException("Invalid Geometry Type");
		}

		private void WritePoint(MapPoint point, BinaryWriter bWriter, bool includeZ, bool includeM)
		{
			WriteCoordinate(point, bWriter, includeZ, includeM);
		}

		private void WriteLineString(Polyline ls, BinaryWriter bWriter, bool includeZ, bool includeM)
		{
			WriteCoordinateCollection(ls.Parts[0].GetPoints().ToList(), bWriter, includeZ, includeM);
		}


		private void WriteMultiLineString(Polyline mls, BinaryWriter bWriter, bool includeZ, bool includeM)
		{
			//Write the number of linestrings.
			WriteUInt32((uint)mls.Parts.Count, bWriter);

			//Loop on the number of linestrings.
			foreach (var ls in mls.Parts)
			{
				//Write LineString Header
				bWriter.Write((byte)this.wkbByteOrder);
				WriteUInt32((uint)WkbGeometryType.wkbLineString, bWriter);
				//Write each linestring.
				WriteCoordinateCollection(ls.GetPoints().ToList(), bWriter, includeZ, includeM);
			}
		}
		

		private void WriteMultiPoint(Multipoint mp, BinaryWriter bWriter, bool includeZ, bool includeM)
		{
			//Write the number of points.
			WriteUInt32((uint)mp.Points.Count, bWriter);

			//Loop on the number of points.
			foreach (MapPoint p in mp.Points)
			{
				//Write Points Header
				bWriter.Write((byte)this.wkbByteOrder);
				WriteUInt32((uint)WkbGeometryType.wkbPoint, bWriter);
				//Write each point.
				WritePoint(p, bWriter, includeZ, includeM);
			}
		}
		private void WritePolygon(Polygon poly, BinaryWriter bWriter, bool includeZ, bool includeM)
		{
			WritePolygonRings(new Tuple<ReadOnlySegmentCollection, IList<ReadOnlySegmentCollection>>(poly.Parts[0], new List<ReadOnlySegmentCollection>()), bWriter, includeZ, includeM);			
		}
		private void WriteMultiPolygon(Polygon mp, BinaryWriter bWriter, bool includeZ, bool includeM)
		{
			var rings = Utilities.SplitMultiPolygon(mp);
			//Write the number of polygons.
			WriteUInt32((uint)rings.Count(), bWriter);

			//Loop on the number of polygons.
			foreach (var poly in rings)
			{
				//Write polygon header
				bWriter.Write((byte)this.wkbByteOrder);
				WriteUInt32((uint)WkbGeometryType.wkbPolygon, bWriter);
				//Write each polygon.
				WritePolygonRings(poly, bWriter, includeZ, includeM);
			}
		}

		private void WritePolygonRings(Tuple<ReadOnlySegmentCollection, IList<ReadOnlySegmentCollection>> poly, BinaryWriter bWriter, bool includeZ, bool includeM)
		{
			//Get the number of rings in this polygon.
			int numRings = poly.Item2.Count + 1;

			//Write the number of rings to the inputStream (add one for the shell)
			WriteUInt32((uint)numRings, bWriter);

			//Write the exterior of this polygon.
			WriteCoordinateCollection(poly.Item1.GetPoints().ToList(), bWriter, includeZ, includeM);

			//Loop on the number of rings - 1 because we already wrote the shell.
			foreach (var lr in poly.Item2)
				//Write the (lineString)LinearRing.
				WriteCoordinateCollection(lr.GetPoints().ToList(), bWriter, includeZ, includeM);
		}

		/*private void writeGeometryCollection(GeometryCollection gc, BinaryWriter bWriter, bool includeZ, bool includeM)
		{
			//Get the number of geometries in this geometrycollection.
			int numGeometries = gc.Geometries.Count;

			//Write the number of geometries.
			WriteUInt32((uint)numGeometries, bWriter);

			//Loop on the number of geometries.
			for (int i = 0; i < numGeometries; i++)
			{
				//Write the byte-order format of the following geometry.
				bWriter.Write((byte)this.wkbByteOrder);
				//Write the type of each geometry.
				WriteGeometry(gc.Geometries[i], bWriter, includeZ, includeM);
				//Write each geometry.
				WriteGeometry(gc.Geometries[i], bWriter, includeZ, includeM);
			}
		}*/

		private void WriteCoordinate(MapPoint coord, BinaryWriter bWriter, bool includeZ, bool includeM)
		{
			WriteDouble(coord.X, bWriter);
			WriteDouble(coord.Y, bWriter);
			if (includeZ)
				WriteDouble(coord.Z, bWriter);
			if (includeM)
				WriteDouble(coord.M, bWriter);
		}

		private void WriteCoordinateCollection(IList<MapPoint> coords, BinaryWriter bWriter, bool includeZ, bool includeM)
		{
			//Write the number of points in this linestring.
			WriteUInt32((uint)coords.Count, bWriter);

			//Loop on each vertices. Reverse for YX format
			//if (!SwapXY)
			//{
				for (int i = 0; i < coords.Count; i++)
					WriteCoordinate(coords[i], bWriter, includeZ, includeM);
			//}
			//else
			//{
			//    for (int i = coords.Count - 1; i >= 0; i--)
			//        WriteCoordinate(coords[i], bWriter, includeZ, includeM);
			//}
		}
		
		#region Number writer methods

		private void WriteUInt32(UInt32 value, BinaryWriter writer)
		{
			if (this.wkbByteOrder == WkbByteOrder.Xdr)
			{
				byte[] bytes = BitConverter.GetBytes(value);
				Array.Reverse(bytes);
				writer.Write(bytes);
			}
			else
				writer.Write(value);
		}

		private void WriteDouble(double value, BinaryWriter writer)
		{
			if (this.wkbByteOrder == WkbByteOrder.Xdr)
			{
				byte[] bytes = BitConverter.GetBytes(value);
				Array.Reverse(bytes);
				writer.Write(bytes);
			}
			else
				writer.Write(value);
		}

		#endregion
    }
}
