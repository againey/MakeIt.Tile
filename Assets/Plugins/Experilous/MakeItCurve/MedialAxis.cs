/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/
#if false
using UnityEngine;
using System.Collections.Generic;
using Experilous.Numerics;
using Experilous.Containers;
using System;

namespace Experilous.MakeItCurve
{
	public class MedialAxis2D
	{
		private struct BoundaryPoint
		{
			public Vector2 position;
			public int region;
			public int firstBoundaryEdge;
			public int firstRegionEdge;

			public BoundaryPoint(Vector2 position, int region, int firstBoundaryEdge, int firstRegionEdge)
			{
				this.position = position;
				this.region = region;
				this.firstBoundaryEdge = firstBoundaryEdge;
				this.firstRegionEdge = firstRegionEdge;
			}
		}

		private struct BoundaryEdge
		{
			public Vector2 direction;
			public int farPoint;
			public int region;
			public int firstRegionEdge;
			public int twinEdge;
			public int prevEdge;
			public int nextEdge;

			public BoundaryEdge(Vector2 direction, int farPoint, int region, int firstRegionEdge, int twinEdge, int prevEdge, int nextEdge)
			{
				this.direction = direction;
				this.farPoint = farPoint;
				this.region = region;
				this.firstRegionEdge = firstRegionEdge;
				this.twinEdge = twinEdge;
				this.prevEdge = prevEdge;
				this.nextEdge = nextEdge;
			}
		}

		private enum RegionType
		{
			External = 0,
			Point = 1,
			Line = 2,
			Arc = 3,
		}

		private struct Region
		{
			public RegionType type;
			public int boundaryIndex;

			public Region(RegionType type, int boundaryIndex)
			{
				this.type = type;
				this.boundaryIndex = boundaryIndex;
			}
		}

		private struct RegionEdge
		{
			public int farRegion;
			public int twinEdge;
			public int prevEdge;
			public int nextEdge;
			public int regionDivision;
			public float farT;

			public RegionEdge(int farRegion, int twinEdge, int prevEdge, int nextEdge, int regionDivision, float farT)
			{
				this.farRegion = farRegion;
				this.twinEdge = twinEdge;
				this.prevEdge = prevEdge;
				this.nextEdge = nextEdge;
				this.regionDivision = regionDivision;
				this.farT = farT;
			}
		}

		[Flags]
		private enum RegionDivisionType
		{
			SimpleLine = 1,
			PythagoreanLine = 2,
			ParallelLine = 4,
			Line = SimpleLine | PythagoreanLine | ParallelLine,
			Parabola = 8,
		}

		private struct RegionDivision
		{
			public RegionDivisionType type;
			public float nearestDistance;
			public Vector2 position;
			public Vector2 vector;

			public RegionDivision(RegionDivisionType type, float nearestDistance, Vector2 position, Vector2 vector)
			{
				this.type = type;
				this.nearestDistance = nearestDistance;
				this.position = position;
				this.vector = vector;
			}

			public float GetDistance(float t)
			{
				switch (type)
				{
					case RegionDivisionType.SimpleLine: return t;
					case RegionDivisionType.PythagoreanLine: return Mathf.Sqrt(t * t + nearestDistance * nearestDistance);
					case RegionDivisionType.ParallelLine: return nearestDistance;
					case RegionDivisionType.Parabola: return nearestDistance * (4f * t * t + 1f);
					default: throw new NotImplementedException();
				}
			}

			public Vector2 GetPosition(float t)
			{
				switch (type)
				{
					case RegionDivisionType.SimpleLine:
					case RegionDivisionType.PythagoreanLine:
					case RegionDivisionType.ParallelLine:
						return vector * t + position;
					case RegionDivisionType.Parabola:
						return (vector * t + Geometry.PerpendicularCW(vector)) * t + position;
					default: throw new NotImplementedException();
				}
			}

			public float Project(Vector2 position)
			{
				var delta = position - this.position;
				switch (type)
				{
					case RegionDivisionType.SimpleLine:
					case RegionDivisionType.PythagoreanLine:
					case RegionDivisionType.ParallelLine:
						return Vector2.Dot(delta, vector) / Vector2.Dot(vector, vector);
					case RegionDivisionType.Parabola:
						return Vector2.Dot(delta, Geometry.PerpendicularCW(vector)) / (16f * nearestDistance * nearestDistance);
					default: throw new NotImplementedException();
				}
			}
		}

		private readonly List<BoundaryPoint> _boundaryPoints = new List<BoundaryPoint>();
		private readonly List<BoundaryEdge> _boundaryEdges = new List<BoundaryEdge>();

		private readonly List<Region> _regions = new List<Region>();
		private RegionEdge[] _regionEdges;
		private int _regionEdgeCount;
		private readonly List<RegionDivision> _regionDivisions = new List<RegionDivision>();

		private MedialAxis2D()
		{
		}

		private void AddRegionEdge(RegionEdge edge)
		{
			if (_regionEdges == null)
			{
				_regionEdges = new RegionEdge[_boundaryPoints.Count + _boundaryEdges.Count + 16];
			}
			else if (_regionEdgeCount == _regionEdges.Length)
			{
				var newBuffer = new RegionEdge[_regionEdges.Length + (_regionEdges.Length >> 2)];
				Array.Copy(_regionEdges, newBuffer, _regionEdgeCount);
				_regionEdges = newBuffer;
			}
			_regionEdges[_regionEdgeCount++] = edge;
		}

		public class Builder
		{
			private readonly List<Vector2> _points = new List<Vector2>();

			private readonly List<int> _pointFirstEdgeIndices = new List<int>();
			private readonly List<int> _edgeTwinEdgeIndices = new List<int>();
			private readonly List<int> _edgeNextEdgeIndices = new List<int>();
			private readonly List<int> _edgeFarPointIndices = new List<int>();

			private int _startPointIndex = -1;
			private int _endPointIndex = -1;

			public int Add(Vector2 position)
			{
				_startPointIndex = _endPointIndex = _points.Count;
				_points.Add(position);
				_pointFirstEdgeIndices.Add(-1);
				return _endPointIndex;
			}

			public int Extend(Vector2 position)
			{
				if (_endPointIndex == -1)
				{
					throw new System.InvalidOperationException("Cannot extend chain if no chain has been started.");
				}
				int newPointIndex = Add(position);
				Connect(_endPointIndex, newPointIndex);
				_endPointIndex = newPointIndex;
				return newPointIndex;
			}

