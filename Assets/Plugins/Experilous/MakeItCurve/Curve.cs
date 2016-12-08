/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using Experilous.Numerics;

#if false
namespace Experilous.MakeItCurve
{
	public class Curve
	{
		private struct Point
		{
			public Vector2 position;
			public float radius;

			public Point(Vector2 position)
			{
				this.position = position;
				radius = 0f;
			}

			public Point(Vector2 position, float radius)
			{
				this.position = position;
				this.radius = radius;
			}
		}

		private struct PointGroup
		{
			public int firstIndex;
			public int lastIndex;
			public bool isClosed;

			public PointGroup(int firstIndex, int lastIndex, bool isClosed)
			{
				this.firstIndex = firstIndex;
				this.lastIndex = lastIndex;
				this.isClosed = isClosed;
			}
		}

		private readonly List<Point> _points = new List<Point>();
		private readonly List<PointGroup> _pointGroups = new List<PointGroup>();
		private bool _isDirty = true;

		public void Add(Vector2 position)
		{
			_points.Add(new Point(position));
			_isDirty = true;
		}

		public void Add(Vector2 position, float radius)
		{
			_points.Add(new Point(position, radius));
			_isDirty = true;
		}

		public void Close()
		{
			EndPointGroup(true);
		}

		public void Break()
		{
			EndPointGroup(false);
		}

		private void EndPointGroup(bool isClosed)
		{
			int firstIndex = _pointGroups.Count == 0 ? 0 : _pointGroups[_pointGroups.Count - 1].lastIndex;
			if (firstIndex == _points.Count) throw new System.InvalidOperationException();

			_pointGroups.Add(new PointGroup(firstIndex, _points.Count, isClosed));
			_isDirty = true;
		}

		private struct Region
		{
		}

		private struct PointSite
		{
			public Vector2 position;
			public int firstConnectionIndex;
		}

		private struct PointSiteConnection
		{
			public Vector2 nearDirection;
			public int farPointSiteIndex;
			public int lineSiteIndex;
			public int reverseConnectionIndex;
			public int nextConnectionIndex;
		}

		private struct RegionEdge
		{
			public int twinEdgeIndex;
			public int farRegionIndex;
			public int prevEdgeIndex;
			public int nextEdgeIndex;
			public int regionDivisionIndex;
			public float t;

			public RegionEdge(int twinEdgeIndex, int farRegionIndex, int prevEdgeIndex, int nextEdgeIndex, int regionDivisionIndex, float t)
			{
				this.twinEdgeIndex = twinEdgeIndex;
				this.farRegionIndex = farRegionIndex;
				this.prevEdgeIndex = prevEdgeIndex;
				this.nextEdgeIndex = nextEdgeIndex;
				this.regionDivisionIndex = regionDivisionIndex;
				this.t = t;
			}

			public static void UpdatePrev(List<RegionEdge> list, int edgeIndex, int prevEdgeIndex)
			{
				var current = list[edgeIndex];
				list[edgeIndex] = new RegionEdge(current.twinEdgeIndex, current.farRegionIndex, prevEdgeIndex, current.nextEdgeIndex, current.regionDivisionIndex, current.t);
			}

			public static void UpdateNext(List<RegionEdge> list, int edgeIndex, int nextEdgeIndex)
			{
				var current = list[edgeIndex];
				list[edgeIndex] = new RegionEdge(current.twinEdgeIndex, current.farRegionIndex, current.prevEdgeIndex, nextEdgeIndex, current.regionDivisionIndex, current.t);
			}

			public static void UpdatePrevNext(List<RegionEdge> list, int edgeIndex, int prevEdgeIndex, int nextEdgeIndex)
			{
				var current = list[edgeIndex];
				list[edgeIndex] = new RegionEdge(current.twinEdgeIndex, current.farRegionIndex, prevEdgeIndex, nextEdgeIndex, current.regionDivisionIndex, current.t);
			}
		}

		private struct RegionDivision
		{
			public int divisionType;
			public float d0;

			public Vector2 a;
			public Vector2 b;
			public Vector2 c;

			public RegionDivision(int divisionType, float d0, Vector2 a, Vector2 b, Vector2 c)
			{
				this.divisionType = divisionType;
				this.d0 = d0;
				this.a = a;
				this.b = b;
				this.c = c;
			}
		}

		private struct PointSiteConnectionConstructionData
		{
			public int nearEdgeIndex;
			public int farEdgeIndex;
		}

