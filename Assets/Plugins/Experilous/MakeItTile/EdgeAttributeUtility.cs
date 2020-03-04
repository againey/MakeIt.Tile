/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using Experilous.Numerics;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// A static utility class for calculating various per-edge attributes.
	/// </summary>
	public static class EdgeAttributeUtility
	{
		#region Calculate[VertexEdgeAttribute]...(...)

		/// <summary>
		/// Calculates the angles, as radians, between the vectors from vertex positions to the positions of neighboring pairs of vertices.
		/// </summary>
		/// <param name="vertexEdges">The collection of vertex edges whose vertex angles are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <returns>The vertex angles of the vertices, as radians.</returns>
		/// <remarks><para>For each edge, the two vectors used to determine the angle between them are those that
		/// start at the current vertex position and end at the positions of the current edge's far vertex and the
		/// next edge's far vertex, respectively.</para></remarks>
		public static IEdgeAttribute<float> CalculateVertexAnglesFromVertexPositions(Topology.VertexEdgesIndexer vertexEdges, IVertexAttribute<Vector3> vertexPositions)
		{
			return CalculateVertexAnglesFromVertexPositions(vertexEdges, vertexPositions, new float[vertexEdges.Count].AsEdgeAttribute());
		}

		/// <summary>
		/// Calculates the angles, as radians, between the vectors from vertex positions to the positions of neighboring pairs of vertices.
		/// </summary>
		/// <param name="vertexEdges">The collection of vertex edges whose vertex angles are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="vertexAngles">A pre-allocated collection in which the vertex angles will be stored.</param>
		/// <returns>The vertex angles of the vertices, as radians.</returns>
		/// <remarks><para>For each edge, the two vectors used to determine the angle between them are those that
		/// start at the current vertex position and end at the positions of the current edge's far vertex and the
		/// next edge's far vertex, respectively.</para></remarks>
		public static IEdgeAttribute<float> CalculateVertexAnglesFromVertexPositions(Topology.VertexEdgesIndexer vertexEdges, IVertexAttribute<Vector3> vertexPositions, IEdgeAttribute<float> vertexAngles)
		{
			foreach (var vertexEdge in vertexEdges)
			{
				var p0 = vertexPositions[vertexEdge.next.farVertex];
				var p1 = vertexPositions[vertexEdge.nearVertex];
				var p2 = vertexPositions[vertexEdge.farVertex];
				vertexAngles[vertexEdge] = Geometry.AngleBetweenVectors(p1 - p0, p2 - p0);
			}

			return vertexAngles;
		}

		/// <summary>
		/// Calculates the length of vertex edges based on the positions of the near and far vertices of each edge.
		/// </summary>
		/// <param name="vertexEdges">The collection of vertex edges whose lengths are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <returns>The lengths of the vertex edges.</returns>
		public static IEdgeAttribute<float> CalculateVertexEdgeLengthsFromVertexPositions(Topology.VertexEdgesIndexer vertexEdges, IVertexAttribute<Vector3> vertexPositions)
		{
			return CalculateVertexEdgeLengthsFromVertexPositions(vertexEdges, vertexPositions, new float[vertexEdges.Count].AsEdgeAttribute());
		}

		/// <summary>
		/// Calculates the length of vertex edges based on the positions of the near and far vertices of each edge.
		/// </summary>
		/// <param name="vertexEdges">The collection of vertex edges whose lengths are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="vertexEdgeLengths">A pre-allocated collection in which the lengths will be stored.</param>
		/// <returns>The lengths of the vertex edges.</returns>
		public static IEdgeAttribute<float> CalculateVertexEdgeLengthsFromVertexPositions(Topology.VertexEdgesIndexer vertexEdges, IVertexAttribute<Vector3> vertexPositions, IEdgeAttribute<float> vertexEdgeLengths)
		{
			foreach (var vertexEdge in vertexEdges)
			{
				vertexEdgeLengths[vertexEdge] = Vector3.Distance(vertexPositions[vertexEdge.nearVertex], vertexPositions[vertexEdge.farVertex]);
			}

			return vertexEdgeLengths;
		}

		/// <summary>
		/// Calculates the midpoint positions of vertex edges based on the positions of the near and far vertices of each edge.
		/// </summary>
		/// <param name="vertexEdges">The collection of vertex edges whose midpoints are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <returns>The midpoint positions of the vertex edges.</returns>
		public static IEdgeAttribute<Vector3> CalculateVertexEdgeMidpointsFromVertexPositions(Topology.VertexEdgesIndexer vertexEdges, IVertexAttribute<Vector3> vertexPositions)
		{
			return CalculateVertexEdgeMidpointsFromVertexPositions(vertexEdges, vertexPositions, new Vector3[vertexEdges.Count].AsEdgeAttribute());
		}

		/// <summary>
		/// Calculates the midpoint positions of vertex edges based on the positions of the near and far vertices of each edge.
		/// </summary>
		/// <param name="vertexEdges">The collection of vertex edges whose midpoints are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="vertexEdgeMidpoints">A pre-allocated collection in which the midpoints will be stored.</param>
		/// <returns>The midpoint positions of the vertex edges.</returns>
		public static IEdgeAttribute<Vector3> CalculateVertexEdgeMidpointsFromVertexPositions(Topology.VertexEdgesIndexer vertexEdges, IVertexAttribute<Vector3> vertexPositions, IEdgeAttribute<Vector3> vertexEdgeMidpoints)
		{
			foreach (var vertexEdge in vertexEdges)
			{
				vertexEdgeMidpoints[vertexEdge] = (vertexPositions[vertexEdge.nearVertex] + vertexPositions[vertexEdge.farVertex]) * 0.5f;
			}

			return vertexEdgeMidpoints;
		}

		#endregion

		#region Calculate[FaceEdgeAttribute]...(...)

		/// <summary>
		/// Calculates the angles, as radians, between the vectors from the positions of neighboring vertices to the positions of the previous and next neighboring vertices.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose interior angles are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <returns>The interior angles of the vertices, as radians.</returns>
		/// <remarks><para>For each edge, the two vectors used to determine the angle between them are those that
		/// start at the position of the edge's previous vertex and end at the position of the previous edge's previous
		/// vertex and the current edge's next vertex, respectively.</para></remarks>
		public static IEdgeAttribute<float> CalculateFaceEdgeInteriorAnglesFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IVertexAttribute<Vector3> vertexPositions)
		{
			return CalculateFaceEdgeInteriorAnglesFromVertexPositions(faceEdges, vertexPositions, new float[faceEdges.Count].AsEdgeAttribute());
		}

		/// <summary>
		/// Calculates the angles, as radians, between the vectors from the positions of neighboring vertices to the positions of the previous and next neighboring vertices.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose interior angles are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="faceAngles">A pre-allocated collection in which the interior angles will be stored.</param>
		/// <returns>The interior angles of the vertices, as radians.</returns>
		/// <remarks><para>For each edge, the two vectors used to determine the angle between them are those that
		/// start at the position of the edge's previous vertex and end at the position of the previous edge's previous
		/// vertex and the current edge's next vertex, respectively.</para></remarks>
		public static IEdgeAttribute<float> CalculateFaceEdgeInteriorAnglesFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IVertexAttribute<Vector3> vertexPositions, IEdgeAttribute<float> faceAngles)
		{
			foreach (var faceEdge in faceEdges)
			{
				var p0 = vertexPositions[faceEdge.prev.prevVertex];
				var p1 = vertexPositions[faceEdge.prevVertex];
				var p2 = vertexPositions[faceEdge.nextVertex];
				faceAngles[faceEdge] = Geometry.AngleBetweenVectors(p1 - p0, p2 - p0);
			}

			return faceAngles;
		}

		/// <summary>
		/// Calculates the angles, as radians, between the vectors from face centroids to the centroids of neighboring pairs of faces.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose face centroid angles are to be calculated.</param>
		/// <param name="faceCentroids">The centroids of the faces.</param>
		/// <returns>The face centroid angles of the vertices, as radians.</returns>
		/// <remarks><para>For each edge, the two vectors used to determine the angle between them are those that
		/// start at the centroid of the edge's near face and end at the centroid of the previous edge's far face
		/// and the current edge's far face, respectively.</para></remarks>
		public static IEdgeAttribute<float> CalculateFaceEdgeArcsFromFaceCentroids(Topology.FaceEdgesIndexer faceEdges, IFaceAttribute<Vector3> faceCentroids)
		{
			return CalculateFaceEdgeArcsFromFaceCentroids(faceEdges, faceCentroids, new float[faceEdges.Count].AsEdgeAttribute());
		}

		/// <summary>
		/// Calculates the angles, as radians, between the vectors from face centroids to the centroids of neighboring pairs of faces.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose face centroid angles are to be calculated.</param>
		/// <param name="faceCentroids">The centroids of the faces.</param>
		/// <param name="faceAngles">A pre-allocated collection in which the face centroid angles will be stored.</param>
		/// <returns>The face centroid angles of the vertices, as radians.</returns>
		/// <remarks><para>For each edge, the two vectors used to determine the angle between them are those that
		/// start at the centroid of the edge's near face and end at the centroid of the previous edge's far face
		/// and the current edge's far face, respectively.</para></remarks>
		public static IEdgeAttribute<float> CalculateFaceEdgeArcsFromFaceCentroids(Topology.FaceEdgesIndexer faceEdges, IFaceAttribute<Vector3> faceCentroids, IEdgeAttribute<float> faceAngles)
		{
			foreach (var faceEdge in faceEdges)
			{
				var p0 = faceCentroids[faceEdge.prev.farFace];
				var p1 = faceCentroids[faceEdge.nearFace];
				var p2 = faceCentroids[faceEdge.farFace];
				faceAngles[faceEdge] = Geometry.AngleBetweenVectors(p0 - p1, p2 - p1);
			}

			return faceAngles;
		}

		/// <summary>
		/// Calculates the bisector vectors of the interior angles of face corners, given the positions of vertices.
		/// </summary>
		/// <param name="faceEdges">The face edges whose bisector vectors are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <returns>The bisector vectors of the interior face corners.</returns>
		/// <remarks><note type="important">All faces are presumed to be convex.  If this is not the
		/// case, the bisectors of concave corners will face in the opposite direction, and any colinear
		/// edges will experience division-by-zero issues.</note></remarks>
		public static IEdgeAttribute<Vector3> CalculateFaceEdgeBisectorsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IEdgeAttribute<Vector3> vertexPositions)
		{
			return CalculateFaceEdgeBisectorsFromVertexPositions(faceEdges, vertexPositions, new Vector3[faceEdges.Count].AsEdgeAttribute());
		}

		/// <summary>
		/// Calculates the bisector vectors of the interior angles of face corners, given the positions of vertices.
		/// </summary>
		/// <param name="faceEdges">The face edges whose bisector vectors are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="bisectors">A pre-allocated collection in which the bisector vectors of the interior face corners will be stored.</param>
		/// <returns>The bisector vectors of the interior face corners.</returns>
		/// <remarks><note type="important">All faces are presumed to be convex.  If this is not the
		/// case, the bisectors of concave corners will face in the opposite direction, and any colinear
		/// edges will experience division-by-zero issues.</note></remarks>
		public static IEdgeAttribute<Vector3> CalculateFaceEdgeBisectorsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IEdgeAttribute<Vector3> vertexPositions, IEdgeAttribute<Vector3> bisectors)
		{
			foreach (var edge in faceEdges)
			{
				var p0 = vertexPositions[edge.prev];
				var p1 = vertexPositions[edge];
				var p2 = vertexPositions[edge.next];
				var v1to0 = (p0 - p1).normalized;
				var v1to2 = (p2 - p1).normalized;
				var sum = v1to0 + v1to2;
				bisectors[edge] = sum.normalized;
			}

			return bisectors;
		}

		/// <summary>
		/// Calculates the bisector vectors of the interior angles of face corners, given the positions of vertices and global planar normal.
		/// </summary>
		/// <param name="faces">The faces whose corners are to have their bisector vectors calculated.</param>
		/// <param name="surface">The surface describing the overall shape of the planar manifold.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <returns>The bisector vectors of the interior face corners.</returns>
		public static IEdgeAttribute<Vector3> CalculateFaceEdgeBisectorsFromVertexPositions(Topology.FacesIndexer faces, PlanarSurface surface, IEdgeAttribute<Vector3> vertexPositions)
		{
			if (faces.Count > 0)
			{
				return CalculateFaceEdgeBisectorsFromVertexPositions(faces, surface, vertexPositions, new Vector3[faces[0].topology.faceEdges.Count].AsEdgeAttribute());
			}
			else
			{
				return new Vector3[0].AsEdgeAttribute();
			}
		}

		/// <summary>
		/// Calculates the bisector vectors of the interior angles of face corners, given the positions of vertices and global planar normal.
		/// </summary>
		/// <param name="faces">The faces whose corners are to have their bisector vectors calculated.</param>
		/// <param name="surface">The surface describing the overall shape of the planar manifold.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="bisectors">A pre-allocated collection in which the bisector vectors of the interior face corners will be stored.</param>
		/// <returns>The bisector vectors of the interior face corners.</returns>
		public static IEdgeAttribute<Vector3> CalculateFaceEdgeBisectorsFromVertexPositions(Topology.FacesIndexer faces, PlanarSurface surface, IEdgeAttribute<Vector3> vertexPositions, IEdgeAttribute<Vector3> bisectors)
		{
			var normal = surface.normal;

			foreach (var face in faces)
			{
				var edge = face.firstEdge;
				var firstEdge = edge;
				var p0 = vertexPositions[edge];
				edge = edge.next;
				var p1 = vertexPositions[edge];
				var edgeNormal01 = Vector3.Cross(normal, p1 - p0).normalized;
				var firstLastEdgeNormal = edgeNormal01;
				var nextEdge = edge.next;

				do
				{
					var p2 = vertexPositions[nextEdge];
					var edgeNormal12 = Vector3.Cross(normal, p2 - p1).normalized;
					bisectors[edge] = (edgeNormal01 + edgeNormal12).normalized;
					p0 = p1;
					p1 = p2;
					edge = nextEdge;
					nextEdge = nextEdge.next;
					edgeNormal01 = edgeNormal12;
				} while (edge != firstEdge);

				bisectors[firstEdge] = (edgeNormal01 + firstLastEdgeNormal).normalized;
			}

			return bisectors;
		}

		/// <summary>
		/// Calculates the bisector vectors of the interior angles of face corners, given the positions of vertices and global planar normal.
		/// </summary>
		/// <param name="faces">The faces whose corners are to have their bisector vectors calculated.</param>
		/// <param name="surface">The surface describing the overall shape of the planar manifold.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="facePositions">The positions of the faces.</param>
		/// <returns>The bisector vectors of the interior face corners.</returns>
		public static IEdgeAttribute<Vector3> CalculateFaceEdgeBisectorsFromVertexPositions(Topology.FacesIndexer faces, ISurface surface, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions)
		{
			if (faces.Count > 0)
			{
				return CalculateFaceEdgeBisectorsFromVertexPositions(faces, surface, vertexPositions, facePositions, new Vector3[faces[0].topology.faceEdges.Count].AsEdgeAttribute());
			}
			else
			{
				return new Vector3[0].AsEdgeAttribute();
			}
		}

		/// <summary>
		/// Calculates the bisector vectors of the interior angles of face corners, given the surface descriptor and positions of vertices and faces.
		/// </summary>
		/// <param name="faces">The faces whose corners are to have their bisector vectors calculated.</param>
		/// <param name="surface">The surface describing the overall shape of the manifold.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="facePositions">The positions of the faces.</param>
		/// <param name="bisectors">A pre-allocated collection in which the bisector vectors of the interior face corners will be stored.</param>
		/// <returns>The bisector vectors of the interior face corners.</returns>
		public static IEdgeAttribute<Vector3> CalculateFaceEdgeBisectorsFromVertexPositions(Topology.FacesIndexer faces, ISurface surface, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, IEdgeAttribute<Vector3> bisectors)
		{
			foreach (var face in faces)
			{
				var normal = surface.GetNormal(facePositions[face]);

				var edge = face.firstEdge;
				var firstEdge = edge;
				var p0 = vertexPositions[edge];
				edge = edge.next;
				var p1 = vertexPositions[edge];
				var edgeNormal01 = Vector3.Cross(normal, p1 - p0).normalized;
				var firstLastEdgeNormal = edgeNormal01;
				var nextEdge = edge.next;

				do
				{
					var p2 = vertexPositions[nextEdge];
					var edgeNormal12 = Vector3.Cross(normal, p2 - p1).normalized;
					bisectors[edge] = (edgeNormal01 + edgeNormal12).normalized;
					p0 = p1;
					p1 = p2;
					edge = nextEdge;
					nextEdge = nextEdge.next;
					edgeNormal01 = edgeNormal12;
				} while (edge != firstEdge);

				bisectors[firstEdge] = (edgeNormal01 + firstLastEdgeNormal).normalized;
			}

			return bisectors;
		}

		/// <summary>
		/// Calculates the bisector vectors of the interior angles of face corners, given the positions of vertices and normals of the faces.
		/// </summary>
		/// <param name="faces">The faces whose corners are to have their bisector vectors calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="faceNormals">The surface normals of the faces.</param>
		/// <returns>The bisector vectors of the interior face corners.</returns>
		public static IEdgeAttribute<Vector3> CalculateFaceEdgeBisectorsFromVertexPositions(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> faceNormals)
		{
			if (faces.Count > 0)
			{
				return CalculateFaceEdgeBisectorsFromVertexPositions(faces, vertexPositions, faceNormals, new Vector3[faces[0].topology.faceEdges.Count].AsEdgeAttribute());
			}
			else
			{
				return new Vector3[0].AsEdgeAttribute();
			}
		}

		/// <summary>
		/// Calculates the bisector vectors of the interior angles of face corners, given the positions of vertices and normals of the faces.
		/// </summary>
		/// <param name="faces">The faces whose corners are to have their bisector vectors calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="faceNormals">The surface normals of the faces.</param>
		/// <param name="bisectors">A pre-allocated collection in which the bisector vectors of the interior face corners will be stored.</param>
		/// <returns>The bisector vectors of the interior face corners.</returns>
		public static IEdgeAttribute<Vector3> CalculateFaceEdgeBisectorsFromVertexPositions(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> faceNormals, IEdgeAttribute<Vector3> bisectors)
		{
			foreach (var face in faces)
			{
				var normal = faceNormals[face];
				var edge = face.firstEdge;
				var firstEdge = edge;
				var p0 = vertexPositions[edge];
				edge = edge.next;
				var p1 = vertexPositions[edge];
				var edgeNormal01 = Vector3.Cross(normal, p1 - p0).normalized;
				var firstLastEdgeNormal = edgeNormal01;
				var nextEdge = edge.next;

				do
				{
					var p2 = vertexPositions[nextEdge];
					var edgeNormal12 = Vector3.Cross(normal, p2 - p1).normalized;
					bisectors[edge] = (edgeNormal01 + edgeNormal12).normalized;
					p0 = p1;
					p1 = p2;
					edge = nextEdge;
					nextEdge = nextEdge.next;
					edgeNormal01 = edgeNormal12;
				} while (edge != firstEdge);

				bisectors[firstEdge] = (edgeNormal01 + firstLastEdgeNormal).normalized;
			}

			return bisectors;
		}

		/// <summary>
		/// Calculates the spherical angles, as radians, between the vectors from face centroids to the centroids of neighboring pairs of faces.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose face centroid angles are to be calculated.</param>
		/// <param name="surface">The surface describing the overall shape of the spherical manifold.</param>
		/// <param name="faceCentroids">The centroids of the faces.</param>
		/// <returns>The face centroid angles of the vertices, as radians.</returns>
		/// <remarks><para>For each edge, the two vectors used to determine the angle between them are those that
		/// start at the centroid of the edge's near face and end at the centroid of the previous edge's far face
		/// and the current edge's far face, respectively, adjusted to be orthogonal to the surface normal at the
		/// edge's near vertex.</para></remarks>
		public static IEdgeAttribute<float> CalculateSphericalFaceCentroidAnglesFromFaceCentroids(Topology.FaceEdgesIndexer faceEdges, SphericalSurface surface, IFaceAttribute<Vector3> faceCentroids)
		{
			return CalculateSphericalFaceCentroidAnglesFromFaceCentroids(faceEdges, surface, faceCentroids, new float[faceEdges.Count].AsEdgeAttribute());
		}

		/// <summary>
		/// Calculates the spherical angles, as radians, between the vectors from face centroids to the centroids of neighboring pairs of faces.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose face centroid angles are to be calculated.</param>
		/// <param name="surface">The surface describing the overall shape of the spherical manifold.</param>
		/// <param name="faceCentroids">The centroids of the faces.</param>
		/// <param name="faceAngles">A pre-allocated collection in which the face centroid angles will be stored.</param>
		/// <returns>The face centroid angles of the vertices, as radians.</returns>
		/// <remarks><para>For each edge, the two vectors used to determine the angle between them are those that
		/// start at the centroid of the edge's near face and end at the centroid of the previous edge's far face
		/// and the current edge's far face, respectively, adjusted to be orthogonal to the surface normal at the
		/// edge's near vertex.</para></remarks>
		public static IEdgeAttribute<float> CalculateSphericalFaceCentroidAnglesFromFaceCentroids(Topology.FaceEdgesIndexer faceEdges, SphericalSurface surface, IFaceAttribute<Vector3> faceCentroids, IEdgeAttribute<float> faceAngles)
		{
			foreach (var faceEdge in faceEdges)
			{
				var p0 = faceCentroids[faceEdge.prev.farFace];
				var p1 = faceCentroids[faceEdge.nearFace];
				var p2 = faceCentroids[faceEdge.farFace];
				var v10 = Vector3.Cross(Vector3.Cross(p0 - p1, p1), p1);
				var v12 = Vector3.Cross(Vector3.Cross(p2 - p1, p1), p1);

				faceAngles[faceEdge] = Geometry.AngleBetweenVectors(v10, v12);
			}

			return faceAngles;
		}

		/// <summary>
		/// Calculates the length of face edges based on the centroids of the near and far faces of each edge.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose lengths are to be calculated.</param>
		/// <param name="faceCentroids">The centroids of the faces.</param>
		/// <param name="boundaryLength">The length to assign to any boundary edges.</param>
		/// <returns>The lengths of the face edges.</returns>
		/// <remarks><para>Because external faces do not typically have a well defined position, it is
		/// meaningless to calculate the edge length between interior and exterior faces.  Therefore, the
		/// boundary length parameter specifies which value to use for boundary edges, depending on your
		/// use case.  Likely options include zero, infinity, or negative one.</para></remarks>
		public static IEdgeAttribute<float> CalculateFaceEdgeLengthsFromFaceCentroids(Topology.FaceEdgesIndexer faceEdges, IFaceAttribute<Vector3> faceCentroids, float boundaryLength = float.PositiveInfinity)
		{
			return CalculateFaceEdgeLengthsFromFaceCentroids(faceEdges, faceCentroids, boundaryLength, new float[faceEdges.Count].AsEdgeAttribute());
		}

		/// <summary>
		/// Calculates the length of face edges based on the centroids of the near and far faces of each edge.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose lengths are to be calculated.</param>
		/// <param name="faceCentroids">The centroids of the faces.</param>
		/// <param name="boundaryLength">The length to assign to any boundary edges.</param>
		/// <param name="faceEdgeLengths">A pre-allocated collection in which the lengths will be stored.</param>
		/// <returns>The lengths of the face edges.</returns>
		/// <remarks><para>Because external faces do not typically have a well defined position, it is
		/// meaningless to calculate the edge length between interior and exterior faces.  Therefore, the
		/// boundary length parameter specifies which value to use for boundary edges, depending on your
		/// use case.  Likely options include zero, infinity, or negative one.</para></remarks>
		public static IEdgeAttribute<float> CalculateFaceEdgeLengthsFromFaceCentroids(Topology.FaceEdgesIndexer faceEdges, IFaceAttribute<Vector3> faceCentroids, float boundaryLength, IEdgeAttribute<float> faceEdgeLengths)
		{
			foreach (var faceEdge in faceEdges)
			{
				if (!faceEdge.isBoundary)
				{
					faceEdgeLengths[faceEdge] = Vector3.Distance(faceCentroids[faceEdge.nearFace], faceCentroids[faceEdge.farFace]);
				}
				else
				{
					faceEdgeLengths[faceEdge] = boundaryLength;
				}
			}

			return faceEdgeLengths;
		}

		/// <summary>
		/// Calculates the midpoint positions of face edges based on the centroids of the near and far faces of each edge.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose midpoints are to be calculated.</param>
		/// <param name="faceCentroids">The centroids of the faces.</param>
		/// <returns>The midpoint positions of the face edges.</returns>
		/// <remarks><para>Because external faces do not typically have a well defined position, it is meaningless
		/// to calculate the midpoints between interior and exterior faces.  Therefore, midpoints of boundary
		/// edges will simply use the centroid of whichever of their two faces are internal.</para></remarks>
		public static IEdgeAttribute<Vector3> CalculateFaceEdgeMidpointsFromFaceCentroids(Topology.FaceEdgesIndexer faceEdges, IFaceAttribute<Vector3> faceCentroids)
		{
			return CalculateFaceEdgeMidpointsFromFaceCentroids(faceEdges, faceCentroids, new Vector3[faceEdges.Count].AsEdgeAttribute());
		}

		/// <summary>
		/// Calculates the midpoint positions of face edges based on the centroids of the near and far faces of each edge.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose midpoints are to be calculated.</param>
		/// <param name="faceCentroids">The centroids of the faces.</param>
		/// <param name="faceEdgeMidpoints">A pre-allocated collection in which the midpoints will be stored.</param>
		/// <returns>The midpoint positions of the face edges.</returns>
		/// <remarks><para>Because external faces do not typically have a well defined position, it is meaningless
		/// to calculate the midpoints between interior and exterior faces.  Therefore, midpoints of boundary
		/// edges will simply use the centroid of whichever of their two faces are internal.</para></remarks>
		public static IEdgeAttribute<Vector3> CalculateFaceEdgeMidpointsFromFaceCentroids(Topology.FaceEdgesIndexer faceEdges, IFaceAttribute<Vector3> faceCentroids, IEdgeAttribute<Vector3> faceEdgeMidpoints)
		{
			foreach (var faceEdge in faceEdges)
			{
				if (faceEdge.isNonBoundary)
				{
					faceEdgeMidpoints[faceEdge] = (faceCentroids[faceEdge.nearFace] + faceCentroids[faceEdge.farFace]) * 0.5f;
				}
				else if (faceEdge.isInnerBoundary)
				{
					faceEdgeMidpoints[faceEdge] = faceCentroids[faceEdge.farFace];
				}
				else // if (faceEdge.isOuterBoundary)
				{
					faceEdgeMidpoints[faceEdge] = faceCentroids[faceEdge.nearFace];
				}
			}

			return faceEdgeMidpoints;
		}

		/// <summary>
		/// Calculates the three positions for each face corner defining a kite shaped region with two sides with a specified length and orthogonal to the face edges, given vertex positions.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose midpoints are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="orthogonalKiteEdgeLength">The length of the kite edges that are orthogonal to the face edges.  This is essentially the distance from the face's outline that the interior point should be.</param>
		/// <param name="leadingPositions">The kite positions on the face outlines before each face corner.</param>
		/// <param name="followingPositions">The kite positions on the face outlines after each face corner.</param>
		/// <param name="interiorPositions">The kite positions along the interior angle bisectors of each face corner.</param>
		/// <remarks><para>The face corner kite positions are particular useful when attempting to generate outlines around
		/// faces with a particular thickness and rounded corners.  In this case, the orthogonal kite edge length is essentially
		/// both the thickness of the outline and the radius of the circles defining the curved corners.  Drawing an arc between
		/// the leading and following kite positions cenetered on the internal kite position will produce a curve that perfectly
		/// lines up with straight lines draw between the following positions of one edge and the leading positions of the next
		/// edge.</para></remarks>
		public static void CalculateFaceEdgeCornerKitePositionsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IEdgeAttribute<Vector3> vertexPositions, float orthogonalKiteEdgeLength, out IEdgeAttribute<Vector3> leadingPositions, out IEdgeAttribute<Vector3> followingPositions, out IEdgeAttribute<Vector3> interiorPositions)
		{
			leadingPositions = new Vector3[faceEdges.Count].AsEdgeAttribute();
			followingPositions = new Vector3[faceEdges.Count].AsEdgeAttribute();
			interiorPositions = new Vector3[faceEdges.Count].AsEdgeAttribute();
			CalculateFaceEdgeCornerKitePositionsFromVertexPositions(faceEdges, vertexPositions, orthogonalKiteEdgeLength, leadingPositions, followingPositions, interiorPositions);
		}

		/// <summary>
		/// Calculates the three positions for each face corner defining a kite shaped region with two sides with a specified length and orthogonal to the face edges, given vertex positions.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose midpoints are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="orthogonalKiteEdgeLength">The length of the kite edges that are orthogonal to the face edges.  This is essentially the distance from the face's outline that the interior point should be.</param>
		/// <param name="leadingPositions">A pre-allocated collection in which the kite positions on the face outlines before each face corner will be stored.</param>
		/// <param name="followingPositions">A pre-allocated collection in which the kite positions on the face outlines after each face corner will be stored.</param>
		/// <param name="interiorPositions">A pre-allocated collection in which the kite positions along the interior angle bisectors of each face corner will be stored.</param>
		/// <remarks><para>The face corner kite positions are particular useful when attempting to generate outlines around
		/// faces with a particular thickness and rounded corners.  In this case, the orthogonal kite edge length is essentially
		/// both the thickness of the outline and the radius of the circles defining the curved corners.  Drawing an arc between
		/// the leading and following kite positions cenetered on the internal kite position will produce a curve that perfectly
		/// lines up with straight lines draw between the following positions of one edge and the leading positions of the next
		/// edge.</para></remarks>
		public static void CalculateFaceEdgeCornerKitePositionsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IEdgeAttribute<Vector3> vertexPositions, float orthogonalKiteEdgeLength, IEdgeAttribute<Vector3> leadingPositions, IEdgeAttribute<Vector3> followingPositions, IEdgeAttribute<Vector3> interiorPositions)
		{
			foreach (var edge in faceEdges)
			{
				var p0 = vertexPositions[edge.prev];
				var p1 = vertexPositions[edge];
				var p2 = vertexPositions[edge.next];
				var v1to0 = (p0 - p1).normalized;
				var v1to2 = (p2 - p1).normalized;
				var bisector = v1to0 + v1to2;
				if (bisector != Vector3.zero)
				{
					bisector.Normalize();
					var interiorHalfAngleSine = Vector3.Cross(bisector, v1to2).magnitude;
					var kiteAxis = bisector * (orthogonalKiteEdgeLength / interiorHalfAngleSine);
					interiorPositions[edge] = p1 + kiteAxis;
					leadingPositions[edge] = p1 + kiteAxis.ProjectOntoUnit(v1to0);
					followingPositions[edge] = p1 + kiteAxis.ProjectOntoUnit(v1to2);
				}
				else
				{
					interiorPositions[edge] = p1;
					leadingPositions[edge] = p1;
					followingPositions[edge] = p1;
				}
			}
		}

		/// <summary>
		/// Calculates edge normals based on the positions of their vertices and the planar surface normal.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose normals are to be calculated.</param>
		/// <param name="surface">The surface describing the overall shape of the planar manifold.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <returns>The normals of the edges, each one orthogonal to both the edge's direction vector and its near face's surface normal.</returns>
		public static IEdgeAttribute<Vector3> CalculatePlanarEdgeNormalsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, PlanarSurface surface, IEdgeAttribute<Vector3> vertexPositions)
		{
			return CalculatePlanarEdgeNormalsFromVertexPositions(faceEdges, surface, vertexPositions, new Vector3[faceEdges.Count].AsEdgeAttribute());
		}

		/// <summary>
		/// Calculates edge normals based on the positions of their vertices and the planar surface normal.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose normals are to be calculated.</param>
		/// <param name="surface">The surface describing the overall shape of the planar manifold.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="edgeNormals">A pre-allocated collection in which the edge normals will be stored.</param>
		/// <returns>The normals of the edges, each one orthogonal to both the edge's direction vector and its near face's surface normal.</returns>
		public static IEdgeAttribute<Vector3> CalculatePlanarEdgeNormalsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, PlanarSurface surface, IEdgeAttribute<Vector3> vertexPositions, IEdgeAttribute<Vector3> edgeNormals)
		{
			foreach (var edge in faceEdges)
			{
				edgeNormals[edge] = Vector3.Cross(vertexPositions[edge] - vertexPositions[edge.prev], surface.normal).normalized;
			}

			return edgeNormals;
		}

		/// <summary>
		/// Calculates edge normals based on the positions of their vertices and the surface normals of their near faces.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose normals are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="faceNormals">The surface normals of the faces.</param>
		/// <returns>The normals of the edges, each one orthogonal to both the edge's direction vector and its near face's surface normal.</returns>
		public static IEdgeAttribute<Vector3> CalculateEdgeNormalsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> faceNormals)
		{
			return CalculateEdgeNormalsFromVertexPositions(faceEdges, vertexPositions, faceNormals, new Vector3[faceEdges.Count].AsEdgeAttribute());
		}

		/// <summary>
		/// Calculates edge normals based on the positions of their vertices and the surface normals of their near faces.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose normals are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="faceNormals">The surface normals of the faces.</param>
		/// <param name="edgeNormals">A pre-allocated collection in which the edge normals will be stored.</param>
		/// <returns>The normals of the edges, each one orthogonal to both the edge's direction vector and its near face's surface normal.</returns>
		public static IEdgeAttribute<Vector3> CalculateEdgeNormalsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> faceNormals, IEdgeAttribute<Vector3> edgeNormals)
		{
			foreach (var edge in faceEdges)
			{
				edgeNormals[edge] = Vector3.Cross(vertexPositions[edge] - vertexPositions[edge.prev], faceNormals[edge.nearFace]).normalized;
			}

			return edgeNormals;
		}

		#region CalculateUVs...(...)

		#region CalculateGlobalUVs...(...)

		#region CalculateGlobalPanarUVs...(...)

		/// <summary>
		/// Calculates the UV coordinates of each face edge, corresponding to its next vertex, based on the planar UV space indicated.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose next-vertex UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <returns>The UV coordinates of the face edges, corresponding to their next-vertex references.</returns>
		/// <remarks><para>Unlike <see cref="O:Experilous.MakeItTile.VertexAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions"/>,
		/// this function accesses vertex positions within the context of a face edge, allowing for additional behavior such as
		/// per-face vertex positions or wrapped vertex positions in a topology with wrapping boundaries.</para></remarks>
		/// <seealso cref="O:Experilous.MakeItTile.VertexAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions"/>
		public static IEdgeAttribute<Vector2> CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IEdgeAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, Vector3.zero, uAxis, vAxis, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		/// <summary>
		/// Calculates the UV coordinates of each face edge, corresponding to its next vertex, based on the planar UV space indicated.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose next-vertex UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="origin">The world space origin of the UV space, where the UV coordinate will be (0, 0).</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <returns>The UV coordinates of the face edges, corresponding to their next-vertex references.</returns>
		/// <remarks><para>Unlike <see cref="O:Experilous.MakeItTile.VertexAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions"/>,
		/// this function accesses vertex positions within the context of a face edge, allowing for additional behavior such as
		/// per-face vertex positions or wrapped vertex positions in a topology with wrapping boundaries.</para></remarks>
		/// <seealso cref="O:Experilous.MakeItTile.VertexAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions"/>
		public static IEdgeAttribute<Vector2> CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IEdgeAttribute<Vector3> vertexPositions, Vector3 origin, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, origin, uAxis, vAxis, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		/// <summary>
		/// Calculates the UV coordinates of each face edge, corresponding to its next vertex, based on the planar UV space indicated.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose next-vertex UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The UV coordinates of the face edges, corresponding to their next-vertex references.</returns>
		/// <remarks><para>Unlike <see cref="O:Experilous.MakeItTile.VertexAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions"/>,
		/// this function accesses vertex positions within the context of a face edge, allowing for additional behavior such as
		/// per-face vertex positions or wrapped vertex positions in a topology with wrapping boundaries.</para></remarks>
		/// <seealso cref="O:Experilous.MakeItTile.VertexAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions"/>
		public static IEdgeAttribute<Vector2> CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IEdgeAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, IEdgeAttribute<Vector2> uvs)
		{
			return CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, Vector3.zero, uAxis, vAxis, uvs);
		}

		/// <summary>
		/// Calculates the UV coordinates of each face edge, corresponding to its next vertex, based on the planar UV space indicated.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose next-vertex UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="origin">The world space origin of the UV space, where the UV coordinate will be (0, 0).</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The UV coordinates of the face edges, corresponding to their next-vertex references.</returns>
		/// <remarks><para>Unlike <see cref="O:Experilous.MakeItTile.VertexAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions"/>,
		/// this function accesses vertex positions within the context of a face edge, allowing for additional behavior such as
		/// per-face vertex positions or wrapped vertex positions in a topology with wrapping boundaries.</para></remarks>
		/// <seealso cref="O:Experilous.MakeItTile.VertexAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions"/>
		public static IEdgeAttribute<Vector2> CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IEdgeAttribute<Vector3> vertexPositions, Vector3 origin, Vector3 uAxis, Vector3 vAxis, IEdgeAttribute<Vector2> uvs)
		{
			var normal = Vector3.Cross(vAxis, uAxis).normalized;
			var uPlane = new Plane(origin, uAxis + origin, normal + origin);
			var vPlane = new Plane(origin, normal + origin, vAxis + origin);

			var uAxisNeg = -uAxis;
			var vAxisNeg = -vAxis;

			foreach (var edge in faceEdges)
			{
				var vertexPosition = vertexPositions[edge];

				uvs[edge] = new Vector2(
					Geometry.GetIntersectionParameter(vPlane, new ScaledRay(vertexPosition, uAxisNeg)),
					Geometry.GetIntersectionParameter(uPlane, new ScaledRay(vertexPosition, vAxisNeg)));
			}

			return uvs;
		}

		/// <summary>
		/// Calculates the UV coordinates of each face edge, corresponding to its next vertex, based on the planar UV space indicated, renormalized afterward over the entire manifold so that UVs are bound to the unit square from (0, 0) to (1, 1).
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose next-vertex UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <returns>The UV coordinates of the face edges, corresponding to their next-vertex references.</returns>
		/// <remarks><para>Unlike <see cref="O:Experilous.MakeItTile.VertexAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions"/>,
		/// this function accesses vertex positions within the context of a face edge, allowing for additional behavior such as
		/// per-face vertex positions or wrapped vertex positions in a topology with wrapping boundaries.</para></remarks>
		/// <seealso cref="O:Experilous.MakeItTile.VertexAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions"/>
		public static IEdgeAttribute<Vector2> CalculateGlobalPlanarNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IEdgeAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculateGlobalPlanarNormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		/// <summary>
		/// Calculates the UV coordinates of each face edge, corresponding to its next vertex, based on the planar UV space indicated, renormalized afterward over the entire manifold so that UVs are bound to the unit square from (0, 0) to (1, 1).
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose next-vertex UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The UV coordinates of the face edges, corresponding to their next-vertex references.</returns>
		/// <remarks><para>Unlike <see cref="O:Experilous.MakeItTile.VertexAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions"/>,
		/// this function accesses vertex positions within the context of a face edge, allowing for additional behavior such as
		/// per-face vertex positions or wrapped vertex positions in a topology with wrapping boundaries.</para></remarks>
		/// <seealso cref="O:Experilous.MakeItTile.VertexAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions"/>
		public static IEdgeAttribute<Vector2> CalculateGlobalPlanarNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, IEdgeAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, IEdgeAttribute<Vector2> uvs)
		{
			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);

			Vector2 uvMin = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
			Vector2 uvMax = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

			foreach (var edge in faceEdges)
			{
				uvMin = Geometry.AxisAlignedMin(uvMin, uvs[edge]);
				uvMax = Geometry.AxisAlignedMax(uvMax, uvs[edge]);
			}

			var uvRange = uvMax - uvMin;
			var uvRangeReciprocal = new Vector2(1f / uvRange.x, 1f / uvRange.y);

			foreach (var edge in faceEdges)
			{
				uvs[edge] = Vector2.Scale(uvs[edge] - uvMin, uvRangeReciprocal);
			}

			return uvs;
		}

		#endregion

		#endregion

		#region CalculatePerFaceUVs...(...)

		#region CalculatePerFaceUnnormalizedUVs...(...)

		/// <summary>
		/// Calculates the UV coordinates of each face edge, corresponding to its next vertex, based on the positions of vertices and the already computed UV frames of the faces.
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="faceUVFrames">The UV frames for the faces, defining for each face independently the origin and axes determining UV values.</param>
		/// <returns>The UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceUnnormalizedUVsFromVertexPositions(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<UVFrame3> faceUVFrames)
		{
			if (faces.Count > 0)
			{
				return CalculatePerFaceUnnormalizedUVsFromVertexPositions(faces, vertexPositions, faceUVFrames, new Vector2[faces[0].topology.faceEdges.Count].AsEdgeAttribute());
			}
			else
			{
				return new Vector2[0].AsEdgeAttribute();
			}
		}

		/// <summary>
		/// Calculates the UV coordinates of each face edge, corresponding to its next vertex, based on the positions of vertices and the already computed UV frames of the faces.
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="faceUVFrames">The UV frames for the faces, defining for each face independently the origin and axes determining UV values.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceUnnormalizedUVsFromVertexPositions(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<UVFrame3> faceUVFrames, IEdgeAttribute<Vector2> uvs)
		{
			foreach (var face in faces)
			{
				var uvFrame = faceUVFrames[face];
				foreach (var edge in face.edges)
				{
					var vertexPosition = vertexPositions[edge];
					uvs[edge] = uvFrame.GetUV(vertexPosition);
				}
			}

			return uvs;
		}

		#endregion

		#region CalculatePerFace...UVsFromUnnormalizedUVs(...)

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, such that the UVs of every face have the same scale and individually fit the UV bounds (0, 0) to (1, 1).
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="unnormalizedUVs">The unnormalized UV coordinates of the face corners.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(Topology.FacesIndexer faces, IEdgeAttribute<Vector2> unnormalizedUVs, IEdgeAttribute<Vector2> uvs)
		{
			var faceMinUVs = new Vector2[faces.Count].AsFaceAttribute();
			var faceRangeUVs = new Vector2[faces.Count].AsFaceAttribute();

			FaceAttributeUtility.CalculateFaceEdgeMinAndRangeValues(faces, unnormalizedUVs, faceMinUVs, faceRangeUVs);

			return CalculatePerFaceUniformlyNormalizedUVsFromFaceUVMinAndRange(faces, faceMinUVs, faceRangeUVs, uvs);
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, such that the UVs of every face have the same scale and individually fit the specified aspect ratio.
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="unnormalizedUVs">The unnormalized UV coordinates of the face corners.</param>
		/// <param name="targetAspectRatio">The desired aspect ratio that the UV coordinates will conform to on each face, regardless of the face's bounding box aspect ratio.</param>
		/// <param name="aspectRatioPreservation">The method used to fit the UV coordinates to the desired aspect ratio.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(Topology.FacesIndexer faces, IEdgeAttribute<Vector2> unnormalizedUVs, float targetAspectRatio, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> uvs)
		{
			if (aspectRatioPreservation == AspectRatioPreservation.None) return CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(faces, unnormalizedUVs, uvs);

			var faceMinUVs = new Vector2[faces.Count].AsFaceAttribute();
			var faceRangeUVs = new Vector2[faces.Count].AsFaceAttribute();

			FaceAttributeUtility.CalculateFaceEdgeMinAndRangeValues(faces, unnormalizedUVs, faceMinUVs, faceRangeUVs);

			var aspectRatioPreservationDelegate = AspectRatioUtility.GetFixedRatioRectAdjustmentDelegate(aspectRatioPreservation, targetAspectRatio);

			foreach (var face in faces)
			{
				Vector2 uvMin = faceMinUVs[face];
				Vector2 uvRange = faceRangeUVs[face];
				Rect adjusted = aspectRatioPreservationDelegate(new Rect(uvMin.x, uvMin.y, uvRange.x, uvRange.y));
				faceMinUVs[face] = adjusted.min;
				faceRangeUVs[face] = adjusted.size;
			}

			return CalculatePerFaceUniformlyNormalizedUVsFromFaceUVMinAndRange(faces, faceMinUVs, faceRangeUVs, uvs);
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, such that the UVs of every face have the same scale, individually fit the UV bounds (0, 0) to (1, 1), and are centered on the provided face UVs.
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="unnormalizedEdgeUVs">The unnormalized UV coordinates of the face corners.</param>
		/// <param name="unnormalizedFaceUVs">The unnormalized UV coordinates of the face centers.  Typically based the face centroids, though any other method of mapping faces to UV coordinates is also allowed.</param>
		/// <param name="edgeUVs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(Topology.FacesIndexer faces, IEdgeAttribute<Vector2> unnormalizedEdgeUVs, IFaceAttribute<Vector2> unnormalizedFaceUVs, IEdgeAttribute<Vector2> edgeUVs)
		{
			var faceMinUVs = new Vector2[faces.Count].AsFaceAttribute();
			var faceRangeUVs = new Vector2[faces.Count].AsFaceAttribute();

			FaceAttributeUtility.CalculateCenteredFaceEdgeMinAndRangeValues(faces, unnormalizedEdgeUVs, unnormalizedFaceUVs, faceMinUVs, faceRangeUVs);

			return CalculatePerFaceUniformlyNormalizedUVsFromFaceUVMinAndRange(faces, faceMinUVs, faceRangeUVs, edgeUVs);
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, such that the UVs of every face have the same scale and individually fit the specified aspect ratio centered on the provided face UVs.
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="unnormalizedEdgeUVs">The unnormalized UV coordinates of the face corners.</param>
		/// <param name="unnormalizedFaceUVs">The unnormalized UV coordinates of the face centers.  Typically based the face centroids, though any other method of mapping faces to UV coordinates is also allowed.</param>
		/// <param name="targetAspectRatio">The desired aspect ratio that the UV coordinates will conform to on each face, regardless of the face's bounding box aspect ratio.</param>
		/// <param name="aspectRatioPreservation">The method used to fit the UV coordinates to the desired aspect ratio.</param>
		/// <param name="edgeUVs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(Topology.FacesIndexer faces, IEdgeAttribute<Vector2> unnormalizedEdgeUVs, IFaceAttribute<Vector2> unnormalizedFaceUVs, float targetAspectRatio, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> edgeUVs)
		{
			if (aspectRatioPreservation == AspectRatioPreservation.None) return CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(faces, unnormalizedEdgeUVs, unnormalizedFaceUVs, edgeUVs);

			var faceMinUVs = new Vector2[faces.Count].AsFaceAttribute();
			var faceRangeUVs = new Vector2[faces.Count].AsFaceAttribute();

			FaceAttributeUtility.CalculateCenteredFaceEdgeMinAndRangeValues(faces, unnormalizedEdgeUVs, unnormalizedFaceUVs, faceMinUVs, faceRangeUVs);

			var aspectRatioPreservationDelegate = AspectRatioUtility.GetFixedRatioRectAdjustmentDelegate(aspectRatioPreservation, targetAspectRatio);

			foreach (var face in faces)
			{
				Vector2 uvMin = faceMinUVs[face];
				Vector2 uvRange = faceRangeUVs[face];
				var adjusted = aspectRatioPreservationDelegate(new Rect(uvMin.x, uvMin.y, uvRange.x, uvRange.y));
				faceMinUVs[face] = adjusted.min;
				faceRangeUVs[face] = adjusted.size;
			}

			return CalculatePerFaceUniformlyNormalizedUVsFromFaceUVMinAndRange(faces, faceMinUVs, faceRangeUVs, edgeUVs);
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, such that the UVs of every face individually fit the UV bounds (0, 0) to (1, 1).
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="unnormalizedUVs">The unnormalized UV coordinates of the face corners.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(Topology.FacesIndexer faces, IEdgeAttribute<Vector2> unnormalizedUVs, IEdgeAttribute<Vector2> uvs)
		{
			foreach (var face in faces)
			{
				Vector2 uvMin = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
				Vector2 uvMax = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

				foreach (var edge in face.edges)
				{
					uvMin = Geometry.AxisAlignedMin(uvMin, unnormalizedUVs[edge]);
					uvMax = Geometry.AxisAlignedMax(uvMax, unnormalizedUVs[edge]);
				}

				var uvRange = uvMax - uvMin;
				var uvRangeReciprocal = new Vector2(1f / uvRange.x, 1f / uvRange.y);

				foreach (var edge in face.edges)
				{
					uvs[edge] = Vector2.Scale(unnormalizedUVs[edge] - uvMin, uvRangeReciprocal);
				}
			}

			return uvs;
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, such that the UVs of every face individually fit the specified aspect ratio.
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="unnormalizedUVs">The unnormalized UV coordinates of the face corners.</param>
		/// <param name="targetAspectRatio">The desired aspect ratio that the UV coordinates will conform to on each face, regardless of the face's bounding box aspect ratio.</param>
		/// <param name="aspectRatioPreservation">The method used to fit the UV coordinates to the desired aspect ratio.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(Topology.FacesIndexer faces, IEdgeAttribute<Vector2> unnormalizedUVs, float targetAspectRatio, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> uvs)
		{
			if (aspectRatioPreservation == AspectRatioPreservation.None) return CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(faces, unnormalizedUVs, uvs);

			var aspectRatioPreservationDelegate = AspectRatioUtility.GetFixedRatioRectAdjustmentDelegate(aspectRatioPreservation, targetAspectRatio);

			foreach (var face in faces)
			{
				Vector2 uvMin = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
				Vector2 uvMax = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

				foreach (var edge in face.edges)
				{
					uvMin = Geometry.AxisAlignedMin(uvMin, unnormalizedUVs[edge]);
					uvMax = Geometry.AxisAlignedMax(uvMax, unnormalizedUVs[edge]);
				}

				var adjusted = aspectRatioPreservationDelegate(new Rect(uvMin.x, uvMin.y, uvMax.x - uvMin.x, uvMax.y - uvMin.y));
				var uvRangeReciprocal = new Vector2(1f / adjusted.width, 1f / adjusted.height);

				foreach (var edge in face.edges)
				{
					uvs[edge] = Vector2.Scale(unnormalizedUVs[edge] - adjusted.min, uvRangeReciprocal);
				}
			}

			return uvs;
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, such that the UVs of every face individually fit the UV bounds (0, 0) to (1, 1) centered on the provided face UVs.
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="unnormalizedEdgeUVs">The unnormalized UV coordinates of the face corners.</param>
		/// <param name="unnormalizedFaceUVs">The unnormalized UV coordinates of the face centers.  Typically based the face centroids, though any other method of mapping faces to UV coordinates is also allowed.</param>
		/// <param name="edgeUVs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(Topology.FacesIndexer faces, IEdgeAttribute<Vector2> unnormalizedEdgeUVs, IFaceAttribute<Vector2> unnormalizedFaceUVs, IEdgeAttribute<Vector2> edgeUVs)
		{
			foreach (var face in faces)
			{
				Vector2 uvMin = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
				Vector2 uvMax = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

				foreach (var edge in face.edges)
				{
					uvMin = Geometry.AxisAlignedMin(uvMin, unnormalizedEdgeUVs[edge]);
					uvMax = Geometry.AxisAlignedMax(uvMax, unnormalizedEdgeUVs[edge]);
				}

				var center = unnormalizedFaceUVs[face];
				var extent = Geometry.AxisAlignedMax(uvMax - center, center - uvMin);
				uvMin = center - extent;
				var uvRange = extent * 2f;
				var uvRangeReciprocal = new Vector2(1f / uvRange.x, 1f / uvRange.y);

				foreach (var edge in face.edges)
				{
					edgeUVs[edge] = Vector2.Scale(unnormalizedEdgeUVs[edge] - uvMin, uvRangeReciprocal);
				}
			}

			return edgeUVs;
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, such that the UVs of every face individually fit the specified aspect ratio centered on the provided face UVs.
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="unnormalizedEdgeUVs">The unnormalized UV coordinates of the face corners.</param>
		/// <param name="unnormalizedFaceUVs">The unnormalized UV coordinates of the face centers.  Typically based the face centroids, though any other method of mapping faces to UV coordinates is also allowed.</param>
		/// <param name="targetAspectRatio">The desired aspect ratio that the UV coordinates will conform to on each face, regardless of the face's bounding box aspect ratio.</param>
		/// <param name="aspectRatioPreservation">The method used to fit the UV coordinates to the desired aspect ratio.</param>
		/// <param name="edgeUVs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(Topology.FacesIndexer faces, IEdgeAttribute<Vector2> unnormalizedEdgeUVs, IFaceAttribute<Vector2> unnormalizedFaceUVs, float targetAspectRatio, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> edgeUVs)
		{
			if (aspectRatioPreservation == AspectRatioPreservation.None) return CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(faces, unnormalizedEdgeUVs, edgeUVs);

			var aspectRatioPreservationDelegate = AspectRatioUtility.GetFixedRatioRectAdjustmentDelegate(aspectRatioPreservation, targetAspectRatio);

			foreach (var face in faces)
			{
				Vector2 uvMin = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
				Vector2 uvMax = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

				foreach (var edge in face.edges)
				{
					uvMin = Geometry.AxisAlignedMin(uvMin, unnormalizedEdgeUVs[edge]);
					uvMax = Geometry.AxisAlignedMax(uvMax, unnormalizedEdgeUVs[edge]);
				}

				var center = unnormalizedFaceUVs[face];
				var extent = Geometry.AxisAlignedMax(uvMax - center, center - uvMin);
				var adjusted = aspectRatioPreservationDelegate(new Rect(center.x - extent.x, center.y - extent.y, extent.x * 2f, extent.y * 2f));
				var uvRangeReciprocal = new Vector2(1f / adjusted.width, 1f / adjusted.height);

				foreach (var edge in face.edges)
				{
					edgeUVs[edge] = Vector2.Scale(unnormalizedEdgeUVs[edge] - adjusted.min, uvRangeReciprocal);
				}
			}

			return edgeUVs;
		}

		#endregion

		#region CalculatePerFace...UVsFromFaceUVMinAndRange(...)

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, such that the UVs of every face have the same scale, based on the per-face UV bounds already calculated.
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="faceMinUVs">The minimum UV coordinates of the faces.</param>
		/// <param name="faceRangeUVs">The UV coordinate ranges of the faces.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromFaceUVMinAndRange(Topology.FacesIndexer faces, IFaceAttribute<Vector2> faceMinUVs, IFaceAttribute<Vector2> faceRangeUVs, IEdgeAttribute<Vector2> uvs)
		{
			var faceMaxUVRange = Vector2.zero;

			foreach (var face in faces)
			{
				faceMaxUVRange = Geometry.AxisAlignedMax(faceMaxUVRange, faceRangeUVs[face]);
			}

			var faceMaxUVExtent = faceMaxUVRange * 0.5f;
			var faceMaxUVRangeReciprocal = new Vector2(1f / faceMaxUVRange.x, 1f / faceMaxUVRange.y);

			foreach (var face in faces)
			{
				var faceOffsetUV = faceMinUVs[face] + faceRangeUVs[face] * 0.5f - faceMaxUVExtent;
				foreach (var edge in face.edges)
				{
					uvs[edge] = Vector2.Scale(uvs[edge] - faceOffsetUV, faceMaxUVRangeReciprocal);
				}
			}

			return uvs;
		}

		#endregion

		#region CalculatePerFacePlanarUVs...(...)

		#region CaclulatePerFacePlanarUniformlyNormalizedUVs...(...)

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face based on the planar UV space indicated, such that the UVs of every face have the same scale and individually fit the UV bounds (0, 0) to (1, 1).
		/// </summary>
		/// <param name="faceEdges">The collection of face eges whose next vertex UV coordinates are to be calculated.</param>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, uAxis, vAxis, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face based on the planar UV space indicated, such that the UVs of every face have the same scale and individually fit the UV bounds (0, 0) to (1, 1).
		/// </summary>
		/// <param name="faceEdges">The collection of face eges whose next vertex UV coordinates are to be calculated.</param>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, IEdgeAttribute<Vector2> uvs)
		{
			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);
			return CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(faces, uvs, uvs);
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face based on the planar UV space indicated, such that the UVs of every face have the same scale and individually fit the aspect ratio dictated by the UV axes.
		/// </summary>
		/// <param name="faceEdges">The collection of face eges whose next vertex UV coordinates are to be calculated.</param>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="aspectRatioPreservation">The method used to fit the UV coordinates to the desired aspect ratio.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, AspectRatioPreservation aspectRatioPreservation)
		{
			return CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, uAxis, vAxis, aspectRatioPreservation, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face based on the planar UV space indicated, such that the UVs of every face have the same scale and individually fit the aspect ratio dictated by the UV axes.
		/// </summary>
		/// <param name="faceEdges">The collection of face eges whose next vertex UV coordinates are to be calculated.</param>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="aspectRatioPreservation">The method used to fit the UV coordinates to the desired aspect ratio.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> uvs)
		{
			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);
			return CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(faces, uvs, Mathf.Sqrt(uAxis.sqrMagnitude / vAxis.sqrMagnitude), aspectRatioPreservation, uvs);
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, such that the UVs of every face have the same scale, individually fit the UV bounds (0, 0) to (1, 1), and are centered on the provided face positions.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose next-vertex UV coordinates are to be calculated.</param>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, facePositions, uAxis, vAxis, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, such that the UVs of every face have the same scale, individually fit the UV bounds (0, 0) to (1, 1), and are centered on the provided face positions.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose next-vertex UV coordinates are to be calculated.</param>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis, IEdgeAttribute<Vector2> uvs)
		{
			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);
			var faceUVs = new Vector2[faces.Count].AsFaceAttribute();
			FaceAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(faces, facePositions, uAxis, vAxis, faceUVs);
			return CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(faces, uvs, faceUVs, uvs);
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, such that the UVs of every face have the same scale, individually fit the fit the aspect ratio dictated by the UV axes, and are centered on the provided face positions.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose next-vertex UV coordinates are to be calculated.</param>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="aspectRatioPreservation">The method used to fit the UV coordinates to the desired aspect ratio.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis, AspectRatioPreservation aspectRatioPreservation)
		{
			return CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, facePositions, uAxis, vAxis, aspectRatioPreservation, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, such that the UVs of every face have the same scale, individually fit the fit the aspect ratio dictated by the UV axes, and are centered on the provided face positions.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose next-vertex UV coordinates are to be calculated.</param>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="aspectRatioPreservation">The method used to fit the UV coordinates to the desired aspect ratio.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarUniformlyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> uvs)
		{
			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);
			var faceUVs = new Vector2[faces.Count].AsFaceAttribute();
			FaceAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(faces, facePositions, uAxis, vAxis, faceUVs);
			return CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(faces, uvs, faceUVs, Mathf.Sqrt(uAxis.sqrMagnitude / vAxis.sqrMagnitude), aspectRatioPreservation, uvs);
		}

		#endregion

		#region CalculatePerFacePlanarVariablyNormalizedUVs...(...)

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, such that the UVs of every face individually fit the UV bounds (0, 0) to (1, 1), based on the planar UV space indicated.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose next-vertex UV coordinates are to be calculated.</param>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, uAxis, vAxis, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, such that the UVs of every face individually fit the UV bounds (0, 0) to (1, 1), based on the planar UV space indicated.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose next-vertex UV coordinates are to be calculated.</param>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, IEdgeAttribute<Vector2> uvs)
		{
			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);
			return CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(faces, uvs, uvs);
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, based on the planar UV space indicated, such that the UVs of every face individually fit the aspect ratio dictated by the UV axes.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose next-vertex UV coordinates are to be calculated.</param>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="aspectRatioPreservation">The method used to fit the UV coordinates to the desired aspect ratio.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, AspectRatioPreservation aspectRatioPreservation)
		{
			return CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, uAxis, vAxis, aspectRatioPreservation, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, based on the planar UV space indicated, such that the UVs of every face individually fit the aspect ratio dictated by the UV axes.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose next-vertex UV coordinates are to be calculated.</param>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="aspectRatioPreservation">The method used to fit the UV coordinates to the desired aspect ratio.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> uvs)
		{
			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);
			return CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(faces, uvs, Mathf.Sqrt(uAxis.sqrMagnitude / vAxis.sqrMagnitude), aspectRatioPreservation, uvs);
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, based on the planar UV space indicated, such that the UVs of every face individually fit the UV bounds (0, 0) to (1, 1) centered on the provided face positions.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose next-vertex UV coordinates are to be calculated.</param>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, facePositions, uAxis, vAxis, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, based on the planar UV space indicated, such that the UVs of every face individually fit the UV bounds (0, 0) to (1, 1) centered on the provided face positions.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose next-vertex UV coordinates are to be calculated.</param>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis, IEdgeAttribute<Vector2> uvs)
		{
			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);
			var faceUVs = new Vector2[faces.Count].AsFaceAttribute();
			FaceAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(faces, facePositions, uAxis, vAxis, faceUVs);
			return CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(faces, uvs, faceUVs, uvs);
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, based on the planar UV space indicated, such that the UVs of every face individually fit the aspect ratio dictated by the UV axes centered on the provided face positions.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose next-vertex UV coordinates are to be calculated.</param>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="aspectRatioPreservation">The method used to fit the UV coordinates to the desired aspect ratio.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis, AspectRatioPreservation aspectRatioPreservation)
		{
			return CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(faceEdges, faces, vertexPositions, facePositions, uAxis, vAxis, aspectRatioPreservation, new Vector2[faceEdges.Count].AsEdgeAttribute());
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, based on the planar UV space indicated, such that the UVs of every face individually fit the aspect ratio dictated by the UV axes centered on the provided face positions.
		/// </summary>
		/// <param name="faceEdges">The collection of face edges whose next-vertex UV coordinates are to be calculated.</param>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="aspectRatioPreservation">The method used to fit the UV coordinates to the desired aspect ratio.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFacePlanarVariablyNormalizedUVsFromVertexPositions(Topology.FaceEdgesIndexer faceEdges, Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> uvs)
		{
			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(faceEdges, vertexPositions, uAxis, vAxis, uvs);
			var faceUVs = new Vector2[faces.Count].AsFaceAttribute();
			FaceAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(faces, facePositions, uAxis, vAxis, faceUVs);
			return CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(faces, uvs, faceUVs, Mathf.Sqrt(uAxis.sqrMagnitude / vAxis.sqrMagnitude), aspectRatioPreservation, uvs);
		}

		#endregion

		#endregion

		#region CalculatePerFaceUniformlyNormalizedUVs...(...)

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, based on the positions of vertices and the already computed UV frames of the faces, such that the UVs of every face have the same scale and individually fit the UV bounds (0, 0) to (1, 1).
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="faceUVFrames">The UV frames for the faces, defining for each face independently the origin and axes determining UV values.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromVertexPositions(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<UVFrame3> faceUVFrames)
		{
			if (faces.Count > 0)
			{
				return CalculatePerFaceUniformlyNormalizedUVsFromVertexPositions(faces, vertexPositions, faceUVFrames, new Vector2[faces[0].topology.faceEdges.Count].AsEdgeAttribute());
			}
			else
			{
				return new Vector2[0].AsEdgeAttribute();
			}
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, based on the positions of vertices and the already computed UV frames of the faces, such that the UVs of every face have the same scale and individually fit the UV bounds (0, 0) to (1, 1).
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="faceUVFrames">The UV frames for the faces, defining for each face independently the origin and axes determining UV values.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromVertexPositions(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<UVFrame3> faceUVFrames, IEdgeAttribute<Vector2> uvs)
		{
			CalculatePerFaceUnnormalizedUVsFromVertexPositions(faces, vertexPositions, faceUVFrames, uvs);
			return CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(faces, uvs, uvs);
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, based on the positions of vertices and the already computed UV frames of the faces, such that the UVs of every face have the same scale, individually fit the UV bounds (0, 0) to (1, 1), and are centered on the provided face positions.
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="faceUVFrames">The UV frames for the faces, defining for each face independently the origin and axes determining UV values.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromVertexPositions(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, IFaceAttribute<UVFrame3> faceUVFrames)
		{
			if (faces.Count > 0)
			{
				return CalculatePerFaceUniformlyNormalizedUVsFromVertexPositions(faces, vertexPositions, facePositions, faceUVFrames, new Vector2[faces[0].topology.faceEdges.Count].AsEdgeAttribute());
			}
			else
			{
				return new Vector2[0].AsEdgeAttribute();
			}
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, based on the positions of vertices and the already computed UV frames of the faces, such that the UVs of every face have the same scale, individually fit the UV bounds (0, 0) to (1, 1), and are centered on the provided face positions.
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="faceUVFrames">The UV frames for the faces, defining for each face independently the origin and axes determining UV values.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromVertexPositions(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, IFaceAttribute<UVFrame3> faceUVFrames, IEdgeAttribute<Vector2> uvs)
		{
			CalculatePerFaceUnnormalizedUVsFromVertexPositions(faces, vertexPositions, faceUVFrames, uvs);
			var faceUVs = new Vector2[faces.Count].AsFaceAttribute();
			FaceAttributeUtility.CalculateUnnormalizedUVsFromFacePositions(faces, facePositions, faceUVFrames, faceUVs);
			return CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(faces, uvs, faceUVs, uvs);
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, based on the positions of vertices and the already computed UV frames of the faces, such that the UVs of every face have the same scale and individually fit the specified aspect ratio.
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="faceUVFrames">The UV frames for the faces, defining for each face independently the origin and axes determining UV values.</param>
		/// <param name="targetAspectRatio">The desired aspect ratio that the UV coordinates will conform to on each face, regardless of the face's bounding box aspect ratio.</param>
		/// <param name="aspectRatioPreservation">The method used to fit the UV coordinates to the desired aspect ratio.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromVertexPositions(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<UVFrame3> faceUVFrames, float targetAspectRatio, AspectRatioPreservation aspectRatioPreservation)
		{
			if (faces.Count > 0)
			{
				return CalculatePerFaceUniformlyNormalizedUVsFromVertexPositions(faces, vertexPositions, faceUVFrames, targetAspectRatio, aspectRatioPreservation, new Vector2[faces[0].topology.faceEdges.Count].AsEdgeAttribute());
			}
			else
			{
				return new Vector2[0].AsEdgeAttribute();
			}
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, based on the positions of vertices and the already computed UV frames of the faces, such that the UVs of every face have the same scale and individually fit the specified aspect ratio.
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="faceUVFrames">The UV frames for the faces, defining for each face independently the origin and axes determining UV values.</param>
		/// <param name="targetAspectRatio">The desired aspect ratio that the UV coordinates will conform to on each face, regardless of the face's bounding box aspect ratio.</param>
		/// <param name="aspectRatioPreservation">The method used to fit the UV coordinates to the desired aspect ratio.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromVertexPositions(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<UVFrame3> faceUVFrames, float targetAspectRatio, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> uvs)
		{
			CalculatePerFaceUnnormalizedUVsFromVertexPositions(faces, vertexPositions, faceUVFrames, uvs);
			return CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(faces, uvs, targetAspectRatio, aspectRatioPreservation, uvs);
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, based on the positions of vertices and the already computed UV frames of the faces, such that the UVs of every face have the same scale and individually fit the specified aspect ratio centered on the provided face positions.
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="faceUVFrames">The UV frames for the faces, defining for each face independently the origin and axes determining UV values.</param>
		/// <param name="targetAspectRatio">The desired aspect ratio that the UV coordinates will conform to on each face, regardless of the face's bounding box aspect ratio.</param>
		/// <param name="aspectRatioPreservation">The method used to fit the UV coordinates to the desired aspect ratio.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromVertexPositions(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, IFaceAttribute<UVFrame3> faceUVFrames, float targetAspectRatio, AspectRatioPreservation aspectRatioPreservation)
		{
			if (faces.Count > 0)
			{
				return CalculatePerFaceUniformlyNormalizedUVsFromVertexPositions(faces, vertexPositions, facePositions, faceUVFrames, targetAspectRatio, aspectRatioPreservation, new Vector2[faces[0].topology.faceEdges.Count].AsEdgeAttribute());
			}
			else
			{
				return new Vector2[0].AsEdgeAttribute();
			}
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, based on the positions of vertices and the already computed UV frames of the faces, such that the UVs of every face have the same scale and individually fit the specified aspect ratio centered on the provided face positions.
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="faceUVFrames">The UV frames for the faces, defining for each face independently the origin and axes determining UV values.</param>
		/// <param name="targetAspectRatio">The desired aspect ratio that the UV coordinates will conform to on each face, regardless of the face's bounding box aspect ratio.</param>
		/// <param name="aspectRatioPreservation">The method used to fit the UV coordinates to the desired aspect ratio.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceUniformlyNormalizedUVsFromVertexPositions(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, IFaceAttribute<UVFrame3> faceUVFrames, float targetAspectRatio, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> uvs)
		{
			CalculatePerFaceUnnormalizedUVsFromVertexPositions(faces, vertexPositions, faceUVFrames, uvs);
			var faceUVs = new Vector2[faces.Count].AsFaceAttribute();
			FaceAttributeUtility.CalculateUnnormalizedUVsFromFacePositions(faces, facePositions, faceUVFrames, faceUVs);
			return CalculatePerFaceUniformlyNormalizedUVsFromUnnormalizedUVs(faces, uvs, faceUVs, targetAspectRatio, aspectRatioPreservation, uvs);
		}

		#endregion

		#region CalculatePerFaceVariablyNormalizedUVs...(...)

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, based on the positions of vertices and the already computed UV frames of the faces, such that the UVs of every face individually fit the UV bounds (0, 0) to (1, 1).
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="faceUVFrames">The UV frames for the faces, defining for each face independently the origin and axes determining UV values.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceVariablyNormalizedUVsFromVertexPositions(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<UVFrame3> faceUVFrames)
		{
			if (faces.Count > 0)
			{
				return CalculatePerFaceVariablyNormalizedUVsFromVertexPositions(faces, vertexPositions, faceUVFrames, new Vector2[faces[0].topology.faceEdges.Count].AsEdgeAttribute());
			}
			else
			{
				return new Vector2[0].AsEdgeAttribute();
			}
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, based on the positions of vertices and the already computed UV frames of the faces, such that the UVs of every face individually fit the UV bounds (0, 0) to (1, 1).
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="faceUVFrames">The UV frames for the faces, defining for each face independently the origin and axes determining UV values.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceVariablyNormalizedUVsFromVertexPositions(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<UVFrame3> faceUVFrames, IEdgeAttribute<Vector2> uvs)
		{
			CalculatePerFaceUnnormalizedUVsFromVertexPositions(faces, vertexPositions, faceUVFrames, uvs);
			return CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(faces, uvs, uvs);
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, based on the positions of vertices and the already computed UV frames of the faces, such that the UVs of every face individually fit the UV bounds (0, 0) to (1, 1) centered on the provided face positions.
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="faceUVFrames">The UV frames for the faces, defining for each face independently the origin and axes determining UV values.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceVariablyNormalizedUVsFromVertexPositions(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, IFaceAttribute<UVFrame3> faceUVFrames)
		{
			if (faces.Count > 0)
			{
				return CalculatePerFaceVariablyNormalizedUVsFromVertexPositions(faces, vertexPositions, facePositions, faceUVFrames, new Vector2[faces[0].topology.faceEdges.Count].AsEdgeAttribute());
			}
			else
			{
				return new Vector2[0].AsEdgeAttribute();
			}
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, based on the positions of vertices and the already computed UV frames of the faces, such that the UVs of every face individually fit the UV bounds (0, 0) to (1, 1) centered on the provided face positions.
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="faceUVFrames">The UV frames for the faces, defining for each face independently the origin and axes determining UV values.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceVariablyNormalizedUVsFromVertexPositions(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, IFaceAttribute<UVFrame3> faceUVFrames, IEdgeAttribute<Vector2> uvs)
		{
			CalculatePerFaceUnnormalizedUVsFromVertexPositions(faces, vertexPositions, faceUVFrames, uvs);
			var faceUVs = new Vector2[faces.Count].AsFaceAttribute();
			FaceAttributeUtility.CalculateUnnormalizedUVsFromFacePositions(faces, facePositions, faceUVFrames, faceUVs);
			return CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(faces, uvs, faceUVs, uvs);
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, based on the positions of vertices and the already computed UV frames of the faces, such that the UVs of every face individually fit the specified aspect ratio.
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="faceUVFrames">The UV frames for the faces, defining for each face independently the origin and axes determining UV values.</param>
		/// <param name="targetAspectRatio">The desired aspect ratio that the UV coordinates will conform to on each face, regardless of the face's bounding box aspect ratio.</param>
		/// <param name="aspectRatioPreservation">The method used to fit the UV coordinates to the desired aspect ratio.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceVariablyNormalizedUVsFromVertexPositions(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<UVFrame3> faceUVFrames, float targetAspectRatio, AspectRatioPreservation aspectRatioPreservation)
		{
			if (faces.Count > 0)
			{
				return CalculatePerFaceVariablyNormalizedUVsFromVertexPositions(faces, vertexPositions, faceUVFrames, targetAspectRatio, aspectRatioPreservation, new Vector2[faces[0].topology.faceEdges.Count].AsEdgeAttribute());
			}
			else
			{
				return new Vector2[0].AsEdgeAttribute();
			}
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, based on the positions of vertices and the already computed UV frames of the faces, such that the UVs of every face individually fit the specified aspect ratio.
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="faceUVFrames">The UV frames for the faces, defining for each face independently the origin and axes determining UV values.</param>
		/// <param name="targetAspectRatio">The desired aspect ratio that the UV coordinates will conform to on each face, regardless of the face's bounding box aspect ratio.</param>
		/// <param name="aspectRatioPreservation">The method used to fit the UV coordinates to the desired aspect ratio.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceVariablyNormalizedUVsFromVertexPositions(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<UVFrame3> faceUVFrames, float targetAspectRatio, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> uvs)
		{
			CalculatePerFaceUnnormalizedUVsFromVertexPositions(faces, vertexPositions, faceUVFrames, uvs);
			return CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(faces, uvs, targetAspectRatio, aspectRatioPreservation, uvs);
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, based on the positions of vertices and the already computed UV frames of the faces, such that the UVs of every face individually fit the specified aspect ratio centered on the provided face positions.
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="faceUVFrames">The UV frames for the faces, defining for each face independently the origin and axes determining UV values.</param>
		/// <param name="targetAspectRatio">The desired aspect ratio that the UV coordinates will conform to on each face, regardless of the face's bounding box aspect ratio.</param>
		/// <param name="aspectRatioPreservation">The method used to fit the UV coordinates to the desired aspect ratio.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceVariablyNormalizedUVsFromVertexPositions(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, IFaceAttribute<UVFrame3> faceUVFrames, float targetAspectRatio, AspectRatioPreservation aspectRatioPreservation)
		{
			if (faces.Count > 0)
			{
				return CalculatePerFaceVariablyNormalizedUVsFromVertexPositions(faces, vertexPositions, facePositions, faceUVFrames, targetAspectRatio, aspectRatioPreservation, new Vector2[faces[0].topology.faceEdges.Count].AsEdgeAttribute());
			}
			else
			{
				return new Vector2[0].AsEdgeAttribute();
			}
		}

		/// <summary>
		/// Calculates the normalized UV coordinates of the corners of each face, based on the positions of vertices and the already computed UV frames of the faces, such that the UVs of every face individually fit the specified aspect ratio centered on the provided face positions.
		/// </summary>
		/// <param name="faces">The collection of faces whose corner UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="faceUVFrames">The UV frames for the faces, defining for each face independently the origin and axes determining UV values.</param>
		/// <param name="targetAspectRatio">The desired aspect ratio that the UV coordinates will conform to on each face, regardless of the face's bounding box aspect ratio.</param>
		/// <param name="aspectRatioPreservation">The method used to fit the UV coordinates to the desired aspect ratio.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The normalized UV coordinates of the face corners.</returns>
		public static IEdgeAttribute<Vector2> CalculatePerFaceVariablyNormalizedUVsFromVertexPositions(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> facePositions, IFaceAttribute<UVFrame3> faceUVFrames, float targetAspectRatio, AspectRatioPreservation aspectRatioPreservation, IEdgeAttribute<Vector2> uvs)
		{
			CalculatePerFaceUnnormalizedUVsFromVertexPositions(faces, vertexPositions, faceUVFrames, uvs);
			var faceUVs = new Vector2[faces.Count].AsFaceAttribute();
			FaceAttributeUtility.CalculateUnnormalizedUVsFromFacePositions(faces, facePositions, faceUVFrames, faceUVs);
			return CalculatePerFaceVariablyNormalizedUVsFromUnnormalizedUVs(faces, uvs, faceUVs, targetAspectRatio, aspectRatioPreservation, uvs);
		}

		#endregion

		#endregion

		#endregion

		#endregion
	}
}
