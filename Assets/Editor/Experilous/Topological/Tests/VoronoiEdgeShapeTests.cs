/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

#if UNITY_5_3_OR_NEWER
using NUnit.Framework;
using NSubstitute;
using UnityEngine;
using Experilous.Topologies.Detail;

namespace Experilous.Topologies.Tests
{
	class VoronoiEdgeShapeTests
	{
		#region Assert

		private static void AssertApproximatelyEqual(float expected, float actual, float margin, string message = null)
		{
			Assert.IsFalse(float.IsNaN(actual), message != null ? message : string.Format("Expected {0:G8} but actual was {1:G8}.", expected, actual));
			Assert.LessOrEqual(Mathf.Abs(expected - actual), margin, message != null ? message : string.Format("Expected {0:G8} but actual was {1:G8}.", expected, actual));
		}

		private static void AssertApproximatelyEqual(Vector2 expected, Vector2 actual, float margin, string message = null)
		{
			Assert.IsFalse(float.IsNaN(actual.x) || float.IsNaN(actual.y), message != null ? message : string.Format("Expected {0} but actual was {1}.", expected.ToString("F4"), actual.ToString("F4")));
			Assert.LessOrEqual(Vector2.Distance(expected, actual), margin, message != null ? message : string.Format("Expected {0} but actual was {1}.", expected.ToString("F4"), actual.ToString("F4")));
		}

		private static void AssertApproximatelyEqual(Vector3 expected, Vector3 actual, float margin, string message = null)
		{
			Assert.IsFalse(float.IsNaN(actual.x) || float.IsNaN(actual.y) || float.IsNaN(actual.z), message != null ? message : string.Format("Expected {0} but actual was {1}.", expected.ToString("F4"), actual.ToString("F4")));
			Assert.LessOrEqual(Vector3.Distance(expected, actual), margin, message != null ? message : string.Format("Expected {0} but actual was {1}.", expected.ToString("F4"), actual.ToString("F4")));
		}

		private static void AssertApproximatelyLessThanOrEqual(float lhs, float rhs, float margin, string message = null)
		{
			Assert.LessOrEqual(lhs - rhs, margin, message != null ? message : string.Format("Expected {0:G8} to be less than or equal to {1:G8} but was actually greater.", lhs, rhs));
		}

		private static void AssertIsNaN(float actual, string message = null)
		{
			Assert.IsTrue(float.IsNaN(actual), message != null ? message : string.Format("Expected NaN but actual was {0:G8}.", actual));
		}

		#endregion

