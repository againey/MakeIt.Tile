/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using Experilous.Numerics;

namespace Experilous.Topologies
{
	public static class VoronoiUtility
	{
		public static float GetSegmentRightBound_PointPoint(Vector2 position0, Vector2 position1, float sweep, float errorMargin)
		{
			var d = position0 - position1;

			if (Mathf.Abs(d.y) >= errorMargin)
			{
				var d0 = new Vector2(position0.x, sweep - position0.y);
				var d1 = new Vector2(position1.x, sweep - position1.y);
				var sqr = d.sqrMagnitude * d0.y * d1.y;
				if (sqr >= errorMargin)
				{
					return (Geometry.DotPerpendicularCW(d0, d1) - Mathf.Sqrt(sqr)) / d.y;
				}
				else if (sqr > -errorMargin)
				{
					return Geometry.DotPerpendicularCW(d0, d1) / d.y;
				}
				else
				{
					return float.NaN;
				}
			}
			else
			{
				return position1.x + d.x / 2f;
			}
		}

		public static float GetSegmentRightBound_PointLine(Vector2 position0, Vector2 position1a, Vector2 position1b, float sweep, float errorMargin)
		{
			var d0 = new Vector2(position0.x, sweep - position0.y);
			var v = position1b - position1a;
			var k = v.magnitude + v.x;
			if (Mathf.Abs(k) >= errorMargin)
			{
				var m = v.y * d0.y / k;
				var d1 = new Vector2(position1a.x, sweep - position1a.y);
				var sqr = m * (m - 2f * d0.x) - (d0.y - 2f * (v.x * d1.y - v.y * d1.x) / k) * d0.y;
				if (sqr >= errorMargin)
				{
					return d0.x - m + Mathf.Sqrt(sqr);
				}
				else if (sqr > -errorMargin)
				{
					return d0.x - m;
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

		public static float GetSegmentRightBound_LineLine(Vector2 position0a, Vector2 position0b, Vector2 position1a, Vector2 position1b, float sweep, float errorMargin)
		{
			var v0 = position0b - position0a;
			var v1 = position1b - position1a;
			var k0 = v0.magnitude + v0.x;
			var k1 = v1.magnitude + v1.x;
			var denominator = v1.y * k0 - v0.y * k1;
			if (Mathf.Abs(denominator) >= errorMargin)
			{
				var d0 = new Vector2(position0a.x, sweep - position0a.y);
				var d1 = new Vector2(position1a.x, sweep - position1a.y);
				return ((v1.x * d1.y + v1.y * d1.x) * k0 - (v0.x * d0.y + v0.y * d0.x) * k1) / denominator;
			}
			else
			{
				// v0 and v1 are parallel
				throw new NotImplementedException();
			}
		}
	}
}
