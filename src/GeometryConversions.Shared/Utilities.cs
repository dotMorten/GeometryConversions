using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeometryConversions
{
    internal static class Utilities
    {
		//SFS requires polygons to be ordered by outer and inner rings. Analyze multi polygons and split in individual polygons with one outer ring and internal rings
		internal static IEnumerable<Tuple<ReadOnlySegmentCollection, IList<ReadOnlySegmentCollection>>> SplitMultiPolygon(Polygon p)
		{
			List<Tuple<ReadOnlySegmentCollection, IList<ReadOnlySegmentCollection>>> outerRings =
				new List<Tuple<ReadOnlySegmentCollection, IList<ReadOnlySegmentCollection>>>();
			List<ReadOnlySegmentCollection> innerRings = new List<ReadOnlySegmentCollection>();
			foreach (var ring in p.Parts)
			{
				if (IsCcw(ring.GetPoints()))
				{
					outerRings.Add(new Tuple<ReadOnlySegmentCollection, IList<ReadOnlySegmentCollection>>(ring, new List<ReadOnlySegmentCollection>()));
				}
				else
					innerRings.Add(ring);
			}
			foreach (var ring in innerRings)
			{
				var inner = new Polygon(ring);
				foreach (var outerRing in outerRings)
				{
					if (GeometryEngine.Within(inner, new Polygon(outerRing.Item1)))
					{
						outerRing.Item2.Add(ring);
						break;
					}
				}
				throw new System.IO.InvalidDataException("Ring orientations are wrong - please simplify geometry first");
			}
			return outerRings;
		}

		/// <summary>
		/// This algorithm is checking the previous and next point around the highest point,
		/// and determines result based on whether the points are to the left or right of the highest point.
		/// </summary>
		/// <param name="points">The points.</param>
		/// <returns></returns>
		private static bool IsCcw(IEnumerable<MapPoint> points)
		{
			var vertices = points.ToList();
			int numPoints = vertices.Count;
			if (numPoints < 4) return false;

			//Look for point with the highest Y
			MapPoint highestPoint = vertices[0];
			int highestIndex = 0;
			for (int i = 1; i < numPoints; i++)
			{
				MapPoint p = vertices[i];
				if (p.Y > highestPoint.Y)
				{
					highestPoint = p;
					highestIndex = i;
				}
			}
			// find points on either side of highest point
			int prevIndex = highestIndex - 1;
			if (prevIndex < 0)
				prevIndex = numPoints - 2;
			int nextIndex = highestIndex + 1;
			if (nextIndex >= numPoints)
				nextIndex = 1;
			MapPoint prev = vertices[prevIndex];
			MapPoint next = vertices[nextIndex];
			// Center around highestPoint to avoid accuracy errors and calculate the cross-product:
			double cross =
				(next.X - highestPoint.X) * (prev.Y - highestPoint.Y) -
				(next.Y - highestPoint.Y) * (prev.X - highestPoint.X);

			// If the cross product is 0 the lines are collinear.
			if (cross == 0.0) // CCW if prev x is right of next x
				return (prev.X > next.X);
			else // CCW if area is positive
				return (cross > 0.0);
		}

		internal static IEnumerable<int> CountEnumerator(int count)
		{
			return CountEnumerator(0, count);
		}

		internal static IEnumerable<int> CountEnumerator(int start, int count)
		{
			for (int i = start; i < count; i++)
			{
				yield return i;
			}
		}
    }
}
