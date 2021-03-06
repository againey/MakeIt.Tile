﻿/******************************************************************************\
* Copyright Andy Gainey                                                        *
*                                                                              *
* Licensed under the Apache License, Version 2.0 (the "License");              *
* you may not use this file except in compliance with the License.             *
* You may obtain a copy of the License at                                      *
*                                                                              *
*     http://www.apache.org/licenses/LICENSE-2.0                               *
*                                                                              *
* Unless required by applicable law or agreed to in writing, software          *
* distributed under the License is distributed on an "AS IS" BASIS,            *
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.     *
* See the License for the specific language governing permissions and          *
* limitations under the License.                                               *
\******************************************************************************/

#if UNITY_5_3_OR_NEWER
using NUnit.Framework;
using System;

namespace MakeIt.Tile.Tests
{
	class TopologyAlterationTests
	{
		[TestCase(Category = "Normal")]
		public void SpinEdgeForwardThrowsOnBoundaryVerticesWithTwoNeighbors()
		{
			var indexer = new ManualFaceNeighborIndexer(3, 6, 1, 1);
			indexer.AddFace(0, 1, 2);
			var topology = TopologyUtility.BuildTopology(indexer);

			foreach (var edge in topology.vertexEdges)
			{
				Assert.Throws<InvalidOperationException>(() => { topology.SpinEdgeForward(edge); });
			}
		}

		[TestCase(Category = "Normal")]
		public void SpinEdgeBackwardThrowsOnBoundaryVerticesWithTwoNeighbors()
		{
			var indexer = new ManualFaceNeighborIndexer(3, 6, 1, 1);
			indexer.AddFace(0, 1, 2);
			var topology = TopologyUtility.BuildTopology(indexer);

			foreach (var edge in topology.vertexEdges)
			{
				Assert.Throws<InvalidOperationException>(() => { topology.SpinEdgeBackward(edge); });
			}
		}

		[TestCase(Category = "Normal")]
		public void SpinEdgeForwardThrowsOnInteriorVerticesWithTwoNeighbors()
		{
			var indexer = new ManualFaceNeighborIndexer(5, 12, 2, 1);
			indexer.AddFace(0, 1, 2, 4);
			indexer.AddFace(2, 3, 0, 4);
			var topology = TopologyUtility.BuildTopology(indexer);

			Assert.Throws<InvalidOperationException>(() => { topology.SpinEdgeForward(topology.vertices[4].firstEdge); });
			Assert.Throws<InvalidOperationException>(() => { topology.SpinEdgeForward(topology.vertices[4].firstEdge.next); });
		}

		[TestCase(Category = "Normal")]
		public void SpinEdgeBackwardThrowsOnInteriorVerticesWithTwoNeighbors()
		{
			var indexer = new ManualFaceNeighborIndexer(5, 12, 2, 1);
			indexer.AddFace(0, 1, 2, 4);
			indexer.AddFace(2, 3, 0, 4);
			var topology = TopologyUtility.BuildTopology(indexer);

			Assert.Throws<InvalidOperationException>(() => { topology.SpinEdgeBackward(topology.vertices[4].firstEdge); });
			Assert.Throws<InvalidOperationException>(() => { topology.SpinEdgeBackward(topology.vertices[4].firstEdge.next); });
		}