			private void InsertEdge(int edgeIndex, int sourcePointIndex, int targetPointIndex)
			{
				int firstEdgeIndex = _pointFirstEdgeIndices[sourcePointIndex];
				if (firstEdgeIndex == -1)
				{
					_pointFirstEdgeIndices[sourcePointIndex] = edgeIndex;
					_edgeNextEdgeIndices.Add(edgeIndex);
				}
				else
				{
					int prevEdgeIndex = firstEdgeIndex;
					int nextEdgeIndex = _edgeNextEdgeIndices[prevEdgeIndex];

					if (prevEdgeIndex == nextEdgeIndex)
					{
						_edgeNextEdgeIndices[prevEdgeIndex] = edgeIndex;
						_edgeNextEdgeIndices.Add(nextEdgeIndex);
					}
					else
					{
						Vector2 sourcePoint = _points[sourcePointIndex];
						Vector2 targetVector = _points[targetPointIndex] - sourcePoint;
						Vector2 prevVector = _points[_edgeFarPointIndices[prevEdgeIndex]] - sourcePoint;
						float prevTargetSine = Geometry.SinMagnitude(prevVector, targetVector);

						do
						{
							int nextPointIndex = _edgeFarPointIndices[nextEdgeIndex];

							if (nextPointIndex == targetPointIndex)
							{
								throw new System.InvalidOperationException("Cannot connect two points that are already connected.");
							}

							Vector2 nextVector = _points[nextPointIndex] - sourcePoint;
							float targetNextSine = Geometry.SinMagnitude(targetVector, nextVector);
							float prevNextSine = Geometry.SinMagnitude(prevVector, nextVector);

							if (prevTargetSine > 0f && targetNextSine > 0f || (prevTargetSine > 0f || targetNextSine > 0f) && prevNextSine < 0f)
							{
								_edgeNextEdgeIndices[prevEdgeIndex] = edgeIndex;
								_edgeNextEdgeIndices.Add(nextEdgeIndex);
								return;
							}

							prevEdgeIndex = nextEdgeIndex;
							nextEdgeIndex = _edgeNextEdgeIndices[nextEdgeIndex];
							prevVector = nextVector;
							prevTargetSine = -targetNextSine;
						} while (prevEdgeIndex != firstEdgeIndex);

						throw new System.InvalidOperationException("No valid place found for inserting edge.");
					}
				}
			}

			public void Connect(int sourcePointIndex, int targetPointIndex)
			{
				if (sourcePointIndex < 0 || sourcePointIndex >= _points.Count)
				{
					throw new System.ArgumentOutOfRangeException("sourcePointIndex");
				}
				if (targetPointIndex < 0 || targetPointIndex >= _points.Count)
				{
					throw new System.ArgumentOutOfRangeException("targetPointIndex");
				}
				if (sourcePointIndex == targetPointIndex)
				{
					throw new System.InvalidOperationException("Cannot connect a point to itself.");
				}

				int edgeIndex = _edgeFarPointIndices.Count;
				int twinEdgeIndex = edgeIndex + 1;

				_edgeTwinEdgeIndices.Add(twinEdgeIndex);
				_edgeFarPointIndices.Add(targetPointIndex);

				_edgeTwinEdgeIndices.Add(edgeIndex);
				_edgeFarPointIndices.Add(sourcePointIndex);

				InsertEdge(edgeIndex, sourcePointIndex, targetPointIndex);
				InsertEdge(twinEdgeIndex, targetPointIndex, sourcePointIndex);
			}

			public void Close()
			{
				if (_endPointIndex == -1 || _startPointIndex == -1)
				{
					throw new System.InvalidOperationException("Cannot close chain when no chain has been started.");
				}
				else if (_endPointIndex == _startPointIndex)
				{
					throw new System.InvalidOperationException("Cannot close chain when the start and end point are the same.");
				}
				Connect(_endPointIndex, _startPointIndex);
				_endPointIndex = _startPointIndex;
			}

			public void MoveTo(int pointIndex)
			{
				if (pointIndex < 0 || pointIndex >= _points.Count)
				{
					throw new System.ArgumentOutOfRangeException("pointIndex");
				}
				_startPointIndex = _endPointIndex = pointIndex;
			}

			public int startPointIndex
			{
				get
				{
					return _startPointIndex;
				}
				set
				{
					if (value < 0 || value >= _points.Count)
					{
						throw new System.ArgumentOutOfRangeException("value");
					}
					_startPointIndex = value;
				}
			}

			public int endPointIndex
			{
				get
				{
					return _endPointIndex;
				}
				set
				{
					if (value < 0 || value >= _points.Count)
					{
						throw new System.ArgumentOutOfRangeException("value");
					}
					_endPointIndex = value;
				}
			}

			public MedialAxis2D Build()
			{
				var medialAxis = new MedialAxis2D();

				BuildBoundariesAndRegions(medialAxis);
				BuildConnectedDivisions(medialAxis);
				CalculateRemainingRegionEdges(medialAxis);

				return medialAxis;
			}

			private void BuildBoundariesAndRegions(MedialAxis2D medialAxis)
			{
				for (int i = 0; i < _points.Count; ++i)
				{
					medialAxis._regions.Add(new Region(RegionType.Point, i));
					medialAxis._boundaryPoints.Add(new BoundaryPoint(_points[i], i, _pointFirstEdgeIndices[i], -1));
				}

				for (int i = 0; i < _edgeFarPointIndices.Count; ++i)
				{
					medialAxis._regions.Add(new Region(RegionType.Line, i));
					var farPointIndex = _edgeFarPointIndices[i];
					var direction = (_points[farPointIndex] - _points[_edgeFarPointIndices[_edgeTwinEdgeIndices[i]]]).normalized;
					var regionEdgeIndex = medialAxis._regionEdgeCount;
					var twinEdge = _edgeTwinEdgeIndices[i];
					medialAxis.AddRegionEdge(new RegionEdge(twinEdge + _points.Count, twinEdge, -1, -1, -1, float.PositiveInfinity));
					medialAxis._boundaryEdges.Add(new BoundaryEdge(direction, farPointIndex, i + _points.Count, i, twinEdge, -1, _edgeNextEdgeIndices[i]));
				}
			}

