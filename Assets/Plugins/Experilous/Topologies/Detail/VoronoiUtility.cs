/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using Experilous.Numerics;

namespace Experilous.Topologies.Detail
{
	public static class VoronoiUtility
	{
		#region Voronoi Event

		public abstract class VoronoiEvent
		{
			public Vector2 position;

			public bool IsBefore(VoronoiEvent otherEvent, float errorMargin)
			{
				if (float.IsInfinity(position.y) || float.IsInfinity(otherEvent.position.y))
				{
					return position.y < otherEvent.position.y || position.y == otherEvent.position.y && position.x < otherEvent.position.x;
				}
				else
				{
					float yDelta = otherEvent.position.y - position.y;
					return yDelta >= errorMargin || yDelta > -errorMargin && otherEvent.position.x - position.x > yDelta;
				}
			}
		}

		#endregion

		#region Split Event

		public class SplitEvent : VoronoiEvent, IEquatable<SplitEvent>
		{
			public int nodeIndex;

			public SplitEvent(Vector2 position, int nodeIndex)
			{
				this.position = position;
				this.nodeIndex = nodeIndex;
			}

			public bool Equals(SplitEvent other)
			{
				return nodeIndex == other.nodeIndex;
			}
		}

		public class SplitEventQueue
		{
			private class PriorityQueue : Containers.PriorityQueue<SplitEvent>
			{
				private float _errorMargin = 0.0001f;

				public PriorityQueue(float errorMargin)
				{
					_errorMargin = errorMargin;
				}

				protected override bool AreOrdered(SplitEvent lhs, SplitEvent rhs)
				{
					return lhs.IsBefore(rhs, _errorMargin);
				}
			}

			private readonly PriorityQueue _queue;
			private readonly List<SplitEvent> _pool = new List<SplitEvent>();

			public SplitEventQueue(float errorMargin)
			{
				_queue = new PriorityQueue(errorMargin);
				_pool = new List<SplitEvent>();
			}

			public SplitEvent Push(Vector2 splitPosition, int nodeIndex)
			{
				SplitEvent ev;
				if (_pool.Count > 0)
				{
					int lastIndex = _pool.Count - 1;
					ev = _pool[lastIndex];
					_pool.RemoveAt(lastIndex);
					ev.position = splitPosition;
					ev.nodeIndex = nodeIndex;
				}
				else
				{
					ev = new SplitEvent(splitPosition, nodeIndex);
				}
				_queue.Push(ev);
				return ev;
			}

			public SplitEvent Pop()
			{
				var ev = _queue.Pop();
				_pool.Add(ev);
				return ev;
			}

			public SplitEvent Peek()
			{
				return _queue.Peek();
			}

			public void Clear()
			{
				_queue.Clear();
			}

			public bool isEmpty { get { return _queue.isEmpty; } }
		}

		#endregion

		#region Merge Event

		public class MergeEvent : VoronoiEvent, IEquatable<MergeEvent>
		{
			public float distance;
			public BeachSegment segment;

			public MergeEvent(Vector2 position, float distance, BeachSegment segment)
			{
				this.position = position;
				this.distance = distance;
				this.segment = segment;
			}

			public bool Equals(MergeEvent other)
			{
				return ReferenceEquals(segment, other.segment);
			}

			public bool isValid
			{
				get
				{
					return segment != null;
				}
			}

			public Vector2 mergePosition { get { return position - new Vector2(0f, distance); } }
		}

		public class MergeEventQueue
		{
			private class PriorityQueue : Containers.PriorityQueue<MergeEvent>
			{
				private float _errorMargin = 0.0001f;

				public PriorityQueue(float errorMargin)
				{
					_errorMargin = errorMargin;
				}

				protected override bool AreOrdered(MergeEvent lhs, MergeEvent rhs)
				{
					return lhs.IsBefore(rhs, _errorMargin);
				}
			}

			private readonly PriorityQueue _queue;
			private readonly List<MergeEvent> _pool = new List<MergeEvent>();

