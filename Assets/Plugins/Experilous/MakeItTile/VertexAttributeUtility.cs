/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using Experilous.Numerics;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// A static utility class for calculating various per-vertex attributes.
	/// </summary>
	public static class VertexAttributeUtility
	{
		#region IVertexAttribute<Vector3> CalculateVertexNormals...(...)

		/// <summary>
		/// Calculates vertex normals using the surface normal reported by the given surface based on the position of each vertex.
		/// </summary>
		/// <param name="vertices">The collection of vertices whose normals are to be calculated.</param>
		/// <param name="surface">The surface descriptor that will provide surface normal information.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <returns>The surface normals of the vertices.</returns>
		public static IVertexAttribute<Vector3> CalculateVertexNormalsFromSurface(Topology.VerticesIndexer vertices, Surface surface, IVertexAttribute<Vector3> vertexPositions)
		{
			return CalculateVertexNormalsFromSurface(vertices, surface, vertexPositions, new Vector3[vertices.Count].AsVertexAttribute());
		}

		/// <summary>
		/// Calculates vertex normals using the surface normal reported by the given surface based on the position of each vertex.
		/// </summary>
		/// <param name="vertices">The collection of vertices whose normals are to be calculated.</param>
		/// <param name="surface">The surface descriptor that will provide surface normal information.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="vertexNormals">A pre-allocated collection in which the vertex normals will be stored.</param>
		/// <returns>The surface normals of the vertices.</returns>
		public static IVertexAttribute<Vector3> CalculateVertexNormalsFromSurface(Topology.VerticesIndexer vertices, Surface surface, IVertexAttribute<Vector3> vertexPositions, IVertexAttribute<Vector3> vertexNormals)
		{
			foreach (var vertex in vertices)
			{
				vertexNormals[vertex] = surface.GetNormal(vertexPositions[vertex]);
			}

			return vertexNormals;
		}

		/// <summary>
		/// Calculates vertex normals based on their positions and the positions of their neighbors.
		/// </summary>
		/// <param name="vertices">The collection of vertices whose normals are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <returns>The surface normals of the vertices.</returns>
		public static IVertexAttribute<Vector3> CalculateVertexNormalsFromVertexPositions(Topology.VerticesIndexer vertices, IVertexAttribute<Vector3> vertexPositions)
		{
			return CalculateVertexNormalsFromVertexPositions(vertices, vertexPositions, new Vector3[vertices.Count].AsVertexAttribute());
		}

		/// <summary>
		/// Calculates vertex normals based on their positions and the positions of their neighbors.
		/// </summary>
		/// <param name="vertices">The collection of vertices whose normals are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="vertexNormals">A pre-allocated collection in which the vertex normals will be stored.</param>
		/// <returns>The surface normals of the vertices.</returns>
		public static IVertexAttribute<Vector3> CalculateVertexNormalsFromVertexPositions(Topology.VerticesIndexer vertices, IVertexAttribute<Vector3> vertexPositions, IVertexAttribute<Vector3> vertexNormals)
		{
			foreach (var vertex in vertices)
			{
				var normalSum = new Vector3(0f, 0f, 0f);
				var vertexPosition = vertexPositions[vertex];

				var edge = vertex.firstEdge;
				var p0 = vertexPositions[edge];
				edge = edge.next;
				var firstEdge = edge;
				do
				{
					var p1 = vertexPositions[edge];
					normalSum += Vector3.Cross(p0 - vertexPosition, p1 - vertexPosition);
					edge = edge.next;
				} while (edge != firstEdge);

				vertexNormals[vertex] = normalSum.normalized;
			}

			return vertexNormals;
		}

		/// <summary>
		/// Calculates vertex normals based on the positions of their neighboring faces.
		/// </summary>
		/// <param name="vertices">The collection of vertices whose normals are to be calculated.</param>
		/// <param name="facePositions">The positions of the faces.</param>
		/// <returns>The surface normals of the vertices.</returns>
		public static IVertexAttribute<Vector3> CalculateVertexNormalsFromFacePositions(Topology.VerticesIndexer vertices, IFaceAttribute<Vector3> facePositions)
		{
			return CalculateVertexNormalsFromFacePositions(vertices, facePositions, new Vector3[vertices.Count].AsVertexAttribute());
		}

		/// <summary>
		/// Calculates vertex normals based on the positions of their neighboring faces.
		/// </summary>
		/// <param name="vertices">The collection of vertices whose normals are to be calculated.</param>
		/// <param name="facePositions">The positions of the faces.</param>
		/// <param name="vertexNormals">A pre-allocated collection in which the vertex normals will be stored.</param>
		/// <returns>The surface normals of the vertices.</returns>
		public static IVertexAttribute<Vector3> CalculateVertexNormalsFromFacePositions(Topology.VerticesIndexer vertices, IFaceAttribute<Vector3> facePositions, IVertexAttribute<Vector3> vertexNormals)
		{
			foreach (var vertex in vertices)
			{
				var normalSum = new Vector3(0f, 0f, 0f);

				var edge = vertex.firstEdge;
				var p0 = facePositions[edge];
				edge = edge.next;
				var p1 = facePositions[edge];
				edge = edge.next;
				var firstEdge = edge;
				do
				{
					var p2 = facePositions[edge];
					normalSum += Vector3.Cross(p0 - p1, p2 - p1);
					edge = edge.next;
				} while (edge != firstEdge);

				vertexNormals[vertex] = normalSum.normalized;
			}

			return vertexNormals;
		}

		/// <summary>
		/// Calculates vertex normals based on the surface normals of their neighboring faces.
		/// </summary>
		/// <param name="vertices">The collection of vertices whose normals are to be calculated.</param>
		/// <param name="faceNormals">The surface normals of the faces.</param>
		/// <returns>The surface normals of the vertices.</returns>
		public static IVertexAttribute<Vector3> CalculateVertexNormalsFromFaceNormals(Topology.VerticesIndexer vertices, IFaceAttribute<Vector3> faceNormals)
		{
			return CalculateVertexNormalsFromFaceNormals(vertices, faceNormals, new Vector3[vertices.Count].AsVertexAttribute());
		}

		/// <summary>
		/// Calculates vertex normals based on the surface normals of their neighboring faces.
		/// </summary>
		/// <param name="vertices">The collection of vertices whose normals are to be calculated.</param>
		/// <param name="faceNormals">The surface normals of the faces.</param>
		/// <param name="vertexNormals">A pre-allocated collection in which the vertex normals will be stored.</param>
		/// <returns>The surface normals of the vertices.</returns>
		public static IVertexAttribute<Vector3> CalculateVertexNormalsFromFaceNormals(Topology.VerticesIndexer vertices, IFaceAttribute<Vector3> faceNormals, IVertexAttribute<Vector3> vertexNormals)
		{
			foreach (var vertex in vertices)
			{
				var normalSum = new Vector3(0f, 0f, 0f);

				foreach (var edge in vertex.edges)
				{
					normalSum += faceNormals[edge];
				}

				vertexNormals[vertex] = normalSum.normalized;
			}

			return vertexNormals;
		}

		/// <summary>
		/// Calculates vertex normals based on their positions and the fact that they are part of a spherical surface centered at the origin.
		/// </summary>
		/// <param name="vertices">The collection of vertices whose normals are to be calculated.</param>
		/// <param name="surface">The surface describing the overall shape of the spherical manifold.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <returns>The surface normals of the vertices.</returns>
		public static IVertexAttribute<Vector3> CalculateSphericalVertexNormalsFromVertexPositions(Topology.VerticesIndexer vertices, SphericalSurface surface, IVertexAttribute<Vector3> vertexPositions)
		{
			return CalculateSphericalVertexNormalsFromVertexPositions(vertices, surface, vertexPositions, new Vector3[vertices.Count].AsVertexAttribute());
		}

		/// <summary>
		/// Calculates vertex normals based on their positions and the fact that they are part of a spherical surface centered at the origin.
		/// </summary>
		/// <param name="vertices">The collection of vertices whose normals are to be calculated.</param>
		/// <param name="surface">The surface describing the overall shape of the spherical manifold.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="vertexNormals">A pre-allocated collection in which the vertex normals will be stored.</param>
		/// <returns>The surface normals of the vertices.</returns>
		public static IVertexAttribute<Vector3> CalculateSphericalVertexNormalsFromVertexPositions(Topology.VerticesIndexer vertices, SphericalSurface surface, IVertexAttribute<Vector3> vertexPositions, IVertexAttribute<Vector3> vertexNormals)
		{
			if (!surface.isInverted)
			{
				foreach (var vertex in vertices)
				{
					vertexNormals[vertex] = vertexPositions[vertex].normalized;
				}
			}
			else
			{
				foreach (var vertex in vertices)
				{
					vertexNormals[vertex] = -vertexPositions[vertex].normalized;
				}
			}

			return vertexNormals;
		}

		#endregion

		#region IVertexAttribute<float> CalculateVertexAreas...(...)

		/// <summary>
		/// Calculates the planar surface area around each vertex that is closest to that vertex, based on the centroid positions of their neighboring faces.  For non-flat surfaces, the calculated areas are only approximate.
		/// </summary>
		/// <param name="vertices">The collection of vertices whose areas are to be calculated.</param>
		/// <param name="faceCentroids">The centroids of the faces.</param>
		/// <returns>The surrounding surface areas of the vertices.</returns>
		public static IVertexAttribute<float> CalculatePlanarVertexAreasFromFaceCentroids(Topology.VerticesIndexer vertices, IFaceAttribute<Vector3> faceCentroids)
		{
			return CalculatePlanarVertexAreasFromFaceCentroids(vertices, faceCentroids, new float[vertices.Count].AsVertexAttribute());
		}

		/// <summary>
		/// Calculates the planar surface area around each vertex that is closest to that vertex, based on the centroid positions of their neighboring faces.  For non-flat surfaces, the calculated areas are only approximate.
		/// </summary>
		/// <param name="vertices">The collection of vertices whose areas are to be calculated.</param>
		/// <param name="faceCentroids">The centroids of the faces.</param>
		/// <param name="vertexAreas">A pre-allocated collection in which the surrounding surface areas will be stored.</param>
		/// <returns>The surrounding surface areas of the vertices.</returns>
		public static IVertexAttribute<float> CalculatePlanarVertexAreasFromFaceCentroids(Topology.VerticesIndexer vertices, IFaceAttribute<Vector3> faceCentroids, IVertexAttribute<float> vertexAreas)
		{
			foreach (var vertex in vertices)
			{
				if (vertex.neighborCount == 3)
				{
					var edge = vertex.firstEdge;
					var p0 = faceCentroids[edge];
					edge = edge.next;
					var p1 = faceCentroids[edge];
					edge = edge.next;
					var p2 = faceCentroids[edge];
					vertexAreas[vertex] = Vector3.Cross(p1 - p0, p2 - p0).magnitude * 0.5f;
				}
				else
				{
					throw new System.NotImplementedException();
				}
			}

			return vertexAreas;
		}

		/// <summary>
		/// Calculates the surface area around each vertex that is closest to that vertex, based on the positions of the vertices and the centroid positions of their neighboring faces.
		/// </summary>
		/// <param name="vertices">The collection of vertices whose areas are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="faceCentroids">The centroids of the faces.</param>
		/// <returns>The surrounding surface areas of the vertices.</returns>
		public static IVertexAttribute<float> CalculateVertexAreasFromVertexPositionsAndFaceCentroids(Topology.VerticesIndexer vertices, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> faceCentroids)
		{
			return CalculateVertexAreasFromVertexPositionsAndFaceCentroids(vertices, vertexPositions, faceCentroids, new float[vertices.Count].AsVertexAttribute());
		}

		/// <summary>
		/// Calculates the surface area around each vertex that is closest to that vertex, based on the positions of the vertices and the centroid positions of their neighboring faces.
		/// </summary>
		/// <param name="vertices">The collection of vertices whose areas are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="faceCentroids">The centroids of the faces.</param>
		/// <param name="vertexAreas">A pre-allocated collection in which the surrounding surface areas will be stored.</param>
		/// <returns>The surrounding surface areas of the vertices.</returns>
		public static IVertexAttribute<float> CalculateVertexAreasFromVertexPositionsAndFaceCentroids(Topology.VerticesIndexer vertices, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> faceCentroids, IVertexAttribute<float> vertexAreas)
		{
			foreach (var vertex in vertices)
			{
				var center = vertexPositions[vertex];
				var quadAreaSum = 0f;
				var edge = vertex.firstEdge;
				var v0 = vertexPositions[edge] - center;
				edge = edge.next;
				var firstEdge = edge;
				do
				{
					var v2 = vertexPositions[edge] - center;
					if (!edge.isOuterBoundary)
					{
						var v1 = faceCentroids[edge] - center;
						quadAreaSum += Vector3.Cross(v0, v1).magnitude + Vector3.Cross(v1, v2).magnitude;
					}
					v0 = v2;
					edge = edge.next;
				} while (edge != firstEdge);

				vertexAreas[vertex] = quadAreaSum / 4;
			}

			return vertexAreas;
		}

		/// <summary>
		/// Calculates the spherical surface area around each vertex that is closest to that vertex, based on face centroid angles of each edge.
		/// </summary>
		/// <param name="vertices">The collection of vertices whose areas are to be calculated.</param>
		/// <param name="surface">The surface describing the overall shape of the spherical manifold.</param>
		/// <param name="faceCentroidAngles">The centroid angles, in radians, of the face edges.</param>
		/// <returns>The surrounding spherical surface areas of the vertices.</returns>
		/// <seealso cref="O:Experilous.MakeItTile.EdgeAttributeUtility.CalculateSphericalFaceCentroidAnglesFromFaceCentroids"/>
		public static IVertexAttribute<float> CalculateSphericalVertexAreasFromFaceCentroidAngles(Topology.VerticesIndexer vertices, SphericalSurface surface, IEdgeAttribute<float> faceCentroidAngles)
		{
			return CalculateSphericalVertexAreasFromFaceCentroidAngles(vertices, surface, faceCentroidAngles, new float[vertices.Count].AsVertexAttribute());
		}

		/// <summary>
		/// Calculates the spherical surface area around each vertex that is closest to that vertex, based on face centroid angles of each edge.
		/// </summary>
		/// <param name="vertices">The collection of vertices whose areas are to be calculated.</param>
		/// <param name="surface">The surface describing the overall shape of the spherical manifold.</param>
		/// <param name="faceCentroidAngles">The centroid angles, in radians, of the face edges.</param>
		/// <param name="vertexAreas">A pre-allocated collection in which the surrounding surface areas will be stored.</param>
		/// <returns>The surrounding spherical surface areas of the vertices.</returns>
		/// <seealso cref="O:Experilous.MakeItTile.EdgeAttributeUtility.CalculateSphericalFaceCentroidAnglesFromFaceCentroids"/>
		public static IVertexAttribute<float> CalculateSphericalVertexAreasFromFaceCentroidAngles(Topology.VerticesIndexer vertices, SphericalSurface surface, IEdgeAttribute<float> faceCentroidAngles, IVertexAttribute<float> vertexAreas)
		{
			var radiusSquared = surface.radius * surface.radius;

			foreach (var vertex in vertices)
			{
				float angleSum = 0f;
				foreach (var edge in vertex.edges)
				{
					angleSum += faceCentroidAngles[edge];
				}
				vertexAreas[vertex] = (angleSum - Mathf.PI) * radiusSquared;
			}

			return vertexAreas;
		}

		#endregion

		#region CalculateUVs...(...)

		#region CalculateGlobalUVs...(...)

		#region CacluateGlobalPlanarUVs...(...)

		/// <summary>
		/// Calculates the UV coordinates of each vertex, based on the planar UV space indicated.
		/// </summary>
		/// <param name="vertices">The collection of vertices whose UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <returns>The UV coordinates of the vertices.</returns>
		/// <seealso cref="O:Experilous.MakeItTile.EdgeAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions"/>
		public static IVertexAttribute<Vector2> CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(Topology.VerticesIndexer vertices, IVertexAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(vertices, vertexPositions, Vector3.zero, uAxis, vAxis, new Vector2[vertices.Count].AsVertexAttribute());
		}

		/// <summary>
		/// Calculates the UV coordinates of each vertex, based on the planar UV space indicated.
		/// </summary>
		/// <param name="vertices">The collection of vertices whose UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="origin">The world space origin of the UV space, where the UV coordinate will be (0, 0).</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <returns>The UV coordinates of the vertices.</returns>
		/// <seealso cref="O:Experilous.MakeItTile.EdgeAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions"/>
		public static IVertexAttribute<Vector2> CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(Topology.VerticesIndexer vertices, IVertexAttribute<Vector3> vertexPositions, Vector3 origin, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(vertices, vertexPositions, origin, uAxis, vAxis, new Vector2[vertices.Count].AsVertexAttribute());
		}

		/// <summary>
		/// Calculates the UV coordinates of each vertex, based on the planar UV space indicated.
		/// </summary>
		/// <param name="vertices">The collection of vertices whose UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The UV coordinates of the vertices.</returns>
		/// <seealso cref="O:Experilous.MakeItTile.EdgeAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions"/>
		public static IVertexAttribute<Vector2> CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(Topology.VerticesIndexer vertices, IVertexAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, IVertexAttribute<Vector2> uvs)
		{
			return CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(vertices, vertexPositions, Vector3.zero, uAxis, vAxis, uvs);
		}

		/// <summary>
		/// Calculates the UV coordinates of each vertex, based on the planar UV space indicated.
		/// </summary>
		/// <param name="vertices">The collection of vertices whose UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="origin">The world space origin of the UV space, where the UV coordinate will be (0, 0).</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The UV coordinates of the vertices.</returns>
		/// <seealso cref="O:Experilous.MakeItTile.EdgeAttributeUtility.CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions"/>
		public static IVertexAttribute<Vector2> CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(Topology.VerticesIndexer vertices, IVertexAttribute<Vector3> vertexPositions, Vector3 origin, Vector3 uAxis, Vector3 vAxis, IVertexAttribute<Vector2> uvs)
		{
			var normal = Vector3.Cross(vAxis, uAxis).normalized;
			var uPlane = new Plane(origin, uAxis + origin, normal + origin);
			var vPlane = new Plane(origin, normal + origin, vAxis + origin);

			var uAxisNeg = -uAxis;
			var vAxisNeg = -vAxis;

			foreach (var vertex in vertices)
			{
				var vertexPosition = vertexPositions[vertex];

				uvs[vertex] = new Vector2(
					Geometry.GetIntersectionParameter(vPlane, new ScaledRay(vertexPosition, uAxisNeg)),
					Geometry.GetIntersectionParameter(uPlane, new ScaledRay(vertexPosition, vAxisNeg)));
			}

			return uvs;
		}

		/// <summary>
		/// Calculates the UV coordinates of each vertex, based on the planar UV space indicated, renormalized afterward over the entire manifold so that UVs are bound to the unit square from (0, 0) to (1, 1).
		/// </summary>
		/// <param name="vertices">The collection of vertices whose UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <returns>The UV coordinates of the vertices.</returns>
		/// <seealso cref="O:Experilous.MakeItTile.EdgeAttributeUtility.CalculateGlobalPlanarNormalizedUVsFromVertexPositions"/>
		public static IVertexAttribute<Vector2> CalculateGlobalPlanarNormalizedUVsFromVertexPositions(Topology.VerticesIndexer vertices, IVertexAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculateGlobalPlanarNormalizedUVsFromVertexPositions(vertices, vertexPositions, uAxis, vAxis, new Vector2[vertices.Count].AsVertexAttribute());
		}

		/// <summary>
		/// Calculates the UV coordinates of each vertex, based on the planar UV space indicated, renormalized afterward over the entire manifold so that UVs are bound to the unit square from (0, 0) to (1, 1).
		/// </summary>
		/// <param name="vertices">The collection of vertices whose UV coordinates are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The UV coordinates of the vertices.</returns>
		/// <seealso cref="O:Experilous.MakeItTile.EdgeAttributeUtility.CalculateGlobalPlanarNormalizedUVsFromVertexPositions"/>
		public static IVertexAttribute<Vector2> CalculateGlobalPlanarNormalizedUVsFromVertexPositions(Topology.VerticesIndexer vertices, IVertexAttribute<Vector3> vertexPositions, Vector3 uAxis, Vector3 vAxis, IVertexAttribute<Vector2> uvs)
		{
			CalculateGlobalPlanarUnnormalizedUVsFromVertexPositions(vertices, vertexPositions, uAxis, vAxis, uvs);

			Vector2 uvMin = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
			Vector2 uvMax = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

			foreach (var vertex in vertices)
			{
				uvMin = Geometry.AxisAlignedMin(uvMin, uvs[vertex]);
				uvMax = Geometry.AxisAlignedMax(uvMax, uvs[vertex]);
			}

			var uvRange = uvMax - uvMin;
			var uvRangeReciprocal = new Vector2(1f / uvRange.x, 1f / uvRange.y);

			foreach (var vertex in vertices)
			{
				uvs[vertex] = Vector2.Scale(uvs[vertex] - uvMin, uvRangeReciprocal);
			}

			return uvs;
		}

		#endregion

		#region CalculateGlobalSphericalUVs...(...)

		/// <summary>
		/// Calculates the UV coordinates of each vertex, based on the spherical longitude/latitude UV space indicated.
		/// </summary>
		/// <param name="vertices">The collection of vertices whose UV coordinates are to be calculated.</param>
		/// <param name="surface">The surface describing the overall shape of the spherical manifold.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="orientation">The world space orientation of the UV space.</param>
		/// <returns>The UV coordinates of the vertices.</returns>
		public static IVertexAttribute<Vector2> CalculateGlobalSphericalUVsFromVertexPositions(Topology.VerticesIndexer vertices, SphericalSurface surface, IVertexAttribute<Vector3> vertexPositions, Quaternion orientation)
		{
			return CalculateGlobalSphericalUVsFromVertexPositions(vertices, surface, vertexPositions, orientation, new Vector2[vertices.Count].AsVertexAttribute());
		}

		/// <summary>
		/// Calculates the UV coordinates of each vertex, based on the spherical longitude/latitude UV space indicated.
		/// </summary>
		/// <param name="vertices">The collection of vertices whose UV coordinates are to be calculated.</param>
		/// <param name="surface">The surface describing the overall shape of the spherical manifold.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="orientation">The world space orientation of the UV space.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The UV coordinates of the vertices.</returns>
		public static IVertexAttribute<Vector2> CalculateGlobalSphericalUVsFromVertexPositions(Topology.VerticesIndexer vertices, SphericalSurface surface, IVertexAttribute<Vector3> vertexPositions, Quaternion orientation, IVertexAttribute<Vector2> uvs)
		{
			var twoPi = Mathf.PI * 2f;
			foreach (var vertex in vertices)
			{
				var normal = (orientation * vertexPositions[vertex]).normalized;
				uvs[vertex] = new Vector2(
					Mathf.Atan2(normal.z, normal.x) / twoPi,
					Mathf.Acos(normal.y) / twoPi);
			}

			return uvs;
		}

		#endregion

		#endregion

		#endregion
	}
}