			private void BuildConnectedDivisions(MedialAxis2D medialAxis)
			{
				for (int i = 0; i < medialAxis._boundaryPoints.Count; ++i)
				{
					var point = medialAxis._boundaryPoints[i];

					int firstEdgeIndex = point.firstBoundaryEdge;

					if (firstEdgeIndex == -1)
					{
						// Boundary point with no connected boundary edges.
					}
					else
					{
						int prevEdgeIndex = firstEdgeIndex;
						int nextEdgeIndex = medialAxis._boundaryEdges[prevEdgeIndex].nextEdge;

						if (prevEdgeIndex == nextEdgeIndex)
						{
							// Boundary point with only one connected boundary edge.

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

							var boundaryEdge = medialAxis._boundaryEdges[nextEdgeIndex];
							var twinBoundaryEdge = medialAxis._boundaryEdges[boundaryEdge.twinEdge];

							var firstDivisionIndex = medialAxis._regionDivisions.Count;
							medialAxis._regionDivisions.Add(new RegionDivision(RegionDivisionType.SimpleLine, 0f, point.position, Geometry.PerpendicularCW(boundaryEdge.direction)));
							var secondDivisionIndex = medialAxis._regionDivisions.Count;
							medialAxis._regionDivisions.Add(new RegionDivision(RegionDivisionType.SimpleLine, 0f, point.position, Geometry.PerpendicularCCW(boundaryEdge.direction)));

							var firstInwardEdgeIndex = medialAxis._regionEdgeCount;
							var firstOutwardEdgeIndex = firstInwardEdgeIndex + 1;
							var forwardExternalEdgeIndex = firstInwardEdgeIndex + 2;
							var backwardExternalEdgeIndex = firstInwardEdgeIndex + 3;
							var secondInwardEdgeIndex = firstInwardEdgeIndex + 1;
							var secondOutwardEdgeIndex = firstInwardEdgeIndex + 2;

							medialAxis.AddRegionEdge(new RegionEdge(point.region, firstOutwardEdgeIndex, -1, boundaryEdge.firstRegionEdge, firstDivisionIndex, 0f));
							medialAxis.AddRegionEdge(new RegionEdge(twinBoundaryEdge.region, firstInwardEdgeIndex, secondInwardEdgeIndex, forwardExternalEdgeIndex, firstDivisionIndex, float.PositiveInfinity));
							medialAxis.AddRegionEdge(new RegionEdge(-1, backwardExternalEdgeIndex, firstOutwardEdgeIndex, secondInwardEdgeIndex, -1, float.PositiveInfinity));
							medialAxis.AddRegionEdge(new RegionEdge(point.region, forwardExternalEdgeIndex, -1, -1, -1, float.PositiveInfinity));
							medialAxis.AddRegionEdge(new RegionEdge(boundaryEdge.region, secondOutwardEdgeIndex, forwardExternalEdgeIndex, firstOutwardEdgeIndex, secondDivisionIndex, 0f));
							medialAxis.AddRegionEdge(new RegionEdge(point.region, secondInwardEdgeIndex, twinBoundaryEdge.firstRegionEdge, -1, secondDivisionIndex, float.PositiveInfinity));

							medialAxis._regionEdges[boundaryEdge.firstRegionEdge].prevEdge = firstInwardEdgeIndex;
							medialAxis._regionEdges[twinBoundaryEdge.firstRegionEdge].nextEdge = secondOutwardEdgeIndex;
						}
						else
						{
							// Boundary point with two or more connected boundary edges.

							var prevBoundaryEdge = medialAxis._boundaryEdges[prevEdgeIndex];
							Vector2 prevDirection = prevBoundaryEdge.direction;

							do
							{
								var nextBoundaryEdge = medialAxis._boundaryEdges[nextEdgeIndex];
								Vector2 nextDirection = nextBoundaryEdge.direction;

								float sineFullAngle = Geometry.SinMagnitude(prevDirection, nextDirection);

								if (sineFullAngle > 0f)
								{
									// Convex angle (less than 180 degrees)

									Vector2 bisector;
									if (Vector2.Dot(prevDirection, nextDirection) > 0f)
									{
										// If angle between is less than 90 degrees, just add and normalize.
										bisector = (prevDirection + nextDirection).normalized;
									}
									else
									{
										// If angle between is greater than 90 degrees, add the normals instead, and then normalize.
										bisector = (Geometry.PerpendicularCW(prevDirection) + Geometry.PerpendicularCCW(nextDirection)).normalized;
									}
									float sineHalfAngle = Geometry.SinMagnitude(bisector, nextDirection);
								
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

									var divisionIndex = medialAxis._regionDivisions.Count;
									medialAxis._regionDivisions.Add(new RegionDivision(RegionDivisionType.SimpleLine, 0f, point.position, bisector / sineHalfAngle));

									var inwardEdgeIndex = medialAxis._regionEdgeCount;
									var outwardEdgeIndex = inwardEdgeIndex + 1;

									var twinNextBoundaryEdge = medialAxis._boundaryEdges[nextBoundaryEdge.twinEdge];

									medialAxis.AddRegionEdge(new RegionEdge(twinNextBoundaryEdge.region, outwardEdgeIndex, -1, prevBoundaryEdge.firstRegionEdge, divisionIndex, 0f));
									medialAxis.AddRegionEdge(new RegionEdge(prevBoundaryEdge.region, inwardEdgeIndex, twinNextBoundaryEdge.firstRegionEdge, -1, divisionIndex, float.PositiveInfinity));

									medialAxis._regionEdges[prevBoundaryEdge.firstRegionEdge].prevEdge = inwardEdgeIndex;
									medialAxis._regionEdges[twinNextBoundaryEdge.firstRegionEdge].nextEdge = outwardEdgeIndex;
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

									var twinNextBoundaryEdge = medialAxis._boundaryEdges[nextBoundaryEdge.twinEdge];

									var firstDivisionIndex = medialAxis._regionDivisions.Count;
									medialAxis._regionDivisions.Add(new RegionDivision(RegionDivisionType.SimpleLine, 0f, point.position, Geometry.PerpendicularCW(prevDirection)));
									var secondDivisionIndex = medialAxis._regionDivisions.Count;
									medialAxis._regionDivisions.Add(new RegionDivision(RegionDivisionType.SimpleLine, 0f, point.position, Geometry.PerpendicularCCW(nextDirection)));

									var firstInwardEdgeIndex = medialAxis._regionEdgeCount;
									var firstOutwardEdgeIndex = firstInwardEdgeIndex + 1;
									var forwardExternalEdgeIndex = firstInwardEdgeIndex + 2;
									var backwardExternalEdgeIndex = firstInwardEdgeIndex + 3;
									var secondInwardEdgeIndex = firstInwardEdgeIndex + 1;
									var secondOutwardEdgeIndex = firstInwardEdgeIndex + 2;

									medialAxis.AddRegionEdge(new RegionEdge(point.region, firstOutwardEdgeIndex, -1, prevBoundaryEdge.firstRegionEdge, firstDivisionIndex, 0f));
									medialAxis.AddRegionEdge(new RegionEdge(prevBoundaryEdge.region, firstInwardEdgeIndex, secondInwardEdgeIndex, forwardExternalEdgeIndex, firstDivisionIndex, float.PositiveInfinity));
									medialAxis.AddRegionEdge(new RegionEdge(-1, backwardExternalEdgeIndex, firstOutwardEdgeIndex, secondInwardEdgeIndex, -1, float.PositiveInfinity));
									medialAxis.AddRegionEdge(new RegionEdge(point.region, forwardExternalEdgeIndex, -1, -1, -1, float.PositiveInfinity));
									medialAxis.AddRegionEdge(new RegionEdge(twinNextBoundaryEdge.region, secondOutwardEdgeIndex, forwardExternalEdgeIndex, firstOutwardEdgeIndex, secondDivisionIndex, 0f));
									medialAxis.AddRegionEdge(new RegionEdge(point.region, secondInwardEdgeIndex, twinNextBoundaryEdge.firstRegionEdge, -1, secondDivisionIndex, float.PositiveInfinity));

									medialAxis._regionEdges[prevBoundaryEdge.firstRegionEdge].prevEdge = firstInwardEdgeIndex;
									medialAxis._regionEdges[twinNextBoundaryEdge.firstRegionEdge].nextEdge = secondOutwardEdgeIndex;
								}
								else if (Vector2.Dot(prevDirection, nextDirection) < 0f)
								{
									// Neither convex nor concave angle (exactly 180 degrees)

									var divisionIndex = medialAxis._regionDivisions.Count;
									medialAxis._regionDivisions.Add(new RegionDivision(RegionDivisionType.SimpleLine, 0f, point.position, Geometry.PerpendicularCCW(nextDirection)));

									var inwardEdgeIndex = medialAxis._regionEdgeCount;
									var outwardEdgeIndex = inwardEdgeIndex + 1;

									var twinNextBoundaryEdge = medialAxis._boundaryEdges[nextBoundaryEdge.twinEdge];

									medialAxis.AddRegionEdge(new RegionEdge(twinNextBoundaryEdge.region, outwardEdgeIndex, -1, prevBoundaryEdge.firstRegionEdge, divisionIndex, 0f));
									medialAxis.AddRegionEdge(new RegionEdge(prevBoundaryEdge.region, inwardEdgeIndex, twinNextBoundaryEdge.firstRegionEdge, -1, divisionIndex, float.PositiveInfinity));

									medialAxis._regionEdges[prevBoundaryEdge.firstRegionEdge].prevEdge = inwardEdgeIndex;
									medialAxis._regionEdges[twinNextBoundaryEdge.firstRegionEdge].nextEdge = outwardEdgeIndex;
								}
								else
								{
									throw new System.InvalidOperationException("Intersecting line segment sites are invalid.");
								}

								prevEdgeIndex = nextEdgeIndex;
								nextEdgeIndex = nextBoundaryEdge.nextEdge;
							} while (prevEdgeIndex != firstEdgeIndex);
						}
					}
				}

				for (int i = 0; i < medialAxis._boundaryEdges.Count; ++i)
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

					var boundaryEdge = medialAxis._boundaryEdges[i];

					var regionEdge = medialAxis._regionEdges[boundaryEdge.firstRegionEdge];
					var twinRegionEdge = medialAxis._regionEdges[regionEdge.twinEdge];

					var farOutwardEdgeIndex = regionEdge.nextEdge;
					var nearInwardEdgeIndex = regionEdge.prevEdge;
					var nearOutwardEdgeIndex = twinRegionEdge.nextEdge;

					var forwardExternalEdgeIndex = medialAxis._regionEdgeCount;
					var backwardExternalEdgeIndex = forwardExternalEdgeIndex + 1;

					medialAxis.AddRegionEdge(new RegionEdge(-1, backwardExternalEdgeIndex, farOutwardEdgeIndex, nearInwardEdgeIndex, -1, float.PositiveInfinity));
					medialAxis.AddRegionEdge(new RegionEdge(boundaryEdge.region, forwardExternalEdgeIndex, -1, -1, -1, float.PositiveInfinity));

					medialAxis._regionEdges[farOutwardEdgeIndex].nextEdge = forwardExternalEdgeIndex;
					medialAxis._regionEdges[nearInwardEdgeIndex].prevEdge = forwardExternalEdgeIndex;
					medialAxis._regionEdges[nearInwardEdgeIndex].nextEdge = nearOutwardEdgeIndex;
					medialAxis._regionEdges[nearOutwardEdgeIndex].prevEdge = nearInwardEdgeIndex;
				}