			public MergeEventQueue(float errorMargin)
			{
				_queue = new PriorityQueue(errorMargin);
				_pool = new List<MergeEvent>();
			}

			public MergeEvent Push(Vector2 mergePosition, float distance, BeachSegment segment)
			{
				MergeEvent ev;
				var eventPosition = new Vector2(mergePosition.x, mergePosition.y + distance);
				if (_pool.Count > 0)
				{
					int lastIndex = _pool.Count - 1;
					ev = _pool[lastIndex];
					_pool.RemoveAt(lastIndex);
					ev.position = eventPosition;
					ev.distance = distance;
				}
				else
				{
					ev = new MergeEvent(eventPosition, distance, segment);
				}
				segment.SetMergeEvent(ev);
				_queue.Push(ev);
				return ev;
			}

			public MergeEvent Pop()
			{
				var ev = _queue.Pop();
				_pool.Add(ev);
				return ev;
			}

			public MergeEvent Peek()
			{
				return _queue.Peek();
			}

			public void Clear()
			{
				_queue.Clear();
			}

			public bool isEmpty { get { return _queue.isEmpty; } }
		}

		#endregion

		#region Beach Segment

		public class BeachSegment
		{
			public BeachSegment prevSegment;
			public BeachSegment nextSegment;
			public int prevEdgeIndex;
			public int nextEdgeIndex;
			public VoronoiSiteType siteType;
			public int siteIndex;
			public MergeEvent mergeEvent;

			public BeachSegment()
			{
				siteType = VoronoiSiteType.None;
				siteIndex = -1;
			}

			public BeachSegment(BeachSegment prevSegment, BeachSegment nextSegment, int prevEdgeIndex, int nextEdgeIndex)
			{
				this.prevSegment = prevSegment;
				this.nextSegment = nextSegment;
				this.prevEdgeIndex = prevEdgeIndex;
				this.nextEdgeIndex = nextEdgeIndex;
				siteType = VoronoiSiteType.None;
				siteIndex = -1;
			}

			public static bool SitesAreEqual(BeachSegment lhs, BeachSegment rhs)
			{
				return lhs.siteType == rhs.siteType && lhs.siteIndex == rhs.siteIndex;
			}

			public bool SiteEquals(GraphNode pointSite)
			{
				return siteType == VoronoiSiteType.Point && siteIndex == pointSite.index;
			}

			public bool SiteEquals(GraphEdge edgeSite)
			{
				return siteType == VoronoiSiteType.Line && siteIndex == edgeSite.index;
			}

			public void SetMergeEvent(MergeEvent mergeEvent)
			{
				if (this.mergeEvent != null)
				{
					this.mergeEvent.segment = null;
				}

				this.mergeEvent = mergeEvent;

				if (this.mergeEvent != null)
				{
					this.mergeEvent.segment = this;
				}
			}
		}

		public class Beach
		{
			private BeachSegment _head;
			private readonly List<BeachSegment> _pool = new List<BeachSegment>();

			public Beach()
			{
				Reset();
			}

			public void Reset()
			{
				while (_head != null)
				{
					ReturnSegment(_head);
					_head = _head.nextSegment;
				}

				_head = GetSegment(null, null, -1, -1);
			}

			public BeachSegment head { get { return _head; } }

			public BeachSegment Split(BeachSegment segment)
			{
				var nextSegment = GetSegment(segment, segment.nextSegment, -1, segment.nextEdgeIndex);

				nextSegment.siteType = segment.siteType;
				nextSegment.siteIndex = segment.siteIndex;

				segment.nextSegment = nextSegment;
				segment.nextEdgeIndex = -1;

				if (nextSegment.nextSegment != null)
				{
					nextSegment.nextSegment.prevSegment = nextSegment;
				}

				return nextSegment;
			}

			public BeachSegment InsertAfter(VoronoiSiteType siteType, int siteIndex, BeachSegment segment)
			{
				var nextSegment = GetSegment(segment, segment.nextSegment, -1, -1);

				nextSegment.siteType = siteType;
				nextSegment.siteIndex = siteIndex;

				segment.nextSegment = nextSegment;

				if (nextSegment.nextSegment != null)
				{
					nextSegment.nextSegment.prevSegment = nextSegment;
				}

				return nextSegment;
			}