		[TestCase(Category = "Normal")]
		public void SpinEdgeBetweenTrianglesForward()
		{
			//   1         1
			//  /^\       /P\
			// 0P|N3 --> 0-->3
			//  \|/       \N/
			//   2         2

			var indexer = new ManualFaceNeighborIndexer(4, 10, 2, 1);
			indexer.AddFace(0, 1, 2);
			indexer.AddFace(3, 2, 1);
			var topology = TopologyUtility.BuildTopology(indexer);

			Topology.VertexEdge edgeToSpin;
			Assert.IsTrue(topology.vertices[2].TryFindEdge(topology.vertices[1], out edgeToSpin));

			topology.SpinEdgeForward(edgeToSpin);

			TopologyUtility.CheckVerticesForInvalidEdgeCycles(topology);
			TopologyUtility.CheckFacesForInvalidEdgeCycles(topology);

			Assert.AreEqual(3, topology.vertices[0].neighborCount);
			Assert.AreEqual(2, topology.vertices[1].neighborCount);
			Assert.AreEqual(2, topology.vertices[2].neighborCount);
			Assert.AreEqual(3, topology.vertices[3].neighborCount);

			Assert.AreEqual(3, topology.internalFaces[0].neighborCount);
			Assert.AreEqual(3, topology.internalFaces[1].neighborCount);

			Assert.AreEqual(topology.vertices[0], edgeToSpin.nearVertex);
			Assert.AreEqual(topology.vertices[3], edgeToSpin.farVertex);
			Assert.AreEqual(topology.internalFaces[0], edgeToSpin.prevFace);
			Assert.AreEqual(topology.internalFaces[1], edgeToSpin.nextFace);

			Assert.AreEqual(topology.vertices[1], edgeToSpin.prev.farVertex);
			Assert.AreEqual(topology.vertices[2], edgeToSpin.next.farVertex);
			Assert.AreEqual(topology.vertices[2], edgeToSpin.twin.prev.farVertex);
			Assert.AreEqual(topology.vertices[1], edgeToSpin.twin.next.farVertex);

			Assert.AreEqual(topology.vertices[3], edgeToSpin.faceEdge.nextVertex);
			Assert.AreEqual(topology.vertices[2], edgeToSpin.faceEdge.next.nextVertex);
			Assert.AreEqual(topology.vertices[0], edgeToSpin.faceEdge.next.next.nextVertex);
			Assert.AreEqual(topology.vertices[3], edgeToSpin.faceEdge.next.next.next.nextVertex);
			Assert.AreEqual(topology.vertices[0], edgeToSpin.twin.faceEdge.nextVertex);
			Assert.AreEqual(topology.vertices[1], edgeToSpin.twin.faceEdge.next.nextVertex);
			Assert.AreEqual(topology.vertices[3], edgeToSpin.twin.faceEdge.next.next.nextVertex);
			Assert.AreEqual(topology.vertices[0], edgeToSpin.twin.faceEdge.next.next.next.nextVertex);
		}

		[TestCase(Category = "Normal")]
		public void SpinEdgeBetweenTrianglesBackward()
		{
			//   1         1
			//  /^\       /N\
			// 0P|N3 --> 0<--3
			//  \|/       \P/
			//   2         2

			var indexer = new ManualFaceNeighborIndexer(4, 10, 2, 1);
			indexer.AddFace(0, 1, 2);
			indexer.AddFace(3, 2, 1);
			var topology = TopologyUtility.BuildTopology(indexer);

			Topology.VertexEdge edgeToSpin;
			Assert.IsTrue(topology.vertices[2].TryFindEdge(topology.vertices[1], out edgeToSpin));

			topology.SpinEdgeBackward(edgeToSpin);

			TopologyUtility.CheckVerticesForInvalidEdgeCycles(topology);
			TopologyUtility.CheckFacesForInvalidEdgeCycles(topology);

			Assert.AreEqual(3, topology.vertices[0].neighborCount);
			Assert.AreEqual(2, topology.vertices[1].neighborCount);
			Assert.AreEqual(2, topology.vertices[2].neighborCount);
			Assert.AreEqual(3, topology.vertices[3].neighborCount);

			Assert.AreEqual(3, topology.internalFaces[0].neighborCount);
			Assert.AreEqual(3, topology.internalFaces[1].neighborCount);

			Assert.AreEqual(topology.vertices[3], edgeToSpin.nearVertex);
			Assert.AreEqual(topology.vertices[0], edgeToSpin.farVertex);
			Assert.AreEqual(topology.internalFaces[0], edgeToSpin.prevFace);
			Assert.AreEqual(topology.internalFaces[1], edgeToSpin.nextFace);

			Assert.AreEqual(topology.vertices[2], edgeToSpin.prev.farVertex);
			Assert.AreEqual(topology.vertices[1], edgeToSpin.next.farVertex);
			Assert.AreEqual(topology.vertices[1], edgeToSpin.twin.prev.farVertex);
			Assert.AreEqual(topology.vertices[2], edgeToSpin.twin.next.farVertex);

			Assert.AreEqual(topology.vertices[0], edgeToSpin.faceEdge.nextVertex);
			Assert.AreEqual(topology.vertices[1], edgeToSpin.faceEdge.next.nextVertex);
			Assert.AreEqual(topology.vertices[3], edgeToSpin.faceEdge.next.next.nextVertex);
			Assert.AreEqual(topology.vertices[0], edgeToSpin.faceEdge.next.next.next.nextVertex);
			Assert.AreEqual(topology.vertices[3], edgeToSpin.twin.faceEdge.nextVertex);
			Assert.AreEqual(topology.vertices[2], edgeToSpin.twin.faceEdge.next.nextVertex);
			Assert.AreEqual(topology.vertices[0], edgeToSpin.twin.faceEdge.next.next.nextVertex);
			Assert.AreEqual(topology.vertices[3], edgeToSpin.twin.faceEdge.next.next.next.nextVertex);
		}