				for (int i = 0; i < medialAxis._regionEdgeCount; ++i)
				{
					var regionEdge = medialAxis._regionEdges[i];
					if (regionEdge.farRegion == -1)
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

						// twin.prev = edge.next.twin.next.twin
						var prevExternalEdgeIndex = medialAxis._regionEdges[medialAxis._regionEdges[medialAxis._regionEdges[regionEdge.nextEdge].twinEdge].nextEdge].twinEdge;
						// twin.next = edge.prev.twin.prev.twin
						var nextExternalEdgeIndex = medialAxis._regionEdges[medialAxis._regionEdges[medialAxis._regionEdges[regionEdge.prevEdge].twinEdge].prevEdge].twinEdge;

						medialAxis._regionEdges[regionEdge.twinEdge].prevEdge = prevExternalEdgeIndex;
						medialAxis._regionEdges[regionEdge.twinEdge].nextEdge = nextExternalEdgeIndex;
					}
				}
			}

			private class BoundaryLookup
			{
				private Builder _builder;
				private MedialAxis2D _medialAxis;

				private Vector2 _center;
				private Vector2 _direction;

				private int[] _sortedPointIndices;
				private int[] _reverseMap;

				private float _maxLengthSquared;

				public BoundaryLookup(Builder builder, MedialAxis2D medialAxis)
				{
					_builder = builder;
					_medialAxis = medialAxis;

					ApproximatePrincipalAxis();
					SortPoints();
					CalculateMaxLineLengthSquared();
				}