		private List<PointSite> _pointSites = new List<PointSite>();
		private List<PointSiteConnection> _pointSiteConnections = new List<PointSiteConnection>();
		private List<Region> _regions= new List<Region>();
		private List<RegionDivision> _regionDivisions= new List<RegionDivision>();
		private List<RegionEdge> _regionEdges = new List<RegionEdge>();

		private void InitializeConstruction()
		{
			var connectionConstructionData = new PointSiteConnectionConstructionData[_pointSiteConnections.Count];

			for (int pointSiteIndex = 0; pointSiteIndex < _pointSites.Count; ++pointSiteIndex)
			{
				Vector2 p = _pointSites[pointSiteIndex].position;

				int firstConnectionIndex = _pointSites[pointSiteIndex].firstConnectionIndex;
				if (firstConnectionIndex == -1)
				{
					// Point site with no connections.
				}
				else
				{
					int previousConnectionIndex = firstConnectionIndex;
					int connectionIndex = _pointSiteConnections[previousConnectionIndex].nextConnectionIndex;

					if (previousConnectionIndex == connectionIndex)
					{
						// Point site with only one connection.

						// ---->/-\--C--\		Point Site:  i
						// <----\-/<---\ \		Line Segment Site:  ii
						//      ^:|    | |		Connections:  a, b
						//      |:A    | |		Divisions:  0, 1
						//      |0|    | |		Create Edge Twins:  A/B, C/D, E/F
						//      B:|    | |		Link:  B -> C -> E -> B
						//      |:v    | |		Temp Associations:
						// -b-> /-\    | |			a.near = A
						//  ii | i |   | |			a.twin.far = F
						// <-a- \-/    | |		All other links will be made when
						//      ^:|    | |			post-processing connections.
						//      |:F    | |
						//      |1|    | |
						//      E:|    | |
						//      |:v    | |
						// ---->/-\--D-/ /
						// <----\-/<----/

						Vector2 v = _pointSiteConnections[connectionIndex].nearDirection;

						var lineSiteIndex = _pointSiteConnections[connectionIndex].lineSiteIndex;

						var divisionIndex = _regionDivisions.Count;
						_regionDivisions.Add(new RegionDivision(0, 0f, Vector2.zero, Geometry.PerpendicularCCW(v), p));

						var lineSiteEdgeIndex = _regionEdges.Count;
						var innerPointSiteEdgeIndex = lineSiteEdgeIndex + 1;
						var outerPointSiteEdgeIndex = lineSiteEdgeIndex + 2;
						var externalEdgeIndex = lineSiteEdgeIndex + 3;
						_regions.Add(new Region());
						_regionEdges.Add(new RegionEdge(innerPointSiteEdgeIndex, pointSiteIndex, -1, -1, divisionIndex, float.PositiveInfinity));
						_regionEdges.Add(new RegionEdge(lineSiteEdgeIndex, lineSiteIndex, outerPointSiteEdgeIndex, outerPointSiteEdgeIndex, divisionIndex, float.PositiveInfinity));
						_regionEdges.Add(new RegionEdge(externalEdgeIndex, -1, innerPointSiteEdgeIndex, innerPointSiteEdgeIndex, -1, float.NaN));
						_regionEdges.Add(new RegionEdge(outerPointSiteEdgeIndex, pointSiteIndex, -1, -1, -1, float.NaN));

						connectionConstructionData[connectionIndex].nearEdgeIndex = lineSiteEdgeIndex;
						connectionConstructionData[_pointSiteConnections[connectionIndex].reverseConnectionIndex].farEdgeIndex = lineSiteEdgeIndex;
					}
					else
					{
						// Point site with two or more connections.

						Vector2 v0 = _pointSiteConnections[previousConnectionIndex].nearDirection;

						int stopConnectionIndex = connectionIndex;
						do
						{
							Vector2 v1 = _pointSiteConnections[connectionIndex].nearDirection;
							float sineFullAngle = Geometry.SinMagnitude(v0, v1);

							if (sineFullAngle > 0f)
							{
								// Convex angle (less than 180 degrees)

								Vector2 bisector;
								if (Vector2.Dot(v0, v1) > 0f)
								{
									bisector = (v0 + v1).normalized;
								}
								else
								{
									bisector = (Geometry.PerpendicularCW(v0) + Geometry.PerpendicularCCW(v1)).normalized;
								}
								float sineHalfAngle = Geometry.SinMagnitude(bisector, v1);
								
								//        ^ /			Point Site:  0
								//       / d			Line Segment Sites:  1, 2
								//      /1/				Connections: a, b as current, previous (also c, d as twins)
								//     b /				Division:  i
								//    / v				Create Edge Twins:  A/B
								//  /-\ --B------>		Link: <nothing>
								// | 0 |----i-----		Temp Associations:
								//  \-/ <------A--			b.near = A
								//    ^ \					a.twin.far = B
								//     \ a				All other links will be made when
								//      \2\					post-processing connections.
								//       c \
								//        \ v

								var lineSiteIndex = _pointSiteConnections[connectionIndex].lineSiteIndex;
								var previousLineSiteIndex = _pointSiteConnections[previousConnectionIndex].lineSiteIndex;

								var divisionIndex = _regionDivisions.Count;
								_regionDivisions.Add(new RegionDivision(1, 0f, Vector2.zero, bisector / sineHalfAngle, p));

								var previousLineSiteEdgeIndex = _regionEdges.Count;
								var lineSiteEdgeIndex = previousLineSiteEdgeIndex + 1;
								_regions.Add(new Region());
								_regionEdges.Add(new RegionEdge(lineSiteEdgeIndex, lineSiteIndex, -1, -1, divisionIndex, 0f));
								_regionEdges.Add(new RegionEdge(previousLineSiteEdgeIndex, previousLineSiteIndex, -1, -1, divisionIndex, float.PositiveInfinity));

								connectionConstructionData[previousConnectionIndex].nearEdgeIndex = previousLineSiteEdgeIndex;
								connectionConstructionData[_pointSiteConnections[connectionIndex].reverseConnectionIndex].farEdgeIndex = lineSiteEdgeIndex;
							}
							else if (sineFullAngle < 0f)
							{
								// Concave angle (more than 180 degrees)
								
								// -------->/-\----C-----\		Point Site:  0
								// <--------\-/<--------\ \		Line Segment Sites:  1, 2
								//          ^:|         | |		Connections:  a, b as current, previous (also c, d as twins)
								//          |:A         | |		Divisions:  i, ii
								//          |i|         D |		Create Edge Twins:  A/B, C/D, E/F
								//          B:|         | |		Link:  B -> C -> E -> B
								//          |:v         | v		Temp Associations:
								// --c----> /-\ --F---->/-\			b.near = A
								//  ---1---| 0 |===ii===  			a.twin.far = F
								// <----b-- \-/ <----E--\-/		All other links will be made when
								//          ^ |         ^ |			post-processing connections.
								//          | a         | |
								//          |2|         | |
								//          c |         | |
								//          | v         | v

								var lineSiteIndex = _pointSiteConnections[connectionIndex].lineSiteIndex;
								var previousLineSiteIndex = _pointSiteConnections[previousConnectionIndex].lineSiteIndex;

								var firstDivisionIndex = _regionDivisions.Count;
								var secondDivisionIndex = firstDivisionIndex + 1;
								_regionDivisions.Add(new RegionDivision(0, 0f, Vector2.zero, Geometry.PerpendicularCW(v0), p));
								_regionDivisions.Add(new RegionDivision(0, 0f, Vector2.zero, Geometry.PerpendicularCCW(v1), p));

								var previousLineSiteEdgeIndex = _regionEdges.Count;
								var firstInnerPointSiteEdgeIndex = previousLineSiteEdgeIndex + 1;
								var outerPointSiteEdgeIndex = previousLineSiteEdgeIndex + 2;
								var externalEdgeIndex = previousLineSiteEdgeIndex + 3;
								var secondInnerPointSiteEdgeIndex = previousLineSiteEdgeIndex + 4;
								var lineSiteEdgeIndex = previousLineSiteEdgeIndex + 5;

								_regionEdges.Add(new RegionEdge(firstInnerPointSiteEdgeIndex, pointSiteIndex, -1, -1, firstDivisionIndex, 0f));
								_regionEdges.Add(new RegionEdge(previousLineSiteEdgeIndex, previousLineSiteIndex, secondInnerPointSiteEdgeIndex, outerPointSiteEdgeIndex, firstDivisionIndex, float.PositiveInfinity));
								_regionEdges.Add(new RegionEdge(externalEdgeIndex, -1, firstInnerPointSiteEdgeIndex, secondInnerPointSiteEdgeIndex, -1, float.NaN));
								_regionEdges.Add(new RegionEdge(outerPointSiteEdgeIndex, pointSiteIndex, -1, -1, -1, float.NaN));
								_regionEdges.Add(new RegionEdge(lineSiteEdgeIndex, lineSiteIndex, outerPointSiteEdgeIndex, firstInnerPointSiteEdgeIndex, secondDivisionIndex, 0f));
								_regionEdges.Add(new RegionEdge(secondInnerPointSiteEdgeIndex, pointSiteIndex, -1, -1, secondDivisionIndex, float.PositiveInfinity));

								connectionConstructionData[previousConnectionIndex].nearEdgeIndex = previousLineSiteEdgeIndex;
								connectionConstructionData[_pointSiteConnections[connectionIndex].reverseConnectionIndex].farEdgeIndex = lineSiteEdgeIndex;
							}
							else if (Vector2.Dot(v0, v1) < 0f)
							{
								// Neither convex nor concave angle (exactly 180 degrees)

								var lineSiteIndex = _pointSiteConnections[connectionIndex].lineSiteIndex;
								var previousLineSiteIndex = _pointSiteConnections[previousConnectionIndex].lineSiteIndex;

								var divisionIndex = _regionDivisions.Count;
								_regionDivisions.Add(new RegionDivision(1, 0f, Vector2.zero, Geometry.PerpendicularCCW(v1), p));

								var previousLineSiteEdgeIndex = _regionEdges.Count;
								var lineSiteEdgeIndex = previousLineSiteEdgeIndex + 1;
								_regions.Add(new Region());
								_regionEdges.Add(new RegionEdge(lineSiteEdgeIndex, lineSiteIndex, -1, -1, divisionIndex, 0f));
								_regionEdges.Add(new RegionEdge(previousLineSiteEdgeIndex, previousLineSiteIndex, -1, -1, divisionIndex, float.PositiveInfinity));

								connectionConstructionData[previousConnectionIndex].nearEdgeIndex = previousLineSiteEdgeIndex;
								connectionConstructionData[_pointSiteConnections[connectionIndex].reverseConnectionIndex].farEdgeIndex = lineSiteEdgeIndex;
							}
							else
							{
								throw new System.InvalidOperationException("Intersecting line segment sites are invalid.");
							}

							previousConnectionIndex = connectionIndex;
							connectionIndex = _pointSiteConnections[connectionIndex].nextConnectionIndex;
						} while (connectionIndex != stopConnectionIndex);
					}
				}
			}

			for (int connectionIndex = 0; connectionIndex < _pointSiteConnections.Count; ++connectionIndex)
			{
				// ---->/-\---------->/-\---->		Point Sites:  1, 2
				// <----\-/<----------\-/<----		Line Segment Sites:  2, 3, 4
				//      ^:|           ^:|			Connections:  a, b as current, twin
				//      |:E           |:G			Existing Edge Twins:  A/B, C/D, E/F, G/H
				//      |:|           |:|			Existing Assocations:
				//      F:|           H:|				a.far = D
				//      |:v           |:v				a.near = A
				// ---> /-\ --a-----> /-\ --->			a.twin.far = F
				// -2--| 0 |----3----| 1 |--4-		Create Edge Twins:  I/J
				// <--- \-/ <-----b-- \-/ <---		Link D -> I -> A -> F
				//      ^:|           ^:|			All other links will be made when
				//      |:B           |:D				post-processing external region
				//      |:|           |:|
				//      A:|           C:|
				//      |:v           |:v
				// ---->/-\-----J---->/-\---->
				// <----\-/<----I-----\-/<----

				int reverseConnectionIndex = _pointSiteConnections[connectionIndex].reverseConnectionIndex;
				int lineSiteIndex = _pointSiteConnections[connectionIndex].lineSiteIndex;

				var farEdgeIndex = connectionConstructionData[connectionIndex].farEdgeIndex;
				var nearEdgeIndex = connectionConstructionData[connectionIndex].nearEdgeIndex;
				var nextNearEdgeIndex = connectionConstructionData[reverseConnectionIndex].farEdgeIndex;

				var outerLineSiteEdgeIndex = _regionEdges.Count;
				var externalEdgeIndex = outerLineSiteEdgeIndex + 1;
				_regions.Add(new Region());
				_regionEdges.Add(new RegionEdge(externalEdgeIndex, -1, farEdgeIndex, nearEdgeIndex, -1, 0f));
				_regionEdges.Add(new RegionEdge(outerLineSiteEdgeIndex, lineSiteIndex, -1, -1, -1, float.PositiveInfinity));

				RegionEdge.UpdateNext(_regionEdges, farEdgeIndex, outerLineSiteEdgeIndex);
				RegionEdge.UpdatePrevNext(_regionEdges, nearEdgeIndex, outerLineSiteEdgeIndex, nextNearEdgeIndex);
				RegionEdge.UpdatePrev(_regionEdges, nextNearEdgeIndex, nearEdgeIndex);
			}

			for (int edgeIndex = 0; edgeIndex < _regionEdges.Count; ++edgeIndex)
			{
				if (_regionEdges[edgeIndex].farRegionIndex == -1)
				{
					// -C-->/-\---A------>/-\-E-->		Point Sites:  1, 2
					// <--D-\-/<------B---\-/<--F-		Line Segment Sites:  2, 3, 4
					//      ^:|           ^:|			Existing Edge Twins:  A/B, C/D, E/F, G/H, I/J
					//      |:G           |:I			Half-link:  F -> B -> D
					//      |:|           |:|
					//      H:|           J:|
					//      |:v           |:v
					// ---> /-\ --------> /-\ --->
					// -2--| 0 |----3----| 1 |--4-
					// <--- \-/ <-------- \-/ <---

					var twinEdgeIndex = _regionEdges[edgeIndex].twinEdgeIndex;
					// twin.prev = edge.next.twin.next.twin
					var prevExternalEdgeIndex = _regionEdges[_regionEdges[_regionEdges[_regionEdges[edgeIndex].nextEdgeIndex].twinEdgeIndex].nextEdgeIndex].twinEdgeIndex;
					// twin.next = edge.prev.twin.prev.twin
					var nextExternalEdgeIndex = _regionEdges[_regionEdges[_regionEdges[_regionEdges[edgeIndex].prevEdgeIndex].twinEdgeIndex].prevEdgeIndex].twinEdgeIndex;

					RegionEdge.UpdatePrevNext(_regionEdges, twinEdgeIndex, prevExternalEdgeIndex, nextExternalEdgeIndex);
				}
			}
		}