		[TestCase(Category = "Normal")]
		public void SpinEdgeBetweenSquaresForward()
		{
			// 0---1     0---1
			// | P |     |\ P|
			// 3-->2 --> 3 \ 2
			// | N |     |N v|
			// 4---5     4---5

			var indexer = new ManualFaceNeighborIndexer(6, 14, 2, 1);
			indexer.AddFace(0, 1, 2, 3);
			indexer.AddFace(5, 4, 3, 2);
			var topology = TopologyUtility.BuildTopology(indexer);

			Topology.VertexEdge edgeToSpin;
			Assert.IsTrue(topology.vertices[3].TryFindEdge(topology.vertices[2], out edgeToSpin));

			topology.SpinEdgeForward(edgeToSpin);

			TopologyUtility.CheckVerticesForInvalidEdgeCycles(topology);
			TopologyUtility.CheckFacesForInvalidEdgeCycles(topology);

			Assert.AreEqual(3, topology.vertices[0].neighborCount);
			Assert.AreEqual(2, topology.vertices[1].neighborCount);
			Assert.AreEqual(2, topology.vertices[2].neighborCount);
			Assert.AreEqual(2, topology.vertices[3].neighborCount);
			Assert.AreEqual(2, topology.vertices[4].neighborCount);
			Assert.AreEqual(3, topology.vertices[5].neighborCount);

			Assert.AreEqual(4, topology.internalFaces[0].neighborCount);
			Assert.AreEqual(4, topology.internalFaces[1].neighborCount);

			Assert.AreEqual(topology.vertices[0], edgeToSpin.nearVertex);
			Assert.AreEqual(topology.vertices[5], edgeToSpin.farVertex);
			Assert.AreEqual(topology.internalFaces[0], edgeToSpin.prevFace);
			Assert.AreEqual(topology.internalFaces[1], edgeToSpin.nextFace);

			Assert.AreEqual(topology.vertices[1], edgeToSpin.prev.farVertex);
			Assert.AreEqual(topology.vertices[3], edgeToSpin.next.farVertex);
			Assert.AreEqual(topology.vertices[4], edgeToSpin.twin.prev.farVertex);
			Assert.AreEqual(topology.vertices[2], edgeToSpin.twin.next.farVertex);

			Assert.AreEqual(topology.vertices[5], edgeToSpin.faceEdge.nextVertex);
			Assert.AreEqual(topology.vertices[4], edgeToSpin.faceEdge.next.nextVertex);
			Assert.AreEqual(topology.vertices[3], edgeToSpin.faceEdge.next.next.nextVertex);
			Assert.AreEqual(topology.vertices[0], edgeToSpin.faceEdge.next.next.next.nextVertex);
			Assert.AreEqual(topology.vertices[5], edgeToSpin.faceEdge.next.next.next.next.nextVertex);
			Assert.AreEqual(topology.vertices[0], edgeToSpin.twin.faceEdge.nextVertex);
			Assert.AreEqual(topology.vertices[1], edgeToSpin.twin.faceEdge.next.nextVertex);
			Assert.AreEqual(topology.vertices[2], edgeToSpin.twin.faceEdge.next.next.nextVertex);
			Assert.AreEqual(topology.vertices[5], edgeToSpin.twin.faceEdge.next.next.next.nextVertex);
			Assert.AreEqual(topology.vertices[0], edgeToSpin.twin.faceEdge.next.next.next.next.nextVertex);
		}