				private void ApproximatePrincipalAxis()
				{
					var pointSum = Vector2.zero;
					foreach (var point in _builder._points)
					{
						pointSum += point;
					}

					_center = pointSum / _builder._points.Count;

					var xSum = Vector2.zero;
					var ySum = Vector2.zero;

					foreach (var point in _builder._points)
					{
						var delta = point - _center;
						xSum += delta * delta.x;
						ySum += delta * delta.y;
					}

					if (Vector2.Dot(xSum, ySum) < 0f)
					{
						ySum = -ySum;
					}

					_direction = xSum + ySum;
				}

				private void SortPoints()
				{
					var axialPositions = new float[_builder._points.Count];
					_sortedPointIndices = new int[_builder._points.Count];

					for (int i = 0; i < _builder._points.Count; ++i)
					{
						axialPositions[i] = Vector2.Dot(_direction, _builder._points[i] - _center);
						_sortedPointIndices[i] = i;
					}

					Array.Sort(axialPositions, _sortedPointIndices);

					_reverseMap = new int[_builder._points.Count];
					for (int i = 0; i < _builder._points.Count; ++i)
					{
						_reverseMap[_sortedPointIndices[i]] = i;
					}
				}

				private void CalculateMaxLineLengthSquared()
				{
					_maxLengthSquared = 0f;
					for (int i = 0; i < _builder._points.Count; ++i)
					{
						var p0 = _builder._points[i];

						var firstEdge = _builder._pointFirstEdgeIndices[i];
						var edge = firstEdge;
						do
						{
							var p1 = _builder._points[_builder._edgeFarPointIndices[edge]];
							var lengthSquared = (p1 - p0).sqrMagnitude;
							if (_maxLengthSquared < lengthSquared) _maxLengthSquared = lengthSquared;
							edge = _builder._edgeNextEdgeIndices[edge];
						} while (edge != firstEdge);
					}
				}

				public int FindNearestNonAdjacentRegion(int regionIndex, out float distance)
				{
					var sourceRegion = _medialAxis._regions[regionIndex];
					if (sourceRegion.type != RegionType.Point) throw new ArgumentException("The specified region is not a point region.", "regionIndex");

					var pointIndex = sourceRegion.boundaryIndex;

					var i = _reverseMap[pointIndex];
					var j = i + 1;
					i -= 1;

				}
			}

			private struct TwoWayRegionIntersection : System.IEquatable<TwoWayRegionIntersection>
			{
				public float nearestDistance;
				public float nearestT;
				public int pointRegion;
				public int otherRegion;

				public TwoWayRegionIntersection(float nearestDistance, float nearestT, int pointRegion, int otherRegion)
				{
					this.nearestDistance = nearestDistance;
					this.nearestT = nearestT;
					this.pointRegion = pointRegion;
					this.otherRegion = otherRegion;
				}

				public bool Equals(TwoWayRegionIntersection other)
				{
					return pointRegion == other.pointRegion && otherRegion == other.otherRegion;
				}

				public static bool AreOrdered(TwoWayRegionIntersection lhs, TwoWayRegionIntersection rhs)
				{
					return lhs.nearestDistance <= rhs.nearestDistance;
				}
			}

			private struct ThreeWayRegionIntersection : IEquatable<ThreeWayRegionIntersection>
			{
				public float nearestDistance;
				public float s;
				public float t;
				public int externalRegionEdge;

				public ThreeWayRegionIntersection(float nearestDistance, float s, float t, int externalRegionEdge)
				{
					this.nearestDistance = nearestDistance;
					this.s = s;
					this.t = t;
					this.externalRegionEdge = externalRegionEdge;
				}

				public bool Equals(ThreeWayRegionIntersection other)
				{
					return externalRegionEdge == other.externalRegionEdge;
				}

				public static bool AreOrdered(ThreeWayRegionIntersection lhs, ThreeWayRegionIntersection rhs)
				{
					return lhs.nearestDistance <= rhs.nearestDistance;
				}
			}

			private RegionDivision DivideRegions(MedialAxis2D medialAxis, int firstRegionIndex, int secondRegionIndex)
			{
				var firstRegion = medialAxis._regions[firstRegionIndex];
				var secondRegion = medialAxis._regions[secondRegionIndex];

				if (firstRegion.type > secondRegion.type)
				{
					Core.GeneralUtility.Swap(ref firstRegionIndex, ref secondRegionIndex);
					firstRegion = medialAxis._regions[firstRegionIndex];
					secondRegion = medialAxis._regions[secondRegionIndex];
				}

				switch (firstRegion.type)
				{
					case RegionType.Point:
					{
						var firstBoundary = medialAxis._boundaryPoints[firstRegion.boundaryIndex];
						switch (secondRegion.type)
						{
							case RegionType.Point:
							{
								var secondBoundary = medialAxis._boundaryPoints[secondRegion.boundaryIndex];
								var delta = secondBoundary.position - firstBoundary.position;
								var midpoint = (firstBoundary.position + secondBoundary.position) * 0.5f;
								var separation = delta.magnitude;
								return new RegionDivision(RegionDivisionType.PythagoreanLine, separation * 0.5f, midpoint, Geometry.PerpendicularCW(delta) / separation);
							}
							case RegionType.Line:
							{
								var secondBoundary = medialAxis._boundaryEdges[secondRegion.boundaryIndex];
								var secondBoundaryPosition = medialAxis._boundaryPoints[secondBoundary.farPoint].position;
								var projection = firstBoundary.position.ProjectOntoUnit(secondBoundaryPosition, secondBoundary.direction);
								var delta = firstBoundary.position - projection;
								var midpoint = (firstBoundary.position + projection) * 0.5f;
								var separation = delta.magnitude;
								return new RegionDivision(RegionDivisionType.Parabola, separation * 0.5f, midpoint, delta * 2f);
							}
						}
						break;
					}
					case RegionType.Line:
					{
						var firstBoundary = medialAxis._boundaryEdges[firstRegion.boundaryIndex];
						var firstBoundaryPosition = medialAxis._boundaryPoints[firstBoundary.farPoint].position;
						switch (secondRegion.type)
						{
							case RegionType.Line:
							{
								var secondBoundary = medialAxis._boundaryEdges[secondRegion.boundaryIndex];
								var secondBoundaryPosition = medialAxis._boundaryPoints[secondBoundary.farPoint].position;

								Vector2 bisector;
								if (Vector2.Dot(firstBoundary.direction, secondBoundary.direction) > 0f)
								{
									// If angle between is less than 90 degrees, just add and normalize.
									bisector = (firstBoundary.direction + secondBoundary.direction).normalized;
								}
								else
								{
									// If angle between is greater than 90 degrees, add the normals instead, and then normalize.
									bisector = (Geometry.PerpendicularCW(firstBoundary.direction) + Geometry.PerpendicularCCW(secondBoundary.direction)).normalized;
								}
								float sineHalfAngle = Geometry.SinMagnitude(bisector, secondBoundary.direction);

								var deltaNormal = Geometry.PerpendicularCCW(secondBoundaryPosition - firstBoundaryPosition);
								var sDenom = Vector2.Dot(secondBoundary.direction, Geometry.PerpendicularCCW(firstBoundary.direction));
								var tDenom = Vector2.Dot(firstBoundary.direction, Geometry.PerpendicularCW(firstBoundary.direction));
								if (sDenom == 0f || tDenom == 0f) throw new NotImplementedException(); // No intersection, parallel lines.
								var s = Vector2.Dot(secondBoundary.direction, deltaNormal) / sDenom;
								var t = Vector2.Dot(firstBoundary.direction, deltaNormal) / tDenom;
								// Should be essentially identical, but average them in case of any small discrepancies.
								var intersection = (firstBoundary.direction * s + firstBoundaryPosition + secondBoundary.direction * t + secondBoundaryPosition) * 0.5f;
								return new RegionDivision(RegionDivisionType.SimpleLine, 0f, intersection, bisector / sineHalfAngle);
							}
						}
						break;
					}
				}

				throw new NotImplementedException(string.Format("Cannot yet divide regions of type {0} and {1}.", firstRegion.type, secondRegion.type));
			}

