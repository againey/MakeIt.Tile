/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/
#if false
using UnityEngine;
using System;
using System.Collections.Generic;
using Experilous.Numerics;
using Experilous.Topologies;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// Static utility class for working with manifolds, topological surfaces in three-dimensional space.
	/// </summary>
	public static class ManifoldUtility
	{
		/// <summary>
		/// Reverses the roles of vertices and faces, as when taking the dual of a polyhedron.
		/// </summary>
		/// <param name="topology">The topology containing the vertices and faces to swap.</param>
		/// <param name="facePositions">The positions of the faces.</param>
		/// <param name="vertexPositions">The positions of the vertices after the swap.</param>
		public static void MakeDual(Topology topology, IList<Vector3> facePositions, out Vector3[] vertexPositions)
		{
			var faceCount = topology.faces.Count;
			topology.MakeDual();
			vertexPositions = new Vector3[faceCount];
			facePositions.CopyTo(vertexPositions, 0);
		}

		/// <summary>
		/// Creates a copy of the specified topology, but with the roles of vertices and faces reversed, as when taking the dual of a polyhedron.
		/// </summary>
		/// <param name="topology">The original topology containing the vertices and faces to swap.</param>
		/// <param name="facePositions">The positions of the faces.</param>
		/// <param name="dualTopology">The copied topology with the vertices and faces swapped.</param>
		/// <param name="vertexPositions">The positions of the vertices after the swap.</param>
		public static void GetDualManifold(Topology topology, IList<Vector3> facePositions, out Topology dualTopology, out Vector3[] vertexPositions)
		{
			dualTopology = (Topology)topology.Clone();
			MakeDual(dualTopology, facePositions, out vertexPositions);
		}

		private static void SubdivideEdge(Vector3 p0, Vector3 p1, int count, Func<Vector3, Vector3, float, Vector3> interpolator, List<Vector3> positions)
		{
			var dt = 1.0f / (float)(count + 1);
			var t = dt;
			var tEnd = 1f - dt * 0.5f;
			while (t < tEnd)
			{
				positions.Add(interpolator(p0, p1, t));
				t += dt;
			}
		}

		private static void SubdivideTriangle(ManualFaceNeighborIndexer indexer, Topology.Face face, int degree, Func<Vector3, Vector3, float, Vector3> interpolator, List<Vector3> subdividedVertexPositions, int[] subdividedEdgeVertices, ref int currentVertexCount)
		{
			var rightEdge = face.firstEdge;
			var bottomEdge = rightEdge.next;
			var leftEdge = bottomEdge.next;

			var topVertex = leftEdge.vertex;
			var bottomRightVertex = rightEdge.vertex;
			var bottomLeftVertex = bottomEdge.vertex;

			int rightVertices = rightEdge.index * degree; // Progresses from the top vertex down to the bottom right vertex.
			int bottomVertices = bottomEdge.twinIndex * degree; // Progresses from the bottom left vertex over to the bottom right vertex.
			int leftVertices = leftEdge.twinIndex * degree; // Progresses from the top vertex down to the bottom left vertex.

			if (degree > 2)
			{
				// Top triangle
				indexer.AddFace(subdividedEdgeVertices[leftVertices], topVertex.index, subdividedEdgeVertices[rightVertices]);

				// Next three triangles above the top inner subdivided vertex
				indexer.AddFace(subdividedEdgeVertices[leftVertices + 1], subdividedEdgeVertices[leftVertices], currentVertexCount);
				indexer.AddFace(currentVertexCount, subdividedEdgeVertices[leftVertices], subdividedEdgeVertices[rightVertices]);
				indexer.AddFace(currentVertexCount, subdividedEdgeVertices[rightVertices], subdividedEdgeVertices[rightVertices + 1]);

				// Top inner subdivided vertex
				subdividedVertexPositions.Add(interpolator(subdividedVertexPositions[subdividedEdgeVertices[leftVertices + 1]], subdividedVertexPositions[subdividedEdgeVertices[rightVertices + 1]], 0.5f));
				++currentVertexCount;

				float t;
				float dt;
				Vector3 p0;
				Vector3 p1;

				int yEnd = degree - 1;

				// Middle rows of inner subdivided vertices
				for (int y = 1; y < yEnd; ++y)
				{
					t = dt = 1f / (y + 2);
					p0 = subdividedVertexPositions[subdividedEdgeVertices[leftVertices + y + 1]];
					p1 = subdividedVertexPositions[subdividedEdgeVertices[rightVertices + y + 1]];

					// First two triangles of the row of faces above this row of inner subdivided vertices
					indexer.AddFace(subdividedEdgeVertices[leftVertices + y + 1], subdividedEdgeVertices[leftVertices + y], currentVertexCount);
					indexer.AddFace(currentVertexCount, subdividedEdgeVertices[leftVertices + y], currentVertexCount - y);

					subdividedVertexPositions.Add(interpolator(p0, p1, t));
					++currentVertexCount;
					t += dt;

					for (int x = 1; x < y; ++x)
					{
						// Next two triangles above the vertex being added
						indexer.AddFace(currentVertexCount - 1, currentVertexCount - y - 1, currentVertexCount);
						indexer.AddFace(currentVertexCount, currentVertexCount - y - 1, currentVertexCount - y);
						subdividedVertexPositions.Add(interpolator(p0, p1, t));
						++currentVertexCount;
						t += dt;
					}

					// Last three triangles of the row of faces above this row of inner subdivided vertices
					indexer.AddFace(currentVertexCount - 1, currentVertexCount - y - 1, currentVertexCount);
					indexer.AddFace(currentVertexCount, currentVertexCount - y - 1, subdividedEdgeVertices[rightVertices + y]);
					indexer.AddFace(currentVertexCount, subdividedEdgeVertices[rightVertices + y], subdividedEdgeVertices[rightVertices + y + 1]);

					subdividedVertexPositions.Add(interpolator(p0, p1, t));
					++currentVertexCount;
				}

				// First two triangles of the last row of faces
				indexer.AddFace(bottomLeftVertex.index, subdividedEdgeVertices[leftVertices + yEnd], subdividedEdgeVertices[bottomVertices]);
				indexer.AddFace(subdividedEdgeVertices[bottomVertices], subdividedEdgeVertices[leftVertices + yEnd], currentVertexCount - yEnd);

				for (int x = 1; x < yEnd; ++x)
				{
					// Next two triangles of the last row of faces
					indexer.AddFace(subdividedEdgeVertices[bottomVertices + x - 1], currentVertexCount - yEnd + x - 1, subdividedEdgeVertices[bottomVertices + x]);
					indexer.AddFace(subdividedEdgeVertices[bottomVertices + x], currentVertexCount - yEnd + x - 1, currentVertexCount - yEnd + x);
				}

				// Last three triangles of the last row of faces
				indexer.AddFace(subdividedEdgeVertices[bottomVertices + yEnd - 1], currentVertexCount - 1, subdividedEdgeVertices[bottomVertices + yEnd]);
				indexer.AddFace(subdividedEdgeVertices[bottomVertices + yEnd], currentVertexCount - 1, subdividedEdgeVertices[rightVertices + yEnd]);
				indexer.AddFace(subdividedEdgeVertices[bottomVertices + yEnd], subdividedEdgeVertices[rightVertices + yEnd], bottomRightVertex.index);
			}
			else if (degree == 2)
			{
				indexer.AddFace(subdividedEdgeVertices[leftVertices], topVertex.index, subdividedEdgeVertices[rightVertices]);
				indexer.AddFace(subdividedEdgeVertices[leftVertices + 1], subdividedEdgeVertices[leftVertices], currentVertexCount);
				indexer.AddFace(currentVertexCount, subdividedEdgeVertices[leftVertices], subdividedEdgeVertices[rightVertices]);
				indexer.AddFace(currentVertexCount, subdividedEdgeVertices[rightVertices], subdividedEdgeVertices[rightVertices + 1]);
				indexer.AddFace(bottomLeftVertex.index, subdividedEdgeVertices[leftVertices + 1], subdividedEdgeVertices[bottomVertices]);
				indexer.AddFace(subdividedEdgeVertices[bottomVertices], subdividedEdgeVertices[leftVertices + 1], currentVertexCount);
				indexer.AddFace(subdividedEdgeVertices[bottomVertices], currentVertexCount, subdividedEdgeVertices[bottomVertices + 1]);
				indexer.AddFace(subdividedEdgeVertices[bottomVertices + 1], currentVertexCount, subdividedEdgeVertices[rightVertices + 1]);
				indexer.AddFace(subdividedEdgeVertices[bottomVertices + 1], subdividedEdgeVertices[rightVertices + 1], bottomRightVertex.index);

				subdividedVertexPositions.Add(interpolator(subdividedVertexPositions[subdividedEdgeVertices[leftVertices + 1]], subdividedVertexPositions[subdividedEdgeVertices[rightVertices + 1]], 0.5f));
				++currentVertexCount;
			}
			else if (degree == 1)
			{
				indexer.AddFace(subdividedEdgeVertices[leftVertices], topVertex.index, subdividedEdgeVertices[rightVertices]);
				indexer.AddFace(bottomLeftVertex.index, subdividedEdgeVertices[leftVertices], subdividedEdgeVertices[bottomVertices]);
				indexer.AddFace(subdividedEdgeVertices[bottomVertices], subdividedEdgeVertices[leftVertices], subdividedEdgeVertices[rightVertices]);
				indexer.AddFace(subdividedEdgeVertices[bottomVertices], subdividedEdgeVertices[rightVertices], bottomRightVertex.index);
			}
		}

		private static void SubdivideQuadrilateral(ManualFaceNeighborIndexer indexer, Topology.Face face, int degree, Func<Vector3, Vector3, float, Vector3> interpolator, List<Vector3> subdividedVertexPositions, int[] subdividedEdgeVertices, ref int currentVertexCount)
		{
			var topEdge = face.firstEdge;
			var rightEdge = topEdge.next;
			var bottomEdge = rightEdge.next;
			var leftEdge = bottomEdge.next;

			var topLeftVertex = leftEdge.vertex;
			var topRightVertex = topEdge.vertex;
			var bottomRightVertex = rightEdge.vertex;
			var bottomLeftVertex = bottomEdge.vertex;

			int topVertices = topEdge.index * degree; // Progresses from top left vertex over to top right vertex.
			int bottomVertices = bottomEdge.twinIndex * degree; // Progresses from bottom left vertex over to bottom right vertex.
			int rightVertices = rightEdge.index * degree; // Progresses from top right vertex down to bottom right vertex.
			int leftVertices = leftEdge.twinIndex * degree; // Progresses from top left vertex down to bottom left vertex.

			var dt = 1f / (degree + 1);

			if (degree > 2)
			{
				int yEnd = degree - 1;
				int xEnd = degree - 1;

				float t;
				Vector3 p0;
				Vector3 p1;

				// Top row of subdivided faces
				indexer.AddFace(topLeftVertex.index, subdividedEdgeVertices[topVertices], currentVertexCount, subdividedEdgeVertices[leftVertices]);
				for (int x = 0; x < xEnd; ++x)
				{
					indexer.AddFace(subdividedEdgeVertices[topVertices + x], subdividedEdgeVertices[topVertices + x + 1], currentVertexCount + x + 1, currentVertexCount + x);
				}
				indexer.AddFace(subdividedEdgeVertices[topVertices + xEnd], topRightVertex.index, subdividedEdgeVertices[rightVertices], currentVertexCount + xEnd);

				// Middle rows of subdivided faces
				for (int y = 0; y < yEnd; ++y)
				{
					var rowFirstVertexIndex = currentVertexCount + y * degree;
					indexer.AddFace(subdividedEdgeVertices[leftVertices + y], rowFirstVertexIndex, rowFirstVertexIndex + degree, subdividedEdgeVertices[leftVertices + y + 1]);
					for (int x = 0; x < xEnd; ++x)
					{
						indexer.AddFace(rowFirstVertexIndex + x, rowFirstVertexIndex + x + 1, rowFirstVertexIndex + degree + x + 1, rowFirstVertexIndex + degree + x);
					}
					indexer.AddFace(rowFirstVertexIndex + xEnd, subdividedEdgeVertices[rightVertices + y], subdividedEdgeVertices[rightVertices + y + 1], rowFirstVertexIndex + degree + xEnd);
				}

				// Bottom row of subdivided faces
				var lastRowFirstVertexIndex = currentVertexCount + yEnd * degree;
				indexer.AddFace(subdividedEdgeVertices[leftVertices + yEnd], lastRowFirstVertexIndex, subdividedEdgeVertices[bottomVertices], bottomLeftVertex.index);
				for (int x = 0; x < xEnd; ++x)
				{
					indexer.AddFace(lastRowFirstVertexIndex + x, lastRowFirstVertexIndex + x + 1, subdividedEdgeVertices[bottomVertices + x + 1], subdividedEdgeVertices[bottomVertices + x]);
				}
				indexer.AddFace(lastRowFirstVertexIndex + xEnd, subdividedEdgeVertices[rightVertices + yEnd], bottomRightVertex.index, subdividedEdgeVertices[bottomVertices + xEnd]);

				// Subdivided vertices
				for (int y = 0; y <= yEnd; ++y)
				{
					t = dt;
					p0 = subdividedVertexPositions[subdividedEdgeVertices[leftVertices + y]];
					p1 = subdividedVertexPositions[subdividedEdgeVertices[rightVertices + y]];

					for (int x = 0; x <= xEnd; ++x)
					{
						subdividedVertexPositions.Add(interpolator(p0, p1, t));
						t += dt;
					}
				}
				currentVertexCount += degree * degree;
			}
			else if (degree == 2)
			{
				indexer.AddFace(topLeftVertex.index, subdividedEdgeVertices[topVertices], currentVertexCount, subdividedEdgeVertices[leftVertices]);
				indexer.AddFace(subdividedEdgeVertices[topVertices], subdividedEdgeVertices[topVertices + 1], currentVertexCount + 1, currentVertexCount);
				indexer.AddFace(subdividedEdgeVertices[topVertices + 1], topRightVertex.index, subdividedEdgeVertices[rightVertices], currentVertexCount + 1);

				indexer.AddFace(subdividedEdgeVertices[leftVertices], currentVertexCount, currentVertexCount + 2, subdividedEdgeVertices[leftVertices + 1]);
				indexer.AddFace(currentVertexCount, currentVertexCount + 1, currentVertexCount + 3, currentVertexCount + 2);
				indexer.AddFace(currentVertexCount + 1, subdividedEdgeVertices[rightVertices], subdividedEdgeVertices[rightVertices + 1], currentVertexCount + 3);

				indexer.AddFace(subdividedEdgeVertices[leftVertices + 1], currentVertexCount + 2, subdividedEdgeVertices[bottomVertices], bottomLeftVertex.index);
				indexer.AddFace(currentVertexCount + 2, currentVertexCount + 3, subdividedEdgeVertices[bottomVertices + 1], subdividedEdgeVertices[bottomVertices]);
				indexer.AddFace(currentVertexCount + 3, subdividedEdgeVertices[rightVertices + 1], bottomRightVertex.index, subdividedEdgeVertices[bottomVertices + 1]);

				subdividedVertexPositions.Add(interpolator(subdividedVertexPositions[subdividedEdgeVertices[leftVertices]], subdividedVertexPositions[subdividedEdgeVertices[rightVertices]], dt));
				subdividedVertexPositions.Add(interpolator(subdividedVertexPositions[subdividedEdgeVertices[leftVertices]], subdividedVertexPositions[subdividedEdgeVertices[rightVertices]], dt + dt));
				subdividedVertexPositions.Add(interpolator(subdividedVertexPositions[subdividedEdgeVertices[leftVertices + 1]], subdividedVertexPositions[subdividedEdgeVertices[rightVertices + 1]], dt));
				subdividedVertexPositions.Add(interpolator(subdividedVertexPositions[subdividedEdgeVertices[leftVertices + 1]], subdividedVertexPositions[subdividedEdgeVertices[rightVertices + 1]], dt + dt));
				currentVertexCount += 4;
			}
			else if (degree == 1)
			{
				indexer.AddFace(topLeftVertex.index, subdividedEdgeVertices[topVertices], currentVertexCount, subdividedEdgeVertices[leftVertices]);
				indexer.AddFace(subdividedEdgeVertices[topVertices], topRightVertex.index, subdividedEdgeVertices[rightVertices], currentVertexCount);
				indexer.AddFace(subdividedEdgeVertices[leftVertices], currentVertexCount, subdividedEdgeVertices[bottomVertices], bottomLeftVertex.index);
				indexer.AddFace(currentVertexCount, subdividedEdgeVertices[rightVertices], bottomRightVertex.index, subdividedEdgeVertices[bottomVertices]);

				subdividedVertexPositions.Add(interpolator(subdividedVertexPositions[subdividedEdgeVertices[leftVertices]], subdividedVertexPositions[subdividedEdgeVertices[rightVertices]], dt));
				++currentVertexCount;
			}
		}

		/// <summary>
		/// Creates a new topology based on the one provided, subdividing each face into multiple smaller faces, and adding extra vertices and edges accordingly.
		/// </summary>
		/// <param name="topology">The original topology to be subdivided.  Cannot contain internal faces with neighbor counts of any value other than 3 or 4.</param>
		/// <param name="vertexPositions">The positions of the original topology's vertices.</param>
		/// <param name="degree">The degree of subdivision, equivalent to the number of additional vertices that will be added along each original edge.  Must be non-negative, and a value of zero will result in an exact duplicate with no subdivision.</param>
		/// <param name="interpolator">The function that interpolates between two vertex positions along an edge for determining the positions of new vertices.  Particularly useful for curved surfaces.</param>
		/// <param name="subdividedTopology">The copied and subdivided topology.</param>
		/// <param name="subdividedVertexPositions">The positions of the subdivided vertices.</param>
		public static void Subdivide(Topology topology, IVertexAttribute<Vector3> vertexPositions, int degree, Func<Vector3, Vector3, float, Vector3> interpolator, out Topology subdividedTopology, out Vector3[] subdividedVertexPositions)
		{
			if (degree < 0) throw new ArgumentOutOfRangeException("Topology subdivision degree cannot be negative.");
			if (degree == 0)
			{
				subdividedTopology = (Topology)topology.Clone();
				var subdividedVertexPositionsArray = new Vector3[topology.vertices.Count];
				vertexPositions.CopyTo(subdividedVertexPositionsArray, 0);
				subdividedVertexPositions = subdividedVertexPositionsArray;
				return;
			}

			int vertexCount = 0;
			int edgeCount = 0;
			int internalFaceCount = 0;

			vertexCount += topology.vertices.Count + (topology.halfEdges.Count / 2) * degree;

			foreach (var face in topology.internalFaces)
			{
				switch (face.neighborCount)
				{
					case 3:
						vertexCount += (degree * (degree - 1)) / 2;
						edgeCount += (degree + 1) * (degree + 1) * 3;
						internalFaceCount += (degree + 1) * (degree + 1);
						break;
					case 4:
						vertexCount += degree * degree;
						edgeCount += (degree + 1) * (degree + 1) * 4;
						internalFaceCount += (degree + 1) * (degree + 1);
						break;
					default:
						throw new InvalidOperationException("Cannot subdivide a face with anything other than 3 or 4 sides.");
				}
			}

			foreach (var face in topology.externalFaces)
			{
				edgeCount += face.neighborCount * (degree + 1);
			}

			var indexer = new ManualFaceNeighborIndexer(vertexCount, edgeCount, internalFaceCount, topology.externalFaces.Count);
			var subdividedVertexPositionsList = new List<Vector3>();

			foreach (var vertex in topology.vertices)
			{
				subdividedVertexPositionsList.Add(vertexPositions[vertex]);
			}

			var currentVertexCount = topology.vertices.Count;

			var dt = 1f / (degree + 1);
			var tEnd = 1f - dt * 0.5f;

			var subdividedEdgeVertices = new int[topology.halfEdges.Count * degree];

			foreach (var edge in topology.vertexEdges)
			{
				if (edge.isFirstTwin)
				{
					var p0 = vertexPositions[edge.nearVertex];
					var p1 = vertexPositions[edge.farVertex];
					var t = dt;
					var subdividedEdgeVertexIndex = edge.index * degree;
					while (t < tEnd)
					{
						subdividedEdgeVertices[subdividedEdgeVertexIndex++] = currentVertexCount++;
						subdividedVertexPositionsList.Add(interpolator(p0, p1, t));
						t += dt;
					}
				}
				else
				{
					var subdividedEdgeVertexIndex = edge.index * degree;
					var subdividedTwinEdgeVertexIndex = (edge.twinIndex + 1) * degree - 1;
					for (int i = 0; i < degree; ++i)
					{
						subdividedEdgeVertices[subdividedEdgeVertexIndex + i] = subdividedEdgeVertices[subdividedTwinEdgeVertexIndex - i];
					}
				}
			}

			foreach (var face in topology.internalFaces)
			{
				switch (face.neighborCount)
				{
					case 3:
						SubdivideTriangle(indexer, face, degree, interpolator, subdividedVertexPositionsList, subdividedEdgeVertices, ref currentVertexCount);
						break;
					case 4:
						SubdivideQuadrilateral(indexer, face, degree, interpolator, subdividedVertexPositionsList, subdividedEdgeVertices, ref currentVertexCount);
						break;
					default:
						throw new InvalidOperationException("Cannot subdivide a face with anything other than 3 or 4 sides.");
				}
			}

			subdividedTopology = TopologyUtility.BuildTopology(indexer);
			subdividedVertexPositions = subdividedVertexPositionsList.ToArray();
		}

		/// <summary>
		/// Creates a new topology based on the one provided, subdividing each face into multiple smaller faces, and adding extra vertices and edges accordingly.  Uses linear interpolation for deriving the positions of new vertices.
		/// </summary>
		/// <param name="topology">The original topology to be subdivided.</param>
		/// <param name="vertexPositions">The positions of the original topology's vertices.</param>
		/// <param name="degree">The degree of subdivision, equivalent to the number of additional vertices that will be added along each original edge.  Must be non-negative, and a value of zero will result in an exact duplicate with no subdivision.</param>
		/// <param name="subdividedTopology">The copied and subdivided topology.</param>
		/// <param name="subdividedVertexPositions">The positions of the subdivided vertices.</param>
		public static void Subdivide(Topology topology, IVertexAttribute<Vector3> vertexPositions, int degree, out Topology subdividedTopology, out Vector3[] subdividedVertexPositions)
		{
			Subdivide(topology, vertexPositions, degree, (Vector3 p0, Vector3 p1, float t) => { return Geometry.LerpUnclamped(p0, p1, t); }, out subdividedTopology, out subdividedVertexPositions);
		}

#region Export

		/// <summary>
		/// Helper class for <see cref="ExportToSVG"/> to define the style used by the resulting SVG file.
		/// </summary>
		public class SVGStyle
		{
			/// <summary>
			/// CSS style dictionary for the top-level svg element.
			/// </summary>
			public readonly Dictionary<string, string> svg = new Dictionary<string, string>();

			/// <summary>
			/// CSS style dictionary for any index elements.
			/// </summary>
			public readonly Dictionary<string, string> index = new Dictionary<string, string>();

			/// <summary>
			/// CSS style dictionary for any textual index elements.
			/// </summary>
			public readonly Dictionary<string, string> vertexCircle = new Dictionary<string, string>();

			/// <summary>
			/// CSS style dictionary for the textual vertex index elements.
			/// </summary>
			public readonly Dictionary<string, string> vertexIndex = new Dictionary<string, string>();

			/// <summary>
			/// CSS style dictionary for the edge path elements.
			/// </summary>
			public readonly Dictionary<string, string> edgePath = new Dictionary<string, string>();

			/// <summary>
			/// CSS style dictionary for the textual edge index elements.
			/// </summary>
			public readonly Dictionary<string, string> edgeIndex = new Dictionary<string, string>();

			/// <summary>
			/// CSS style dictionary for the face polygon elements.
			/// </summary>
			public readonly Dictionary<string, string> facePolygon = new Dictionary<string, string>();

			/// <summary>
			/// CSS style dictionary for the textual face index elements.
			/// </summary>
			public readonly Dictionary<string, string> faceIndex = new Dictionary<string, string>();

			/// <summary>
			/// The radius of the circle drawn around vertices.
			/// </summary>
			public float vertexCircleRadius = 0.05f;

			/// <summary>
			/// The inset distance from the outer edge of a face to the polygon that is draw inside a face.
			/// </summary>
			public float faceInset = 0.1f;

			/// <summary>
			/// The distance between the drawn lines of half-edge twins.
			/// </summary>
			public float edgeSeparation = 0.025f;

			/// <summary>
			/// The vector describing the half-arrow drawn at the tip of a half-edge, with the x axis corresponding to the axial length of arrow and the y axis corresponding to the width.
			/// </summary>
			public Vector2 edgeArrowOffset = new Vector2(0.035f, 0.025f);

			/// <summary>
			/// The distance from the drawn half-edge line where its numerical index should be placed.
			/// </summary>
			public float edgeIndexOffset = 0.025f;

			/// <summary>
			/// The amount of padding, in pixels, around the whole manifold.
			/// </summary>
			public Vector2 padding = new Vector2(10f, 10f);

			/// <summary>
			/// Indicates if the numerical face indices should be drawn at the middle of each internal face.
			/// </summary>
			public bool showFaceIndices = true;

			/// <summary>
			/// Indicates if the numerical vertex indices should be drawn inside the circle of each vertex.
			/// </summary>
			public bool showVertexIndices = true;

			/// <summary>
			/// Indicates if the numerical edge indices should be drawn to the side of each half-edge.
			/// </summary>
			public bool showEdgeIndices = true;

			/// <summary>
			/// Constructs a default instance of <see cref="SVGStyle"/>.
			/// </summary>
			public SVGStyle()
			{
			}

			/// <summary>
			/// Constructs an instance of <see cref="SVGStyle"/> with the given properties.
			/// </summary>
			/// <param name="vertexCircleRadius">The radius of the circle drawn around vertices.</param>
			/// <param name="faceInset">The inset distance from the outer edge of a face to the polygon that is draw inside a face.</param>
			/// <param name="edgeSeparation">The distance between the drawn lines of half-edge twins.</param>
			/// <param name="edgeArrowOffset">The vector describing the half-arrow drawn at the tip of a half-edge.</param>
			/// <param name="edgeIndexOffset">The distance from the drawn half-edge line where its numerical index should be placed.</param>
			/// <param name="padding">The amount of padding, in pixels, around the whole manifold.</param>
			/// <param name="showFaceIndices">Indicates if the numerical face indices should be drawn at the middle of each internal face.</param>
			/// <param name="showVertexIndices">Indicates if the numerical vertex indices should be drawn inside the circle of each vertex.</param>
			/// <param name="showEdgeIndices">Indicates if the numerical edge indices should be drawn to the side of each half-edge.</param>
			public SVGStyle(float vertexCircleRadius, float faceInset, float edgeSeparation, Vector2 edgeArrowOffset, float edgeIndexOffset, Vector2 padding, bool showFaceIndices, bool showVertexIndices, bool showEdgeIndices)
			{
				this.vertexCircleRadius = vertexCircleRadius;

				this.faceInset = faceInset;

				this.edgeSeparation = edgeSeparation;
				this.edgeArrowOffset = edgeArrowOffset;
				this.edgeIndexOffset = edgeIndexOffset;

				this.padding = padding;

				this.showFaceIndices = showFaceIndices;
				this.showVertexIndices = showVertexIndices;
				this.showEdgeIndices = showEdgeIndices;
			}
		}

		private static void ExportEdgeToSVG(System.IO.TextWriter writer, ISurface surface, Topology.FaceEdge edge, Topology.FaceEdge nearVertexFaceEdge, Topology.FaceEdge farVertexFaceEdge, Func<Vector3, Vector2> flatten, IVertexAttribute<Vector3> vertexPositions, string arrowFormat, string edgeIndexFormat, SVGStyle style, string additionalClasses)
		{
			var p0 = vertexPositions[nearVertexFaceEdge];
			var p1 = vertexPositions[farVertexFaceEdge];
			var n0 = surface.GetNormal(p0);
			var n1 = surface.GetNormal(p1);
			var edgeDirection = (p1 - p0).normalized;
			var edgeNormal0 = (n0 + n1).normalized;
			var edgeNormal1 = Vector3.Cross(edgeDirection, edgeNormal0);

			var offset0 = edgeDirection * (style.vertexCircleRadius + style.edgeSeparation);
			var offset1 = edgeNormal1 * style.edgeSeparation * 0.5f;
			var offset2 = -edgeDirection * style.edgeArrowOffset.x + edgeNormal1 * style.edgeArrowOffset.y;

			Vector2 arrow0 = flatten(p0 + offset0 + offset1);
			Vector2 arrow1 = flatten(p1 - offset0 + offset1);
			Vector2 arrow2 = flatten(p1 - offset0 + offset1 + offset2);

			writer.WriteLine(arrowFormat, arrow0.x, arrow0.y, arrow1.x, arrow1.y, arrow2.x, arrow2.y, additionalClasses);

			if (style.showEdgeIndices)
			{
				var edgeCenter = flatten((p0 + p1) * 0.5f + edgeNormal1 * (style.edgeSeparation * 0.5f + style.edgeIndexOffset));
				writer.WriteLine(edgeIndexFormat, edgeCenter.x, edgeCenter.y, edge.index, additionalClasses);
			}
		}

		private static void ExportVertexToSVG(System.IO.TextWriter writer, Topology.Vertex vertex, Topology.FaceEdge edge, Func<Vector3, Vector2> flatten, IVertexAttribute<Vector3> vertexPositions, Vector2 transformedVertexDotRadius, string vertexFormat, string vertexIndexFormat, SVGStyle style, string additionalClasses)
		{
			var p = flatten(vertexPositions[edge]);
			writer.WriteLine(vertexFormat, p.x, p.y, transformedVertexDotRadius.x, transformedVertexDotRadius.y, additionalClasses);

			if (style.showVertexIndices)
			{
				writer.WriteLine(vertexIndexFormat, p.x, p.y, vertex.index, additionalClasses);
			}
		}

		/// <summary>
		/// Exports the given manifold to SVG format with the given style, flattening it onto a plane if necessary.
		/// </summary>
		/// <param name="writer">The writer to which the SVG-formatted manifold will be written.</param>
		/// <param name="surface">The surface describing the manifold.</param>
		/// <param name="topology">The topology of the manifold.</param>
		/// <param name="orientation">The orientation of the plane to which the manifold will be flattened.</param>
		/// <param name="scale">The scale which will be applied to the manifold while writing out SVG positions.</param>
		/// <param name="vertexPositions">The vertex positions of the manifold.</param>
		/// <param name="numericFormat">The format specifier to use for each number written to <paramref name="writer"/>.</param>
		/// <param name="style">The style details to apply to the SVG content.</param>
		public static void ExportToSVG(System.IO.TextWriter writer, ISurface surface, Topology topology, Quaternion orientation, Vector3 scale, IVertexAttribute<Vector3> vertexPositions, string numericFormat, SVGStyle style)
		{
			var inverseOrientation = Quaternion.Inverse(orientation);

			var plane = new Plane(orientation * Vector3.back, 0f);

			var transform = Matrix4x4.TRS(Vector3.zero, inverseOrientation, scale);

			Vector2 transformedVertexDotRadius = new Vector2(style.vertexCircleRadius * transform.m00, style.vertexCircleRadius * transform.m11);

			Func<Vector3, Vector2> flatten = (Vector3 point) => transform * plane.ClosestPoint(point);

			Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
			Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
			foreach (var face in topology.faces)
			{
				foreach (var edge in face.edges)
				{
					var p = flatten(vertexPositions[edge]);
					min = Geometry.AxisAlignedMin(min, p);
					max = Geometry.AxisAlignedMax(max, p);
				}
			}

			var range = max - min;

			flatten = (Vector3 point) =>
			{
				Vector2 p = transform * plane.ClosestPoint(point);
				p.y = max.y - p.y + min.y;
				return p;
			};

			writer.WriteLine("<svg viewBox=\"{0} {1} {2} {3}\" xmlns=\"http://www.w3.org/2000/svg\">",
				(min.x - style.padding.x).ToString(numericFormat), (min.y - style.padding.y).ToString(numericFormat),
				(range.x + style.padding.x * 2f).ToString(numericFormat), (range.y + style.padding.y * 2f).ToString(numericFormat));

			writer.WriteLine("\t<style>");

			writer.WriteLine("\t\tsvg");
			writer.WriteLine("\t\t{");
			if (!style.svg.ContainsKey("width")) writer.WriteLine("\t\t\twidth: 100%;");
			if (!style.svg.ContainsKey("height")) writer.WriteLine("\t\t\theight: 100%;");
			foreach (var keyValue in style.svg)
			{
				writer.WriteLine("\t\t\t{0}: {1};", keyValue.Key, keyValue.Value);
			}
			writer.WriteLine("\t\t}");

			writer.WriteLine("\t\ttext.index");
			writer.WriteLine("\t\t{");
			foreach (var keyValue in style.index)
			{
				writer.WriteLine("\t\t\t{0}: {1};", keyValue.Key, keyValue.Value);
			}
			writer.WriteLine("\t\t}");

			writer.WriteLine("\t\tellipse.vertex");
			writer.WriteLine("\t\t{");
			if (!style.vertexCircle.ContainsKey("stroke")) writer.WriteLine("\t\t\tstroke: black;");
			if (!style.vertexCircle.ContainsKey("stroke-width")) writer.WriteLine("\t\t\tstroke-width: 1;");
			if (!style.vertexCircle.ContainsKey("fill")) writer.WriteLine("\t\t\tfill: none;");
			foreach (var keyValue in style.vertexCircle)
			{
				writer.WriteLine("\t\t\t{0}: {1};", keyValue.Key, keyValue.Value);
			}
			writer.WriteLine("\t\t}");

			writer.WriteLine("\t\ttext.vertex.index");
			writer.WriteLine("\t\t{");
			if (!style.vertexIndex.ContainsKey("fill")) writer.WriteLine("\t\t\tfill: black;");
			if (!style.vertexIndex.ContainsKey("font-size")) writer.WriteLine("\t\t\tfont-size: {0:F0}px;", transformedVertexDotRadius.x);
			if (!style.vertexIndex.ContainsKey("text-anchor")) writer.WriteLine("\t\t\ttext-anchor: middle;");
			if (!style.vertexIndex.ContainsKey("dominant-baseline")) writer.WriteLine("\t\t\tdominant-baseline: central;");
			foreach (var keyValue in style.vertexIndex)
			{
				writer.WriteLine("\t\t\t{0}: {1};", keyValue.Key, keyValue.Value);
			}
			writer.WriteLine("\t\t}");

			writer.WriteLine("\t\tpolygon.face");
			writer.WriteLine("\t\t{");
			if (!style.facePolygon.ContainsKey("fill")) writer.WriteLine("\t\t\tfill: #CCC;");
			if (!style.facePolygon.ContainsKey("stroke")) writer.WriteLine("\t\t\tstroke: none;");
			foreach (var keyValue in style.facePolygon)
			{
				writer.WriteLine("\t\t\t{0}: {1};", keyValue.Key, keyValue.Value);
			}
			writer.WriteLine("\t\t}");

			writer.WriteLine("\t\ttext.face.index");
			writer.WriteLine("\t\t{");
			if (!style.faceIndex.ContainsKey("fill")) writer.WriteLine("\t\t\tfill: black;");
			if (!style.faceIndex.ContainsKey("font-size")) writer.WriteLine("\t\t\tfont-size: {0:F0}px;", transformedVertexDotRadius.x * 2f);
			if (!style.faceIndex.ContainsKey("text-anchor")) writer.WriteLine("\t\t\ttext-anchor: middle;");
			if (!style.faceIndex.ContainsKey("dominant-baseline")) writer.WriteLine("\t\t\tdominant-baseline: central;");
			foreach (var keyValue in style.faceIndex)
			{
				writer.WriteLine("\t\t\t{0}: {1};", keyValue.Key, keyValue.Value);
			}
			writer.WriteLine("\t\t}");

			writer.WriteLine("\t\tpolyline.edge");
			writer.WriteLine("\t\t{");
			if (!style.edgePath.ContainsKey("stroke")) writer.WriteLine("\t\t\tstroke: black;");
			if (!style.edgePath.ContainsKey("stroke-width")) writer.WriteLine("\t\t\tstroke-width: 1;");
			if (!style.edgePath.ContainsKey("stroke-linecap")) writer.WriteLine("\t\t\tstroke-linecap: square;");
			if (!style.edgePath.ContainsKey("stroke-linejoin")) writer.WriteLine("\t\t\tstroke-linejoin: miter;");
			if (!style.edgePath.ContainsKey("fill")) writer.WriteLine("\t\t\tfill: none;");
			foreach (var keyValue in style.edgePath)
			{
				writer.WriteLine("\t\t\t{0}: {1};", keyValue.Key, keyValue.Value);
			}
			writer.WriteLine("\t\t}");

			writer.WriteLine("\t\ttext.edge.index");
			writer.WriteLine("\t\t{");
			if (!style.edgeIndex.ContainsKey("fill")) writer.WriteLine("\t\t\tfill: black;");
			if (!style.edgeIndex.ContainsKey("font-size")) writer.WriteLine("\t\t\tfont-size: {0:F0}px;", style.edgeIndexOffset * Mathf.Sqrt(transform.m00 * transform.m11));
			if (!style.edgeIndex.ContainsKey("text-anchor")) writer.WriteLine("\t\t\ttext-anchor: middle;");
			if (!style.edgeIndex.ContainsKey("dominant-baseline")) writer.WriteLine("\t\t\tdominant-baseline: central;");
			foreach (var keyValue in style.edgeIndex)
			{
				writer.WriteLine("\t\t\t{0}: {1};", keyValue.Key, keyValue.Value);
			}
			writer.WriteLine("\t\t}");

			writer.WriteLine("\t</style>");

			var facePointFormat = string.Format("{{0:{0}}},{{1:{0}}}", numericFormat);
			var faceIndexFormat = string.Format("\t<text class=\"face index\" x=\"{{0:{0}}}\" y=\"{{1:{0}}}\">{{2}}</text>", numericFormat);
			var arrowFormat = string.Format("\t<polyline class=\"edge{{6}}\" points=\"{{0:{0}}},{{1:{0}}} {{2:{0}}},{{3:{0}}} {{4:{0}}},{{5:{0}}}\" />", numericFormat);
			var edgeIndexFormat = string.Format("\t<text class=\"edge index{{3}}\" x=\"{{0:{0}}}\" y=\"{{1:{0}}}\">{{2}}</text>", numericFormat);
			var vertexFormat = string.Format("\t<ellipse class=\"vertex{{4}}\" cx=\"{{0:{0}}}\" cy=\"{{1:{0}}}\" rx=\"{{2:{0}}}\" ry=\"{{3:{0}}}\" />", numericFormat);
			var vertexIndexFormat = string.Format("\t<text class=\"vertex index{{3}}\" x=\"{{0:{0}}}\" y=\"{{1:{0}}}\">{{2}}</text>", numericFormat);

			var centroids = FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.internalFaces, vertexPositions);
			var bisectors = EdgeAttributeUtility.CalculateFaceEdgeBisectorsFromVertexPositions(topology.internalFaces, surface, vertexPositions, centroids);

			writer.WriteLine();
			writer.WriteLine("\t<!-- Faces -->");

			foreach (var face in topology.internalFaces)
			{
				writer.Write("\t<polygon class=\"face\" points=\"");
				foreach (var edge in face.edges)
				{
					if (edge != face.firstEdge) writer.Write(" ");
					var p = flatten(vertexPositions[edge] + bisectors[edge] * style.faceInset);
					writer.Write(facePointFormat, p.x, p.y);
				}
				writer.WriteLine("\" />");

				if (style.showFaceIndices)
				{
					var p = flatten(centroids[face]);
					writer.WriteLine(faceIndexFormat, p.x, p.y, face.index);
				}
			}

			writer.WriteLine();
			writer.WriteLine("\t<!-- Edges -->");

			foreach (var face in topology.faces)
			{
				foreach (var edge in face.edges)
				{
					ExportEdgeToSVG(writer, surface, edge, edge.prev, edge, flatten, vertexPositions, arrowFormat, edgeIndexFormat, style, " not-wrapped");
					if ((edge.wrap & EdgeWrap.FaceToFace) != EdgeWrap.None)
					{
						ExportEdgeToSVG(writer, surface, edge, edge.twin, edge.twin.vertexEdge.next.twin.faceEdge, flatten, vertexPositions, arrowFormat, edgeIndexFormat, style, " wrapped");
					}
				}
			}

			writer.WriteLine();
			writer.WriteLine("\t<!-- Vertices -->");

			foreach (var vertex in topology.vertices)
			{
				bool neitherAxis = false;
				bool axis0 = false;
				bool axis1 = false;
				bool bothAxes = false;
				foreach (var edge in vertex.edges)
				{
					var faceEdge = edge.twin.faceEdge;
					var wrap = faceEdge.wrap & EdgeWrap.FaceToVert;
					if (EdgeWrapUtility.WrapsOnNeitherAxis(wrap))
					{
						if (!neitherAxis)
						{
							neitherAxis = true;
							ExportVertexToSVG(writer, vertex, faceEdge, flatten, vertexPositions, transformedVertexDotRadius, vertexFormat, vertexIndexFormat, style, " wrapped-neither");
						}
					}
					else if (EdgeWrapUtility.WrapsOnOnlyAxis0(wrap))
					{
						if (!axis0)
						{
							axis0 = true;
							ExportVertexToSVG(writer, vertex, faceEdge, flatten, vertexPositions, transformedVertexDotRadius, vertexFormat, vertexIndexFormat, style, " wrapped-axis-0");
						}
					}
					else if (EdgeWrapUtility.WrapsOnOnlyAxis1(wrap))
					{
						if (!axis1)
						{
							axis1 = true;
							ExportVertexToSVG(writer, vertex, faceEdge, flatten, vertexPositions, transformedVertexDotRadius, vertexFormat, vertexIndexFormat, style, " wrapped-axis-1");
						}
					}
					else
					{
						if (!bothAxes)
						{
							bothAxes = true;
							ExportVertexToSVG(writer, vertex, faceEdge, flatten, vertexPositions, transformedVertexDotRadius, vertexFormat, vertexIndexFormat, style, " wrapped-both");
						}
					}
				}
			}

			writer.WriteLine("</svg>");
		}

#endregion
	}
}
#endif