		[TestCase(Category = "Normal")]
		public void SpinEdgeBetweenSquaresBackward()
		{
			// 0---1     0---1
			// | P |     |P ^|
			// 3-->2 --> 3 / 2
			// | N |     |/ N|
			// 4---5     4---5

			var indexer = new ManualFaceNeighborIndexer(6, 14, 2, 1);
			indexer.AddFace(0, 1, 2, 3);
			indexer.AddFace(5, 4, 3, 2);
			var topology = TopologyUtility.BuildTopology(indexer);

			Topology.VertexEdge edgeToSpin;
			Assert.IsTrue(topology.vertices[3].TryFindEdge(topology.vertices[2], out edgeToSpin));

			topology.SpinEdgeBackward(edgeToSpin);

			TopologyUtility.CheckVerticesForInvalidEdgeCycles(topology);
			TopologyUtility.CheckFacesForInvalidEdgeCycles(topology);

			Assert.AreEqual(2, topology.vertices[0].neighborCount);
			Assert.AreEqual(3, topology.vertices[1].neighborCount);
			Assert.AreEqual(2, topology.vertices[2].neighborCount);
			Assert.AreEqual(2, topology.vertices[3].neighborCount);
			Assert.AreEqual(3, topology.vertices[4].neighborCount);
			Assert.AreEqual(2, topology.vertices[5].neighborCount);

			Assert.AreEqual(4, topology.internalFaces[0].neighborCount);
			Assert.AreEqual(4, topology.internalFaces[1].neighborCount);

			Assert.AreEqual(topology.vertices[4], edgeToSpin.nearVertex);
			Assert.AreEqual(topology.vertices[1], edgeToSpin.farVertex);
			Assert.AreEqual(topology.internalFaces[0], edgeToSpin.prevFace);
			Assert.AreEqual(topology.internalFaces[1], edgeToSpin.nextFace);

			Assert.AreEqual(topology.vertices[3], edgeToSpin.prev.farVertex);
			Assert.AreEqual(topology.vertices[5], edgeToSpin.next.farVertex);
			Assert.AreEqual(topology.vertices[2], edgeToSpin.twin.prev.farVertex);
			Assert.AreEqual(topology.vertices[0], edgeToSpin.twin.next.farVertex);

			Assert.AreEqual(topology.vertices[1], edgeToSpin.faceEdge.nextVertex);
			Assert.AreEqual(topology.vertices[2], edgeToSpin.faceEdge.next.nextVertex);
			Assert.AreEqual(topology.vertices[5], edgeToSpin.faceEdge.next.next.nextVertex);
			Assert.AreEqual(topology.vertices[4], edgeToSpin.faceEdge.next.next.next.nextVertex);
			Assert.AreEqual(topology.vertices[1], edgeToSpin.faceEdge.next.next.next.next.nextVertex);
			Assert.AreEqual(topology.vertices[4], edgeToSpin.twin.faceEdge.nextVertex);
			Assert.AreEqual(topology.vertices[3], edgeToSpin.twin.faceEdge.next.nextVertex);
			Assert.AreEqual(topology.vertices[0], edgeToSpin.twin.faceEdge.next.next.nextVertex);
			Assert.AreEqual(topology.vertices[1], edgeToSpin.twin.faceEdge.next.next.next.nextVertex);
			Assert.AreEqual(topology.vertices[4], edgeToSpin.twin.faceEdge.next.next.next.next.nextVertex);
		}
	}
}
#endif