			private float IntersectRegions(MedialAxis2D medialAxis, int prevRegionIndex, int prevDivisionIndex, int middleRegionIndex, int nextDivisionIndex, int nextRegionIndex, out float s, out float t)
			{
				var prevDivision = medialAxis._regionDivisions[prevDivisionIndex];
				var nextDivision = medialAxis._regionDivisions[nextDivisionIndex];

				// If the already computed divisions are both linear, we can easily get their intersection.
				if ((prevDivision.type & RegionDivisionType.Line) != 0 && (nextDivision.type & RegionDivisionType.Line) != 0)
				{
					return IntersectLineDivisions(prevDivision, nextDivision, out s, out t);
				}
				else
				{
					var prevRegion = medialAxis._regions[prevRegionIndex];
					var middleRegion = medialAxis._regions[middleRegionIndex];
					var nextRegion = medialAxis._regions[nextRegionIndex];

					// If the outer two regions are both points or both lines, then their division is a line,
					// so we should use that with one of the other two divisions.  Preferably another line, but
					// it is possible that the already computed divisions are both parabolas.
					if (prevRegion.type == RegionType.Point && nextRegion.type == RegionType.Point ||
						prevRegion.type == RegionType.Line && nextRegion.type == RegionType.Line)
					{
						var middleDivision = DivideRegions(medialAxis, prevRegionIndex, nextRegionIndex);
						if ((prevDivision.type & RegionDivisionType.Line) != 0)
						{
							var nearestDistance = IntersectLineDivisions(prevDivision, middleDivision, out s, out t);
							t = nextDivision.Project(middleDivision.GetPosition(t));
							return nearestDistance;
						}
						else if ((nextDivision.type & RegionDivisionType.Line) != 0)
						{
							var nearestDistance = IntersectLineDivisions(middleDivision, nextDivision, out s, out t);
							s = prevDivision.Project(middleDivision.GetPosition(s));
							return nearestDistance;
						}
						else
						{
							var nearestDistance = IntersectLineParabolaDivisions(middleDivision, nextDivision, out s, out t);
							s = prevDivision.Project(middleDivision.GetPosition(s));
							return nearestDistance;
						}
					}
					// The middle region division is not linear, so we'll just intersect the previous and next, one of which is a parabola.
					else
					{
						if ((prevDivision.type & RegionDivisionType.Line) != 0)
						{
							return IntersectLineParabolaDivisions(prevDivision, nextDivision, out s, out t);
						}
						else if ((nextDivision.type & RegionDivisionType.Line) != 0)
						{
							var nearestDistance = IntersectLineParabolaDivisions(nextDivision, prevDivision, out s, out t);
							Core.GeneralUtility.Swap(ref s, ref t);
							return nearestDistance;
						}
						else
						{
							throw new InvalidOperationException("None of the intersecting region divisions are linear, which should not be possible.");
						}
					}
				}
			}

			private float IntersectLineDivisions(RegionDivision prevDivision, RegionDivision nextDivision, out float s, out float t)
			{
				var deltaNormal = Geometry.PerpendicularCCW(nextDivision.position - prevDivision.position);
				var sDenom = Vector2.Dot(nextDivision.vector, Geometry.PerpendicularCCW(prevDivision.vector));
				var tDenom = Vector2.Dot(prevDivision.vector, Geometry.PerpendicularCW(nextDivision.vector));
				if (sDenom == 0f || tDenom == 0f)
				{
					s = t = float.PositiveInfinity;
					return float.PositiveInfinity; // Never intersect, parallel lines.
				}
				s = Vector2.Dot(nextDivision.vector, deltaNormal) / sDenom;
				t = Vector2.Dot(prevDivision.vector, deltaNormal) / tDenom;
				return (prevDivision.GetDistance(s) + nextDivision.GetDistance(t)) * 0.5f; // Should be essentially identical, but average them in case of any small discrepancies.
			}

