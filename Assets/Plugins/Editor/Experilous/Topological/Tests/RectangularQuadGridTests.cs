#if UNITY_5_3
using UnityEngine;
using NUnit.Framework;

namespace Experilous.Topological.Tests
{
	public class RectangularQuadGridTests
	{
		[Test]
		public static void ValidateWideGridElementCounts()
		{
			var grid = RectangularQuadGrid.Create(new PlanarDescriptor(Vector3.right, false, Vector3.up, false), new Index2D(5, 3));

			Assert.AreEqual(5, grid.faceAxis0Count);
			Assert.AreEqual(3, grid.faceAxis1Count);
			Assert.AreEqual(6, grid.vertexAxis0Count);
			Assert.AreEqual(4, grid.vertexAxis1Count);

			Assert.AreEqual(16, grid.faceCount);
			Assert.AreEqual(15, grid.internalFaceCount);
			Assert.AreEqual(1, grid.externalFaceCount);
			Assert.AreEqual(76, grid.edgeCount);
			Assert.AreEqual(24, grid.vertexCount);
		}

		[Test]
		public static void ValidateTallGridElementCounts()
		{
			var grid = RectangularQuadGrid.Create(new PlanarDescriptor(Vector3.right, false, Vector3.up, false), new Index2D(3, 5));

			Assert.AreEqual(3, grid.faceAxis0Count);
			Assert.AreEqual(5, grid.faceAxis1Count);
			Assert.AreEqual(4, grid.vertexAxis0Count);
			Assert.AreEqual(6, grid.vertexAxis1Count);

			Assert.AreEqual(16, grid.faceCount);
			Assert.AreEqual(15, grid.internalFaceCount);
			Assert.AreEqual(1, grid.externalFaceCount);
			Assert.AreEqual(76, grid.edgeCount);
			Assert.AreEqual(24, grid.vertexCount);
		}

		[Test]
		public static void ValidateSquareGridElementCounts()
		{
			var grid = RectangularQuadGrid.Create(new PlanarDescriptor(Vector3.right, false, Vector3.up, false), new Index2D(4, 4));

			Assert.AreEqual(4, grid.faceAxis0Count);
			Assert.AreEqual(4, grid.faceAxis1Count);
			Assert.AreEqual(5, grid.vertexAxis0Count);
			Assert.AreEqual(5, grid.vertexAxis1Count);

			Assert.AreEqual(17, grid.faceCount);
			Assert.AreEqual(16, grid.internalFaceCount);
			Assert.AreEqual(1, grid.externalFaceCount);
			Assert.AreEqual(80, grid.edgeCount);
			Assert.AreEqual(25, grid.vertexCount);
		}

		[Test]
		public static void ValidateWideHorizontallyWrappedGridElementCounts()
		{
			var grid = RectangularQuadGrid.Create(new PlanarDescriptor(Vector3.right, true, Vector3.up, false), new Index2D(5, 3));

			Assert.AreEqual(5, grid.faceAxis0Count);
			Assert.AreEqual(3, grid.faceAxis1Count);
			Assert.AreEqual(5, grid.vertexAxis0Count);
			Assert.AreEqual(4, grid.vertexAxis1Count);

			Assert.AreEqual(17, grid.faceCount);
			Assert.AreEqual(15, grid.internalFaceCount);
			Assert.AreEqual(2, grid.externalFaceCount);
			Assert.AreEqual(70, grid.edgeCount);
			Assert.AreEqual(20, grid.vertexCount);
		}

		[Test]
		public static void ValidateTallHorizontallyWrappedGridElementCounts()
		{
			var grid = RectangularQuadGrid.Create(new PlanarDescriptor(Vector3.right, true, Vector3.up, false), new Index2D(3, 5));

			Assert.AreEqual(3, grid.faceAxis0Count);
			Assert.AreEqual(5, grid.faceAxis1Count);
			Assert.AreEqual(3, grid.vertexAxis0Count);
			Assert.AreEqual(6, grid.vertexAxis1Count);

			Assert.AreEqual(17, grid.faceCount);
			Assert.AreEqual(15, grid.internalFaceCount);
			Assert.AreEqual(2, grid.externalFaceCount);
			Assert.AreEqual(66, grid.edgeCount);
			Assert.AreEqual(18, grid.vertexCount);
		}

		[Test]
		public static void ValidateSquareHorizontallyWrappedGridElementCounts()
		{
			var grid = RectangularQuadGrid.Create(new PlanarDescriptor(Vector3.right, true, Vector3.up, false), new Index2D(4, 4));

			Assert.AreEqual(4, grid.faceAxis0Count);
			Assert.AreEqual(4, grid.faceAxis1Count);
			Assert.AreEqual(4, grid.vertexAxis0Count);
			Assert.AreEqual(5, grid.vertexAxis1Count);

			Assert.AreEqual(18, grid.faceCount);
			Assert.AreEqual(16, grid.internalFaceCount);
			Assert.AreEqual(2, grid.externalFaceCount);
			Assert.AreEqual(72, grid.edgeCount);
			Assert.AreEqual(20, grid.vertexCount);
		}