			public BeachSegment InsertAfter(GraphNode pointSite, BeachSegment segment)
			{
				return InsertAfter(VoronoiSiteType.Point, pointSite.index, segment);
			}

			public BeachSegment InsertAfter(GraphEdge edgeSite, BeachSegment segment)
			{
				return InsertAfter(VoronoiSiteType.Line, edgeSite.index, segment);
			}

			public void Remove(BeachSegment segment)
			{
				segment.nextSegment.prevSegment = segment.prevSegment;
				segment.prevSegment.nextSegment = segment.nextSegment;

				_pool.Add(segment);
			}

			private BeachSegment GetSegment(BeachSegment prevSegment, BeachSegment nextSegment, int prevEdgeIndex, int nextEdgeIndex)
			{
				BeachSegment segment;
				if (_pool.Count > 0)
				{
					int lastIndex = _pool.Count - 1;
					segment = _pool[lastIndex];
					_pool.RemoveAt(lastIndex);
					segment.prevSegment = prevSegment;
					segment.nextSegment = nextSegment;
					segment.prevEdgeIndex = prevEdgeIndex;
					segment.nextEdgeIndex = nextEdgeIndex;
					segment.siteType = VoronoiSiteType.None;
					segment.siteIndex = -1;
					segment.mergeEvent = null;
				}
				else
				{
					segment = new BeachSegment(prevSegment, nextSegment, prevEdgeIndex, nextEdgeIndex);
				}
				return segment;
			}

			private void ReturnSegment(BeachSegment segment)
			{
				segment.SetMergeEvent(null);
				_pool.Add(segment);
			}
		}

		#endregion

		#region Directed Edge

		public class DirectedEdge
		{
			public Vector2 direction;
			public float order;
			public int edgeIndex;

			public DirectedEdge(Vector2 direction, int edgeIndex)
			{
				this.direction = direction;
				order = ComputeOrder(direction);
				this.edgeIndex = edgeIndex;
			}

			public static float ComputeOrder(Vector2 direction)
			{
				var sum = direction.x + direction.y;
				var sub = direction.x - direction.y;
				var mul = sum * sub;
				if (direction.y <= 0f)
				{
					if (direction.x < 0f)
					{
						return -3f - mul; // first quadrant (bottom-left), [+1, -1) -> [-4, -2)
					}
					else
					{
						return -1f + mul; // second quadrant (bottom-right), [-1, +1) -> [-2, -0)
					}
				}
				else
				{
					if (direction.x > 0f)
					{
						return +1f - mul; // third quadrant (top-right), [+1, -1) -> [+0, +2)
					}
					else
					{
						return +3f + mul; // fourth quadrant (top-left), [-1, +1) -> [+2, +4)
					}
				}
			}
		}

		#endregion

		#region GetSegmentRightBound

		public static float GetSegmentRightBound_PointPoint(Vector2 p0, Vector2 p1, float sweep, float errorMargin)
		{
			var d = p0 - p1;

			if (Mathf.Abs(d.y) >= errorMargin)
			{
				var d0 = new Vector2(p0.x, p0.y - sweep);
				var d1 = new Vector2(p1.x, p1.y - sweep);
				var sqr = d.sqrMagnitude * d0.y * d1.y;
				if (sqr >= errorMargin)
				{
					return (Geometry.DotPerpendicularCCW(d0, d1) + Mathf.Sqrt(sqr)) / d.y;
				}
				else if (sqr > -errorMargin)
				{
					return Geometry.DotPerpendicularCCW(d0, d1) / d.y;
				}
				else
				{
					return float.NaN;
				}
			}
			else
			{
				return p1.x + d.x / 2f;
			}
		}