			private float IntersectLineParabolaDivisions(RegionDivision lineDivision, RegionDivision parabolaDivision, out float s, out float t)
			{
				var a = Geometry.SinMagnitude(parabolaDivision.vector, lineDivision.vector);
				var b = -Vector2.Dot(lineDivision.vector, parabolaDivision.vector);
				var c = Geometry.SinMagnitude(parabolaDivision.position - lineDivision.position, lineDivision.vector);
				float t0, t1;
				var roots = Numerics.Math.SolveQuadratic(a, b, c, out t0, out t1);
				switch (roots)
				{
					case 1:
						// Find the linear parameter, make sure it isn't negative.
						s = lineDivision.Project(parabolaDivision.GetPosition(t0));
						if (s < 0f) goto default;
						t = t0;
						return lineDivision.GetDistance(s);

					case 2:
						// Make sure that t0 does not have a larger absolute magnitide than t1; we want to check the smaller magnitude first.
						if (Mathf.Abs(t0) > Mathf.Abs(t1)) Core.GeneralUtility.Swap(ref t0, ref t1);

						// Find the linear parameter for the parabolic parameter with the smaller magnitude.
						s = lineDivision.Project(parabolaDivision.GetPosition(t0));
						if (s < 0f)
						{
							// If it is a negative value, we can't use it; try the other parameter instead.
							s = lineDivision.Project(parabolaDivision.GetPosition(t1));
							if (s < 0f) goto default;

							t = t1;
						}
						else
						{
							t = t0;
						}

						return lineDivision.GetDistance(s);

					default:
						s = t = float.PositiveInfinity;
						return float.PositiveInfinity;
				}
			}

			private void DoTwoWayIntersection(MedialAxis2D medialAxis, TwoWayRegionIntersection intersection, DelegateOrderedPriorityQueue<TwoWayRegionIntersection> twoWayQueue, DelegateOrderedPriorityQueue<ThreeWayRegionIntersection> threeWayQueue)
			{
				var pointRegion = medialAxis._regions[intersection.pointRegion];
				var otherRegion = medialAxis._regions[intersection.otherRegion];

				var boundaryPoint = medialAxis._boundaryPoints[pointRegion.boundaryIndex];

				if (otherRegion.type == RegionType.Point)
				{
					var otherBoundaryPoint = medialAxis._boundaryPoints[otherRegion.boundaryIndex];
				}
				else if (otherRegion.type == RegionType.Line)
				{
					var boundaryEdge = medialAxis._boundaryEdges[otherRegion.boundaryIndex];

					var t = Vector2.Dot(boundaryPoint.position, boundaryEdge.direction);

					var firstEdgeIndex = boundaryEdge.firstRegionEdge;
					var lastEdgeIndex = medialAxis._regionEdges[firstEdgeIndex].prevEdge;
					var edgeIndex = medialAxis._regionEdges[medialAxis._regionEdges[firstEdgeIndex].nextEdge].nextEdge;
					do
					{
						var edge = medialAxis._regionEdges[edgeIndex];
						if (edge.farRegion == -1)
						{
							var twinEdge = medialAxis._regionEdges[edge.twinEdge];
							if (edge.farT < t && twinEdge.farT > t)
							{
								// Add the two pairs of new external region edges.
								var prevExternalEdgeIndex = medialAxis._regionEdgeCount;
								var prevExternalTwinEdgeIndex = prevExternalEdgeIndex + 1;
								var nextExternalEdgeIndex = prevExternalEdgeIndex + 2;
								var nextExternalTwinEdgeIndex = prevExternalEdgeIndex + 3;
								medialAxis.AddRegionEdge(new RegionEdge(edge.farRegion, prevExternalTwinEdgeIndex, edge.prevEdge, edgeIndex, -1, t));
								medialAxis.AddRegionEdge(new RegionEdge(twinEdge.farRegion, prevExternalEdgeIndex, edge.twinEdge, twinEdge.nextEdge, -1, twinEdge.farT));
								medialAxis.AddRegionEdge(new RegionEdge(edge.farRegion, nextExternalTwinEdgeIndex, edgeIndex, edge.nextEdge, -1, edge.farT));
								medialAxis.AddRegionEdge(new RegionEdge(twinEdge.farRegion, nextExternalEdgeIndex, twinEdge.prevEdge, edge.twinEdge, -1, t));

								// Hook up the middle edge pair between the two new external pairs.
								medialAxis._regionEdges[edgeIndex].prevEdge = prevExternalEdgeIndex;
								medialAxis._regionEdges[edgeIndex].nextEdge = nextExternalEdgeIndex;
								medialAxis._regionEdges[edge.twinEdge].prevEdge = nextExternalTwinEdgeIndex;
								medialAxis._regionEdges[edge.twinEdge].nextEdge = prevExternalTwinEdgeIndex;

								medialAxis._regionEdges[edgeIndex].farRegion = intersection.pointRegion;

								var divisionIndex = medialAxis._regionDivisions.Count;


								if (boundaryPoint.firstRegionEdge == -1)
								{
								}
								else
								{
									throw new NotImplementedException();
								}

								return;
							}
						}
						edgeIndex = edge.nextEdge;
					} while (edgeIndex != lastEdgeIndex);

					throw new InvalidOperationException();
				}
				else
				{
					throw new NotImplementedException();
				}
			}

