/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using Experilous.Numerics;

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

		private struct Division
		{
			public Vector2 a;
			public Vector2 b;
			public Vector2 c;
			public float t0;
			public float t1;

			public Division(Vector2 a, Vector2 b, Vector2 c)
			{
				this.a = a;
				this.b = b;
				this.c = c;
				t0 = 0f;
				t1 = float.PositiveInfinity;
			}
		}

		private struct Region
		{
			public int firstDivisionIndex;
			public int lastDivisionIndex;

			public Region(int firstDivisionIndex, int lastDivisionIndex)
			{
				this.firstDivisionIndex = firstDivisionIndex;
				this.lastDivisionIndex = lastDivisionIndex;
			}
		}

		private List<Division> _divisions;
		private List<Region> _regions;

		public Contour GetContour(float distance = 0f, float maxArcDegrees = 360f, float maxStraightLength = float.PositiveInfinity)
		{
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