		[Test]
		public void FromPointPoint()
		{
			var edgeShape = VoronoiEdgeShape.FromPointPoint(
				new Vector3(0f, 0f, 0f),
				new Vector3(8f, 6f, 0f),
				new Vector3(1f, 7f, 0f),
				new Vector3(10f, -5f, 0f),
				Vector3.back);

			Assert.AreEqual(VoronoiEdgeShapeType.PythagoreanLine, edgeShape.type);
			AssertApproximatelyEqual(new Vector3(0f, 0f, 0f), edgeShape.u, 0.0001f);
			AssertApproximatelyEqual(new Vector3(0.6f, -0.8f, 0f), edgeShape.v, 0.0001f);
			AssertApproximatelyEqual(new Vector3(4f, 3f, 0f), edgeShape.p, 0.0001f);
			AssertApproximatelyEqual(25f, edgeShape.s, 0.0001f);
			AssertApproximatelyEqual(-5f, edgeShape.t0, 0.0001f);
			AssertApproximatelyEqual(+10f, edgeShape.t1, 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetNearest(), 0.0001f);
			AssertApproximatelyEqual(+10f, edgeShape.GetFarthest(), 0.0001f);
			AssertApproximatelyEqual(5f, edgeShape.GetNearestDistance(), 0.0001f);
			AssertApproximatelyEqual(Mathf.Sqrt(125f), edgeShape.GetFarthestDistance(), 0.0001f);
			AssertApproximatelyEqual(new Vector3(1f, 7f, 0f), edgeShape.Evaluate(-5f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(4f, 3f, 0f), edgeShape.Evaluate(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(7f, -1f, 0f), edgeShape.Evaluate(+5f), 0.0001f);
			AssertApproximatelyEqual(Mathf.Sqrt(125f), edgeShape.GetDistance(-10f), 0.0001f);
			AssertApproximatelyEqual(Mathf.Sqrt(50f), edgeShape.GetDistance(-5f), 0.0001f);
			AssertApproximatelyEqual(5f, edgeShape.GetDistance(0f), 0.0001f);
			AssertApproximatelyEqual(Mathf.Sqrt(50f), edgeShape.GetDistance(+5f), 0.0001f);
			AssertApproximatelyEqual(Mathf.Sqrt(125f), edgeShape.GetDistance(+10f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(0.6f, -0.8f, 0f), edgeShape.GetDirection(-3f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(0.6f, -0.8f, 0f), edgeShape.GetDirection(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(0.6f, -0.8f, 0f), edgeShape.GetDirection(+3f), 0.0001f);

			float t0, t1;
			Assert.IsFalse(edgeShape.Intersect(0f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
			Assert.IsFalse(edgeShape.Intersect(4f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
			Assert.IsTrue(edgeShape.Intersect(5f, out t0, out t1));
			AssertApproximatelyEqual(0f, t0, 0.0001f);
			AssertApproximatelyEqual(0f, t1, 0.0001f);
			Assert.IsTrue(edgeShape.Intersect(6f, out t0, out t1));
			AssertApproximatelyEqual(-Mathf.Sqrt(11f), t0, 0.0001f);
			AssertApproximatelyEqual(+Mathf.Sqrt(11f), t1, 0.0001f);
			Assert.IsTrue(edgeShape.Intersect(10f, out t0, out t1));
			AssertIsNaN(t0);
			AssertApproximatelyEqual(+Mathf.Sqrt(75f), t1, 0.0001f);
			Assert.IsFalse(edgeShape.Intersect(13f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
		}

		[Test]
		public void FromPointLine()
		{
			var edgeShape = VoronoiEdgeShape.FromPointLine(
				new Vector3(4.5f, -0.25f, 0f),
				new Vector3(0f, 0f, 0f),
				new Vector3(4f, -3f, 0f),
				new Vector3(4.9375f, -1.75f, 0f),
				new Vector3(2.5f, 1.25f, 0f),
				Vector3.back);

			Assert.AreEqual(VoronoiEdgeShapeType.Parabola, edgeShape.type);
			AssertApproximatelyEqual(new Vector3(3f, 4f, 0f), edgeShape.u, 0.0001f);
			AssertApproximatelyEqual(new Vector3(-4f, 3f, 0f), edgeShape.v, 0.0001f);
			AssertApproximatelyEqual(new Vector3(3.75f, -1.25f, 0f), edgeShape.p, 0.0001f);
			AssertApproximatelyEqual(1.25f, edgeShape.s, 0.0001f);
			AssertApproximatelyEqual(-0.25f, edgeShape.t0, 0.0001f);
			AssertApproximatelyEqual(+0.5f, edgeShape.t1, 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetNearest(), 0.0001f);
			AssertApproximatelyEqual(+0.5f, edgeShape.GetFarthest(), 0.0001f);
			AssertApproximatelyEqual(1.25f, edgeShape.GetNearestDistance(), 0.0001f);
			AssertApproximatelyEqual(2.5f, edgeShape.GetFarthestDistance(), 0.0001f);
			AssertApproximatelyEqual(new Vector3(10.75f, -0.25f, 0f), edgeShape.Evaluate(-1f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(3.75f, -1.25f, 0f), edgeShape.Evaluate(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.75f, 5.75f, 0f), edgeShape.Evaluate(+1f), 0.0001f);
			AssertApproximatelyEqual(6.25f, edgeShape.GetDistance(-1f), 0.0001f);
			AssertApproximatelyEqual(2.5f, edgeShape.GetDistance(-0.5f), 0.0001f);
			AssertApproximatelyEqual(1.25f, edgeShape.GetDistance(0f), 0.0001f);
			AssertApproximatelyEqual(1.5625f, edgeShape.GetDistance(+0.25f), 0.0001f);
			AssertApproximatelyEqual(21.25f, edgeShape.GetDistance(+2f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-2f / Mathf.Sqrt(5f), -1f / Mathf.Sqrt(5f), 0f), edgeShape.GetDirection(-1f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-7f / Mathf.Sqrt(50f), -1f / Mathf.Sqrt(50f), 0f), edgeShape.GetDirection(-1f / 2f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-0.8f, +0.6f, 0f), edgeShape.GetDirection(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-1f / Mathf.Sqrt(50f), +7f / Mathf.Sqrt(50f), 0f), edgeShape.GetDirection(+1f / 2f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(+2f / Mathf.Sqrt(125f), +11f / Mathf.Sqrt(125f), 0f), edgeShape.GetDirection(+1f), 0.0001f);

			float t0, t1;
			Assert.IsFalse(edgeShape.Intersect(0f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
			Assert.IsFalse(edgeShape.Intersect(1f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
			Assert.IsTrue(edgeShape.Intersect(1.25f, out t0, out t1));
			AssertApproximatelyEqual(0f, t0, 0.0001f);
			AssertApproximatelyEqual(0f, t1, 0.0001f);
			Assert.IsTrue(edgeShape.Intersect(1.5625f, out t0, out t1));
			AssertApproximatelyEqual(-0.25f, t0, 0.0001f);
			AssertApproximatelyEqual(+0.25f, t1, 0.0001f);
			Assert.IsTrue(edgeShape.Intersect(2.5f, out t0, out t1));
			AssertIsNaN(t0);
			AssertApproximatelyEqual(+0.5f, t1, 0.0001f);
			Assert.IsFalse(edgeShape.Intersect(6.25f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
		}

		[Test]
		public void FromSourcePointLine()
		{
			var edgeShape = VoronoiEdgeShape.FromSourcePointLine(
				new Vector3(3f, -2.25f, 0f),
				new Vector3(4f, -3f, 0f),
				new Vector3(4.5f, -0.25f, 0f),
				new Vector3(3f, -2.25f, 0f),
				Vector3.back);

			Assert.AreEqual(VoronoiEdgeShapeType.OrthogonalLine, edgeShape.type);
			AssertApproximatelyEqual(new Vector3(0f, 0f, 0f), edgeShape.u, 0.0001f);
			AssertApproximatelyEqual(new Vector3(-0.6f, -0.8f, 0f), edgeShape.v, 0.0001f);
			AssertApproximatelyEqual(new Vector3(3f, -2.25f, 0f), edgeShape.p, 0.0001f);
			AssertApproximatelyEqual(-1f, edgeShape.s, 0.0001f);
			AssertApproximatelyEqual(-2.5f, edgeShape.t0, 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.t1, 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetNearest(), 0.0001f);
			AssertApproximatelyEqual(-2.5f, edgeShape.GetFarthest(), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetNearestDistance(), 0.0001f);
			AssertApproximatelyEqual(2.5f, edgeShape.GetFarthestDistance(), 0.0001f);
			AssertApproximatelyEqual(new Vector3(4.2f, -0.65f, 0f), edgeShape.Evaluate(-2f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(3.6f, -1.45f, 0f), edgeShape.Evaluate(-1f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(3f, -2.25f, 0f), edgeShape.Evaluate(0f), 0.0001f);
			AssertApproximatelyEqual(+1f, edgeShape.GetDistance(-1f), 0.0001f);
			AssertApproximatelyEqual(+0.5f, edgeShape.GetDistance(-0.5f), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(0f), 0.0001f);
			AssertApproximatelyEqual(-0.25f, edgeShape.GetDistance(+0.25f), 0.0001f);
			AssertApproximatelyEqual(-2f, edgeShape.GetDistance(+2f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-0.6f, -0.8f, 0f), edgeShape.GetDirection(-3f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-0.6f, -0.8f, 0f), edgeShape.GetDirection(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-0.6f, -0.8f, 0f), edgeShape.GetDirection(+3f), 0.0001f);

			float t0, t1;
			Assert.IsTrue(edgeShape.Intersect(0f, out t0, out t1));
			AssertIsNaN(t0);
			AssertApproximatelyEqual(0f, t1, 0.0001f);
			Assert.IsTrue(edgeShape.Intersect(2f, out t0, out t1));
			AssertIsNaN(t0);
			AssertApproximatelyEqual(-2f, t1, 0.0001f);
			Assert.IsFalse(edgeShape.Intersect(3f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
		}

		[Test]
		public void FromTargetPointLine()
		{
			var edgeShape = VoronoiEdgeShape.FromTargetPointLine(
				new Vector3(3f, -2.25f, 0f),
				new Vector3(4f, -3f, 0f),
				new Vector3(3f, -2.25f, 0f),
				new Vector3(4.5f, -0.25f, 0f),
				Vector3.back);

			Assert.AreEqual(VoronoiEdgeShapeType.OrthogonalLine, edgeShape.type);
			AssertApproximatelyEqual(new Vector3(0f, 0f, 0f), edgeShape.u, 0.0001f);
			AssertApproximatelyEqual(new Vector3(+0.6f, +0.8f, 0f), edgeShape.v, 0.0001f);
			AssertApproximatelyEqual(new Vector3(3f, -2.25f, 0f), edgeShape.p, 0.0001f);
			AssertApproximatelyEqual(+1f, edgeShape.s, 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.t0, 0.0001f);
			AssertApproximatelyEqual(+2.5f, edgeShape.t1, 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetNearest(), 0.0001f);
			AssertApproximatelyEqual(+2.5f, edgeShape.GetFarthest(), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetNearestDistance(), 0.0001f);
			AssertApproximatelyEqual(2.5f, edgeShape.GetFarthestDistance(), 0.0001f);
			AssertApproximatelyEqual(new Vector3(3f, -2.25f, 0f), edgeShape.Evaluate(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(3.6f, -1.45f, 0f), edgeShape.Evaluate(+1f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(4.2f, -0.65f, 0f), edgeShape.Evaluate(+2f), 0.0001f);
			AssertApproximatelyEqual(-1f, edgeShape.GetDistance(-1f), 0.0001f);
			AssertApproximatelyEqual(-0.5f, edgeShape.GetDistance(-0.5f), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(0f), 0.0001f);
			AssertApproximatelyEqual(+0.25f, edgeShape.GetDistance(+0.25f), 0.0001f);
			AssertApproximatelyEqual(+2f, edgeShape.GetDistance(+2f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(+0.6f, +0.8f, 0f), edgeShape.GetDirection(-3f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(+0.6f, +0.8f, 0f), edgeShape.GetDirection(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(+0.6f, +0.8f, 0f), edgeShape.GetDirection(+3f), 0.0001f);

			float t0, t1;
			Assert.IsTrue(edgeShape.Intersect(0f, out t0, out t1));
			AssertApproximatelyEqual(0f, t0, 0.0001f);
			AssertIsNaN(t1);
			Assert.IsTrue(edgeShape.Intersect(2f, out t0, out t1));
			AssertApproximatelyEqual(+2f, t0, 0.0001f);
			AssertIsNaN(t1);
			Assert.IsFalse(edgeShape.Intersect(3f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
		}

		[Test]
		public void FromLinePoint()
		{
			var edgeShape = VoronoiEdgeShape.FromLinePoint(
				new Vector3(0f, 0f, 0f),
				new Vector3(4f, -3f, 0f),
				new Vector3(4.5f, -0.25f, 0f),
				new Vector3(2.5f, 1.25f, 0f),
				new Vector3(4.9375f, -1.75f, 0f),
				Vector3.back);

			Assert.AreEqual(VoronoiEdgeShapeType.Parabola, edgeShape.type);
			AssertApproximatelyEqual(new Vector3(3f, 4f, 0f), edgeShape.u, 0.0001f);
			AssertApproximatelyEqual(new Vector3(4f, -3f, 0f), edgeShape.v, 0.0001f);
			AssertApproximatelyEqual(new Vector3(3.75f, -1.25f, 0f), edgeShape.p, 0.0001f);
			AssertApproximatelyEqual(1.25f, edgeShape.s, 0.0001f);
			AssertApproximatelyEqual(-0.5f, edgeShape.t0, 0.0001f);
			AssertApproximatelyEqual(+0.25f, edgeShape.t1, 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetNearest(), 0.0001f);
			AssertApproximatelyEqual(-0.5f, edgeShape.GetFarthest(), 0.0001f);
			AssertApproximatelyEqual(1.25f, edgeShape.GetNearestDistance(), 0.0001f);
			AssertApproximatelyEqual(2.5f, edgeShape.GetFarthestDistance(), 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.75f, 5.75f, 0f), edgeShape.Evaluate(-1f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(3.75f, -1.25f, 0f), edgeShape.Evaluate(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(10.75f, -0.25f, 0f), edgeShape.Evaluate(+1f), 0.0001f);
			AssertApproximatelyEqual(6.25f, edgeShape.GetDistance(-1f), 0.0001f);
			AssertApproximatelyEqual(2.5f, edgeShape.GetDistance(-0.5f), 0.0001f);
			AssertApproximatelyEqual(1.25f, edgeShape.GetDistance(0f), 0.0001f);
			AssertApproximatelyEqual(1.5625f, edgeShape.GetDistance(+0.25f), 0.0001f);
			AssertApproximatelyEqual(21.25f, edgeShape.GetDistance(+2f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-2f / Mathf.Sqrt(125f), -11f / Mathf.Sqrt(125f), 0f), edgeShape.GetDirection(-1f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(+1f / Mathf.Sqrt(50f), -7f / Mathf.Sqrt(50f), 0f), edgeShape.GetDirection(-1f / 2f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(+0.8f, -0.6f, 0f), edgeShape.GetDirection(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(+7f / Mathf.Sqrt(50f), +1f / Mathf.Sqrt(50f), 0f), edgeShape.GetDirection(+1f / 2f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(+2f / Mathf.Sqrt(5f), +1f / Mathf.Sqrt(5f), 0f), edgeShape.GetDirection(+1f), 0.0001f);

			float t0, t1;
			Assert.IsFalse(edgeShape.Intersect(0f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
			Assert.IsFalse(edgeShape.Intersect(1f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
			Assert.IsTrue(edgeShape.Intersect(1.25f, out t0, out t1));
			AssertApproximatelyEqual(0f, t0, 0.0001f);
			AssertApproximatelyEqual(0f, t1, 0.0001f);
			Assert.IsTrue(edgeShape.Intersect(1.5625f, out t0, out t1));
			AssertApproximatelyEqual(-0.25f, t0, 0.0001f);
			AssertApproximatelyEqual(+0.25f, t1, 0.0001f);
			Assert.IsTrue(edgeShape.Intersect(2.5f, out t0, out t1));
			AssertApproximatelyEqual(-0.5f, t0, 0.0001f);
			AssertIsNaN(t1);
			Assert.IsFalse(edgeShape.Intersect(6.25f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
		}

		[Test]
		public void FromLineSourcePoint()
		{
			var edgeShape = VoronoiEdgeShape.FromLineSourcePoint(
				new Vector3(4f, -3f, 0f),
				new Vector3(3f, -2.25f, 0f),
				new Vector3(3.6f, -1.45f, 0f),
				new Vector3(4.5f, -0.25f, 0f),
				Vector3.back);

			Assert.AreEqual(VoronoiEdgeShapeType.OrthogonalLine, edgeShape.type);
			AssertApproximatelyEqual(new Vector3(0f, 0f, 0f), edgeShape.u, 0.0001f);
			AssertApproximatelyEqual(new Vector3(+0.6f, +0.8f, 0f), edgeShape.v, 0.0001f);
			AssertApproximatelyEqual(new Vector3(3f, -2.25f, 0f), edgeShape.p, 0.0001f);
			AssertApproximatelyEqual(+1f, edgeShape.s, 0.0001f);
			AssertApproximatelyEqual(+1f, edgeShape.t0, 0.0001f);
			AssertApproximatelyEqual(+2.5f, edgeShape.t1, 0.0001f);
			AssertApproximatelyEqual(+1f, edgeShape.GetNearest(), 0.0001f);
			AssertApproximatelyEqual(+2.5f, edgeShape.GetFarthest(), 0.0001f);
			AssertApproximatelyEqual(1f, edgeShape.GetNearestDistance(), 0.0001f);
			AssertApproximatelyEqual(2.5f, edgeShape.GetFarthestDistance(), 0.0001f);
			AssertApproximatelyEqual(new Vector3(3f, -2.25f, 0f), edgeShape.Evaluate(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(3.6f, -1.45f, 0f), edgeShape.Evaluate(+1f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(4.2f, -0.65f, 0f), edgeShape.Evaluate(+2f), 0.0001f);
			AssertApproximatelyEqual(-1f, edgeShape.GetDistance(-1f), 0.0001f);
			AssertApproximatelyEqual(-0.5f, edgeShape.GetDistance(-0.5f), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(0f), 0.0001f);
			AssertApproximatelyEqual(+0.25f, edgeShape.GetDistance(+0.25f), 0.0001f);
			AssertApproximatelyEqual(+2f, edgeShape.GetDistance(+2f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(+0.6f, +0.8f, 0f), edgeShape.GetDirection(-3f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(+0.6f, +0.8f, 0f), edgeShape.GetDirection(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(+0.6f, +0.8f, 0f), edgeShape.GetDirection(+3f), 0.0001f);

			float t0, t1;
			Assert.IsFalse(edgeShape.Intersect(0f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
			Assert.IsTrue(edgeShape.Intersect(2f, out t0, out t1));
			AssertApproximatelyEqual(+2f, t0, 0.0001f);
			AssertIsNaN(t1);
			Assert.IsFalse(edgeShape.Intersect(3f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
		}

		[Test]
		public void FromLineTargetPoint()
		{
			var edgeShape = VoronoiEdgeShape.FromLineTargetPoint(
				new Vector3(4f, -3f, 0f),
				new Vector3(3f, -2.25f, 0f),
				new Vector3(4.5f, -0.25f, 0f),
				new Vector3(3.6f, -1.45f, 0f),
				Vector3.back);

			Assert.AreEqual(VoronoiEdgeShapeType.OrthogonalLine, edgeShape.type);
			AssertApproximatelyEqual(new Vector3(0f, 0f, 0f), edgeShape.u, 0.0001f);
			AssertApproximatelyEqual(new Vector3(-0.6f, -0.8f, 0f), edgeShape.v, 0.0001f);
			AssertApproximatelyEqual(new Vector3(3f, -2.25f, 0f), edgeShape.p, 0.0001f);
			AssertApproximatelyEqual(-1f, edgeShape.s, 0.0001f);
			AssertApproximatelyEqual(-2.5f, edgeShape.t0, 0.0001f);
			AssertApproximatelyEqual(-1f, edgeShape.t1, 0.0001f);
			AssertApproximatelyEqual(-1f, edgeShape.GetNearest(), 0.0001f);
			AssertApproximatelyEqual(-2.5f, edgeShape.GetFarthest(), 0.0001f);
			AssertApproximatelyEqual(1f, edgeShape.GetNearestDistance(), 0.0001f);
			AssertApproximatelyEqual(2.5f, edgeShape.GetFarthestDistance(), 0.0001f);
			AssertApproximatelyEqual(new Vector3(4.2f, -0.65f, 0f), edgeShape.Evaluate(-2f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(3.6f, -1.45f, 0f), edgeShape.Evaluate(-1f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(3f, -2.25f, 0f), edgeShape.Evaluate(0f), 0.0001f);
			AssertApproximatelyEqual(+1f, edgeShape.GetDistance(-1f), 0.0001f);
			AssertApproximatelyEqual(+0.5f, edgeShape.GetDistance(-0.5f), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(0f), 0.0001f);
			AssertApproximatelyEqual(-0.25f, edgeShape.GetDistance(+0.25f), 0.0001f);
			AssertApproximatelyEqual(-2f, edgeShape.GetDistance(+2f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-0.6f, -0.8f, 0f), edgeShape.GetDirection(-3f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-0.6f, -0.8f, 0f), edgeShape.GetDirection(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-0.6f, -0.8f, 0f), edgeShape.GetDirection(+3f), 0.0001f);

			float t0, t1;
			Assert.IsFalse(edgeShape.Intersect(0f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
			Assert.IsTrue(edgeShape.Intersect(2f, out t0, out t1));
			AssertIsNaN(t0);
			AssertApproximatelyEqual(-2f, t1, 0.0001f);
			Assert.IsFalse(edgeShape.Intersect(3f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
		}

		[Test]
		public void FromLineLine_Obtuse()
		{
			var edgeShape = VoronoiEdgeShape.FromLineLine(
				new Vector3(0f, 0f, 0f),
				new Vector3(3f, 0f, 0f),
				new Vector3(5f, 2f, 0f),
				new Vector3(3f, 4f, 0f),
				new Vector3(2f, 3f, 0f),
				new Vector3(2.5f, 2f, 0f),
				Vector3.back,
				0.0001f);

			float scale = 2f / Mathf.Sqrt(5f);

			Assert.AreEqual(VoronoiEdgeShapeType.HalfAngleLine, edgeShape.type);
			AssertApproximatelyEqual(new Vector3(0f, 0f, 0f), edgeShape.u, 0.0001f);
			AssertApproximatelyEqual(new Vector3(+1f/2f * scale, -scale, 0f), edgeShape.v, 0.0001f);
			AssertApproximatelyEqual(new Vector3(3.5f, 0f, 0f), edgeShape.p, 0.0001f);
			AssertApproximatelyEqual(-1f / scale, edgeShape.s, 0.0001f);
			AssertApproximatelyEqual(-3f / scale, edgeShape.t0, 0.0001f);
			AssertApproximatelyEqual(-2f / scale, edgeShape.t1, 0.0001f);
			AssertApproximatelyEqual(-2f / scale, edgeShape.GetNearest(), 0.0001f);
			AssertApproximatelyEqual(-3f / scale, edgeShape.GetFarthest(), 0.0001f);
			AssertApproximatelyEqual(2f, edgeShape.GetNearestDistance(), 0.0001f);
			AssertApproximatelyEqual(3f, edgeShape.GetFarthestDistance(), 0.0001f);
			AssertApproximatelyEqual(new Vector3(3.5f, 0f, 0f), edgeShape.Evaluate(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(3f, 1f, 0f), edgeShape.Evaluate(-1f / scale), 0.0001f);
			AssertApproximatelyEqual(new Vector3(1f, 5f, 0f), edgeShape.Evaluate(-5f / scale), 0.0001f);
			AssertApproximatelyEqual(-1f, edgeShape.GetDistance(+1f / scale), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(0f), 0.0001f);
			AssertApproximatelyEqual(+scale, edgeShape.GetDistance(-1f), 0.0001f);
			AssertApproximatelyEqual(+1f, edgeShape.GetDistance(-1f / scale), 0.0001f);
			AssertApproximatelyEqual(+5f, edgeShape.GetDistance(-5f / scale), 0.0001f);
			AssertApproximatelyEqual(new Vector3(+1f / 2f * scale, -scale, 0f), edgeShape.GetDirection(-3f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(+1f / 2f * scale, -scale, 0f), edgeShape.GetDirection(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(+1f / 2f * scale, -scale, 0f), edgeShape.GetDirection(+3f), 0.0001f);

			float t0, t1;
			Assert.IsFalse(edgeShape.Intersect(0f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
			Assert.IsTrue(edgeShape.Intersect(2.5f, out t0, out t1));
			AssertIsNaN(t0);
			AssertApproximatelyEqual(-2.5f / scale, t1, 0.0001f);
			Assert.IsFalse(edgeShape.Intersect(3.5f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
		}

		[Test]
		public void FromLineLine_ObtuseReversed()
		{
			var edgeShape = VoronoiEdgeShape.FromLineLine(
				new Vector3(5f, 2f, 0f),
				new Vector3(3f, 4f, 0f),
				new Vector3(0f, 0f, 0f),
				new Vector3(3f, 0f, 0f),
				new Vector3(2.5f, 2f, 0f),
				new Vector3(2f, 3f, 0f),
				Vector3.back,
				0.0001f);

			float scale = 2f / Mathf.Sqrt(5f);

			Assert.AreEqual(VoronoiEdgeShapeType.HalfAngleLine, edgeShape.type);
			AssertApproximatelyEqual(new Vector3(0f, 0f, 0f), edgeShape.u, 0.0001f);
			AssertApproximatelyEqual(new Vector3(-1f / 2f * scale, +scale, 0f), edgeShape.v, 0.0001f);
			AssertApproximatelyEqual(new Vector3(3.5f, 0f, 0f), edgeShape.p, 0.0001f);
			AssertApproximatelyEqual(+1f / scale, edgeShape.s, 0.0001f);
			AssertApproximatelyEqual(+2f / scale, edgeShape.t0, 0.0001f);
			AssertApproximatelyEqual(+3f / scale, edgeShape.t1, 0.0001f);
			AssertApproximatelyEqual(+2f / scale, edgeShape.GetNearest(), 0.0001f);
			AssertApproximatelyEqual(+3f / scale, edgeShape.GetFarthest(), 0.0001f);
			AssertApproximatelyEqual(2f, edgeShape.GetNearestDistance(), 0.0001f);
			AssertApproximatelyEqual(3f, edgeShape.GetFarthestDistance(), 0.0001f);
			AssertApproximatelyEqual(new Vector3(3.5f, 0f, 0f), edgeShape.Evaluate(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(3f, 1f, 0f), edgeShape.Evaluate(+1f / scale), 0.0001f);
			AssertApproximatelyEqual(new Vector3(1f, 5f, 0f), edgeShape.Evaluate(+5f / scale), 0.0001f);
			AssertApproximatelyEqual(-1f, edgeShape.GetDistance(-1f / scale), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(0f), 0.0001f);
			AssertApproximatelyEqual(+scale, edgeShape.GetDistance(+1f), 0.0001f);
			AssertApproximatelyEqual(+1f, edgeShape.GetDistance(+1f / scale), 0.0001f);
			AssertApproximatelyEqual(+5f, edgeShape.GetDistance(+5f / scale), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-1f / 2f * scale, +scale, 0f), edgeShape.GetDirection(-3f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-1f / 2f * scale, +scale, 0f), edgeShape.GetDirection(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-1f / 2f * scale, +scale, 0f), edgeShape.GetDirection(+3f), 0.0001f);

			float t0, t1;
			Assert.IsFalse(edgeShape.Intersect(0f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
			Assert.IsTrue(edgeShape.Intersect(2.5f, out t0, out t1));
			AssertApproximatelyEqual(+2.5f / scale, t0, 0.0001f);
			AssertIsNaN(t1);
			Assert.IsFalse(edgeShape.Intersect(3.5f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
		}

		[Test]
		public void FromLineLine_Acute()
		{
			var edgeShape = VoronoiEdgeShape.FromLineLine(
				new Vector3(5f, 2f, 0f),
				new Vector3(-3f, -4f, 0f),
				new Vector3(0f, 0f, 0f),
				new Vector3(3f, 0f, 0f),
				new Vector3(9.5f, 3f, 0f),
				new Vector3(4.5f, 0.5f, 0f),
				Vector3.back,
				0.0001f);

			float scale = 1f / Mathf.Sqrt(5f);

			Assert.AreEqual(VoronoiEdgeShapeType.HalfAngleLine, edgeShape.type);
			AssertApproximatelyEqual(new Vector3(0f, 0f, 0f), edgeShape.u, 0.0001f);
			AssertApproximatelyEqual(new Vector3(-2f * scale, -1f * scale, 0f), edgeShape.v, 0.0001f);
			AssertApproximatelyEqual(new Vector3(3.5f, 0f, 0f), edgeShape.p, 0.0001f);
			AssertApproximatelyEqual(-1f / scale, edgeShape.s, 0.0001f);
			AssertApproximatelyEqual(-3f / scale, edgeShape.t0, 0.0001f);
			AssertApproximatelyEqual(-0.5f / scale, edgeShape.t1, 0.0001f);
			AssertApproximatelyEqual(-0.5f / scale, edgeShape.GetNearest(), 0.0001f);
			AssertApproximatelyEqual(-3f / scale, edgeShape.GetFarthest(), 0.0001f);
			AssertApproximatelyEqual(0.5f, edgeShape.GetNearestDistance(), 0.0001f);
			AssertApproximatelyEqual(3f, edgeShape.GetFarthestDistance(), 0.0001f);
			AssertApproximatelyEqual(new Vector3(3.5f, 0f, 0f), edgeShape.Evaluate(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(5.5f, 1f, 0f), edgeShape.Evaluate(-1f / scale), 0.0001f);
			AssertApproximatelyEqual(new Vector3(13.5f, 5f, 0f), edgeShape.Evaluate(-5f / scale), 0.0001f);
			AssertApproximatelyEqual(-1f, edgeShape.GetDistance(+1f / scale), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(0f), 0.0001f);
			AssertApproximatelyEqual(+scale, edgeShape.GetDistance(-1f), 0.0001f);
			AssertApproximatelyEqual(+1f, edgeShape.GetDistance(-1f / scale), 0.0001f);
			AssertApproximatelyEqual(+5f, edgeShape.GetDistance(-5f / scale), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-2f * scale, -1f * scale, 0f), edgeShape.GetDirection(-3f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-2f * scale, -1f * scale, 0f), edgeShape.GetDirection(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-2f * scale, -1f * scale, 0f), edgeShape.GetDirection(+3f), 0.0001f);

			float t0, t1;
			Assert.IsFalse(edgeShape.Intersect(0f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
			Assert.IsTrue(edgeShape.Intersect(2f, out t0, out t1));
			AssertIsNaN(t0);
			AssertApproximatelyEqual(-2f / scale, t1, 0.0001f);
			Assert.IsFalse(edgeShape.Intersect(3.5f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
		}

		[Test]
		public void FromLineLine_AcuteReversed()
		{
			var edgeShape = VoronoiEdgeShape.FromLineLine(
				new Vector3(0f, 0f, 0f),
				new Vector3(3f, 0f, 0f),
				new Vector3(5f, 2f, 0f),
				new Vector3(-3f, -4f, 0f),
				new Vector3(4.5f, 0.5f, 0f),
				new Vector3(9.5f, 3f, 0f),
				Vector3.back,
				0.0001f);

			float scale = 1f / Mathf.Sqrt(5f);

			Assert.AreEqual(VoronoiEdgeShapeType.HalfAngleLine, edgeShape.type);
			AssertApproximatelyEqual(new Vector3(0f, 0f, 0f), edgeShape.u, 0.0001f);
			AssertApproximatelyEqual(new Vector3(+2f * scale, +1f * scale, 0f), edgeShape.v, 0.0001f);
			AssertApproximatelyEqual(new Vector3(3.5f, 0f, 0f), edgeShape.p, 0.0001f);
			AssertApproximatelyEqual(+1f / scale, edgeShape.s, 0.0001f);
			AssertApproximatelyEqual(+0.5f / scale, edgeShape.t0, 0.0001f);
			AssertApproximatelyEqual(+3f / scale, edgeShape.t1, 0.0001f);
			AssertApproximatelyEqual(+0.5f / scale, edgeShape.GetNearest(), 0.0001f);
			AssertApproximatelyEqual(+3f / scale, edgeShape.GetFarthest(), 0.0001f);
			AssertApproximatelyEqual(0.5f, edgeShape.GetNearestDistance(), 0.0001f);
			AssertApproximatelyEqual(3f, edgeShape.GetFarthestDistance(), 0.0001f);
			AssertApproximatelyEqual(new Vector3(3.5f, 0f, 0f), edgeShape.Evaluate(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(5.5f, 1f, 0f), edgeShape.Evaluate(+1f / scale), 0.0001f);
			AssertApproximatelyEqual(new Vector3(13.5f, 5f, 0f), edgeShape.Evaluate(+5f / scale), 0.0001f);
			AssertApproximatelyEqual(-1f, edgeShape.GetDistance(-1f / scale), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(0f), 0.0001f);
			AssertApproximatelyEqual(+scale, edgeShape.GetDistance(+1f), 0.0001f);
			AssertApproximatelyEqual(+1f, edgeShape.GetDistance(+1f / scale), 0.0001f);
			AssertApproximatelyEqual(+5f, edgeShape.GetDistance(+5f / scale), 0.0001f);
			AssertApproximatelyEqual(new Vector3(+2f * scale, +1f * scale, 0f), edgeShape.GetDirection(-3f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(+2f * scale, +1f * scale, 0f), edgeShape.GetDirection(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(+2f * scale, +1f * scale, 0f), edgeShape.GetDirection(+3f), 0.0001f);

			float t0, t1;
			Assert.IsFalse(edgeShape.Intersect(0f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
			Assert.IsTrue(edgeShape.Intersect(2f, out t0, out t1));
			AssertApproximatelyEqual(+2f / scale, t0, 0.0001f);
			AssertIsNaN(t1);
			Assert.IsFalse(edgeShape.Intersect(3.5f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
		}

		[Test]
		public void FromLineLine_ParallelSameDirection()
		{
			var edgeShape = VoronoiEdgeShape.FromLineLine(
				new Vector3(0f, 0f, 0f),
				new Vector3(3f, 0f, 0f),
				new Vector3(5f, 4f, 0f),
				new Vector3(1f, 0f, 0f),
				new Vector3(2.5f, 10f, 0f),
				new Vector3(2.5f, 3f, 0f),
				Vector3.back,
				0.0001f);

			Assert.AreEqual(VoronoiEdgeShapeType.OrthogonalLine, edgeShape.type);
			AssertApproximatelyEqual(new Vector3(0f, 0f, 0f), edgeShape.u, 0.0001f);
			AssertApproximatelyEqual(new Vector3(0f, -1f, 0f), edgeShape.v, 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.5f, 2f, 0f), edgeShape.p, 0.0001f);
			AssertApproximatelyEqual(-1f, edgeShape.s, 0.0001f);
			AssertApproximatelyEqual(-8f, edgeShape.t0, 0.0001f);
			AssertApproximatelyEqual(-1f, edgeShape.t1, 0.0001f);
			AssertApproximatelyEqual(-1f, edgeShape.GetNearest(), 0.0001f);
			AssertApproximatelyEqual(-8f, edgeShape.GetFarthest(), 0.0001f);
			AssertApproximatelyEqual(1f, edgeShape.GetNearestDistance(), 0.0001f);
			AssertApproximatelyEqual(8f, edgeShape.GetFarthestDistance(), 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.5f, 2f, 0f), edgeShape.Evaluate(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.5f, 3f, 0f), edgeShape.Evaluate(-1f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.5f, 7f), edgeShape.Evaluate(-5f), 0.0001f);
			AssertApproximatelyEqual(-1f, edgeShape.GetDistance(+1f), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(0f), 0.0001f);
			AssertApproximatelyEqual(+1f, edgeShape.GetDistance(-1f), 0.0001f);
			AssertApproximatelyEqual(+5f, edgeShape.GetDistance(-5f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(0f, -1f, 0f), edgeShape.GetDirection(-3f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(0f, -1f, 0f), edgeShape.GetDirection(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(0f, -1f, 0f), edgeShape.GetDirection(+3f), 0.0001f);

			float t0, t1;
			Assert.IsFalse(edgeShape.Intersect(0f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
			Assert.IsTrue(edgeShape.Intersect(2f, out t0, out t1));
			AssertIsNaN(t0);
			AssertApproximatelyEqual(-2f, t1, 0.0001f);
			Assert.IsTrue(edgeShape.Intersect(5f, out t0, out t1));
			AssertIsNaN(t0);
			AssertApproximatelyEqual(-5f, t1, 0.0001f);
			Assert.IsFalse(edgeShape.Intersect(10f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
		}

		[Test]
		public void FromLineLine_ParallelSameDirectionReversed()
		{
			var edgeShape = VoronoiEdgeShape.FromLineLine(
				new Vector3(5f, 4f, 0f),
				new Vector3(1f, 0f, 0f),
				new Vector3(0f, 0f, 0f),
				new Vector3(3f, 0f, 0f),
				new Vector3(2.5f, 3f, 0f),
				new Vector3(2.5f, 10f, 0f),
				Vector3.back,
				0.0001f);

			Assert.AreEqual(VoronoiEdgeShapeType.OrthogonalLine, edgeShape.type);
			AssertApproximatelyEqual(new Vector3(0f, 0f, 0f), edgeShape.u, 0.0001f);
			AssertApproximatelyEqual(new Vector3(0f, +1f, 0f), edgeShape.v, 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.5f, 2f, 0f), edgeShape.p, 0.0001f);
			AssertApproximatelyEqual(+1f, edgeShape.s, 0.0001f);
			AssertApproximatelyEqual(+1f, edgeShape.t0, 0.0001f);
			AssertApproximatelyEqual(+8f, edgeShape.t1, 0.0001f);
			AssertApproximatelyEqual(+1f, edgeShape.GetNearest(), 0.0001f);
			AssertApproximatelyEqual(+8f, edgeShape.GetFarthest(), 0.0001f);
			AssertApproximatelyEqual(1f, edgeShape.GetNearestDistance(), 0.0001f);
			AssertApproximatelyEqual(8f, edgeShape.GetFarthestDistance(), 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.5f, 2f, 0f), edgeShape.Evaluate(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.5f, 3f, 0f), edgeShape.Evaluate(+1f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.5f, 7f), edgeShape.Evaluate(+5f), 0.0001f);
			AssertApproximatelyEqual(-1f, edgeShape.GetDistance(-1f), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(0f), 0.0001f);
			AssertApproximatelyEqual(+1f, edgeShape.GetDistance(+1f), 0.0001f);
			AssertApproximatelyEqual(+5f, edgeShape.GetDistance(+5f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(0f, +1f, 0f), edgeShape.GetDirection(-3f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(0f, +1f, 0f), edgeShape.GetDirection(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(0f, +1f, 0f), edgeShape.GetDirection(+3f), 0.0001f);

			float t0, t1;
			Assert.IsFalse(edgeShape.Intersect(0f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
			Assert.IsTrue(edgeShape.Intersect(2f, out t0, out t1));
			AssertApproximatelyEqual(+2f, t0, 0.0001f);
			AssertIsNaN(t1);
			Assert.IsTrue(edgeShape.Intersect(5f, out t0, out t1));
			AssertApproximatelyEqual(+5f, t0, 0.0001f);
			AssertIsNaN(t1);
			Assert.IsFalse(edgeShape.Intersect(10f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
		}

		[Test]
		public void FromLineLine_ParallelOppositeDirection()
		{
			var edgeShape = VoronoiEdgeShape.FromLineLine(
				new Vector3(0f, 0f, 0f),
				new Vector3(3f, 0f, 0f),
				new Vector3(5f, 4f, 0f),
				new Vector3(-1f, 0f, 0f),
				new Vector3(0f, 2f, 0f),
				new Vector3(1.5f, 2f, 0f),
				Vector3.back,
				0.0001f);

			Assert.AreEqual(VoronoiEdgeShapeType.ParallelLine, edgeShape.type);
			AssertApproximatelyEqual(new Vector3(0f, 0f, 0f), edgeShape.u, 0.0001f);
			AssertApproximatelyEqual(new Vector3(+1f, 0f, 0f), edgeShape.v, 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.5f, 2f, 0f), edgeShape.p, 0.0001f);
			AssertApproximatelyEqual(2f, edgeShape.s, 0.0001f);
			AssertApproximatelyEqual(-2.5f, edgeShape.t0, 0.0001f);
			AssertApproximatelyEqual(-1f, edgeShape.t1, 0.0001f);
			AssertApproximatelyEqual(-2.5f, edgeShape.GetNearest(), 0.0001f);
			AssertApproximatelyEqual(-1f, edgeShape.GetFarthest(), 0.0001f);
			AssertApproximatelyEqual(2f, edgeShape.GetNearestDistance(), 0.0001f);
			AssertApproximatelyEqual(2f, edgeShape.GetFarthestDistance(), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-2.5f, 2f, 0f), edgeShape.Evaluate(-5f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(1.5f, 2f, 0f), edgeShape.Evaluate(-1f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.5f, 2f, 0f), edgeShape.Evaluate(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(3.5f, 2f, 0f), edgeShape.Evaluate(+1f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(7.5f, 2f, 0f), edgeShape.Evaluate(+5f), 0.0001f);
			AssertApproximatelyEqual(2f, edgeShape.GetDistance(-5f), 0.0001f);
			AssertApproximatelyEqual(2f, edgeShape.GetDistance(-1f), 0.0001f);
			AssertApproximatelyEqual(2f, edgeShape.GetDistance(0f), 0.0001f);
			AssertApproximatelyEqual(2f, edgeShape.GetDistance(+1f), 0.0001f);
			AssertApproximatelyEqual(2f, edgeShape.GetDistance(+5f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(+1f, 0f, 0f), edgeShape.GetDirection(-3f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(+1f, 0f, 0f), edgeShape.GetDirection(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(+1f, 0f, 0f), edgeShape.GetDirection(+3f), 0.0001f);

			float t0, t1;
			Assert.IsFalse(edgeShape.Intersect(0f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
			Assert.IsFalse(edgeShape.Intersect(1f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
			Assert.IsTrue(edgeShape.Intersect(2f, out t0, out t1));
			AssertApproximatelyEqual(-2.5f, t0, 0.0001f);
			AssertApproximatelyEqual(-1f, t1, 0.0001f);
			Assert.IsFalse(edgeShape.Intersect(3f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
		}

		[Test]
		public void FromLineLine_ParallelOppositeDirectionReversed()
		{
			var edgeShape = VoronoiEdgeShape.FromLineLine(
				new Vector3(5f, 4f, 0f),
				new Vector3(-1f, 0f, 0f),
				new Vector3(0f, 0f, 0f),
				new Vector3(3f, 0f, 0f),
				new Vector3(1.5f, 2f, 0f),
				new Vector3(0f, 2f, 0f),
				Vector3.back,
				0.0001f);

			Assert.AreEqual(VoronoiEdgeShapeType.ParallelLine, edgeShape.type);
			AssertApproximatelyEqual(new Vector3(0f, 0f, 0f), edgeShape.u, 0.0001f);
			AssertApproximatelyEqual(new Vector3(-1f, 0f, 0f), edgeShape.v, 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.5f, 2f, 0f), edgeShape.p, 0.0001f);
			AssertApproximatelyEqual(2f, edgeShape.s, 0.0001f);
			AssertApproximatelyEqual(+1f, edgeShape.t0, 0.0001f);
			AssertApproximatelyEqual(+2.5f, edgeShape.t1, 0.0001f);
			AssertApproximatelyEqual(+1f, edgeShape.GetNearest(), 0.0001f);
			AssertApproximatelyEqual(+2.5f, edgeShape.GetFarthest(), 0.0001f);
			AssertApproximatelyEqual(2f, edgeShape.GetNearestDistance(), 0.0001f);
			AssertApproximatelyEqual(2f, edgeShape.GetFarthestDistance(), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-2.5f, 2f, 0f), edgeShape.Evaluate(+5f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(1.5f, 2f, 0f), edgeShape.Evaluate(+1f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.5f, 2f, 0f), edgeShape.Evaluate(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(3.5f, 2f, 0f), edgeShape.Evaluate(-1f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(7.5f, 2f, 0f), edgeShape.Evaluate(-5f), 0.0001f);
			AssertApproximatelyEqual(2f, edgeShape.GetDistance(+5f), 0.0001f);
			AssertApproximatelyEqual(2f, edgeShape.GetDistance(+1f), 0.0001f);
			AssertApproximatelyEqual(2f, edgeShape.GetDistance(0f), 0.0001f);
			AssertApproximatelyEqual(2f, edgeShape.GetDistance(-1f), 0.0001f);
			AssertApproximatelyEqual(2f, edgeShape.GetDistance(-5f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-1f, 0f, 0f), edgeShape.GetDirection(-3f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-1f, 0f, 0f), edgeShape.GetDirection(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-1f, 0f, 0f), edgeShape.GetDirection(+3f), 0.0001f);

			float t0, t1;
			Assert.IsFalse(edgeShape.Intersect(0f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
			Assert.IsFalse(edgeShape.Intersect(1f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
			Assert.IsTrue(edgeShape.Intersect(2f, out t0, out t1));
			AssertApproximatelyEqual(+1f, t0, 0.0001f);
			AssertApproximatelyEqual(+2.5f, t1, 0.0001f);
			Assert.IsFalse(edgeShape.Intersect(3f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
		}

		[Test]
		public void FromLineLine_ColinearSameDirection()
		{
			var edgeShape = VoronoiEdgeShape.FromLineLine(
				new Vector3(0f, 0f, 0f),
				new Vector3(3f, 0f, 0f),
				new Vector3(5f, 0f, 0f),
				new Vector3(1f, 0f, 0f),
				new Vector3(2.5f, 10f, 0f),
				new Vector3(2.5f, 3f, 0f),
				Vector3.back,
				0.0001f);

			Assert.AreEqual(VoronoiEdgeShapeType.OrthogonalLine, edgeShape.type);
			AssertApproximatelyEqual(new Vector3(0f, 0f, 0f), edgeShape.u, 0.0001f);
			AssertApproximatelyEqual(new Vector3(0f, -1f, 0f), edgeShape.v, 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.5f, 0f, 0f), edgeShape.p, 0.0001f);
			AssertApproximatelyEqual(-1f, edgeShape.s, 0.0001f);
			AssertApproximatelyEqual(-10f, edgeShape.t0, 0.0001f);
			AssertApproximatelyEqual(-3f, edgeShape.t1, 0.0001f);
			AssertApproximatelyEqual(-3f, edgeShape.GetNearest(), 0.0001f);
			AssertApproximatelyEqual(-10f, edgeShape.GetFarthest(), 0.0001f);
			AssertApproximatelyEqual(3f, edgeShape.GetNearestDistance(), 0.0001f);
			AssertApproximatelyEqual(10f, edgeShape.GetFarthestDistance(), 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.5f, 0f, 0f), edgeShape.Evaluate(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.5f, 1f, 0f), edgeShape.Evaluate(-1f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.5f, 5f), edgeShape.Evaluate(-5f), 0.0001f);
			AssertApproximatelyEqual(-1f, edgeShape.GetDistance(+1f), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(0f), 0.0001f);
			AssertApproximatelyEqual(+1f, edgeShape.GetDistance(-1f), 0.0001f);
			AssertApproximatelyEqual(+5f, edgeShape.GetDistance(-5f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(0f, -1f, 0f), edgeShape.GetDirection(-3f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(0f, -1f, 0f), edgeShape.GetDirection(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(0f, -1f, 0f), edgeShape.GetDirection(+3f), 0.0001f);

			float t0, t1;
			Assert.IsFalse(edgeShape.Intersect(0f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
			Assert.IsFalse(edgeShape.Intersect(2f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
			Assert.IsTrue(edgeShape.Intersect(4f, out t0, out t1));
			AssertIsNaN(t0);
			AssertApproximatelyEqual(-4f, t1, 0.0001f);
			Assert.IsTrue(edgeShape.Intersect(7f, out t0, out t1));
			AssertIsNaN(t0);
			AssertApproximatelyEqual(-7f, t1, 0.0001f);
			Assert.IsFalse(edgeShape.Intersect(12f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
		}

		[Test]
		public void FromLineLine_ColinearSameDirectionReversed()
		{
			var edgeShape = VoronoiEdgeShape.FromLineLine(
				new Vector3(5f, 0f, 0f),
				new Vector3(1f, 0f, 0f),
				new Vector3(0f, 0f, 0f),
				new Vector3(3f, 0f, 0f),
				new Vector3(2.5f, 3f, 0f),
				new Vector3(2.5f, 10f, 0f),
				Vector3.back,
				0.0001f);

			Assert.AreEqual(VoronoiEdgeShapeType.OrthogonalLine, edgeShape.type);
			AssertApproximatelyEqual(new Vector3(0f, 0f, 0f), edgeShape.u, 0.0001f);
			AssertApproximatelyEqual(new Vector3(0f, +1f, 0f), edgeShape.v, 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.5f, 0f, 0f), edgeShape.p, 0.0001f);
			AssertApproximatelyEqual(+1f, edgeShape.s, 0.0001f);
			AssertApproximatelyEqual(+3f, edgeShape.t0, 0.0001f);
			AssertApproximatelyEqual(+10f, edgeShape.t1, 0.0001f);
			AssertApproximatelyEqual(+3f, edgeShape.GetNearest(), 0.0001f);
			AssertApproximatelyEqual(+10f, edgeShape.GetFarthest(), 0.0001f);
			AssertApproximatelyEqual(3f, edgeShape.GetNearestDistance(), 0.0001f);
			AssertApproximatelyEqual(10f, edgeShape.GetFarthestDistance(), 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.5f, 0f, 0f), edgeShape.Evaluate(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.5f, 1f, 0f), edgeShape.Evaluate(+1f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.5f, 5f), edgeShape.Evaluate(+5f), 0.0001f);
			AssertApproximatelyEqual(-1f, edgeShape.GetDistance(-1f), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(0f), 0.0001f);
			AssertApproximatelyEqual(+1f, edgeShape.GetDistance(+1f), 0.0001f);
			AssertApproximatelyEqual(+5f, edgeShape.GetDistance(+5f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(0f, +1f, 0f), edgeShape.GetDirection(-3f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(0f, +1f, 0f), edgeShape.GetDirection(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(0f, +1f, 0f), edgeShape.GetDirection(+3f), 0.0001f);

			float t0, t1;
			Assert.IsFalse(edgeShape.Intersect(0f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
			Assert.IsFalse(edgeShape.Intersect(2f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
			Assert.IsTrue(edgeShape.Intersect(4f, out t0, out t1));
			AssertApproximatelyEqual(+4f, t0, 0.0001f);
			AssertIsNaN(t1);
			Assert.IsTrue(edgeShape.Intersect(7f, out t0, out t1));
			AssertApproximatelyEqual(+7f, t0, 0.0001f);
			AssertIsNaN(t1);
			Assert.IsFalse(edgeShape.Intersect(12f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
		}

		[Test]
		public void FromLineLine_ColinearOppositeDirection()
		{
			var edgeShape = VoronoiEdgeShape.FromLineLine(
				new Vector3(0f, 0f, 0f),
				new Vector3(3f, 0f, 0f),
				new Vector3(5f, 0f, 0f),
				new Vector3(-1f, 0f, 0f),
				new Vector3(0f, 0f, 0f),
				new Vector3(1.5f, 0f, 0f),
				Vector3.back,
				0.0001f);

			Assert.AreEqual(VoronoiEdgeShapeType.ParallelLine, edgeShape.type);
			AssertApproximatelyEqual(new Vector3(0f, 0f, 0f), edgeShape.u, 0.0001f);
			AssertApproximatelyEqual(new Vector3(+1f, 0f, 0f), edgeShape.v, 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.5f, 0f, 0f), edgeShape.p, 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.s, 0.0001f);
			AssertApproximatelyEqual(-2.5f, edgeShape.t0, 0.0001f);
			AssertApproximatelyEqual(-1f, edgeShape.t1, 0.0001f);
			AssertApproximatelyEqual(-2.5f, edgeShape.GetNearest(), 0.0001f);
			AssertApproximatelyEqual(-1f, edgeShape.GetFarthest(), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetNearestDistance(), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetFarthestDistance(), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-2.5f, 0f, 0f), edgeShape.Evaluate(-5f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(1.5f, 0f, 0f), edgeShape.Evaluate(-1f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.5f, 0f, 0f), edgeShape.Evaluate(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(3.5f, 0f, 0f), edgeShape.Evaluate(+1f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(7.5f, 0f, 0f), edgeShape.Evaluate(+5f), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(-5f), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(-1f), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(0f), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(+1f), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(+5f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(+1f, 0f, 0f), edgeShape.GetDirection(-3f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(+1f, 0f, 0f), edgeShape.GetDirection(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(+1f, 0f, 0f), edgeShape.GetDirection(+3f), 0.0001f);

			float t0, t1;
			Assert.IsTrue(edgeShape.Intersect(0f, out t0, out t1));
			AssertApproximatelyEqual(-2.5f, t0, 0.0001f);
			AssertApproximatelyEqual(-1f, t1, 0.0001f);
			Assert.IsFalse(edgeShape.Intersect(1f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
		}

		[Test]
		public void FromLineLine_ColinearOppositeDirectionReversed()
		{
			var edgeShape = VoronoiEdgeShape.FromLineLine(
				new Vector3(5f, 0f, 0f),
				new Vector3(-1f, 0f, 0f),
				new Vector3(0f, 0f, 0f),
				new Vector3(3f, 0f, 0f),
				new Vector3(1.5f, 0f, 0f),
				new Vector3(0f, 0f, 0f),
				Vector3.back,
				0.0001f);

			Assert.AreEqual(VoronoiEdgeShapeType.ParallelLine, edgeShape.type);
			AssertApproximatelyEqual(new Vector3(0f, 0f, 0f), edgeShape.u, 0.0001f);
			AssertApproximatelyEqual(new Vector3(-1f, 0f, 0f), edgeShape.v, 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.5f, 0f, 0f), edgeShape.p, 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.s, 0.0001f);
			AssertApproximatelyEqual(+1f, edgeShape.t0, 0.0001f);
			AssertApproximatelyEqual(+2.5f, edgeShape.t1, 0.0001f);
			AssertApproximatelyEqual(+1f, edgeShape.GetNearest(), 0.0001f);
			AssertApproximatelyEqual(+2.5f, edgeShape.GetFarthest(), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetNearestDistance(), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetFarthestDistance(), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-2.5f, 0f, 0f), edgeShape.Evaluate(+5f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(1.5f, 0f, 0f), edgeShape.Evaluate(+1f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(2.5f, 0f, 0f), edgeShape.Evaluate(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(3.5f, 0f, 0f), edgeShape.Evaluate(-1f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(7.5f, 0f, 0f), edgeShape.Evaluate(-5f), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(+5f), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(+1f), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(0f), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(-1f), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(-5f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-1f, 0f, 0f), edgeShape.GetDirection(-3f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-1f, 0f, 0f), edgeShape.GetDirection(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-1f, 0f, 0f), edgeShape.GetDirection(+3f), 0.0001f);

			float t0, t1;
			Assert.IsTrue(edgeShape.Intersect(0f, out t0, out t1));
			AssertApproximatelyEqual(+1f, t0, 0.0001f);
			AssertApproximatelyEqual(+2.5f, t1, 0.0001f);
			Assert.IsFalse(edgeShape.Intersect(1f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
		}

		[Test]
		public void FromTwinLines()
		{
			var edgeShape = VoronoiEdgeShape.FromTwinLines(
				new Vector3(0f, 0f, 0f),
				new Vector3(4f, 3f, 0f),
				new Vector3(0f, 0f, 0f),
				new Vector3(4f, 3f, 0f));

			Assert.AreEqual(VoronoiEdgeShapeType.ParallelLine, edgeShape.type);
			AssertApproximatelyEqual(new Vector3(0f, 0f, 0f), edgeShape.u, 0.0001f);
			AssertApproximatelyEqual(new Vector3(-0.8f, -0.6f, 0f), edgeShape.v, 0.0001f);
			AssertApproximatelyEqual(new Vector3(2f, 1.5f, 0f), edgeShape.p, 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.s, 0.0001f);
			AssertApproximatelyEqual(-2.5f, edgeShape.t0, 0.0001f);
			AssertApproximatelyEqual(+2.5f, edgeShape.t1, 0.0001f);
			AssertApproximatelyEqual(-2.5f, edgeShape.GetNearest(), 0.0001f);
			AssertApproximatelyEqual(+2.5f, edgeShape.GetFarthest(), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetNearestDistance(), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetFarthestDistance(), 0.0001f);
			AssertApproximatelyEqual(new Vector3(4f, 3f, 0f), edgeShape.Evaluate(-2.5f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(3.2f, 2.4f, 0f), edgeShape.Evaluate(-1.5f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(2f, 1.5f, 0f), edgeShape.Evaluate(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(0.8f, 0.6f, 0f), edgeShape.Evaluate(+1.5f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(0f, 0f, 0f), edgeShape.Evaluate(+2.5f), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(+5f), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(+1f), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(0f), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(-1f), 0.0001f);
			AssertApproximatelyEqual(0f, edgeShape.GetDistance(-5f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-0.8f, -0.6f, 0f), edgeShape.GetDirection(-3f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-0.8f, -0.6f, 0f), edgeShape.GetDirection(0f), 0.0001f);
			AssertApproximatelyEqual(new Vector3(-0.8f, -0.6f, 0f), edgeShape.GetDirection(+3f), 0.0001f);

			float t0, t1;
			Assert.IsTrue(edgeShape.Intersect(0f, out t0, out t1));
			AssertApproximatelyEqual(-2.5f, t0, 0.0001f);
			AssertApproximatelyEqual(+2.5f, t1, 0.0001f);
			Assert.IsFalse(edgeShape.Intersect(1f, out t0, out t1));
			AssertIsNaN(t0);
			AssertIsNaN(t1);
		}

		[Test]
		public void ParabolaCurvature_ClockwiseReverse()
		{
			var edgeShape = VoronoiEdgeShape.FromPointLine(new Vector3(0f, 0f, 0f), new Vector3(-7f, -1f, 0f), new Vector3(+8f, -6f, 0f), new Vector3(-7f, -1f, 0f), new Vector3(1f, -7f, 0f), Vector3.back);

			AssertApproximatelyEqual(Mathf.Sqrt(5f) / 125f, edgeShape.GetCurvature(-2f, Vector3.back), 0.0001f);
			AssertApproximatelyEqual(Mathf.Sqrt(2f) / 20f, edgeShape.GetCurvature(-1f, Vector3.back), 0.0001f);
			AssertApproximatelyEqual(2f / 10f, edgeShape.GetCurvature(0f, Vector3.back), 0.0001f);
			AssertApproximatelyEqual(Mathf.Sqrt(2f) / 20f, edgeShape.GetCurvature(+1f, Vector3.back), 0.0001f);
			AssertApproximatelyEqual(Mathf.Sqrt(5f) / 125f, edgeShape.GetCurvature(+2f, Vector3.back), 0.0001f);
		}

		[Test]
		public void ParabolaCurvature_CounterClockwise()
		{
			var edgeShape = VoronoiEdgeShape.FromLinePoint(new Vector3(-7f, -1f, 0f), new Vector3(+8f, -6f, 0f), new Vector3(0f, 0f, 0f), new Vector3(-7f, -1f, 0f), new Vector3(1f, -7f, 0f), Vector3.back);

			AssertApproximatelyEqual(-Mathf.Sqrt(5f) / 125f, edgeShape.GetCurvature(-2f, Vector3.back), 0.0001f);
			AssertApproximatelyEqual(-Mathf.Sqrt(2f) / 20f, edgeShape.GetCurvature(-1f, Vector3.back), 0.0001f);
			AssertApproximatelyEqual(-2f / 10f, edgeShape.GetCurvature(0f, Vector3.back), 0.0001f);
			AssertApproximatelyEqual(-Mathf.Sqrt(2f) / 20f, edgeShape.GetCurvature(+1f, Vector3.back), 0.0001f);
			AssertApproximatelyEqual(-Mathf.Sqrt(5f) / 125f, edgeShape.GetCurvature(+2f, Vector3.back), 0.0001f);
		}
	}
}
#endif