		public static float GetSegmentRightBound_PointLine(Vector2 p0, Vector2 p1a, Vector2 p1b, float sweep, float errorMargin)
		{
			var v = p1b - p1a;
			var k = v.magnitude + v.x;
			if (Mathf.Abs(k) >= errorMargin)
			{
				var d0 = new Vector2(p0.x, p0.y - sweep);
				var d1 = new Vector2(p1a.x, p1a.y - sweep);
				var m = v.y * d0.y / k;
				var sqr = m * (m + 2f * d0.x) - (d0.y + 2f * (v.x * -d1.y + v.y * d1.x) / k) * d0.y;
				if (sqr >= errorMargin)
				{
					return d0.x + m + Mathf.Sqrt(sqr);
				}
				else if (sqr > -errorMargin)
				{
					return d0.x + m;
				}
				else
				{
					return float.NaN;
				}
			}
			else
			{
				// v is parallel to the sweep line and points leftward
				throw new NotImplementedException();
			}
		}

		public static float GetSegmentRightBound_LinePoint(Vector2 p0a, Vector2 p0b, Vector2 p1, float sweep, float errorMargin)
		{
			var v = p0b - p0a;
			var k = v.magnitude + v.x;
			if (Mathf.Abs(k) >= errorMargin)
			{
				var d0 = new Vector2(p0a.x, p0a.y - sweep);
				var d1 = new Vector2(p1.x, p1.y - sweep);
				var m = v.y * d1.y / k;
				var sqr = m * (m + 2f * d1.x) - (d1.y + 2f * (v.x * -d0.y + v.y * d0.x) / k) * d1.y;
				if (sqr >= errorMargin)
				{
					return d1.x + m - Mathf.Sqrt(sqr);
				}
				else if (sqr > -errorMargin)
				{
					return d1.x + m;
				}
				else
				{
					return float.NaN;
				}
			}
			else
			{
				// v is parallel to the sweep line and points leftward
				throw new NotImplementedException();
			}
		}

		public static float GetSegmentRightBound_LineLine(Vector2 p0a, Vector2 p0b, Vector2 p1a, Vector2 p1b, float sweep, float errorMargin)
		{
			var v0 = p0b - p0a;
			var v1 = p1b - p1a;
			var k0 = v0.magnitude + v0.x;
			var k1 = v1.magnitude + v1.x;
			var denominator = v1.y * k0 - v0.y * k1;
			if (Mathf.Abs(denominator) >= errorMargin)
			{
				var d0 = new Vector2(p0a.x, sweep - p0a.y);
				var d1 = new Vector2(p1a.x, sweep - p1a.y);
				return ((v1.x * d1.y + v1.y * d1.x) * k0 - (v0.x * d0.y + v0.y * d0.x) * k1) / denominator;
			}
			else
			{
				// v0 and v1 are parallel
				throw new NotImplementedException();
			}
		}

		#endregion

		#region CheckForMergeEvent

		private static void CheckForMergeEvent_PointPointPoint(Vector2 p0, Vector2 p1, Vector2 p2, BeachSegment segment, MergeEventQueue queue)
		{
			var v0 = p1 - p0;
			var v2 = p1 - p2;

			float determinant = Geometry.DotPerpendicularCCW(v0, v2);
			if (determinant > 0f)
			{
				float lenSqr0 = v0.sqrMagnitude;
				float lenSqr2 = v2.sqrMagnitude;
				float cx = 0.5f * (v2.y * lenSqr0 - v0.y * lenSqr2) / determinant;
				float cy = 0.5f * (v0.x * lenSqr2 - v2.x * lenSqr0) / determinant;

				var mergeOffset = new Vector2(cx, cy);
				queue.Push(p1 + mergeOffset, mergeOffset.magnitude, segment);
			}
		}