		public Contour GetContour(float distance = 0f, float maxArcDegrees = 360f, float maxStraightLength = float.PositiveInfinity)
		{
			InitializeConstruction();

			// For each line segment site
			//_regions.Add(new Region());


			/* // <Description>
			1) Determine longest axis from point distribution.
			2) Sort points along axis.
			3) Determine longest line segment site length.
			4) For each point site:
				4.1) For each connected line segment site:
					4.1.1) If it is the only connected line segment site:
						4.1.1.1) Add a single perpendicular region boundary line between the point site and the line segment site.
					4.1.2) Else, if it forms a convex corner with the previous line segment site:
						4.1.2.1) Add the bisector of the two line segments as a region boundary between the two line segment site.
					4.1.3) Else, if it forms a concave corner with the previous line segment site:
						4.1.3.1) Add a perpedicular region boundary line between the first line segment site and the point site.
						4.1.3.2) Add a perpedicular region boundary line between the second line segment site and the point site.
					4.1.4) Else, if the lines are therefore colinear:
						4.1.4.1) Add a single perpendicular region boundary line between the two line segment sites.
					4.1.5) If the far end point site has already been processed:
						??? find intersections and add to the three-site queue.
				4.2) For each possibly nearest point site that is not directly connected:
					4.2.1) If it is closer than the previously found closest not-yet-adjacent site:
						4.2.1.1) Record as closest.
						4.2.1.2) For each connected line segment site of target point:
							4.2.1.2.1) If it is closer than the previously found closest not-yet-adjacent site:
								4.2.1.2.1.1) Record as closest.
				4.3) Add adjency with closest not-yet-adjacent site to the two-site queue.
			5) If both queues are empty, terminate.
			6) Else, if the shortest-distance item in the two-site queue is shorter than the shortest-distance item in the three-site queue:
				6.1) Pop intersection.
				6.2) 
			7) Else, if the shortest-distance item in the three-site queue is shorter than the shortest-distance item in the two-site queue:
			8) Go to (5).
			*/ // </Description>

			/* // <Description>
			1) Initialize all regions.
			2) Initialize all zero-distance boundaries.
			3) Intersect all adjacent boundaries and insert intersections into priority queue by distance of intersection from sites.
			4) For each point site,
				4.1) Find the nearest other point site whose region overlaps, and insert into priority queue.
				4.2) Find the nearest line segment site whose region overlaps, and insert into priority queue.
			5) While priority queue is not empty, pop the front item from the queue.
				5.1) If the adjacency is a point adjacency with only one boundary,
					5.1.1) ???
				5.2) Else if neither boundary of adjacency has been finalized,
					5.2.1) Finalize both boundaries.
					5.2.2) Compute the boundary of the adjacency.
					5.2.3) Intersect the two adjacent boundaries, and insert into priority queue.
				5.3) Else skip.
			*/ // </Description>

			/* // <Description>
			1) 
			2) Determine the 
			2) If there are no pending points, terminate.
			3) If there are no pending line segments, add a pending point.
				3.1) Determine which existing region the point is in.
				3.2) Determine the bounding line between the point's new region and the existing region that it is in.
				3.3) For each bounding line in the existing region,
					3.3.1) If the new bounding line intersects with the existing bounding line,
						3.3.1.1) Truncate both.
						3.3.1.2) Recurse to (3.2) with the new region and the region on the other side of the bounding line.
				3.4) Mark all not-yet-added line segments connected to the new point as pending.
				3.5) Go to (2).
			4) Add a pending line segment.
				4.1) Determine the bounding lines between the new region of the line segment and the regions of its two endpoints.
				4.2) For each bounding line in the 
				4.y) Mark all not-yet-added line segments connected to the new line segment as pending.
				4.z) Go to (2).
			*/ // </Description>

			// <PSEUDOCODE>
			BuildRegionsAndDivisions();

			foreach (var division in divisions)
			{
				float shortestDistance = float.PositiveInfinity;
				Division shortestNewDivision;
				foreach (var region in regions)
				{
					Division newDivision;
					if (ValidRegion(division, region))
					{
						if (region.shape == division.prevRegion.shape)
						{
							newDivision = GetDivision(division.prevRegion, region);
						}
						else
						{
							newDivision = GetDivision(division.nextRegion, region);
						}

						float t;
						Vector2 p;
						if (Intersect(division, newDivision, out t, out p))
						{
							if (t >= division.t0 && t <= division.t1)
							{
								float distance = GetDistance(division, t);
								if (distance < shortestDistance && RegionContains(region, p))
								{
									shortestDistance = distance;
									shortestNewDivision = newDivision;
								}
							}
						}
					}
				}

				if (shortestDistance < float.PositiveInfinity)
				{
					divisions.Add(shortestNewDivision);

				}
			}
			// </PSEUDOCODE>

			_divisions = new List<Division>();
			_regions = new List<Region>();

			foreach (var pointGroup in _pointGroups)
			{
				if (pointGroup.isClosed && pointGroup.lastIndex - pointGroup.firstIndex >= 3)
				{
					int firstDivisionIndex = _regions.Count;

					Vector2 p0 = _points[pointGroup.lastIndex - 2].position;
					Vector2 p1 = _points[pointGroup.firstIndex].position;
					Vector2 v1to0 = (p0 - p1).normalized;
					for (int i = pointGroup.firstIndex + 1; i < pointGroup.lastIndex; ++i)
					{
						Vector2 p2 = _points[i].position;
						Vector2 v1to2 = (p2 - p1).normalized;

						var bisector = v1to0 + v1to2;
						if (bisector != Vector2.zero)
						{
							bisector.Normalize();
							var interiorHalfAngleSine = Geometry.SinMagnitude(bisector, v1to2);
							if (interiorHalfAngleSine > 0f)
							{
								_divisions.Add(new Division(Vector2.zero, bisector / interiorHalfAngleSine, p1));
							}
							else
							{
								_divisions.Add(new Division(Vector2.zero, new Vector2(-v1to0.y, v1to0.x), p1));
								_divisions.Add(new Division(Vector2.zero, new Vector2(v1to2.y, -v1to2.x), p1));
							}
						}
						else
						{
							_divisions.Add(new Division(Vector2.zero, new Vector2(v1to2.y, -v1to2.x), p1));
						}

						p0 = p1;
						p1 = p2;
						v1to0 = -v1to2;
					}

					for (int i = firstDivisionIndex + 1; i < _divisions.Count; ++i)
					{
						_regions.Add(new Region(i - 1, i));
					}
					_regions.Add(new Region(_divisions.Count - 1, firstDivisionIndex));


				}
			}
		}
	}
}
#endif