		[Test]
		public static void ValidateWideVerticallyWrappedGridElementCounts()
		{
			var grid = RectangularQuadGrid.Create(new PlanarDescriptor(Vector3.right, false, Vector3.up, true), new Index2D(5, 3));

			Assert.AreEqual(5, grid.faceAxis0Count);
			Assert.AreEqual(3, grid.faceAxis1Count);
			Assert.AreEqual(6, grid.vertexAxis0Count);
			Assert.AreEqual(3, grid.vertexAxis1Count);

			Assert.AreEqual(17, grid.faceCount);
			Assert.AreEqual(15, grid.internalFaceCount);
			Assert.AreEqual(2, grid.externalFaceCount);
			Assert.AreEqual(66, grid.edgeCount);
			Assert.AreEqual(18, grid.vertexCount);
		}

		[Test]
		public static void ValidateTallVerticallyWrappedGridElementCounts()
		{
			var grid = RectangularQuadGrid.Create(new PlanarDescriptor(Vector3.right, false, Vector3.up, true), new Index2D(3, 5));

			Assert.AreEqual(3, grid.faceAxis0Count);
			Assert.AreEqual(5, grid.faceAxis1Count);
			Assert.AreEqual(4, grid.vertexAxis0Count);
			Assert.AreEqual(5, grid.vertexAxis1Count);

			Assert.AreEqual(17, grid.faceCount);
			Assert.AreEqual(15, grid.internalFaceCount);
			Assert.AreEqual(2, grid.externalFaceCount);
			Assert.AreEqual(70, grid.edgeCount);
			Assert.AreEqual(20, grid.vertexCount);
		}

		[Test]
		public static void ValidateSquareVerticallyWrappedGridElementCounts()
		{
			var grid = RectangularQuadGrid.Create(new PlanarDescriptor(Vector3.right, false, Vector3.up, true), new Index2D(4, 4));

			Assert.AreEqual(4, grid.faceAxis0Count);
			Assert.AreEqual(4, grid.faceAxis1Count);
			Assert.AreEqual(5, grid.vertexAxis0Count);
			Assert.AreEqual(4, grid.vertexAxis1Count);

			Assert.AreEqual(18, grid.faceCount);
			Assert.AreEqual(16, grid.internalFaceCount);
			Assert.AreEqual(2, grid.externalFaceCount);
			Assert.AreEqual(72, grid.edgeCount);
			Assert.AreEqual(20, grid.vertexCount);
		}

		[Test]
		public static void ValidateWideFullyWrappedGridElementCounts()
		{
			var grid = RectangularQuadGrid.Create(new PlanarDescriptor(Vector3.right, true, Vector3.up, true), new Index2D(5, 3));

			Assert.AreEqual(5, grid.faceAxis0Count);
			Assert.AreEqual(3, grid.faceAxis1Count);
			Assert.AreEqual(5, grid.vertexAxis0Count);
			Assert.AreEqual(3, grid.vertexAxis1Count);

			Assert.AreEqual(15, grid.faceCount);
			Assert.AreEqual(15, grid.internalFaceCount);
			Assert.AreEqual(0, grid.externalFaceCount);
			Assert.AreEqual(60, grid.edgeCount);
			Assert.AreEqual(15, grid.vertexCount);
		}

		[Test]
		public static void ValidateTallFullyWrappedGridElementCounts()
		{
			var grid = RectangularQuadGrid.Create(new PlanarDescriptor(Vector3.right, true, Vector3.up, true), new Index2D(3, 5));

			Assert.AreEqual(3, grid.faceAxis0Count);
			Assert.AreEqual(5, grid.faceAxis1Count);
			Assert.AreEqual(3, grid.vertexAxis0Count);
			Assert.AreEqual(5, grid.vertexAxis1Count);

			Assert.AreEqual(15, grid.faceCount);
			Assert.AreEqual(15, grid.internalFaceCount);
			Assert.AreEqual(0, grid.externalFaceCount);
			Assert.AreEqual(60, grid.edgeCount);
			Assert.AreEqual(15, grid.vertexCount);
		}

		[Test]
		public static void ValidateSquareFullyWrappedGridElementCounts()
		{
			var grid = RectangularQuadGrid.Create(new PlanarDescriptor(Vector3.right, true, Vector3.up, true), new Index2D(4, 4));

			Assert.AreEqual(4, grid.faceAxis0Count);
			Assert.AreEqual(4, grid.faceAxis1Count);
			Assert.AreEqual(4, grid.vertexAxis0Count);
			Assert.AreEqual(4, grid.vertexAxis1Count);

			Assert.AreEqual(16, grid.faceCount);
			Assert.AreEqual(16, grid.internalFaceCount);
			Assert.AreEqual(0, grid.externalFaceCount);
			Assert.AreEqual(64, grid.edgeCount);
			Assert.AreEqual(16, grid.vertexCount);
		}
	}
}
#endif
