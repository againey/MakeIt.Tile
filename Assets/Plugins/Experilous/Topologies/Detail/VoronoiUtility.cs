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
		#region ComputeDirectionalOrder

		public static float ComputeDirectionalOrder(Vector2 direction)
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

		private static bool CheckForMergeEvent_NoMerge(out Vector2 mergePosition, out float distance)
		{
			mergePosition = new Vector2(float.NaN, float.NaN);
			distance = 0f;
			return false;
		}

		public static bool CheckForMergeEvent_PointPointPoint(Vector2 p0, Vector2 p1, Vector2 p2, out Vector2 mergePosition, out float distance)
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
				distance = mergeOffset.magnitude;
				mergePosition = p1 + mergeOffset;
				return true;
			}
			else
			{
				return CheckForMergeEvent_NoMerge(out mergePosition, out distance);
			}
		}

		private static bool CheckForMergeEvent_PointLinePoint(Vector2 p0, Vector2 p1a, Vector2 p1b, Vector2 p2, float errorMargin, bool lineInMiddle, out Vector2 mergePosition, out float distance)
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
						return CheckForMergeEvent_NoMerge(out mergePosition, out distance);
					}
				}
				else
				{
					return CheckForMergeEvent_NoMerge(out mergePosition, out distance);
				}
			}
			else if (lineInMiddle)
			{
				x = (q0.x + q2.x) / 2f;
				y = ((x - q0.x - q2.x) * x + (q0.sqrMagnitude + q2.sqrMagnitude) / 2f) / (q0.y + q2.y);
			}
			else
			{
				return CheckForMergeEvent_NoMerge(out mergePosition, out distance);
			}

			distance = Mathf.Sqrt(vSqrLen) * Mathf.Abs(y);
			mergePosition = p1a + v1 * x + n1 * y;
			return true;
		}

		private static bool CheckForMergeEvent_PointLineNormal(Vector2 p0, Vector2 p1, Vector2 v1, Vector2 n1, out Vector2 mergePosition, out float distance)
		{
			var vSqrLen = v1.sqrMagnitude;

			var d0 = p0 - p1;
			var q0 = new Vector2(Vector2.Dot(d0, v1), Vector2.Dot(d0, n1)) / vSqrLen;

			float y = (q0.x * q0.x / q0.y + q0.y) / 2f;

			distance = Mathf.Sqrt(vSqrLen) * y;
			mergePosition = p1 + n1 * y;
			return true;
		}

		private static bool CheckForMergeEvent_LinePointLine(Vector2 p0a, Vector2 p0b, Vector2 p1, Vector2 p2a, Vector2 p2b, float errorMargin, bool pointInMiddle, out Vector2 mergePosition, out float distance)
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
						distance = (-b + Mathf.Sqrt(sqr) * Mathf.Sign(Geometry.DotPerpendicularCW(v0, v2)) * (pointInMiddle ? +1f : -1f)) / (2f * a);
						mergePosition.x = p1.x + gx * distance + hx;
						mergePosition.y = p1.y + gy * distance + hy;
						return true;
					}
					else if (sqr > -errorMargin)
					{
						distance = -0.5f * b / a;
						mergePosition.x = p1.x + gx * distance + hx;
						mergePosition.y = p1.y + gy * distance + hy;
						return true;
					}
					else
					{
						return CheckForMergeEvent_NoMerge(out mergePosition, out distance);
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

		private static bool CheckForMergeEvent_NormalLineLine(Vector2 p1, Vector2 v1, Vector2 p2, Vector2 v2, Vector2 n2, float errorMargin, out Vector2 mergePosition, out float distance)
		{
			if (Vector3.Dot(v1, n2) > -errorMargin)
			{
				return CheckForMergeEvent_NoMerge(out mergePosition, out distance);
			}

			Vector2 q2 = p2 - p1;
			float len1 = v1.magnitude;
			float len2 = v2.magnitude;

			float f2 = Vector2.Dot(n2, q2);
			float determinant = -Vector2.Dot(v1, n2);

			float gx = (len1 * v2.x - len2 * v1.x) / determinant;
			float gy = (len1 * v2.y - len2 * v1.y) / determinant;
			float hx = -f2 * v1.x / determinant;
			float hy = -f2 * v1.y / determinant;

			float a = gx * gx + gy * gy - 1f;
			float b = 2f * (gx * hx + gy * hy);

			distance = -0.5f * b / a;
			mergePosition.x = p1.x + gx * distance + hx;
			mergePosition.y = p1.y + gy * distance + hy;
			return true;
		}

		public static bool CheckForMergeEvent_LineLineLine(Vector2 p0a, Vector2 p0b, Vector2 p1a, Vector2 p1b, Vector2 p2a, Vector2 p2b, float errorMargin, out Vector2 mergePosition, out float distance)
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

			distance = (v1.y * hx - v1.x * hy) / (v1.x * gy - v1.y * gx - len1);
			mergePosition.x = p1a.x + gx * distance + hx;
			mergePosition.y = p1a.y + gy * distance + hy;
			return true;
		}





		public static bool CheckForMergeEvent_PointPointPoint(int i0, int i1, int i2, IGraphNodeData<Vector2> nodePositions, float errorMargin, out Vector2 mergePosition, out float distance)
		{
			return CheckForMergeEvent_PointPointPoint(nodePositions[i0], nodePositions[i1], nodePositions[i2], out mergePosition, out distance);
		}

		public static bool CheckForMergeEvent_PointPointLine(int i0, int i1, int i2a, int i2b, IGraphNodeData<Vector2> nodePositions, float errorMargin, out Vector2 mergePosition, out float distance)
		{
			var p2a = nodePositions[i2a];
			var p2b = nodePositions[i2b];
			if (i1 == i2b)
			{
				var v1 = p2a - p2b;
				var n1 = v1.PerpendicularCW();
				return CheckForMergeEvent_PointLineNormal(nodePositions[i0], p2b, v1, n1, out mergePosition, out distance);
			}
			else
			{
				var p0 = nodePositions[i0];
				var p1 = nodePositions[i1];
				if (Geometry.DotPerpendicularCW(p2b - p2a, p0 - p1) > errorMargin)
				{
					return CheckForMergeEvent_PointLinePoint(p0, p2a, p2b, p1, errorMargin, false, out mergePosition, out distance);
				}
				else
				{
					return CheckForMergeEvent_NoMerge(out mergePosition, out distance);
				}
			}
		}

		public static bool CheckForMergeEvent_PointLinePoint(int i0, int i1a, int i1b, int i2, IGraphNodeData<Vector2> nodePositions, float errorMargin, out Vector2 mergePosition, out float distance)
		{
			var p1a = nodePositions[i1a];
			var p1b = nodePositions[i1b];
			if (i0 == i1a)
			{
				if (i2 == i1b)
				{
					return CheckForMergeEvent_NoMerge(out mergePosition, out distance);
				}
				else
				{
					var v1 = p1b - p1a;
					var n1 = v1.PerpendicularCCW();
					return CheckForMergeEvent_PointLineNormal(nodePositions[i2], p1a, v1, n1, out mergePosition, out distance);
				}
			}
			else if (i2 == i1b)
			{
				var v1 = p1a - p1b;
				var n1 = v1.PerpendicularCW();
				return CheckForMergeEvent_PointLineNormal(nodePositions[i0], p1b, v1, n1, out mergePosition, out distance);
			}
			else
			{
				var p0 = nodePositions[i0];
				var p2 = nodePositions[i2];
				return CheckForMergeEvent_PointLinePoint(p0, p1a, p1b, p2, errorMargin, true, out mergePosition, out distance);
			}
		}

		public static bool CheckForMergeEvent_LinePointPoint(int i0a, int i0b, int i1, int i2, IGraphNodeData<Vector2> nodePositions, float errorMargin, out Vector2 mergePosition, out float distance)
		{
			var p0a = nodePositions[i0a];
			var p0b = nodePositions[i0b];
			if (i1 == i0a)
			{
				var v1 = p0b - p0a;
				var n1 = v1.PerpendicularCCW();
				return CheckForMergeEvent_PointLineNormal(nodePositions[i2], p0a, v1, n1, out mergePosition, out distance);
			}
			else
			{
				var p1 = nodePositions[i1];
				var p2 = nodePositions[i2];
				if (Geometry.DotPerpendicularCW(p0b - p0a, p2 - p1) > errorMargin)
				{
					return CheckForMergeEvent_PointLinePoint(p2, p0b, p0a, p1, errorMargin, false, out mergePosition, out distance);
				}
				else
				{
					return CheckForMergeEvent_NoMerge(out mergePosition, out distance);
				}
			}
		}

		public static bool CheckForMergeEvent_PointLineLine(int i0, int i1a, int i1b, int i2a, int i2b, IGraphNodeData<Vector2> nodePositions, float errorMargin, out Vector2 mergePosition, out float distance)
		{
			if (i1a == i2b && i1b == i2a)
			{
				return CheckForMergeEvent_NoMerge(out mergePosition, out distance);
			}

			var p1a = nodePositions[i1a];
			var p1b = nodePositions[i1b];
			var p2a = nodePositions[i2a];
			var p2b = nodePositions[i2b];

			if (i0 == i1a)
			{
				var v1 = p1b - p1a;
				var v2 = p2b - p2a;
				var n2 = v2.PerpendicularCCW();
				return CheckForMergeEvent_NormalLineLine(p1a, v1, p2a, v2, n2, errorMargin, out mergePosition, out distance);
			}
			else
			{
				var p0 = nodePositions[i0];
				return CheckForMergeEvent_LinePointLine(p1a, p1b, p0, p2a, p2b, errorMargin, false, out mergePosition, out distance);
			}
		}

		public static bool CheckForMergeEvent_LinePointLine(int i0a, int i0b, int i1, int i2a, int i2b, IGraphNodeData<Vector2> nodePositions, float errorMargin, out Vector2 mergePosition, out float distance)
		{
			if ((i0a == i2a && i0b == i2b) || (i0a == i2b && i0b == i2a))
			{
				return CheckForMergeEvent_NoMerge(out mergePosition, out distance);
			}
			
			var p0a = nodePositions[i0a];
			var p0b = nodePositions[i0b];
			var p2a = nodePositions[i2a];
			var p2b = nodePositions[i2b];

			if (i0b == i1)
			{
				if (i1 == i2a)
				{
					return CheckForMergeEvent_NoMerge(out mergePosition, out distance);
				}
				else
				{
					var v1 = p2a - p2b;
					var v0 = p0a - p0b;
					var n0 = v0.PerpendicularCW();
					return CheckForMergeEvent_NormalLineLine(p2b, v1, p0b, v0, n0, errorMargin, out mergePosition, out distance);
				}
			}
			else if (i1 == i2a)
			{
				var v1 = p0b - p0a;
				var v2 = p2b - p2a;
				var n2 = v2.PerpendicularCCW();
				return CheckForMergeEvent_NormalLineLine(p0a, v1, p2a, v2, n2, errorMargin, out mergePosition, out distance);
			}
			else
			{
				var p1 = nodePositions[i1];
				return CheckForMergeEvent_LinePointLine(p0a, p0b, p1, p2a, p2b, errorMargin, true, out mergePosition, out distance);
			}
		}

		public static bool CheckForMergeEvent_LineLinePoint(int i0a, int i0b, int i1a, int i1b, int i2, IGraphNodeData<Vector2> nodePositions, float errorMargin, out Vector2 mergePosition, out float distance)
		{
			if (i0a == i1b && i0b == i1a)
			{
				return CheckForMergeEvent_NoMerge(out mergePosition, out distance);
			}

			var p0a = nodePositions[i0a];
			var p0b = nodePositions[i0b];
			var p1a = nodePositions[i1a];
			var p1b = nodePositions[i1b];

			if (i1b == i2)
			{
				var v1 = p1a - p1b;
				var v0 = p0a - p0b;
				var n0 = v0.PerpendicularCW();
				return CheckForMergeEvent_NormalLineLine(p1b, v1, p0b, v0, n0, errorMargin, out mergePosition, out distance);
			}
			else
			{
				var p2 = nodePositions[i2];
				return CheckForMergeEvent_LinePointLine(p0a, p0b, p2, p1a, p1b, errorMargin, false, out mergePosition, out distance);
			}
		}

		public static bool CheckForMergeEvent_LineLineLine(int i0a, int i0b, int i1a, int i1b, int i2a, int i2b, IGraphNodeData<Vector2> nodePositions, float errorMargin, out Vector2 mergePosition, out float distance)
		{
			if ((i0a == i1b && i0b == i1a) || (i1a == i2b && i1b == i2a))
			{
				return CheckForMergeEvent_NoMerge(out mergePosition, out distance);
			}
			
			var p0a = nodePositions[i0a];
			var p0b = nodePositions[i0b];
			var p1a = nodePositions[i1a];
			var p1b = nodePositions[i1b];
			var p2a = nodePositions[i2a];
			var p2b = nodePositions[i2b];

			return CheckForMergeEvent_LineLineLine(p0a, p0b, p1a, p1b, p2a, p2b, errorMargin, out mergePosition, out distance);
		}








		public static bool CheckForMergeEvent_PointPointLine(Vector2 p0, Vector2 p1, Vector2 p2a, Vector2 p2b, float errorMargin, out Vector2 mergePosition, out float distance)
		{
			if (Geometry.DotPerpendicularCW(p2b - p2a, p0 - p1) > errorMargin)
			{
				return CheckForMergeEvent_PointLinePoint(p0, p2a, p2b, p1, errorMargin, false, out mergePosition, out distance);
			}
			else
			{
				return CheckForMergeEvent_NoMerge(out mergePosition, out distance);
			}
		}

		public static bool CheckForMergeEvent_PointTargetPointLine(Vector2 p0, Vector2 p2a, Vector2 p2b, out Vector2 mergePosition, out float distance)
		{
			var v1 = p2a - p2b;
			var n1 = v1.PerpendicularCW();
			return CheckForMergeEvent_PointLineNormal(p0, p2b, v1, n1, out mergePosition, out distance);
		}

		public static bool CheckForMergeEvent_PointLinePoint(Vector2 p0, Vector2 p1a, Vector2 p1b, Vector2 p2, float errorMargin, out Vector2 mergePosition, out float distance)
		{
			return CheckForMergeEvent_PointLinePoint(p0, p1a, p1b, p2, errorMargin, true, out mergePosition, out distance);
		}

		public static bool CheckForMergeEvent_SourcePointLinePoint(Vector2 p1a, Vector2 p1b, Vector2 p2, out Vector2 mergePosition, out float distance)
		{
			var v1 = p1b - p1a;
			var n1 = v1.PerpendicularCCW();
			return CheckForMergeEvent_PointLineNormal(p2, p1a, v1, n1, out mergePosition, out distance);
		}

		public static bool CheckForMergeEvent_PointLineTargetPoint(Vector2 p0, Vector2 p1a, Vector2 p1b, out Vector2 mergePosition, out float distance)
		{
			var v1 = p1a - p1b;
			var n1 = v1.PerpendicularCW();
			return CheckForMergeEvent_PointLineNormal(p0, p1b, v1, n1, out mergePosition, out distance);
		}

		public static bool CheckForMergeEvent_PointLineLine(Vector2 p0, Vector2 p1a, Vector2 p1b, Vector2 p2a, Vector2 p2b, float errorMargin, out Vector2 mergePosition, out float distance)
		{
			return CheckForMergeEvent_LinePointLine(p1a, p1b, p0, p2a, p2b, errorMargin, false, out mergePosition, out distance);
		}

		public static bool CheckForMergeEvent_SourcePointLineLine(Vector2 p1a, Vector2 p1b, Vector2 p2a, Vector2 p2b, out Vector2 mergePosition, out float distance)
		{
			var v1 = p1b - p1a;
			var v2 = p2b - p2a;
			var n2 = v2.PerpendicularCCW();
			return CheckForMergeEvent_NormalLineLine(p1a, v1, p2a, v2, n2, 0.0001f, out mergePosition, out distance);
		}

		public static bool CheckForMergeEvent_LineSourcePointPoint(Vector2 p0a, Vector2 p0b, Vector2 p2, out Vector2 mergePosition, out float distance)
		{
			var v1 = p0b - p0a;
			var n1 = v1.PerpendicularCCW();
			return CheckForMergeEvent_PointLineNormal(p2, p0a, v1, n1, out mergePosition, out distance);
		}

		public static bool CheckForMergeEvent_LinePointPoint(Vector2 p0a, Vector2 p0b, Vector2 p1, Vector2 p2, float errorMargin, out Vector2 mergePosition, out float distance)
		{
			if (Geometry.DotPerpendicularCW(p0b - p0a, p2 - p1) > errorMargin)
			{
				return CheckForMergeEvent_PointLinePoint(p2, p0b, p0a, p1, errorMargin, false, out mergePosition, out distance);
			}
			else
			{
				return CheckForMergeEvent_NoMerge(out mergePosition, out distance);
			}
		}

		public static bool CheckForMergeEvent_LinePointLine(Vector2 p0a, Vector2 p0b, Vector2 p1, Vector2 p2a, Vector2 p2b, float errorMargin, out Vector2 mergePosition, out float distance)
		{
			return CheckForMergeEvent_LinePointLine(p0a, p0b, p1, p2a, p2b, errorMargin, true, out mergePosition, out distance);
		}

		public static bool CheckForMergeEvent_LineTargetPointLine(Vector2 p0a, Vector2 p0b, Vector2 p2a, Vector2 p2b, out Vector2 mergePosition, out float distance)
		{
			var v1 = p2a - p2b;
			var v0 = p0a - p0b;
			var n0 = v0.PerpendicularCW();
			return CheckForMergeEvent_NormalLineLine(p2b, v1, p0b, v0, n0, 0.0001f, out mergePosition, out distance);
		}

		public static bool CheckForMergeEvent_LineSourcePointLine(Vector2 p0a, Vector2 p0b, Vector2 p2a, Vector2 p2b, out Vector2 mergePosition, out float distance)
		{
			var v1 = p0b - p0a;
			var v2 = p2b - p2a;
			var n2 = v2.PerpendicularCCW();
			return CheckForMergeEvent_NormalLineLine(p0a, v1, p2a, v2, n2, 0.0001f, out mergePosition, out distance);
		}

		public static bool CheckForMergeEvent_LineLinePoint(Vector2 p0a, Vector2 p0b, Vector2 p1a, Vector2 p1b, Vector2 p2, float errorMargin, out Vector2 mergePosition, out float distance)
		{
			return CheckForMergeEvent_LinePointLine(p0a, p0b, p2, p1a, p1b, errorMargin, false, out mergePosition, out distance);
		}

		public static bool CheckForMergeEvent_LineLineTargetPoint(Vector2 p0a, Vector2 p0b, Vector2 p1a, Vector2 p1b, out Vector2 mergePosition, out float distance)
		{
			var v1 = p1a - p1b;
			var v0 = p0a - p0b;
			var n0 = v0.PerpendicularCW();
			return CheckForMergeEvent_NormalLineLine(p1b, v1, p0b, v0, n0, 0.0001f, out mergePosition, out distance);
		}

		#endregion
	}
}