		private static void CheckForMergeEvent_PointLinePoint(Vector2 p0, Vector2 p1a, Vector2 p1b, Vector2 p2, float errorMargin, bool lineInMiddle, BeachSegment segment, MergeEventQueue queue)
		{
			var v1 = p1b - p1a;
			var n1 = v1.PerpendicularCCW();
			var vSqrLen = v1.sqrMagnitude;

			var d0 = p0 - p1a;
			var d2 = p2 - p1a;

			var q0 = new Vector2(Vector2.Dot(d0, v1), Vector2.Dot(d0, n1)) / vSqrLen;
			var q2 = new Vector2(Vector2.Dot(d2, v1), Vector2.Dot(d2, n1)) / vSqrLen;

			float qDeltaY = q2.y - q0.y;

			float x, y;

			if (Mathf.Abs(qDeltaY) >= errorMargin)
			{
				float qProductY = q0.y * q2.y;
				if (qProductY >= errorMargin)
				{
					var dq = q2 - q0;
					x = (Geometry.DotPerpendicularCW(q0, q2) - Mathf.Sqrt(dq.sqrMagnitude * qProductY) * Mathf.Sign(qDeltaY) * (lineInMiddle ? +1f : -1f)) / qDeltaY;
					y = (((q0.y + q2.y) * x - 2f * (q0.x * q2.y + q0.y * q2.x)) * x + q0.sqrMagnitude * q2.y + q2.sqrMagnitude * q0.y) / (4f * qProductY);
				}
				else if (qProductY > -errorMargin)
				{
					x = Geometry.DotPerpendicularCW(q0, q2) / qDeltaY;

					float absY0 = Mathf.Abs(q0.y);
					float absY2 = Mathf.Abs(q2.y);
					if (absY0 > errorMargin && absY0 > absY2)
					{
						float dx = x - q0.x;
						y = (dx * dx / q0.y + q0.y) / 2f;
					}
					else if (absY2 > errorMargin && absY2 > absY0)
					{
						float dx = x - q2.x;
						y = (dx * dx / q2.y + q2.y) / 2f;
					}
					else
					{
						return;
					}
				}
				else
				{
					return;
				}
			}
			else if (lineInMiddle)
			{
				x = (q0.x + q2.x) / 2f;
				y = ((x - q0.x - q2.x) * x + (q0.sqrMagnitude + q2.sqrMagnitude) / 2f) / (q0.y + q2.y);
			}
			else
			{
				return;
			}

			queue.Push(p1a + v1 * x + n1 * y, Mathf.Sqrt(vSqrLen) * Mathf.Abs(y), segment);
		}

		private static void CheckForMergeEvent_PointLineNormal(Vector2 p0, Vector2 p1, Vector2 v1, Vector2 n1, BeachSegment segment, MergeEventQueue queue)
		{
			var vSqrLen = v1.sqrMagnitude;

			var d0 = p0 - p1;
			var q0 = new Vector2(Vector2.Dot(d0, v1), Vector2.Dot(d0, n1)) / vSqrLen;

			float y = (q0.x * q0.x / q0.y + q0.y) / 2f;

			queue.Push(p1 + n1 * y, Mathf.Sqrt(vSqrLen) * y, segment);
		}