			private void DoThreeWayIntersection(MedialAxis2D medialAxis, ThreeWayRegionIntersection intersection, DelegateOrderedPriorityQueue<ThreeWayRegionIntersection> threeWayQueue)
			{
				var outwardEdgeIndex = intersection.externalRegionEdge;
				var outwardEdge = medialAxis._regionEdges[outwardEdgeIndex];
				var inwardEdgeIndex = outwardEdge.twinEdge;
				var inwardEdge = medialAxis._regionEdges[inwardEdgeIndex];
				var prevEdge = medialAxis._regionEdges[outwardEdge.prevEdge];
				var nextEdge = medialAxis._regionEdges[outwardEdge.nextEdge];

				// Remove external region edge from chain.
				medialAxis._regionEdges[outwardEdge.prevEdge].nextEdge = outwardEdge.nextEdge;
				medialAxis._regionEdges[outwardEdge.nextEdge].prevEdge = outwardEdge.prevEdge;

				// Remove twin of external region edge from chain.
				medialAxis._regionEdges[inwardEdge.prevEdge].nextEdge = inwardEdge.nextEdge;
				medialAxis._regionEdges[inwardEdge.nextEdge].prevEdge = inwardEdge.prevEdge;

				// Insert external region edge into chain.
				var farOutwardEdge = medialAxis._regionEdges[nextEdge.twinEdge];
				medialAxis._regionEdges[outwardEdgeIndex].prevEdge = nextEdge.twinEdge;
				medialAxis._regionEdges[outwardEdgeIndex].nextEdge = farOutwardEdge.nextEdge;
				medialAxis._regionEdges[farOutwardEdge.nextEdge].prevEdge = outwardEdgeIndex;
				medialAxis._regionEdges[nextEdge.twinEdge].nextEdge = outwardEdgeIndex;

				// Insert twin external region edge into chain.
				var nearInwardEdge = medialAxis._regionEdges[prevEdge.twinEdge];
				medialAxis._regionEdges[inwardEdgeIndex].nextEdge = prevEdge.twinEdge;
				medialAxis._regionEdges[inwardEdgeIndex].nextEdge = nearInwardEdge.prevEdge;
				medialAxis._regionEdges[nearInwardEdge.prevEdge].nextEdge = outwardEdge.twinEdge;
				medialAxis._regionEdges[prevEdge.twinEdge].prevEdge = outwardEdge.twinEdge;

				// Update regions of outward and inward edges.
				medialAxis._regionEdges[outwardEdgeIndex].farRegion = prevEdge.farRegion;
				medialAxis._regionEdges[inwardEdgeIndex].farRegion = nextEdge.farRegion;

				// Add the new region division.
				var newDivisionIndex = medialAxis._regionDivisions.Count;
				medialAxis._regionEdges[outwardEdgeIndex].regionDivision = newDivisionIndex;
				medialAxis._regionEdges[inwardEdgeIndex].regionDivision = newDivisionIndex;
				medialAxis._regionDivisions.Add(DivideRegions(medialAxis, prevEdge.farRegion, nextEdge.farRegion));

				// Calculate and add new intersection.
				var farPrevEdge = medialAxis._regionEdges[medialAxis._regionEdges[medialAxis._regionEdges[inwardEdgeIndex].prevEdge].prevEdge];
				var farNextEdge = medialAxis._regionEdges[medialAxis._regionEdges[medialAxis._regionEdges[outwardEdgeIndex].nextEdge].nextEdge];

				// Find intersections of the two sets of three regions on the previous and next ends of the four relevant regions.
				float prevS, prevT, nextS, nextT;
				var prevNearestDistance = IntersectRegions(medialAxis, farPrevEdge.farRegion, farPrevEdge.regionDivision, prevEdge.farRegion, newDivisionIndex, nextEdge.farRegion, out prevS, out prevT);
				var nextNearestDistance = IntersectRegions(medialAxis, prevEdge.farRegion, newDivisionIndex, nextEdge.farRegion, farNextEdge.regionDivision, farNextEdge.farRegion, out nextS, out nextT);

				// Update region division parameters of three edges converging onto intersection point.
				medialAxis._regionEdges[outwardEdge.prevEdge].farT = intersection.nearestDistance;
				medialAxis._regionEdges[nextEdge.twinEdge].farT = intersection.nearestDistance;
				medialAxis._regionEdges[inwardEdgeIndex].farT = intersection.nearestDistance;

				if (prevNearestDistance <= nextNearestDistance)
				{
					threeWayQueue.Push(new ThreeWayRegionIntersection(prevNearestDistance, prevS, prevT, medialAxis._regionEdges[inwardEdgeIndex].prevEdge));
				}
				else
				{
					threeWayQueue.Push(new ThreeWayRegionIntersection(nextNearestDistance, nextS, nextT, medialAxis._regionEdges[outwardEdgeIndex].nextEdge));
				}

				//TODO:  Fix existing intersections in queue?
			}

			private void CalculateRemainingRegionEdges(MedialAxis2D medialAxis)
			{
				var twoWayQueue = new DelegateOrderedPriorityQueue<TwoWayRegionIntersection>(TwoWayRegionIntersection.AreOrdered);
				var threeWayQueue = new DelegateOrderedPriorityQueue<ThreeWayRegionIntersection>(ThreeWayRegionIntersection.AreOrdered);
				var boundaryLookup = new BoundaryLookup(this, medialAxis);

				for (int i = 0; i < medialAxis._boundaryPoints.Count; ++i)
				{
					float nearestDistance;
					int regionIndex = medialAxis._boundaryPoints[i].region;
					int nearestRegionIndex = boundaryLookup.FindNearestNonAdjacentRegion(regionIndex, out nearestDistance);
					if (nearestRegionIndex != -1)
					{
						twoWayQueue.Push(new TwoWayRegionIntersection(nearestDistance, regionIndex, nearestRegionIndex));
					}
				}

				for (int i = 0; i < medialAxis._boundaryEdges.Count; ++i)
				{
					var boundaryEdge = medialAxis._boundaryEdges[i];
					var regionEdge = medialAxis._regionEdges[boundaryEdge.firstRegionEdge];
					var prevRegionEdge = medialAxis._regionEdges[regionEdge.prevEdge];
					var nextRegionEdge = medialAxis._regionEdges[regionEdge.nextEdge];
					var prevDivision = medialAxis._regionDivisions[prevRegionEdge.regionDivision];
					var nextDivision = medialAxis._regionDivisions[nextRegionEdge.regionDivision];
					var prevRegion = medialAxis._regions[prevRegionEdge.farRegion];
					var nextRegion = medialAxis._regions[nextRegionEdge.farRegion];

					if (prevRegion.type == RegionType.Line || nextRegion.type == RegionType.Line)
					{
						float s, t;
						var nearestDistance = IntersectLineDivisions(prevDivision, nextDivision, out s, out t);
						if (nearestDistance >= 0f && nearestDistance < float.PositiveInfinity)
						{
							threeWayQueue.Push(new ThreeWayRegionIntersection(nearestDistance, s, t, nextRegionEdge.nextEdge));
						}
					}
				}

				while (!twoWayQueue.isEmpty || !threeWayQueue.isEmpty)
				{
					if (!twoWayQueue.isEmpty && !threeWayQueue.isEmpty)
					{
						if (twoWayQueue.Peek().nearestDistance < threeWayQueue.Peek().nearestDistance)
						{
							DoTwoWayIntersection(medialAxis, twoWayQueue.Pop(), twoWayQueue, threeWayQueue);
						}
						else
						{
							DoThreeWayIntersection(medialAxis, threeWayQueue.Pop(), threeWayQueue);
						}
					}
					else
					{
						if (!twoWayQueue.isEmpty)
						{
							DoTwoWayIntersection(medialAxis, twoWayQueue.Pop(), twoWayQueue, threeWayQueue);
						}
						else
						{
							DoThreeWayIntersection(medialAxis, threeWayQueue.Pop(), threeWayQueue);
						}
					}
				}
			}
		}
	}
}
#endif
