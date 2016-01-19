using UnityEngine;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Experilous.Topological.Tests
{
	public class RowMajorQuadGridFaceNeighborIndexerTests
	{
		[Test]
		public static void ValidateWideGridElementCounts()
		{
			var indexer = RowMajorQuadGridFaceNeighborIndexer.CreateInstance(5, 3);

			Assert.AreEqual(5, indexer.faceColumnCount);
			Assert.AreEqual(3, indexer.faceRowCount);
			Assert.AreEqual(6, indexer.vertexColumnCount);
			Assert.AreEqual(4, indexer.vertexRowCount);

			Assert.AreEqual(16, indexer.faceCount);
			Assert.AreEqual(15, indexer.internalFaceCount);
			Assert.AreEqual(1, indexer.externalFaceCount);
			Assert.AreEqual(76, indexer.edgeCount);
			Assert.AreEqual(60, indexer.internalEdgeCount);
			Assert.AreEqual(16, indexer.externalEdgeCount);
			Assert.AreEqual(24, indexer.vertexCount);
		}

		[Test]
		public static void ValidateTallGridElementCounts()
		{
			var indexer = RowMajorQuadGridFaceNeighborIndexer.CreateInstance(3, 5);

			Assert.AreEqual(3, indexer.faceColumnCount);
			Assert.AreEqual(5, indexer.faceRowCount);
			Assert.AreEqual(4, indexer.vertexColumnCount);
			Assert.AreEqual(6, indexer.vertexRowCount);

			Assert.AreEqual(16, indexer.faceCount);
			Assert.AreEqual(15, indexer.internalFaceCount);
			Assert.AreEqual(1, indexer.externalFaceCount);
			Assert.AreEqual(76, indexer.edgeCount);
			Assert.AreEqual(60, indexer.internalEdgeCount);
			Assert.AreEqual(16, indexer.externalEdgeCount);
			Assert.AreEqual(24, indexer.vertexCount);
		}

		[Test]
		public static void ValidateSquareGridElementCounts()
		{
			var indexer = RowMajorQuadGridFaceNeighborIndexer.CreateInstance(4, 4);

			Assert.AreEqual(4, indexer.faceColumnCount);
			Assert.AreEqual(4, indexer.faceRowCount);
			Assert.AreEqual(5, indexer.vertexColumnCount);
			Assert.AreEqual(5, indexer.vertexRowCount);

			Assert.AreEqual(17, indexer.faceCount);
			Assert.AreEqual(16, indexer.internalFaceCount);
			Assert.AreEqual(1, indexer.externalFaceCount);
			Assert.AreEqual(80, indexer.edgeCount);
			Assert.AreEqual(64, indexer.internalEdgeCount);
			Assert.AreEqual(16, indexer.externalEdgeCount);
			Assert.AreEqual(25, indexer.vertexCount);
		}

		[Test]
		public static void ValidateWideGridExternalFaceNeighbors()
		{
			var indexer = RowMajorQuadGridFaceNeighborIndexer.CreateInstance(5, 3);

			Assert.AreEqual(4, indexer.GetNeighborCount(0));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(0, 0));
			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(0, 0));

			Assert.AreEqual(4, indexer.GetNeighborCount(1));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(1, 0));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(1, 0));

			Assert.AreEqual(4, indexer.GetNeighborCount(2));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(2, 0));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(2, 0));

			Assert.AreEqual(4, indexer.GetNeighborCount(3));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(3, 0));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(3, 0));

			Assert.AreEqual(4, indexer.GetNeighborCount(4));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(4, 0));
			Assert.AreEqual(4, indexer.GetInverseNeighborIndex(4, 0));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(4, 3));
			Assert.AreEqual(5, indexer.GetInverseNeighborIndex(4, 3));

			Assert.AreEqual(4, indexer.GetNeighborCount(9));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(9, 3));
			Assert.AreEqual(6, indexer.GetInverseNeighborIndex(9, 3));

			Assert.AreEqual(4, indexer.GetNeighborCount(14));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(14, 3));
			Assert.AreEqual(7, indexer.GetInverseNeighborIndex(14, 3));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(14, 2));
			Assert.AreEqual(8, indexer.GetInverseNeighborIndex(14, 2));

			Assert.AreEqual(4, indexer.GetNeighborCount(13));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(13, 2));
			Assert.AreEqual(9, indexer.GetInverseNeighborIndex(13, 2));

			Assert.AreEqual(4, indexer.GetNeighborCount(12));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(12, 2));
			Assert.AreEqual(10, indexer.GetInverseNeighborIndex(12, 2));

			Assert.AreEqual(4, indexer.GetNeighborCount(11));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(11, 2));
			Assert.AreEqual(11, indexer.GetInverseNeighborIndex(11, 2));

			Assert.AreEqual(4, indexer.GetNeighborCount(10));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(10, 2));
			Assert.AreEqual(12, indexer.GetInverseNeighborIndex(10, 2));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(10, 1));
			Assert.AreEqual(13, indexer.GetInverseNeighborIndex(10, 1));

			Assert.AreEqual(4, indexer.GetNeighborCount(5));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(5, 1));
			Assert.AreEqual(14, indexer.GetInverseNeighborIndex(5, 1));

			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(0, 1));
			Assert.AreEqual(15, indexer.GetInverseNeighborIndex(0, 1));

			Assert.AreEqual(16, indexer.GetNeighborCount(15));

			Assert.AreEqual(0, indexer.GetNeighborFaceIndex(15, 0));
			Assert.AreEqual(1, indexer.GetNeighborFaceIndex(15, 1));
			Assert.AreEqual(2, indexer.GetNeighborFaceIndex(15, 2));
			Assert.AreEqual(3, indexer.GetNeighborFaceIndex(15, 3));
			Assert.AreEqual(4, indexer.GetNeighborFaceIndex(15, 4));
			Assert.AreEqual(4, indexer.GetNeighborFaceIndex(15, 5));
			Assert.AreEqual(9, indexer.GetNeighborFaceIndex(15, 6));
			Assert.AreEqual(14, indexer.GetNeighborFaceIndex(15, 7));
			Assert.AreEqual(14, indexer.GetNeighborFaceIndex(15, 8));
			Assert.AreEqual(13, indexer.GetNeighborFaceIndex(15, 9));
			Assert.AreEqual(12, indexer.GetNeighborFaceIndex(15, 10));
			Assert.AreEqual(11, indexer.GetNeighborFaceIndex(15, 11));
			Assert.AreEqual(10, indexer.GetNeighborFaceIndex(15, 12));
			Assert.AreEqual(10, indexer.GetNeighborFaceIndex(15, 13));
			Assert.AreEqual(5, indexer.GetNeighborFaceIndex(15, 14));
			Assert.AreEqual(0, indexer.GetNeighborFaceIndex(15, 15));

			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(15, 0));
			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(15, 1));
			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(15, 2));
			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(15, 3));
			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(15, 4));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(15, 5));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(15, 6));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(15, 7));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(15, 8));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(15, 9));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(15, 10));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(15, 11));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(15, 12));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(15, 13));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(15, 14));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(15, 15));
		}

		[Test]
		public static void ValidateTallGridExternalFaceNeighbors()
		{
			var indexer = RowMajorQuadGridFaceNeighborIndexer.CreateInstance(3, 5);

			Assert.AreEqual(4, indexer.GetNeighborCount(0));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(0, 0));
			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(0, 0));

			Assert.AreEqual(4, indexer.GetNeighborCount(1));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(1, 0));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(1, 0));

			Assert.AreEqual(4, indexer.GetNeighborCount(2));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(2, 0));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(2, 0));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(2, 3));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(2, 3));

			Assert.AreEqual(4, indexer.GetNeighborCount(5));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(5, 3));
			Assert.AreEqual(4, indexer.GetInverseNeighborIndex(5, 3));

			Assert.AreEqual(4, indexer.GetNeighborCount(8));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(8, 3));
			Assert.AreEqual(5, indexer.GetInverseNeighborIndex(8, 3));

			Assert.AreEqual(4, indexer.GetNeighborCount(11));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(11, 3));
			Assert.AreEqual(6, indexer.GetInverseNeighborIndex(11, 3));

			Assert.AreEqual(4, indexer.GetNeighborCount(14));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(14, 3));
			Assert.AreEqual(7, indexer.GetInverseNeighborIndex(14, 3));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(14, 2));
			Assert.AreEqual(8, indexer.GetInverseNeighborIndex(14, 2));

			Assert.AreEqual(4, indexer.GetNeighborCount(13));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(13, 2));
			Assert.AreEqual(9, indexer.GetInverseNeighborIndex(13, 2));

			Assert.AreEqual(4, indexer.GetNeighborCount(12));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(12, 2));
			Assert.AreEqual(10, indexer.GetInverseNeighborIndex(12, 2));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(12, 1));
			Assert.AreEqual(11, indexer.GetInverseNeighborIndex(12, 1));

			Assert.AreEqual(4, indexer.GetNeighborCount(9));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(9, 1));
			Assert.AreEqual(12, indexer.GetInverseNeighborIndex(9, 1));

			Assert.AreEqual(4, indexer.GetNeighborCount(6));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(6, 1));
			Assert.AreEqual(13, indexer.GetInverseNeighborIndex(6, 1));

			Assert.AreEqual(4, indexer.GetNeighborCount(3));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(3, 1));
			Assert.AreEqual(14, indexer.GetInverseNeighborIndex(3, 1));

			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(0, 1));
			Assert.AreEqual(15, indexer.GetInverseNeighborIndex(0, 1));

			Assert.AreEqual(16, indexer.GetNeighborCount(15));

			Assert.AreEqual(0, indexer.GetNeighborFaceIndex(15, 0));
			Assert.AreEqual(1, indexer.GetNeighborFaceIndex(15, 1));
			Assert.AreEqual(2, indexer.GetNeighborFaceIndex(15, 2));
			Assert.AreEqual(2, indexer.GetNeighborFaceIndex(15, 3));
			Assert.AreEqual(5, indexer.GetNeighborFaceIndex(15, 4));
			Assert.AreEqual(8, indexer.GetNeighborFaceIndex(15, 5));
			Assert.AreEqual(11, indexer.GetNeighborFaceIndex(15, 6));
			Assert.AreEqual(14, indexer.GetNeighborFaceIndex(15, 7));
			Assert.AreEqual(14, indexer.GetNeighborFaceIndex(15, 8));
			Assert.AreEqual(13, indexer.GetNeighborFaceIndex(15, 9));
			Assert.AreEqual(12, indexer.GetNeighborFaceIndex(15, 10));
			Assert.AreEqual(12, indexer.GetNeighborFaceIndex(15, 11));
			Assert.AreEqual(9, indexer.GetNeighborFaceIndex(15, 12));
			Assert.AreEqual(6, indexer.GetNeighborFaceIndex(15, 13));
			Assert.AreEqual(3, indexer.GetNeighborFaceIndex(15, 14));
			Assert.AreEqual(0, indexer.GetNeighborFaceIndex(15, 15));

			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(15, 0));
			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(15, 1));
			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(15, 2));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(15, 3));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(15, 4));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(15, 5));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(15, 6));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(15, 7));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(15, 8));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(15, 9));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(15, 10));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(15, 11));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(15, 12));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(15, 13));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(15, 14));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(15, 15));
		}

		[Test]
		public static void ValidateSquareGridExternalFaceNeighbors()
		{
			var indexer = RowMajorQuadGridFaceNeighborIndexer.CreateInstance(4, 4);

			Assert.AreEqual(4, indexer.GetNeighborCount(0));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(0, 0));
			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(0, 0));

			Assert.AreEqual(4, indexer.GetNeighborCount(1));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(1, 0));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(1, 0));

			Assert.AreEqual(4, indexer.GetNeighborCount(2));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(2, 0));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(2, 0));

			Assert.AreEqual(4, indexer.GetNeighborCount(3));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(3, 0));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(3, 0));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(3, 3));
			Assert.AreEqual(4, indexer.GetInverseNeighborIndex(3, 3));

			Assert.AreEqual(4, indexer.GetNeighborCount(7));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(7, 3));
			Assert.AreEqual(5, indexer.GetInverseNeighborIndex(7, 3));

			Assert.AreEqual(4, indexer.GetNeighborCount(11));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(11, 3));
			Assert.AreEqual(6, indexer.GetInverseNeighborIndex(11, 3));

			Assert.AreEqual(4, indexer.GetNeighborCount(15));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(15, 3));
			Assert.AreEqual(7, indexer.GetInverseNeighborIndex(15, 3));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(15, 2));
			Assert.AreEqual(8, indexer.GetInverseNeighborIndex(15, 2));

			Assert.AreEqual(4, indexer.GetNeighborCount(14));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(14, 2));
			Assert.AreEqual(9, indexer.GetInverseNeighborIndex(14, 2));

			Assert.AreEqual(4, indexer.GetNeighborCount(13));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(13, 2));
			Assert.AreEqual(10, indexer.GetInverseNeighborIndex(13, 2));

			Assert.AreEqual(4, indexer.GetNeighborCount(12));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(12, 2));
			Assert.AreEqual(11, indexer.GetInverseNeighborIndex(12, 2));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(12, 1));
			Assert.AreEqual(12, indexer.GetInverseNeighborIndex(12, 1));

			Assert.AreEqual(4, indexer.GetNeighborCount(8));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(8, 1));
			Assert.AreEqual(13, indexer.GetInverseNeighborIndex(8, 1));

			Assert.AreEqual(4, indexer.GetNeighborCount(4));
			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(4, 1));
			Assert.AreEqual(14, indexer.GetInverseNeighborIndex(4, 1));

			Assert.AreEqual(16, indexer.GetNeighborFaceIndex(0, 1));
			Assert.AreEqual(15, indexer.GetInverseNeighborIndex(0, 1));

			Assert.AreEqual(16, indexer.GetNeighborCount(16));

			Assert.AreEqual(0, indexer.GetNeighborFaceIndex(16, 0));
			Assert.AreEqual(1, indexer.GetNeighborFaceIndex(16, 1));
			Assert.AreEqual(2, indexer.GetNeighborFaceIndex(16, 2));
			Assert.AreEqual(3, indexer.GetNeighborFaceIndex(16, 3));
			Assert.AreEqual(3, indexer.GetNeighborFaceIndex(16, 4));
			Assert.AreEqual(7, indexer.GetNeighborFaceIndex(16, 5));
			Assert.AreEqual(11, indexer.GetNeighborFaceIndex(16, 6));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(16, 7));
			Assert.AreEqual(15, indexer.GetNeighborFaceIndex(16, 8));
			Assert.AreEqual(14, indexer.GetNeighborFaceIndex(16, 9));
			Assert.AreEqual(13, indexer.GetNeighborFaceIndex(16, 10));
			Assert.AreEqual(12, indexer.GetNeighborFaceIndex(16, 11));
			Assert.AreEqual(12, indexer.GetNeighborFaceIndex(16, 12));
			Assert.AreEqual(8, indexer.GetNeighborFaceIndex(16, 13));
			Assert.AreEqual(4, indexer.GetNeighborFaceIndex(16, 14));
			Assert.AreEqual(0, indexer.GetNeighborFaceIndex(16, 15));

			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(16, 0));
			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(16, 1));
			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(16, 2));
			Assert.AreEqual(0, indexer.GetInverseNeighborIndex(16, 3));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(16, 4));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(16, 5));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(16, 6));
			Assert.AreEqual(3, indexer.GetInverseNeighborIndex(16, 7));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(16, 8));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(16, 9));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(16, 10));
			Assert.AreEqual(2, indexer.GetInverseNeighborIndex(16, 11));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(16, 12));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(16, 13));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(16, 14));
			Assert.AreEqual(1, indexer.GetInverseNeighborIndex(16, 15));
		}
	}
}