		private static void CheckForMergeEvent_LinePointLine(Vector2 p0a, Vector2 p0b, Vector2 p1, Vector2 p2a, Vector2 p2b, float errorMargin, bool pointInMiddle, BeachSegment segment, MergeEventQueue queue)
		{
			Vector2 q0a = p0a - p1;
			Vector2 q2a = p2a - p1;
			Vector2 v0 = p0b - p0a;
			Vector2 v2 = p2b - p2a;
			float len0 = v0.magnitude;
			float len2 = v2.magnitude;

			float f0 = Geometry.DotPerpendicularCW(v0, q0a);
			float f2 = Geometry.DotPerpendicularCW(v2, q2a);
			float determinant = Geometry.DotPerpendicularCW(v0, v2);

			if (Mathf.Abs(determinant) >= errorMargin)
			{
				float gx = (len0 * v2.x - len2 * v0.x) / determinant;
				float gy = (len0 * v2.y - len2 * v0.y) / determinant;
				float hx = (f0 * v2.x - f2 * v0.x) / determinant;
				float hy = (f0 * v2.y - f2 * v0.y) / determinant;

				float a = gx * gx + gy * gy - 1f;

				if (Mathf.Abs(a) >= errorMargin)
				{
					float b = 2f * (gx * hx + gy * hy);
					float c = hx * hx + hy * hy;

					float sqr = b * b - 4f * a * c;
					if (sqr >= errorMargin)
					{
						var distance = (-b + Mathf.Sqrt(sqr) * Mathf.Sign(Geometry.DotPerpendicularCW(v0, v2)) * (pointInMiddle ? +1f : -1f)) / (2f * a);
						var mergePosition = new Vector2(p1.x + gx * distance + hx, p1.y + gy * distance + hy);
						queue.Push(mergePosition, distance, segment);
					}
					else if (sqr > -errorMargin)
					{
						var distance = -0.5f * b / a;
						var mergePosition = new Vector2(p1.x + gx * distance + hx, p1.y + gy * distance + hy);
						queue.Push(mergePosition, distance, segment);
					}
				}
				else
				{
					throw new NotImplementedException();
				}
			}
			else
			{
				float lineSeparation = Geometry.DotPerpendicularCW(v0, p2a - p0a) / len0;
				float pointSeparation = Geometry.DotPerpendicularCW(v0, p1 - p0a) / len0;
				throw new NotImplementedException();
			}
		}

		private static void CheckForMergeEvent_NormalLineLine(Vector2 n0, Vector2 p1, Vector2 v1, Vector2 p2, Vector2 v2, float errorMargin, BeachSegment segment, MergeEventQueue queue)
		{
			var q = p2 - p1;
			float f = Geometry.DotPerpendicularCCW(q, v2);
			if (f < errorMargin) // Point with normal must be on the CCW normal side of the second line.
			{
				float len1 = v1.magnitude;
				float len2 = v2.magnitude;

				float determinant = Vector2.Dot(v2, n0);

				float gx = (len1 * v2.x - len2 * v1.x) / determinant;
				float gy = (len1 * v2.y - len2 * v1.y) / determinant;
				float hx = -f * v1.x / determinant;
				float hy = -f * v1.y / determinant;

				float a = gx * gx + gy * gy - 1f;
				float b = 2f * (gx * hx + gy * hy);

				var distance = -0.5f * b / a;
				var mergePosition = new Vector2(p1.x + gx * distance + hx, p1.y + gy * distance + hy);
				queue.Push(mergePosition, distance, segment);
			}
		}

		private static void CheckForMergeEvent_LineLineLine(Vector2 p0a, Vector2 p0b, Vector2 p1a, Vector2 p1b, Vector2 p2a, Vector2 p2b, float errorMargin, BeachSegment segment, MergeEventQueue queue)
		{
			Vector2 q0a = p0a - p1a;
			Vector2 q2a = p2a - p1a;
			Vector2 v0 = p0b - p0a;
			Vector2 v1 = p1b - p1a;
			Vector2 v2 = p2b - p2a;
			float len0 = v0.magnitude;
			float len1 = v1.magnitude;
			float len2 = v2.magnitude;

			float f0 = Geometry.DotPerpendicularCW(v0, q0a);
			float f2 = Geometry.DotPerpendicularCW(v2, q2a);
			float determinant = Geometry.DotPerpendicularCW(v0, v2);

			float gx = (len0 * v2.x - len2 * v0.x) / determinant;
			float gy = (len0 * v2.y - len2 * v0.y) / determinant;
			float hx = (f0 * v2.x - f2 * v0.x) / determinant;
			float hy = (f0 * v2.y - f2 * v0.y) / determinant;

			var distance = (v1.y * hx - v1.x * hy) / (v1.x * gy - v1.y * gx - len1);
			var mergePosition = new Vector2(p1a.x + gx * distance + hx, p1a.y + gy * distance + hy);
			queue.Push(mergePosition, distance, segment);
		}

		public static void CheckForMergeEvent_PointPointPoint(int i0, int i1, int i2, IGraphNodeData<Vector2> nodePositions, float errorMargin, BeachSegment segment, MergeEventQueue queue)
		{
			CheckForMergeEvent_PointPointPoint(nodePositions[i0], nodePositions[i1], nodePositions[i2], segment, queue);
		}

