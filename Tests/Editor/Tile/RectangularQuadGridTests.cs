/******************************************************************************\
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
using UnityEngine;
using NUnit.Framework;
using MakeIt.Numerics;

namespace MakeIt.Tile.Tests
{
	class RectangularQuadGridTests
	{
		[TestCase(Category = "Normal")]
		public static void ValidateWideGridElementCounts()
		{
			var grid = RectangularQuadGrid.Create(Vector2.right, Vector2.up, Vector3.zero, Quaternion.identity, false, false, new IntVector2(5, 3));

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

		[TestCase(Category = "Normal")]
		public static void ValidateTallGridElementCounts()
		{
			var grid = RectangularQuadGrid.Create(Vector2.right, Vector2.up, Vector3.zero, Quaternion.identity, false, false, new IntVector2(3, 5));

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

		[TestCase(Category = "Normal")]
		public static void ValidateSquareGridElementCounts()
		{
			var grid = RectangularQuadGrid.Create(Vector2.right, Vector2.up, Vector3.zero, Quaternion.identity, false, false, new IntVector2(4, 4));

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

		[TestCase(Category = "Normal")]
		public static void ValidateWideHorizontallyWrappedGridElementCounts()
		{
			var grid = RectangularQuadGrid.Create(Vector2.right, Vector2.up, Vector3.zero, Quaternion.identity, true, false, new IntVector2(5, 3));

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

		[TestCase(Category = "Normal")]
		public static void ValidateTallHorizontallyWrappedGridElementCounts()
		{
			var grid = RectangularQuadGrid.Create(Vector2.right, Vector2.up, Vector3.zero, Quaternion.identity, true, false, new IntVector2(3, 5));

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

		[TestCase(Category = "Normal")]
		public static void ValidateSquareHorizontallyWrappedGridElementCounts()
		{
			var grid = RectangularQuadGrid.Create(Vector2.right, Vector2.up, Vector3.zero, Quaternion.identity, true, false, new IntVector2(4, 4));

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

		[TestCase(Category = "Normal")]
		public static void ValidateWideVerticallyWrappedGridElementCounts()
		{
			var grid = RectangularQuadGrid.Create(Vector2.right, Vector2.up, Vector3.zero, Quaternion.identity, false, true, new IntVector2(5, 3));

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

		[TestCase(Category = "Normal")]
		public static void ValidateTallVerticallyWrappedGridElementCounts()
		{
			var grid = RectangularQuadGrid.Create(Vector2.right, Vector2.up, Vector3.zero, Quaternion.identity, false, true, new IntVector2(3, 5));

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

		[TestCase(Category = "Normal")]
		public static void ValidateSquareVerticallyWrappedGridElementCounts()
		{
			var grid = RectangularQuadGrid.Create(Vector2.right, Vector2.up, Vector3.zero, Quaternion.identity, false, true, new IntVector2(4, 4));

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

		[TestCase(Category = "Normal")]
		public static void ValidateWideFullyWrappedGridElementCounts()
		{
			var grid = RectangularQuadGrid.Create(Vector2.right, Vector2.up, Vector3.zero, Quaternion.identity, true, true, new IntVector2(5, 3));

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

		[TestCase(Category = "Normal")]
		public static void ValidateTallFullyWrappedGridElementCounts()
		{
			var grid = RectangularQuadGrid.Create(Vector2.right, Vector2.up, Vector3.zero, Quaternion.identity, true, true, new IntVector2(3, 5));

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

		[TestCase(Category = "Normal")]
		public static void ValidateSquareFullyWrappedGridElementCounts()
		{
			var grid = RectangularQuadGrid.Create(Vector2.right, Vector2.up, Vector3.zero, Quaternion.identity, true, true, new IntVector2(4, 4));

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
