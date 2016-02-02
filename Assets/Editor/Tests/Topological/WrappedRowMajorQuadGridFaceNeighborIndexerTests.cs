using UnityEngine;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Experilous.Topological.Tests
{
	public class WrappedRowMajorQuadGridFaceNeighborIndexerTests
	{
		[Test]
		public static void ValidateWideHorizontallyWrappedGridElementCounts()
		{
			var indexer = WrappedRowMajorQuadGridFaceNeighborIndexer.CreateInstance(5, 3, true, false);

			Assert.AreEqual(5, indexer.faceColumnCount);
			Assert.AreEqual(3, indexer.faceRowCount);
			Assert.AreEqual(5, indexer.vertexColumnCount);
			Assert.AreEqual(4, indexer.vertexRowCount);

			Assert.AreEqual(17, indexer.faceCount);
			Assert.AreEqual(15, indexer.internalFaceCount);
			Assert.AreEqual(2, indexer.externalFaceCount);
			Assert.AreEqual(70, indexer.edgeCount);
			Assert.AreEqual(60, indexer.internalEdgeCount);
			Assert.AreEqual(10, indexer.externalEdgeCount);
			Assert.AreEqual(20, indexer.vertexCount);
		}

		[Test]
		public static void ValidateTallHorizontallyWrappedGridElementCounts()
		{
			var indexer = WrappedRowMajorQuadGridFaceNeighborIndexer.CreateInstance(3, 5, true, false);

			Assert.AreEqual(3, indexer.faceColumnCount);
			Assert.AreEqual(5, indexer.faceRowCount);
			Assert.AreEqual(3, indexer.vertexColumnCount);
			Assert.AreEqual(6, indexer.vertexRowCount);

			Assert.AreEqual(17, indexer.faceCount);
			Assert.AreEqual(15, indexer.internalFaceCount);
			Assert.AreEqual(2, indexer.externalFaceCount);
			Assert.AreEqual(66, indexer.edgeCount);
			Assert.AreEqual(60, indexer.internalEdgeCount);
			Assert.AreEqual(6, indexer.externalEdgeCount);
			Assert.AreEqual(18, indexer.vertexCount);
		}

		[Test]
		public static void ValidateSquareHorizontallyWrappedGridElementCounts()
		{
			var indexer = WrappedRowMajorQuadGridFaceNeighborIndexer.CreateInstance(4, 4, true, false);

			Assert.AreEqual(4, indexer.faceColumnCount);
			Assert.AreEqual(4, indexer.faceRowCount);
			Assert.AreEqual(4, indexer.vertexColumnCount);
			Assert.AreEqual(5, indexer.vertexRowCount);

			Assert.AreEqual(18, indexer.faceCount);
			Assert.AreEqual(16, indexer.internalFaceCount);
			Assert.AreEqual(2, indexer.externalFaceCount);
			Assert.AreEqual(72, indexer.edgeCount);
			Assert.AreEqual(64, indexer.internalEdgeCount);
			Assert.AreEqual(8, indexer.externalEdgeCount);
			Assert.AreEqual(20, indexer.vertexCount);
		}

		[Test]
		public static void ValidateWideVerticallyWrappedGridElementCounts()
		{
			var indexer = WrappedRowMajorQuadGridFaceNeighborIndexer.CreateInstance(5, 3, false, true);

			Assert.AreEqual(5, indexer.faceColumnCount);
			Assert.AreEqual(3, indexer.faceRowCount);
			Assert.AreEqual(6, indexer.vertexColumnCount);
			Assert.AreEqual(3, indexer.vertexRowCount);

			Assert.AreEqual(17, indexer.faceCount);
			Assert.AreEqual(15, indexer.internalFaceCount);
			Assert.AreEqual(2, indexer.externalFaceCount);
			Assert.AreEqual(66, indexer.edgeCount);
			Assert.AreEqual(60, indexer.internalEdgeCount);
			Assert.AreEqual(6, indexer.externalEdgeCount);
			Assert.AreEqual(18, indexer.vertexCount);
		}

		[Test]
		public static void ValidateTallVerticallyWrappedGridElementCounts()
		{
			var indexer = WrappedRowMajorQuadGridFaceNeighborIndexer.CreateInstance(3, 5, false, true);

			Assert.AreEqual(3, indexer.faceColumnCount);
			Assert.AreEqual(5, indexer.faceRowCount);
			Assert.AreEqual(4, indexer.vertexColumnCount);
			Assert.AreEqual(5, indexer.vertexRowCount);

			Assert.AreEqual(17, indexer.faceCount);
			Assert.AreEqual(15, indexer.internalFaceCount);
			Assert.AreEqual(2, indexer.externalFaceCount);
			Assert.AreEqual(70, indexer.edgeCount);
			Assert.AreEqual(60, indexer.internalEdgeCount);
			Assert.AreEqual(10, indexer.externalEdgeCount);
			Assert.AreEqual(20, indexer.vertexCount);
		}

		[Test]
		public static void ValidateSquareVerticallyWrappedGridElementCounts()
		{
			var indexer = WrappedRowMajorQuadGridFaceNeighborIndexer.CreateInstance(4, 4, false, true);

			Assert.AreEqual(4, indexer.faceColumnCount);
			Assert.AreEqual(4, indexer.faceRowCount);
			Assert.AreEqual(5, indexer.vertexColumnCount);
			Assert.AreEqual(4, indexer.vertexRowCount);

			Assert.AreEqual(18, indexer.faceCount);
			Assert.AreEqual(16, indexer.internalFaceCount);
			Assert.AreEqual(2, indexer.externalFaceCount);
			Assert.AreEqual(72, indexer.edgeCount);
			Assert.AreEqual(64, indexer.internalEdgeCount);
			Assert.AreEqual(8, indexer.externalEdgeCount);
			Assert.AreEqual(20, indexer.vertexCount);
		}

		[Test]
		public static void ValidateWideFullyWrappedGridElementCounts()
		{
			var indexer = WrappedRowMajorQuadGridFaceNeighborIndexer.CreateInstance(5, 3, true, true);

			Assert.AreEqual(5, indexer.faceColumnCount);
			Assert.AreEqual(3, indexer.faceRowCount);
			Assert.AreEqual(5, indexer.vertexColumnCount);
			Assert.AreEqual(3, indexer.vertexRowCount);

			Assert.AreEqual(15, indexer.faceCount);
			Assert.AreEqual(15, indexer.internalFaceCount);
			Assert.AreEqual(0, indexer.externalFaceCount);
			Assert.AreEqual(60, indexer.edgeCount);
			Assert.AreEqual(60, indexer.internalEdgeCount);
			Assert.AreEqual(0, indexer.externalEdgeCount);
			Assert.AreEqual(15, indexer.vertexCount);
		}

		[Test]
		public static void ValidateTallFullyWrappedGridElementCounts()
		{
			var indexer = WrappedRowMajorQuadGridFaceNeighborIndexer.CreateInstance(3, 5, true, true);

			Assert.AreEqual(3, indexer.faceColumnCount);
			Assert.AreEqual(5, indexer.faceRowCount);
			Assert.AreEqual(3, indexer.vertexColumnCount);
			Assert.AreEqual(5, indexer.vertexRowCount);

			Assert.AreEqual(15, indexer.faceCount);
			Assert.AreEqual(15, indexer.internalFaceCount);
			Assert.AreEqual(0, indexer.externalFaceCount);
			Assert.AreEqual(60, indexer.edgeCount);
			Assert.AreEqual(60, indexer.internalEdgeCount);
			Assert.AreEqual(0, indexer.externalEdgeCount);
			Assert.AreEqual(15, indexer.vertexCount);
		}

		[Test]
		public static void ValidateSquareFullyWrappedGridElementCounts()
		{
			var indexer = WrappedRowMajorQuadGridFaceNeighborIndexer.CreateInstance(4, 4, true, true);

			Assert.AreEqual(4, indexer.faceColumnCount);
			Assert.AreEqual(4, indexer.faceRowCount);
			Assert.AreEqual(4, indexer.vertexColumnCount);
			Assert.AreEqual(4, indexer.vertexRowCount);

			Assert.AreEqual(16, indexer.faceCount);
			Assert.AreEqual(16, indexer.internalFaceCount);
			Assert.AreEqual(0, indexer.externalFaceCount);
			Assert.AreEqual(64, indexer.edgeCount);
			Assert.AreEqual(64, indexer.internalEdgeCount);
			Assert.AreEqual(0, indexer.externalEdgeCount);
			Assert.AreEqual(16, indexer.vertexCount);
		}

		[Test]
		public static void ValidateWideHorizontallyWrappedGridExternalFaceNeighbors()
		{
			var indexer = WrappedRowMajorQuadGridFaceNeighborIndexer.CreateInstance(5, 3, true, false);

			Assert.AreEqual(4, indexer.GetNeighborCount(0));
			Assert.AreEqual(4, indexer.GetNeighborCount(1));
			Assert.AreEqual(4, indexer.GetNeighborCount(2));
			Assert.AreEqual(4, indexer.GetNeighborCount(3));
			Assert.AreEqual(4, indexer.GetNeighborCount(4));

			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(0, 0));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(1, 0));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(2, 0));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(3, 0));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(4, 0));

			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(0, 0));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(1, 0));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(2, 0));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(3, 0));
			Assert.AreEqual(4, indexer.GetInverseNeighborIndex(4, 0));

			Assert.AreEqual(4, indexer.GetNeighborCount(14));
			Assert.AreEqual(4, indexer.GetNeighborCount(13));
			Assert.AreEqual(4, indexer.GetNeighborCount(12));
			Assert.AreEqual(4, indexer.GetNeighborCount(11));
			Assert.AreEqual(4, indexer.GetNeighborCount(10));

			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(14, 2));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(13, 2));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(12, 2));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(11, 2));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(10, 2));

			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(14, 2));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(13, 2));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(12, 2));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(11, 2));
			Assert.AreEqual(4, indexer.GetInverseNeighborIndex(10, 2));

			Assert.AreEqual(5, indexer.GetNeighborCount(15));

			Assert.AreEqual(0, indexer.GetNeighborFaceIndex(15, 0));
			Assert.AreEqual(1, indexer.GetNeighborFaceIndex(15, 1));
			Assert.AreEqual(2, indexer.GetNeighborFaceIndex(15, 2));
			Assert.AreEqual(3, indexer.GetNeighborFaceIndex(15, 3));
			Assert.AreEqual(4, indexer.GetNeighborFaceIndex(15, 4));

			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(15, 0));
			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(15, 1));
			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(15, 2));
			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(15, 3));
			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(15, 4));

			Assert.AreEqual(5, indexer.GetNeighborCount(16));

			Assert.AreEqual(14, indexer.GetNeighborFaceIndex(16, 0));
			Assert.AreEqual(13, indexer.GetNeighborFaceIndex(16, 1));
			Assert.AreEqual(12, indexer.GetNeighborFaceIndex(16, 2));
			Assert.AreEqual(11, indexer.GetNeighborFaceIndex(16, 3));
			Assert.AreEqual(10, indexer.GetNeighborFaceIndex(16, 4));

			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(16, 0));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(16, 1));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(16, 2));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(16, 3));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(16, 4));
		}

		[Test]
		public static void ValidateTallHorizontallyWrappedGridExternalFaceNeighbors()
		{
			var indexer = WrappedRowMajorQuadGridFaceNeighborIndexer.CreateInstance(3, 5, true, false);

			Assert.AreEqual(4, indexer.GetNeighborCount(0));
			Assert.AreEqual(4, indexer.GetNeighborCount(1));
			Assert.AreEqual(4, indexer.GetNeighborCount(2));

			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(0, 0));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(1, 0));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(2, 0));

			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(0, 0));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(1, 0));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(2, 0));

			Assert.AreEqual(4, indexer.GetNeighborCount(14));
			Assert.AreEqual(4, indexer.GetNeighborCount(13));
			Assert.AreEqual(4, indexer.GetNeighborCount(12));

			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(14, 2));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(13, 2));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(12, 2));

			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(14, 2));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(13, 2));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(12, 2));

			Assert.AreEqual(3, indexer.GetNeighborCount(15));

			Assert.AreEqual(0, indexer.GetNeighborFaceIndex(15, 0));
			Assert.AreEqual(1, indexer.GetNeighborFaceIndex(15, 1));
			Assert.AreEqual(2, indexer.GetNeighborFaceIndex(15, 2));

			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(15, 0));
			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(15, 1));
			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(15, 2));

			Assert.AreEqual(3, indexer.GetNeighborCount(16));

			Assert.AreEqual(14, indexer.GetNeighborFaceIndex(16, 0));
			Assert.AreEqual(13, indexer.GetNeighborFaceIndex(16, 1));
			Assert.AreEqual(12, indexer.GetNeighborFaceIndex(16, 2));

			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(16, 0));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(16, 1));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(16, 2));
		}

		[Test]
		public static void ValidateSquareHorizontallyWrappedGridExternalFaceNeighbors()
		{
			var indexer = WrappedRowMajorQuadGridFaceNeighborIndexer.CreateInstance(4, 4, true, false);

			Assert.AreEqual(4, indexer.GetNeighborCount(0));
			Assert.AreEqual(4, indexer.GetNeighborCount(1));
			Assert.AreEqual(4, indexer.GetNeighborCount(2));
			Assert.AreEqual(4, indexer.GetNeighborCount(3));

			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(0, 0));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(1, 0));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(2, 0));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(3, 0));

			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(0, 0));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(1, 0));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(2, 0));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(3, 0));

			Assert.AreEqual(4, indexer.GetNeighborCount(15));
			Assert.AreEqual(4, indexer.GetNeighborCount(14));
			Assert.AreEqual(4, indexer.GetNeighborCount(13));
			Assert.AreEqual(4, indexer.GetNeighborCount(12));

			Assert.AreEqual(17, indexer.GetNeighborFaceIndex(15, 2));
			Assert.AreEqual(17, indexer.GetNeighborFaceIndex(14, 2));
			Assert.AreEqual(17, indexer.GetNeighborFaceIndex(13, 2));
			Assert.AreEqual(17, indexer.GetNeighborFaceIndex(12, 2));

			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(15, 2));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(14, 2));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(13, 2));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(12, 2));

			Assert.AreEqual(4, indexer.GetNeighborCount(16));

			Assert.AreEqual(0, indexer.GetNeighborFaceIndex(16, 0));
			Assert.AreEqual(1, indexer.GetNeighborFaceIndex(16, 1));
			Assert.AreEqual(2, indexer.GetNeighborFaceIndex(16, 2));
			Assert.AreEqual(3, indexer.GetNeighborFaceIndex(16, 3));

			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(16, 0));
			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(16, 1));
			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(16, 2));
			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(16, 3));

			Assert.AreEqual(4, indexer.GetNeighborCount(17));

			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(17, 0));
			Assert.AreEqual(14, indexer.GetNeighborFaceIndex(17, 1));
			Assert.AreEqual(13, indexer.GetNeighborFaceIndex(17, 2));
			Assert.AreEqual(12, indexer.GetNeighborFaceIndex(17, 3));

			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(17, 0));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(17, 1));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(17, 2));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(17, 3));
		}

		[Test]
		public static void ValidateWideVerticallyWrappedGridExternalFaceNeighbors()
		{
			var indexer = WrappedRowMajorQuadGridFaceNeighborIndexer.CreateInstance(5, 3, false, true);

			Assert.AreEqual(4, indexer.GetNeighborCount(10));
			Assert.AreEqual(4, indexer.GetNeighborCount(5));
			Assert.AreEqual(4, indexer.GetNeighborCount(0));

			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(10, 1));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(5, 1));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(0, 1));

			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(10, 1));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(5, 1));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(0, 1));

			Assert.AreEqual(4, indexer.GetNeighborCount(4));
			Assert.AreEqual(4, indexer.GetNeighborCount(9));
			Assert.AreEqual(4, indexer.GetNeighborCount(14));

			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(4, 3));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(9, 3));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(14, 3));

			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(4, 3));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(9, 3));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(14, 3));

			Assert.AreEqual(3, indexer.GetNeighborCount(15));

			Assert.AreEqual(10, indexer.GetNeighborFaceIndex(15, 0));
			Assert.AreEqual(5, indexer.GetNeighborFaceIndex(15, 1));
			Assert.AreEqual(0, indexer.GetNeighborFaceIndex(15, 2));

			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(15, 0));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(15, 1));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(15, 2));

			Assert.AreEqual(3, indexer.GetNeighborCount(16));

			Assert.AreEqual(4, indexer.GetNeighborFaceIndex(16, 0));
			Assert.AreEqual(9, indexer.GetNeighborFaceIndex(16, 1));
			Assert.AreEqual(14, indexer.GetNeighborFaceIndex(16, 2));

			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(16, 0));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(16, 1));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(16, 2));
		}

		[Test]
		public static void ValidateTallVerticallyWrappedGridExternalFaceNeighbors()
		{
			var indexer = WrappedRowMajorQuadGridFaceNeighborIndexer.CreateInstance(3, 5, false, true);

			Assert.AreEqual(4, indexer.GetNeighborCount(12));
			Assert.AreEqual(4, indexer.GetNeighborCount(9));
			Assert.AreEqual(4, indexer.GetNeighborCount(6));
			Assert.AreEqual(4, indexer.GetNeighborCount(3));
			Assert.AreEqual(4, indexer.GetNeighborCount(0));

			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(12, 1));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(9, 1));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(6, 1));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(3, 1));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(0, 1));

			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(12, 1));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(9, 1));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(6, 1));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(3, 1));
			Assert.AreEqual(4, indexer.GetInverseNeighborIndex(0, 1));

			Assert.AreEqual(4, indexer.GetNeighborCount(14));
			Assert.AreEqual(4, indexer.GetNeighborCount(11));
			Assert.AreEqual(4, indexer.GetNeighborCount(8));
			Assert.AreEqual(4, indexer.GetNeighborCount(5));
			Assert.AreEqual(4, indexer.GetNeighborCount(2));

			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(2, 3));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(5, 3));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(8, 3));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(11, 3));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(14, 3));

			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(2, 3));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(5, 3));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(8, 3));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(11, 3));
			Assert.AreEqual(4, indexer.GetInverseNeighborIndex(14, 3));

			Assert.AreEqual(5, indexer.GetNeighborCount(15));

			Assert.AreEqual(12, indexer.GetNeighborFaceIndex(15, 0));
			Assert.AreEqual(9, indexer.GetNeighborFaceIndex(15, 1));
			Assert.AreEqual(6, indexer.GetNeighborFaceIndex(15, 2));
			Assert.AreEqual(3, indexer.GetNeighborFaceIndex(15, 3));
			Assert.AreEqual(0, indexer.GetNeighborFaceIndex(15, 4));

			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(15, 0));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(15, 1));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(15, 2));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(15, 3));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(15, 4));

			Assert.AreEqual(5, indexer.GetNeighborCount(16));

			Assert.AreEqual(2, indexer.GetNeighborFaceIndex(16, 0));
			Assert.AreEqual(5, indexer.GetNeighborFaceIndex(16, 1));
			Assert.AreEqual(8, indexer.GetNeighborFaceIndex(16, 2));
			Assert.AreEqual(11, indexer.GetNeighborFaceIndex(16, 3));
			Assert.AreEqual(14, indexer.GetNeighborFaceIndex(16, 4));

			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(16, 0));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(16, 1));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(16, 2));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(16, 3));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(16, 4));
		}

		[Test]
		public static void ValidateSquareVerticallyWrappedGridExternalFaceNeighbors()
		{
			var indexer = WrappedRowMajorQuadGridFaceNeighborIndexer.CreateInstance(4, 4, false, true);

			Assert.AreEqual(4, indexer.GetNeighborCount(12));
			Assert.AreEqual(4, indexer.GetNeighborCount(8));
			Assert.AreEqual(4, indexer.GetNeighborCount(4));
			Assert.AreEqual(4, indexer.GetNeighborCount(0));

			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(12, 1));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(8, 1));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(4, 1));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(0, 1));

			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(12, 1));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(8, 1));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(4, 1));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(0, 1));

			Assert.AreEqual(4, indexer.GetNeighborCount(3));
			Assert.AreEqual(4, indexer.GetNeighborCount(7));
			Assert.AreEqual(4, indexer.GetNeighborCount(11));
			Assert.AreEqual(4, indexer.GetNeighborCount(15));

			Assert.AreEqual(17, indexer.GetNeighborFaceIndex(3, 3));
			Assert.AreEqual(17, indexer.GetNeighborFaceIndex(7, 3));
			Assert.AreEqual(17, indexer.GetNeighborFaceIndex(11, 3));
			Assert.AreEqual(17, indexer.GetNeighborFaceIndex(15, 3));

			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(3, 3));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(7, 3));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(11, 3));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(15, 3));

			Assert.AreEqual(4, indexer.GetNeighborCount(16));

			Assert.AreEqual(12, indexer.GetNeighborFaceIndex(16, 0));
			Assert.AreEqual(8, indexer.GetNeighborFaceIndex(16, 1));
			Assert.AreEqual(4, indexer.GetNeighborFaceIndex(16, 2));
			Assert.AreEqual(0, indexer.GetNeighborFaceIndex(16, 3));

			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(16, 0));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(16, 1));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(16, 2));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(16, 3));

			Assert.AreEqual(4, indexer.GetNeighborCount(17));

			Assert.AreEqual(3, indexer.GetNeighborFaceIndex(17, 0));
			Assert.AreEqual(7, indexer.GetNeighborFaceIndex(17, 1));
			Assert.AreEqual(11, indexer.GetNeighborFaceIndex(17, 2));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(17, 3));

			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(17, 0));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(17, 1));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(17, 2));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(17, 3));
		}

		[Test]
		public static void ValidateSquareHorizontallyWrappedGridEdgeIndices()
		{
			var indexer = WrappedRowMajorQuadGridFaceNeighborIndexer.CreateInstance(3, 3, true, false);

			Assert.AreEqual(42, indexer.edgeCount);
			Assert.AreEqual(9, indexer.internalFaceCount);
			Assert.AreEqual(2, indexer.externalFaceCount);

			Assert.AreEqual(1, indexer.GetNeighborEdgeIndex(0, 0));
			Assert.AreEqual(2, indexer.GetNeighborEdgeIndex(0, 2));
			Assert.AreEqual(3, indexer.GetNeighborEdgeIndex(3, 0));
			Assert.AreEqual(4, indexer.GetNeighborEdgeIndex(3, 2));
			Assert.AreEqual(5, indexer.GetNeighborEdgeIndex(6, 0));
			Assert.AreEqual(6, indexer.GetNeighborEdgeIndex(6, 2));

			Assert.AreEqual(9, indexer.GetNeighborEdgeIndex(1, 0));
			Assert.AreEqual(10, indexer.GetNeighborEdgeIndex(1, 2));
			Assert.AreEqual(11, indexer.GetNeighborEdgeIndex(4, 0));
			Assert.AreEqual(12, indexer.GetNeighborEdgeIndex(4, 2));
			Assert.AreEqual(13, indexer.GetNeighborEdgeIndex(7, 0));
			Assert.AreEqual(14, indexer.GetNeighborEdgeIndex(7, 2));

			Assert.AreEqual(17, indexer.GetNeighborEdgeIndex(2, 0));
			Assert.AreEqual(18, indexer.GetNeighborEdgeIndex(2, 2));
			Assert.AreEqual(19, indexer.GetNeighborEdgeIndex(5, 0));
			Assert.AreEqual(20, indexer.GetNeighborEdgeIndex(5, 2));
			Assert.AreEqual(21, indexer.GetNeighborEdgeIndex(8, 0));
			Assert.AreEqual(22, indexer.GetNeighborEdgeIndex(8, 2));

			Assert.AreEqual(25, indexer.GetNeighborEdgeIndex(0, 1));
			Assert.AreEqual(26, indexer.GetNeighborEdgeIndex(0, 3));
			Assert.AreEqual(27, indexer.GetNeighborEdgeIndex(1, 1));
			Assert.AreEqual(28, indexer.GetNeighborEdgeIndex(1, 3));
			Assert.AreEqual(29, indexer.GetNeighborEdgeIndex(2, 1));
			Assert.AreEqual(24, indexer.GetNeighborEdgeIndex(2, 3));

			Assert.AreEqual(31, indexer.GetNeighborEdgeIndex(3, 1));
			Assert.AreEqual(32, indexer.GetNeighborEdgeIndex(3, 3));
			Assert.AreEqual(33, indexer.GetNeighborEdgeIndex(4, 1));
			Assert.AreEqual(34, indexer.GetNeighborEdgeIndex(4, 3));
			Assert.AreEqual(35, indexer.GetNeighborEdgeIndex(5, 1));
			Assert.AreEqual(30, indexer.GetNeighborEdgeIndex(5, 3));

			Assert.AreEqual(37, indexer.GetNeighborEdgeIndex(6, 1));
			Assert.AreEqual(38, indexer.GetNeighborEdgeIndex(6, 3));
			Assert.AreEqual(39, indexer.GetNeighborEdgeIndex(7, 1));
			Assert.AreEqual(40, indexer.GetNeighborEdgeIndex(7, 3));
			Assert.AreEqual(41, indexer.GetNeighborEdgeIndex(8, 1));
			Assert.AreEqual(36, indexer.GetNeighborEdgeIndex(8, 3));

			Assert.AreEqual(0, indexer.GetNeighborEdgeIndex(9, 0));
			Assert.AreEqual(8, indexer.GetNeighborEdgeIndex(9, 1));
			Assert.AreEqual(16, indexer.GetNeighborEdgeIndex(9, 2));

			Assert.AreEqual(23, indexer.GetNeighborEdgeIndex(10, 0));
			Assert.AreEqual(15, indexer.GetNeighborEdgeIndex(10, 1));
			Assert.AreEqual(7, indexer.GetNeighborEdgeIndex(10, 2));
		}

		[Test]
		public static void ValidateSquareVerticallyWrappedGridEdgeIndices()
		{
			var indexer = WrappedRowMajorQuadGridFaceNeighborIndexer.CreateInstance(3, 3, false, true);

			Assert.AreEqual(42, indexer.edgeCount);
			Assert.AreEqual(9, indexer.internalFaceCount);
			Assert.AreEqual(2, indexer.externalFaceCount);

			Assert.AreEqual(1, indexer.GetNeighborEdgeIndex(0, 0));
			Assert.AreEqual(2, indexer.GetNeighborEdgeIndex(0, 2));
			Assert.AreEqual(3, indexer.GetNeighborEdgeIndex(3, 0));
			Assert.AreEqual(4, indexer.GetNeighborEdgeIndex(3, 2));
			Assert.AreEqual(5, indexer.GetNeighborEdgeIndex(6, 0));
			Assert.AreEqual(0, indexer.GetNeighborEdgeIndex(6, 2));

			Assert.AreEqual(7, indexer.GetNeighborEdgeIndex(1, 0));
			Assert.AreEqual(8, indexer.GetNeighborEdgeIndex(1, 2));
			Assert.AreEqual(9, indexer.GetNeighborEdgeIndex(4, 0));
			Assert.AreEqual(10, indexer.GetNeighborEdgeIndex(4, 2));
			Assert.AreEqual(11, indexer.GetNeighborEdgeIndex(7, 0));
			Assert.AreEqual(6, indexer.GetNeighborEdgeIndex(7, 2));

			Assert.AreEqual(13, indexer.GetNeighborEdgeIndex(2, 0));
			Assert.AreEqual(14, indexer.GetNeighborEdgeIndex(2, 2));
			Assert.AreEqual(15, indexer.GetNeighborEdgeIndex(5, 0));
			Assert.AreEqual(16, indexer.GetNeighborEdgeIndex(5, 2));
			Assert.AreEqual(17, indexer.GetNeighborEdgeIndex(8, 0));
			Assert.AreEqual(12, indexer.GetNeighborEdgeIndex(8, 2));

			Assert.AreEqual(19, indexer.GetNeighborEdgeIndex(0, 1));
			Assert.AreEqual(20, indexer.GetNeighborEdgeIndex(0, 3));
			Assert.AreEqual(21, indexer.GetNeighborEdgeIndex(1, 1));
			Assert.AreEqual(22, indexer.GetNeighborEdgeIndex(1, 3));
			Assert.AreEqual(23, indexer.GetNeighborEdgeIndex(2, 1));
			Assert.AreEqual(24, indexer.GetNeighborEdgeIndex(2, 3));

			Assert.AreEqual(27, indexer.GetNeighborEdgeIndex(3, 1));
			Assert.AreEqual(28, indexer.GetNeighborEdgeIndex(3, 3));
			Assert.AreEqual(29, indexer.GetNeighborEdgeIndex(4, 1));
			Assert.AreEqual(30, indexer.GetNeighborEdgeIndex(4, 3));
			Assert.AreEqual(31, indexer.GetNeighborEdgeIndex(5, 1));
			Assert.AreEqual(32, indexer.GetNeighborEdgeIndex(5, 3));

			Assert.AreEqual(35, indexer.GetNeighborEdgeIndex(6, 1));
			Assert.AreEqual(36, indexer.GetNeighborEdgeIndex(6, 3));
			Assert.AreEqual(37, indexer.GetNeighborEdgeIndex(7, 1));
			Assert.AreEqual(38, indexer.GetNeighborEdgeIndex(7, 3));
			Assert.AreEqual(39, indexer.GetNeighborEdgeIndex(8, 1));
			Assert.AreEqual(40, indexer.GetNeighborEdgeIndex(8, 3));

			Assert.AreEqual(34, indexer.GetNeighborEdgeIndex(9, 0));
			Assert.AreEqual(26, indexer.GetNeighborEdgeIndex(9, 1));
			Assert.AreEqual(18, indexer.GetNeighborEdgeIndex(9, 2));

			Assert.AreEqual(25, indexer.GetNeighborEdgeIndex(10, 0));
			Assert.AreEqual(33, indexer.GetNeighborEdgeIndex(10, 1));
			Assert.AreEqual(41, indexer.GetNeighborEdgeIndex(10, 2));
		}

		[Test]
		public static void ValidateSquareFullyWrappedGridEdgeIndices()
		{
			var indexer = WrappedRowMajorQuadGridFaceNeighborIndexer.CreateInstance(3, 3, true, true);

			Assert.AreEqual(36, indexer.edgeCount);
			Assert.AreEqual(9, indexer.internalFaceCount);
			Assert.AreEqual(0, indexer.externalFaceCount);

			Assert.AreEqual(1, indexer.GetNeighborEdgeIndex(0, 0));
			Assert.AreEqual(2, indexer.GetNeighborEdgeIndex(0, 2));
			Assert.AreEqual(3, indexer.GetNeighborEdgeIndex(3, 0));
			Assert.AreEqual(4, indexer.GetNeighborEdgeIndex(3, 2));
			Assert.AreEqual(5, indexer.GetNeighborEdgeIndex(6, 0));
			Assert.AreEqual(0, indexer.GetNeighborEdgeIndex(6, 2));

			Assert.AreEqual(7, indexer.GetNeighborEdgeIndex(1, 0));
			Assert.AreEqual(8, indexer.GetNeighborEdgeIndex(1, 2));
			Assert.AreEqual(9, indexer.GetNeighborEdgeIndex(4, 0));
			Assert.AreEqual(10, indexer.GetNeighborEdgeIndex(4, 2));
			Assert.AreEqual(11, indexer.GetNeighborEdgeIndex(7, 0));
			Assert.AreEqual(6, indexer.GetNeighborEdgeIndex(7, 2));

			Assert.AreEqual(13, indexer.GetNeighborEdgeIndex(2, 0));
			Assert.AreEqual(14, indexer.GetNeighborEdgeIndex(2, 2));
			Assert.AreEqual(15, indexer.GetNeighborEdgeIndex(5, 0));
			Assert.AreEqual(16, indexer.GetNeighborEdgeIndex(5, 2));
			Assert.AreEqual(17, indexer.GetNeighborEdgeIndex(8, 0));
			Assert.AreEqual(12, indexer.GetNeighborEdgeIndex(8, 2));

			Assert.AreEqual(19, indexer.GetNeighborEdgeIndex(0, 1));
			Assert.AreEqual(20, indexer.GetNeighborEdgeIndex(0, 3));
			Assert.AreEqual(21, indexer.GetNeighborEdgeIndex(1, 1));
			Assert.AreEqual(22, indexer.GetNeighborEdgeIndex(1, 3));
			Assert.AreEqual(23, indexer.GetNeighborEdgeIndex(2, 1));
			Assert.AreEqual(18, indexer.GetNeighborEdgeIndex(2, 3));

			Assert.AreEqual(25, indexer.GetNeighborEdgeIndex(3, 1));
			Assert.AreEqual(26, indexer.GetNeighborEdgeIndex(3, 3));
			Assert.AreEqual(27, indexer.GetNeighborEdgeIndex(4, 1));
			Assert.AreEqual(28, indexer.GetNeighborEdgeIndex(4, 3));
			Assert.AreEqual(29, indexer.GetNeighborEdgeIndex(5, 1));
			Assert.AreEqual(24, indexer.GetNeighborEdgeIndex(5, 3));

			Assert.AreEqual(31, indexer.GetNeighborEdgeIndex(6, 1));
			Assert.AreEqual(32, indexer.GetNeighborEdgeIndex(6, 3));
			Assert.AreEqual(33, indexer.GetNeighborEdgeIndex(7, 1));
			Assert.AreEqual(34, indexer.GetNeighborEdgeIndex(7, 3));
			Assert.AreEqual(35, indexer.GetNeighborEdgeIndex(8, 1));
			Assert.AreEqual(30, indexer.GetNeighborEdgeIndex(8, 3));
		}
	}
}