		public static void CheckForMergeEvent_PointPointLine(int i0, int i1, int i2a, int i2b, IGraphNodeData<Vector2> nodePositions, float errorMargin, BeachSegment segment, MergeEventQueue queue)
		{
			var p2a = nodePositions[i2a];
			var p2b = nodePositions[i2b];
			if (i1 == i2b)
			{
				var v1 = p2a - p2b;
				var n1 = v1.PerpendicularCW();
				CheckForMergeEvent_PointLineNormal(nodePositions[i0], p2b, v1, n1, segment, queue);
			}
			else
			{
				var p0 = nodePositions[i0];
				var p1 = nodePositions[i1];
				if (Geometry.DotPerpendicularCW(p2b - p2a, p0 - p1) > errorMargin)
				{
					CheckForMergeEvent_PointLinePoint(p0, p2a, p2b, p1, errorMargin, false, segment, queue);
				}
			}
		}

		public static void CheckForMergeEvent_PointLinePoint(int i0, int i1a, int i1b, int i2, IGraphNodeData<Vector2> nodePositions, float errorMargin, BeachSegment segment, MergeEventQueue queue)
		{
			var p1a = nodePositions[i1a];
			var p1b = nodePositions[i1b];
			if (i0 == i1a)
			{
				if (i2 != i1b)
				{
					var v1 = p1b - p1a;
					var n1 = v1.PerpendicularCCW();
					CheckForMergeEvent_PointLineNormal(nodePositions[i2], p1a, v1, n1, segment, queue);
				}
			}
			else if (i2 == i1b)
			{
				var v1 = p1a - p1b;
				var n1 = v1.PerpendicularCW();
				CheckForMergeEvent_PointLineNormal(nodePositions[i0], p1b, v1, n1, segment, queue);
			}
			else
			{
				var p0 = nodePositions[i0];
				var p2 = nodePositions[i2];
				CheckForMergeEvent_PointLinePoint(p0, p1a, p1b, p2, errorMargin, true, segment, queue);
			}
		}

		public static void CheckForMergeEvent_LinePointPoint(int i0a, int i0b, int i1, int i2, IGraphNodeData<Vector2> nodePositions, float errorMargin, BeachSegment segment, MergeEventQueue queue)
		{
			var p0a = nodePositions[i0a];
			var p0b = nodePositions[i0b];
			if (i1 == i0a)
			{
				var v1 = p0b - p0a;
				var n1 = v1.PerpendicularCCW();
				CheckForMergeEvent_PointLineNormal(nodePositions[i2], p0a, v1, n1, segment, queue);
			}
			else
			{
				var p1 = nodePositions[i1];
				var p2 = nodePositions[i2];
				if (Geometry.DotPerpendicularCW(p0b - p0a, p2 - p1) > errorMargin)
				{
					CheckForMergeEvent_PointLinePoint(p2, p0b, p0a, p1, errorMargin, false, segment, queue);
				}
			}
		}

		public static void CheckForMergeEvent_PointLineLine(int i0, int i1a, int i1b, int i2a, int i2b, IGraphNodeData<Vector2> nodePositions, float errorMargin, BeachSegment segment, MergeEventQueue queue)
		{
			if (i1a != i2b || i1b != i2a)
			{
				var p1a = nodePositions[i1a];
				var p1b = nodePositions[i1b];
				var p2a = nodePositions[i2a];
				var p2b = nodePositions[i2b];

				if (i0 == i1a)
				{
					// Source Point of next (p1a), Line, Line
					var v1 = p1b - p1a;
					var v2 = p2b - p2a;
					var n0 = v1.PerpendicularCCW();
					if (Vector2.Dot(n0, v2) > -errorMargin) // Second line must point in roughly same direction as the normal (lines must be concave).
					{
						CheckForMergeEvent_NormalLineLine(n0, p1a, v1, p2a, v2, errorMargin, segment, queue);
					}
				}
				else
				{
					var p0 = nodePositions[i0];
					CheckForMergeEvent_LinePointLine(p1a, p1b, p0, p2a, p2b, errorMargin, false, segment, queue);
				}
			}
		}

		public static void CheckForMergeEvent_LinePointLine(int i0a, int i0b, int i1, int i2a, int i2b, IGraphNodeData<Vector2> nodePositions, float errorMargin, BeachSegment segment, MergeEventQueue queue)
		{
			if ((i0a != i2a || i0b != i2b) && (i0a != i2b || i0b != i2a))
			{
				var p0a = nodePositions[i0a];
				var p0b = nodePositions[i0b];
				var p2a = nodePositions[i2a];
				var p2b = nodePositions[i2b];

				if (i0b == i1)
				{
					if (i1 != i2a)
					{
						// Line, Target Point of prev (p0b), Line
						var v0 = p0b - p0a;
						var v2 = p2b - p2a;
						var n1 = v0.PerpendicularCCW();
						if (Vector2.Dot(n1, v2) > -errorMargin) // Second line must point in roughly same direction as the normal (lines must be concave).
						{
							CheckForMergeEvent_NormalLineLine(n1, p0b, v0, p2a, v2, errorMargin, segment, queue);
						}
					}
				}
				else if (i1 == i2a)
				{
					// Line, Source Point of next (p2a), Line
					var v0 = p0b - p0a;
					var v2 = p2b - p2a;
					var n1 = v2.PerpendicularCCW();
					if (Vector2.Dot(v0, n1) < errorMargin) // First line must point in roughly opposite direction as the normal (lines must be concave).
					{
						CheckForMergeEvent_NormalLineLine(n1, p2a, v2, p0b, v0, errorMargin, segment, queue);
					}
				}
				else
				{
					var p1 = nodePositions[i1];
					CheckForMergeEvent_LinePointLine(p0a, p0b, p1, p2a, p2b, errorMargin, true, segment, queue);
				}
			}
		}

		public static void CheckForMergeEvent_LineLinePoint(int i0a, int i0b, int i1a, int i1b, int i2, IGraphNodeData<Vector2> nodePositions, float errorMargin, BeachSegment segment, MergeEventQueue queue)
		{
			if (i0a != i1b || i0b != i1a)
			{
				var p0a = nodePositions[i0a];
				var p0b = nodePositions[i0b];
				var p1a = nodePositions[i1a];
				var p1b = nodePositions[i1b];

				if (i1b == i2)
				{
					// Line, Line, Target Point of prev (p1b)
					var v0 = p0b - p0a;
					var v1 = p1b - p1a;
					var n2 = v1.PerpendicularCCW();
					if (Vector2.Dot(v0, n2) < errorMargin) // First line must point in roughly opposite direction as the normal (lines must be concave).
					{
						CheckForMergeEvent_NormalLineLine(n2, p1b, v1, p0b, v0, errorMargin, segment, queue);
					}
				}
				else
				{
					var p2 = nodePositions[i2];
					CheckForMergeEvent_LinePointLine(p0a, p0b, p2, p1a, p1b, errorMargin, false, segment, queue);
				}
			}
		}

		public static void CheckForMergeEvent_LineLineLine(int i0a, int i0b, int i1a, int i1b, int i2a, int i2b, IGraphNodeData<Vector2> nodePositions, float errorMargin, BeachSegment segment, MergeEventQueue queue)
		{
			if ((i0a != i1b || i0b != i1a) && (i1a != i2b || i1b != i2a))
			{
				var p0a = nodePositions[i0a];
				var p0b = nodePositions[i0b];
				var p1a = nodePositions[i1a];
				var p1b = nodePositions[i1b];
				var p2a = nodePositions[i2a];
				var p2b = nodePositions[i2b];

				CheckForMergeEvent_LineLineLine(p0a, p0b, p1a, p1b, p2a, p2b, errorMargin, segment, queue);
			}
		}

		#endregion
	}
}
