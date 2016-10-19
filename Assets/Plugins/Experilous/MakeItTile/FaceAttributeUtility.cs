/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using Experilous.Numerics;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// A static utility class for calculating various per-face attributes.
	/// </summary>
	public static class FaceAttributeUtility
	{
		#region IFaceAttribute<Vector3> CacluateFaceCentroids...(...)

		/// <summary>
		/// Calculates face centroids based on the positions of their neighboring vertices.
		/// </summary>
		/// <param name="faces">The collection of faces whose centroids are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <returns>The centroids of the vertices.</returns>
		public static IFaceAttribute<Vector3> CalculateFaceCentroidsFromVertexPositions(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions)
		{
			return CalculateFaceCentroidsFromVertexPositions(faces, vertexPositions, new Vector3[faces.Count].AsFaceAttribute());
		}

		/// <summary>
		/// Calculates face centroids based on the positions of their neighboring vertices.
		/// </summary>
		/// <param name="faces">The collection of faces whose centroids are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="faceCentroids">A pre-allocated collection in which the face centroids will be stored.</param>
		/// <returns>The centroids of the vertices.</returns>
		public static IFaceAttribute<Vector3> CalculateFaceCentroidsFromVertexPositions(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> faceCentroids)
		{
			foreach (var face in faces)
			{
				var centroid = new Vector3(0f, 0f, 0f);
				foreach (var edge in face.edges)
				{
					centroid += vertexPositions[edge];
				}
				faceCentroids[face] = centroid / face.neighborCount;
			}

			return faceCentroids;
		}

		/// <summary>
		/// Calculates spherical face centroids based on the positions of their neighboring vertices.
		/// </summary>
		/// <param name="faces">The collection of faces whose centroids are to be calculated.</param>
		/// <param name="surface">The surface describing the overall shape of the spherical manifold.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <returns>The centroids of the vertices.</returns>
		public static IFaceAttribute<Vector3> CalculateSphericalFaceCentroidsFromVertexPositions(Topology.FacesIndexer faces, SphericalSurface surface, IVertexAttribute<Vector3> vertexPositions)
		{
			return CalculateSphericalFaceCentroidsFromVertexPositions(faces, surface, vertexPositions, new Vector3[faces.Count].AsFaceAttribute());
		}

		/// <summary>
		/// Calculates spherical face centroids based on the positions of their neighboring vertices.
		/// </summary>
		/// <param name="faces">The collection of faces whose centroids are to be calculated.</param>
		/// <param name="surface">The surface describing the overall shape of the spherical manifold.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="faceCentroids">A pre-allocated collection in which the face centroids will be stored.</param>
		/// <returns>The centroids of the vertices.</returns>
		public static IFaceAttribute<Vector3> CalculateSphericalFaceCentroidsFromVertexPositions(Topology.FacesIndexer faces, SphericalSurface surface, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> faceCentroids)
		{
			foreach (var face in faces)
			{
				var centroid = new Vector3(0f, 0f, 0f);
				foreach (var edge in face.edges)
				{
					centroid += vertexPositions[edge];
				}
				faceCentroids[face] = centroid * (surface.radius / centroid.magnitude);
			}

			return faceCentroids;
		}

		#endregion

		#region IFaceAttribute<Vector3> CalculateFaceNormals...(...)

		/// <summary>
		/// Calculates face normals using the surface normal reported by the given surface based on the position of each face.
		/// </summary>
		/// <param name="faces">The collection of vertices whose normals are to be calculated.</param>
		/// <param name="surface">The surface descriptor that will provide surface normal information.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <returns>The surface normals of the faces.</returns>
		public static IFaceAttribute<Vector3> CalculateFaceNormalsFromSurface(Topology.FacesIndexer faces, Surface surface, IFaceAttribute<Vector3> facePositions)
		{
			return CalculateFaceNormalsFromSurface(faces, surface, facePositions, new Vector3[faces.Count].AsFaceAttribute());
		}

		/// <summary>
		/// Calculates face normals using the surface normal reported by the given surface based on the position of each face.
		/// </summary>
		/// <param name="faces">The collection of vertices whose normals are to be calculated.</param>
		/// <param name="surface">The surface descriptor that will provide surface normal information.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="faceNormals">A pre-allocated collection in which the face normals will be stored.</param>
		/// <returns>The surface normals of the faces.</returns>
		public static IFaceAttribute<Vector3> CalculateFaceNormalsFromSurface(Topology.FacesIndexer faces, Surface surface, IFaceAttribute<Vector3> facePositions, IFaceAttribute<Vector3> faceNormals)
		{
			foreach (var face in faces)
			{
				faceNormals[face] = surface.GetNormal(facePositions[face]);
			}

			return faceNormals;
		}

		/// <summary>
		/// Calculates face normals based on their positions and the positions of their neighbors.
		/// </summary>
		/// <param name="faces">The collection of faces whose normals are to be calculated.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <returns>The surface normals of the faces.</returns>
		public static IFaceAttribute<Vector3> CalculateFaceNormalsFromFacePositions(Topology.FacesIndexer faces, IFaceAttribute<Vector3> facePositions)
		{
			return CalculateFaceNormalsFromFacePositions(faces, facePositions, new Vector3[faces.Count].AsFaceAttribute());
		}

		/// <summary>
		/// Calculates face normals based on their positions and the positions of their neighbors.
		/// </summary>
		/// <param name="faces">The collection of faces whose normals are to be calculated.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="faceNormals">A pre-allocated collection in which the face normals will be stored.</param>
		/// <returns>The surface normals of the faces.</returns>
		public static IFaceAttribute<Vector3> CalculateFaceNormalsFromFacePositions(Topology.FacesIndexer faces, IFaceAttribute<Vector3> facePositions, IFaceAttribute<Vector3> faceNormals)
		{
			foreach (var face in faces)
			{
				var normalSum = new Vector3(0f, 0f, 0f);
				var facePosition = facePositions[face];

				var edge = face.firstEdge;
				var p0 = facePositions[edge];
				edge = edge.next;
				var firstEdge = edge;
				do
				{
					var p1 = facePositions[edge];
					normalSum += Vector3.Cross(p0 - facePosition, p1 - facePosition);
					edge = edge.next;
				} while (edge != firstEdge);

				faceNormals[face] = normalSum.normalized;
			}

			return faceNormals;
		}

		/// <summary>
		/// Calculates face normals based on the positions of their neighboring vertices.
		/// </summary>
		/// <param name="faces">The collection of faces whose normals are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <returns>The surface normals of the faces.</returns>
		public static IFaceAttribute<Vector3> CalculateFaceNormalsFromVertexPositions(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions)
		{
			return CalculateFaceNormalsFromVertexPositions(faces, vertexPositions, new Vector3[faces.Count].AsFaceAttribute());
		}

		/// <summary>
		/// Calculates face normals based on the positions of their neighboring vertices.
		/// </summary>
		/// <param name="faces">The collection of faces whose normals are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="faceNormals">A pre-allocated collection in which the face normals will be stored.</param>
		/// <returns>The surface normals of the faces.</returns>
		public static IFaceAttribute<Vector3> CalculateFaceNormalsFromVertexPositions(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> faceNormals)
		{
			foreach (var face in faces)
			{
				var normalSum = new Vector3(0f, 0f, 0f);

				var edge = face.firstEdge;
				var p0 = vertexPositions[edge];
				edge = edge.next;
				var p1 = vertexPositions[edge];
				edge = edge.next;
				var firstEdge = edge;
				do
				{
					var p2 = vertexPositions[edge];
					normalSum += Vector3.Cross(p0 - p1, p2 - p1);
					edge = edge.next;
				} while (edge != firstEdge);

				faceNormals[face] = normalSum.normalized;
			}

			return faceNormals;
		}

		/// <summary>
		/// Calculates face normals based on the surface normals of their neighboring vertices.
		/// </summary>
		/// <param name="faces">The collection of faces whose normals are to be calculated.</param>
		/// <param name="vertexNormals">The surface normals of the vertices.</param>
		/// <returns>The surface normals of the faces.</returns>
		public static IFaceAttribute<Vector3> CalculateFaceNormalsFromVertexNormals(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexNormals)
		{
			return CalculateFaceNormalsFromVertexNormals(faces, vertexNormals, new Vector3[faces.Count].AsFaceAttribute());
		}

		/// <summary>
		/// Calculates face normals based on the surface normals of their neighboring vertices.
		/// </summary>
		/// <param name="faces">The collection of faces whose normals are to be calculated.</param>
		/// <param name="vertexNormals">The surface normals of the vertices.</param>
		/// <param name="faceNormals">A pre-allocated collection in which the face normals will be stored.</param>
		/// <returns>The surface normals of the faces.</returns>
		public static IFaceAttribute<Vector3> CalculateFaceNormalsFromVertexNormals(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexNormals, IFaceAttribute<Vector3> faceNormals)
		{
			foreach (var face in faces)
			{
				var normalSum = new Vector3(0f, 0f, 0f);

				foreach (var edge in face.edges)
				{
					normalSum += vertexNormals[edge];
				}

				faceNormals[face] = normalSum.normalized;
			}

			return faceNormals;
		}

		/// <summary>
		/// Calculates face normals based on the positions of their neighboring vertices and the fact that they are part of a spherical surface centered at the origin.
		/// </summary>
		/// <param name="faces">The collection of faces whose normals are to be calculated.</param>
		/// <param name="surface">The surface describing the overall shape of the spherical manifold.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <returns>The surface normals of the faces.</returns>
		public static IFaceAttribute<Vector3> CalculateSphericalFaceNormalsFromVertexPositions(Topology.FacesIndexer faces, SphericalSurface surface, IVertexAttribute<Vector3> vertexPositions)
		{
			return CalculateSphericalFaceNormalsFromVertexPositions(faces, surface, vertexPositions);
		}
		
		/// <summary>
		/// Calculates face normals based on the positions of their neighboring vertices and the fact that they are part of a spherical surface centered at the origin.
		/// </summary>
		/// <param name="faces">The collection of faces whose normals are to be calculated.</param>
		/// <param name="surface">The surface describing the overall shape of the spherical manifold.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="faceNormals">A pre-allocated collection in which the face normals will be stored.</param>
		/// <returns>The surface normals of the faces.</returns>
		public static IFaceAttribute<Vector3> CalculateSphericalFaceNormalsFromVertexPositions(Topology.FacesIndexer faces, SphericalSurface surface, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<Vector3> faceNormals)
		{
			return CalculateSphericalFaceCentroidsFromVertexPositions(faces, surface, vertexPositions, faceNormals);
		}
		
		/// <summary>
		/// Calculates face normals based on their positions and the fact that they are part of a spherical surface centered at the origin.
		/// </summary>
		/// <param name="faces">The collection of faces whose normals are to be calculated.</param>
		/// <param name="surface">The surface describing the overall shape of the spherical manifold.</param>
		/// <param name="facePositions">The positions of the faces.</param>
		/// <returns>The surface normals of the faces.</returns>
		public static IFaceAttribute<Vector3> CalculateSphericalFaceNormalsFromFacePositions(Topology.FacesIndexer faces, SphericalSurface surface, IFaceAttribute<Vector3> facePositions)
		{
			return CalculateSphericalFaceNormalsFromFacePositions(faces, surface, facePositions, new Vector3[faces.Count].AsFaceAttribute());
		}

		/// <summary>
		/// Calculates face normals based on their positions and the fact that they are part of a spherical surface centered at the origin.
		/// </summary>
		/// <param name="faces">The collection of faces whose normals are to be calculated.</param>
		/// <param name="surface">The surface describing the overall shape of the spherical manifold.</param>
		/// <param name="facePositions">The positions of the faces.</param>
		/// <param name="faceNormals">A pre-allocated collection in which the face normals will be stored.</param>
		/// <returns>The surface normals of the faces.</returns>
		public static IFaceAttribute<Vector3> CalculateSphericalFaceNormalsFromFacePositions(Topology.FacesIndexer faces, SphericalSurface surface, IFaceAttribute<Vector3> facePositions, IFaceAttribute<Vector3> faceNormals)
		{
			if (!surface.isInverted)
			{
				foreach (var face in faces)
				{
					faceNormals[face] = facePositions[face].normalized;
				}
			}
			else
			{
				foreach (var face in faces)
				{
					faceNormals[face] = -facePositions[face].normalized;
				}
			}
			return faceNormals;
		}

		#endregion

		#region IFaceAttribute<float> CalculateFaceAreas...(...)

		/// <summary>
		/// Calculates the planar surface area of each face, based on the positions of their neighboring vertices.  For non-flat surfaces, the calculated areas are only approximate.
		/// </summary>
		/// <param name="faces">The collection of faces whose areas are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <returns>The surface areas of the faces.</returns>
		public static IFaceAttribute<float> CalculatePlanarFaceAreasFromVertexPositions(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions)
		{
			return CalculatePlanarFaceAreasFromVertexPositions(faces, vertexPositions, new float[faces.Count].AsFaceAttribute());
		}

		/// <summary>
		/// Calculates the planar surface area of each face, based on the positions of their neighboring vertices.  For non-flat surfaces, the calculated areas are only approximate.
		/// </summary>
		/// <param name="faces">The collection of faces whose areas are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="faceAreas">A pre-allocated collection in which the surface areas will be stored.</param>
		/// <returns>The surface areas of the faces.</returns>
		public static IFaceAttribute<float> CalculatePlanarFaceAreasFromVertexPositions(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<float> faceAreas)
		{
			foreach (var face in faces)
			{
				if (face.neighborCount == 3)
				{
					var edge = face.firstEdge;
					var p0 = vertexPositions[edge];
					edge = edge.next;
					var p1 = vertexPositions[edge];
					edge = edge.next;
					var p2 = vertexPositions[edge];
					faceAreas[face] = Vector3.Cross(p1 - p0, p2 - p0).magnitude * 0.5f;
				}
				else
				{
					throw new System.NotImplementedException();
				}
			}

			return faceAreas;
		}

		/// <summary>
		/// Calculates the spherical surface area of each face, based on face centroid angles of each edge.
		/// </summary>
		/// <param name="faces">The collection of faces whose areas are to be calculated.</param>
		/// <param name="surface">The surface describing the overall shape of the spherical manifold.</param>
		/// <param name="faceCentroidAngles">The centroid angles, in radians, of the face edges.</param>
		/// <returns>The spherical surface areas of the faces.</returns>
		/// <seealso cref="O:Experilous.MakeItTile.EdgeAttributeUtility.CalculateSphericalFaceCentroidAnglesFromFaceCentroids"/>
		public static IFaceAttribute<float> CalculateSphericalFaceAreasFromFaceCentroidAngles(Topology.FacesIndexer faces, SphericalSurface surface, IEdgeAttribute<float> faceCentroidAngles)
		{
			return CalculateSphericalFaceAreasFromFaceCentroidAngles(faces, surface, faceCentroidAngles, new float[faces.Count].AsFaceAttribute());
		}

		/// <summary>
		/// Calculates the spherical surface area of each face, based on face centroid angles of each edge.
		/// </summary>
		/// <param name="faces">The collection of faces whose areas are to be calculated.</param>
		/// <param name="surface">The surface describing the overall shape of the spherical manifold.</param>
		/// <param name="faceCentroidAngles">The centroid angles, in radians, of the face edges.</param>
		/// <param name="faceAreas">A pre-allocated collection in which the surface areas will be stored.</param>
		/// <returns>The spherical surface areas of the faces.</returns>
		/// <seealso cref="O:Experilous.MakeItTile.EdgeAttributeUtility.CalculateSphericalFaceCentroidAnglesFromFaceCentroids"/>
		public static IFaceAttribute<float> CalculateSphericalFaceAreasFromFaceCentroidAngles(Topology.FacesIndexer faces, SphericalSurface surface, IEdgeAttribute<float> faceCentroidAngles, IFaceAttribute<float> faceAreas)
		{
			var radiusSquared = surface.radius * surface.radius;

			foreach (var face in faces)
			{
				float angleSum = 0f;
				foreach (var edge in face.edges)
				{
					angleSum += faceCentroidAngles[edge];
				}
				faceAreas[face] = (angleSum - Mathf.PI) * radiusSquared;
			}

			return faceAreas;
		}

		#endregion

		/// <summary>
		/// Calculates the circumference of each face, based on the positions of their neighboring vertices.
		/// </summary>
		/// <param name="faces">The collection of faces whose circumferences are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <returns>The circumferences of the faces.</returns>
		public static IFaceAttribute<float> CalculateFaceCircumferencesFromVertexPositions(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions)
		{
			return CalculateFaceCircumferencesFromVertexPositions(faces, vertexPositions, new float[faces.Count].AsFaceAttribute());
		}

		/// <summary>
		/// Calculates the circumference of each face, based on the positions of their neighboring vertices.
		/// </summary>
		/// <param name="faces">The collection of faces whose circumferences are to be calculated.</param>
		/// <param name="vertexPositions">The positions of the vertices.</param>
		/// <param name="faceCircumferences">A pre-allocated collection in which the circumferences will be stored.</param>
		/// <returns>The circumferences of the faces.</returns>
		public static IFaceAttribute<float> CalculateFaceCircumferencesFromVertexPositions(Topology.FacesIndexer faces, IVertexAttribute<Vector3> vertexPositions, IFaceAttribute<float> faceCircumferences)
		{
			foreach (var face in faces)
			{
				float circumference = 0f;
				var firstEdge = face.firstEdge;
				var priorPosition = vertexPositions[firstEdge];
				firstEdge = firstEdge.next;
				var nextEdge = firstEdge;
				do
				{
					var currentPosition = vertexPositions[nextEdge];
					circumference += Vector3.Distance(priorPosition, currentPosition);
					priorPosition = currentPosition;
					nextEdge = nextEdge.next;
				} while (nextEdge != firstEdge);

				faceCircumferences[face] = circumference;
			}

			return faceCircumferences;
		}

		/// <summary>
		/// Calculates the circumference of each face, based on the lengths of their neighboring edges.
		/// </summary>
		/// <param name="faces">The collection of faces whose circumferences are to be calculated.</param>
		/// <param name="edgeLengths">The lengths of the edges, as measured between the two vertices of each edge.</param>
		/// <returns>The circumferences of the faces.</returns>
		public static IFaceAttribute<float> CalculateFaceCircumferencesFromEdgeLengths(Topology.FacesIndexer faces, IEdgeAttribute<float> edgeLengths)
		{
			return CalculateFaceCircumferencesFromEdgeLengths(faces, edgeLengths, new float[faces.Count].AsFaceAttribute());
		}

		/// <summary>
		/// Calculates the circumference of each face, based on the lengths of their neighboring edges.
		/// </summary>
		/// <param name="faces">The collection of faces whose circumferences are to be calculated.</param>
		/// <param name="edgeLengths">The lengths of the edges, as measured between the two vertices of each edge.</param>
		/// <param name="faceCircumferences">A pre-allocated collection in which the circumferences will be stored.</param>
		/// <returns>The circumferences of the faces.</returns>
		public static IFaceAttribute<float> CalculateFaceCircumferencesFromEdgeLengths(Topology.FacesIndexer faces, IEdgeAttribute<float> edgeLengths, IFaceAttribute<float> faceCircumferences)
		{
			foreach (var face in faces)
			{
				float circumference = 0f;
				foreach (var edge in face.edges)
				{
					circumference += edgeLengths[edge];
				}
				faceCircumferences[face] = circumference;
			}

			return faceCircumferences;
		}

		#region CalculateMinAndRangeValues...(...)

		/// <summary>
		/// Determines the minimum value and the range between the minimum and maximum values of an arbitrary edge attribute for the neighbors of each face.
		/// </summary>
		/// <param name="faces">The faces whose minimum and range of neighboring edge attributes are to be determined.</param>
		/// <param name="edgeValues">The edge values to be examined.</param>
		/// <param name="minValues">A pre-allocated collection in which the minimum values will be stored.</param>
		/// <param name="ranges">A pre-allocated collection in which the range of values will be stored.</param>
		public static void CalculateFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<float> edgeValues, IFaceAttribute<float> minValues, IFaceAttribute<float> ranges)
		{
			foreach (var face in faces)
			{
				var min = float.MaxValue;
				var max = float.MinValue;
				foreach (var edge in face.edges)
				{
					min = Mathf.Min(min, edgeValues[edge]);
					max = Mathf.Max(max, edgeValues[edge]);
				}
				minValues[face] = min;
				ranges[face] = max - min;
			}
		}

		/// <summary>
		/// Determines the minimum value and the range between the minimum and maximum values of an arbitrary edge attribute for the neighbors of each face.
		/// </summary>
		/// <param name="faces">The faces whose minimum and range of neighboring edge attributes are to be determined.</param>
		/// <param name="edgeValues">The edge values to be examined.</param>
		/// <param name="minValues">A pre-allocated collection in which the minimum values will be stored.</param>
		/// <param name="ranges">A pre-allocated collection in which the range of values will be stored.</param>
		public static void CalculateFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<Vector2> edgeValues, IFaceAttribute<Vector2> minValues, IFaceAttribute<Vector2> ranges)
		{
			foreach (var face in faces)
			{
				var min = new Vector2(float.MaxValue, float.MaxValue);
				var max = new Vector2(float.MinValue, float.MinValue);
				foreach (var edge in face.edges)
				{
					min = Geometry.AxisAlignedMin(min, edgeValues[edge]);
					max = Geometry.AxisAlignedMax(max, edgeValues[edge]);
				}
				minValues[face] = min;
				ranges[face] = max - min;
			}
		}

		/// <summary>
		/// Determines the minimum value and the range between the minimum and maximum values of an arbitrary edge attribute for the neighbors of each face.
		/// </summary>
		/// <param name="faces">The faces whose minimum and range of neighboring edge attributes are to be determined.</param>
		/// <param name="edgeValues">The edge values to be examined.</param>
		/// <param name="minValues">A pre-allocated collection in which the minimum values will be stored.</param>
		/// <param name="ranges">A pre-allocated collection in which the range of values will be stored.</param>
		public static void CalculateFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> edgeValues, IFaceAttribute<Vector3> minValues, IFaceAttribute<Vector3> ranges)
		{
			foreach (var face in faces)
			{
				var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
				var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
				foreach (var edge in face.edges)
				{
					min = Geometry.AxisAlignedMin(min, edgeValues[edge]);
					max = Geometry.AxisAlignedMax(max, edgeValues[edge]);
				}
				minValues[face] = min;
				ranges[face] = max - min;
			}
		}

		/// <summary>
		/// Determines the minimum value and the range between the minimum and maximum values of an arbitrary edge attribute for the neighbors of each face.
		/// </summary>
		/// <param name="faces">The faces whose minimum and range of neighboring edge attributes are to be determined.</param>
		/// <param name="edgeValues">The edge values to be examined.</param>
		/// <param name="minValues">A pre-allocated collection in which the minimum values will be stored.</param>
		/// <param name="ranges">A pre-allocated collection in which the range of values will be stored.</param>
		public static void CalculateFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<Vector4> edgeValues, IFaceAttribute<Vector4> minValues, IFaceAttribute<Vector4> ranges)
		{
			foreach (var face in faces)
			{
				var min = new Vector4(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);
				var max = new Vector4(float.MinValue, float.MinValue, float.MinValue, float.MinValue);
				foreach (var edge in face.edges)
				{
					min = Geometry.AxisAlignedMin(min, edgeValues[edge]);
					max = Geometry.AxisAlignedMax(max, edgeValues[edge]);
				}
				minValues[face] = min;
				ranges[face] = max - min;
			}
		}

		/// <summary>
		/// Expands the minimum value and the range between the minimum and maximum values of an arbitrary edge attribute for the neighbors of each face, based on new edge attribute values.
		/// </summary>
		/// <param name="faces">The faces whose minimum and range of neighboring edge attributes are to be expanded.</param>
		/// <param name="edgeValues">The new edge values to be examined.</param>
		/// <param name="minValues">The existing minimum values to be updated.  These values will either stay unchanged or will get smaller; none of them will ever increase.</param>
		/// <param name="ranges">The existing ranges of values to be updated.  These values will either stay unchanged or will get larger; none of them will ever decrease.</param>
		public static void ExpandFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<float> edgeValues, IFaceAttribute<float> minValues, IFaceAttribute<float> ranges)
		{
			foreach (var face in faces)
			{
				var min = minValues[face];
				var max = min + ranges[face];
				foreach (var edge in face.edges)
				{
					min = Mathf.Min(min, edgeValues[edge]);
					max = Mathf.Max(max, edgeValues[edge]);
				}
				minValues[face] = min;
				ranges[face] = max - min;
			}
		}

		/// <summary>
		/// Expands the minimum value and the range between the minimum and maximum values of an arbitrary edge attribute for the neighbors of each face, based on new edge attribute values.
		/// </summary>
		/// <param name="faces">The faces whose minimum and range of neighboring edge attributes are to be expanded.</param>
		/// <param name="edgeValues">The new edge values to be examined.</param>
		/// <param name="minValues">The existing minimum values to be updated.  These values will either stay unchanged or will get smaller; none of them will ever increase.</param>
		/// <param name="ranges">The existing ranges of values to be updated.  These values will either stay unchanged or will get larger; none of them will ever decrease.</param>
		public static void ExpandFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<Vector2> edgeValues, IFaceAttribute<Vector2> minValues, IFaceAttribute<Vector2> ranges)
		{
			foreach (var face in faces)
			{
				var min = minValues[face];
				var max = min + ranges[face];
				foreach (var edge in face.edges)
				{
					min = Geometry.AxisAlignedMin(min, edgeValues[edge]);
					max = Geometry.AxisAlignedMax(max, edgeValues[edge]);
				}
				minValues[face] = min;
				ranges[face] = max - min;
			}
		}

		/// <summary>
		/// Expands the minimum value and the range between the minimum and maximum values of an arbitrary edge attribute for the neighbors of each face, based on new edge attribute values.
		/// </summary>
		/// <param name="faces">The faces whose minimum and range of neighboring edge attributes are to be expanded.</param>
		/// <param name="edgeValues">The new edge values to be examined.</param>
		/// <param name="minValues">The existing minimum values to be updated.  These values will either stay unchanged or will get smaller; none of them will ever increase.</param>
		/// <param name="ranges">The existing ranges of values to be updated.  These values will either stay unchanged or will get larger; none of them will ever decrease.</param>
		public static void ExpandFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> edgeValues, IFaceAttribute<Vector3> minValues, IFaceAttribute<Vector3> ranges)
		{
			foreach (var face in faces)
			{
				var min = minValues[face];
				var max = min + ranges[face];
				foreach (var edge in face.edges)
				{
					min = Geometry.AxisAlignedMin(min, edgeValues[edge]);
					max = Geometry.AxisAlignedMax(max, edgeValues[edge]);
				}
				minValues[face] = min;
				ranges[face] = max - min;
			}
		}

		/// <summary>
		/// Expands the minimum value and the range between the minimum and maximum values of an arbitrary edge attribute for the neighbors of each face, based on new edge attribute values.
		/// </summary>
		/// <param name="faces">The faces whose minimum and range of neighboring edge attributes are to be expanded.</param>
		/// <param name="edgeValues">The new edge values to be examined.</param>
		/// <param name="minValues">The existing minimum values to be updated.  These values will either stay unchanged or will get smaller; none of them will ever increase.</param>
		/// <param name="ranges">The existing ranges of values to be updated.  These values will either stay unchanged or will get larger; none of them will ever decrease.</param>
		public static void ExpandFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<Vector4> edgeValues, IFaceAttribute<Vector4> minValues, IFaceAttribute<Vector4> ranges)
		{
			foreach (var face in faces)
			{
				var min = minValues[face];
				var max = min + ranges[face];
				foreach (var edge in face.edges)
				{
					min = Geometry.AxisAlignedMin(min, edgeValues[edge]);
					max = Geometry.AxisAlignedMax(max, edgeValues[edge]);
				}
				minValues[face] = min;
				ranges[face] = max - min;
			}
		}

		#endregion

		#region CalculateCenteredMinAndRangeValues...(...)

		/// <summary>
		/// Determines the minimum value and the range between the minimum and maximum values of an arbitrary edge attribute for the neighbors of each face, with the minimums and ranges adjusted to maintain the center values of the faces.
		/// </summary>
		/// <param name="faces">The faces whose minimum and range of neighboring edge attributes are to be determined.</param>
		/// <param name="edgeValues">The edge values to be examined.</param>
		/// <param name="centerValues">The center values of the faces.</param>
		/// <param name="minValues">A pre-allocated collection in which the minimum values will be stored.</param>
		/// <param name="ranges">A pre-allocated collection in which the range of values will be stored.</param>
		/// <remarks><para>Once the minimum and maximum value for each face has been calculated, the maximum
		/// distance from either of them to the face's center value is determined, and the final minimum and
		/// range values are chosen such that the center value remains in the middle of that span.</para></remarks>
		public static void CalculateCenteredFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<float> edgeValues, IFaceAttribute<float> centerValues, IFaceAttribute<float> minValues, IFaceAttribute<float> ranges)
		{
			foreach (var face in faces)
			{
				var min = float.MaxValue;
				var max = float.MinValue;
				foreach (var edge in face.edges)
				{
					min = Mathf.Min(min, edgeValues[edge]);
					max = Mathf.Max(max, edgeValues[edge]);
				}
				var center = centerValues[face];
				var extent = Mathf.Max(max - center, center - min);
				minValues[face] = center - extent;
				ranges[face] = extent * 2f;
			}
		}

		/// <summary>
		/// Determines the minimum value and the range between the minimum and maximum values of an arbitrary edge attribute for the neighbors of each face, with the minimums and ranges adjusted to maintain the center values of the faces.
		/// </summary>
		/// <param name="faces">The faces whose minimum and range of neighboring edge attributes are to be determined.</param>
		/// <param name="edgeValues">The edge values to be examined.</param>
		/// <param name="centerValues">The center values of the faces.</param>
		/// <param name="minValues">A pre-allocated collection in which the minimum values will be stored.</param>
		/// <param name="ranges">A pre-allocated collection in which the range of values will be stored.</param>
		/// <remarks><para>Once the minimum and maximum value for each face has been calculated, the maximum
		/// distance from either of them to the face's center value is determined, and the final minimum and
		/// range values are chosen such that the center value remains in the middle of that span.</para></remarks>
		public static void CalculateCenteredFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<Vector2> edgeValues, IFaceAttribute<Vector2> centerValues, IFaceAttribute<Vector2> minValues, IFaceAttribute<Vector2> ranges)
		{
			foreach (var face in faces)
			{
				var min = new Vector2(float.MaxValue, float.MaxValue);
				var max = new Vector2(float.MinValue, float.MinValue);
				foreach (var edge in face.edges)
				{
					min = Geometry.AxisAlignedMin(min, edgeValues[edge]);
					max = Geometry.AxisAlignedMax(max, edgeValues[edge]);
				}
				var center = centerValues[face];
				var extent = Geometry.AxisAlignedMax(max - center, center - min);
				minValues[face] = center - extent;
				ranges[face] = extent * 2f;
			}
		}

		/// <summary>
		/// Determines the minimum value and the range between the minimum and maximum values of an arbitrary edge attribute for the neighbors of each face, with the minimums and ranges adjusted to maintain the center values of the faces.
		/// </summary>
		/// <param name="faces">The faces whose minimum and range of neighboring edge attributes are to be determined.</param>
		/// <param name="edgeValues">The edge values to be examined.</param>
		/// <param name="centerValues">The center values of the faces.</param>
		/// <param name="minValues">A pre-allocated collection in which the minimum values will be stored.</param>
		/// <param name="ranges">A pre-allocated collection in which the range of values will be stored.</param>
		/// <remarks><para>Once the minimum and maximum value for each face has been calculated, the maximum
		/// distance from either of them to the face's center value is determined, and the final minimum and
		/// range values are chosen such that the center value remains in the middle of that span.</para></remarks>
		public static void CalculateCenteredFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> edgeValues, IFaceAttribute<Vector3> centerValues, IFaceAttribute<Vector3> minValues, IFaceAttribute<Vector3> ranges)
		{
			foreach (var face in faces)
			{
				var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
				var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
				foreach (var edge in face.edges)
				{
					min = Geometry.AxisAlignedMin(min, edgeValues[edge]);
					max = Geometry.AxisAlignedMax(max, edgeValues[edge]);
				}
				var center = centerValues[face];
				var extent = Geometry.AxisAlignedMax(max - center, center - min);
				minValues[face] = center - extent;
				ranges[face] = extent * 2f;
			}
		}

		/// <summary>
		/// Determines the minimum value and the range between the minimum and maximum values of an arbitrary edge attribute for the neighbors of each face, with the minimums and ranges adjusted to maintain the center values of the faces.
		/// </summary>
		/// <param name="faces">The faces whose minimum and range of neighboring edge attributes are to be determined.</param>
		/// <param name="edgeValues">The edge values to be examined.</param>
		/// <param name="centerValues">The center values of the faces.</param>
		/// <param name="minValues">A pre-allocated collection in which the minimum values will be stored.</param>
		/// <param name="ranges">A pre-allocated collection in which the range of values will be stored.</param>
		/// <remarks><para>Once the minimum and maximum value for each face has been calculated, the maximum
		/// distance from either of them to the face's center value is determined, and the final minimum and
		/// range values are chosen such that the center value remains in the middle of that span.</para></remarks>
		public static void CalculateCenteredFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<Vector4> edgeValues, IFaceAttribute<Vector4> centerValues, IFaceAttribute<Vector4> minValues, IFaceAttribute<Vector4> ranges)
		{
			foreach (var face in faces)
			{
				var min = new Vector4(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);
				var max = new Vector4(float.MinValue, float.MinValue, float.MinValue, float.MinValue);
				foreach (var edge in face.edges)
				{
					min = Geometry.AxisAlignedMin(min, edgeValues[edge]);
					max = Geometry.AxisAlignedMax(max, edgeValues[edge]);
				}
				var center = centerValues[face];
				var extent = Geometry.AxisAlignedMax(max - center, center - min);
				minValues[face] = center - extent;
				ranges[face] = extent * 2f;
			}
		}

		/// <summary>
		/// Expands the minimum value and the range between the minimum and maximum values of an arbitrary edge attribute for the neighbors of each face, based on new edge attribute values, with the minimums and ranges adjusted to maintain the center values of the faces.
		/// </summary>
		/// <param name="faces">The faces whose minimum and range of neighboring edge attributes are to be expanded.</param>
		/// <param name="edgeValues">The new edge values to be examined.</param>
		/// <param name="centerValues">The new center values of the faces.</param>
		/// <param name="minValues">The existing minimum values to be updated.  These values will either stay unchanged or will get smaller; none of them will ever increase.</param>
		/// <param name="ranges">The existing ranges of values to be updated.  These values will either stay unchanged or will get larger; none of them will ever decrease.</param>
		/// <remarks><para>Once the minimum and maximum value for each face has been calculated, the maximum
		/// distance from either of them to the face's center value is determined, and the final minimum and
		/// range values are chosen such that the center value remains in the middle of that span.</para></remarks>
		public static void ExpandCenteredFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<float> edgeValues, IFaceAttribute<float> centerValues, IFaceAttribute<float> minValues, IFaceAttribute<float> ranges)
		{
			foreach (var face in faces)
			{
				var min = minValues[face];
				var max = min + ranges[face];
				foreach (var edge in face.edges)
				{
					min = Mathf.Min(min, edgeValues[edge]);
					max = Mathf.Max(max, edgeValues[edge]);
				}
				var center = centerValues[face];
				var extent = Mathf.Max(max - center, center - min);
				minValues[face] = center - extent;
				ranges[face] = extent * 2f;
			}
		}

		/// <summary>
		/// Expands the minimum value and the range between the minimum and maximum values of an arbitrary edge attribute for the neighbors of each face, based on new edge attribute values, with the minimums and ranges adjusted to maintain the center values of the faces.
		/// </summary>
		/// <param name="faces">The faces whose minimum and range of neighboring edge attributes are to be expanded.</param>
		/// <param name="edgeValues">The new edge values to be examined.</param>
		/// <param name="centerValues">The new center values of the faces.</param>
		/// <param name="minValues">The existing minimum values to be updated.  These values will either stay unchanged or will get smaller; none of them will ever increase.</param>
		/// <param name="ranges">The existing ranges of values to be updated.  These values will either stay unchanged or will get larger; none of them will ever decrease.</param>
		/// <remarks><para>Once the minimum and maximum value for each face has been calculated, the maximum
		/// distance from either of them to the face's center value is determined, and the final minimum and
		/// range values are chosen such that the center value remains in the middle of that span.</para></remarks>
		public static void ExpandCenteredFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<Vector2> edgeValues, IFaceAttribute<Vector2> centerValues, IFaceAttribute<Vector2> minValues, IFaceAttribute<Vector2> ranges)
		{
			foreach (var face in faces)
			{
				var min = minValues[face];
				var max = min + ranges[face];
				foreach (var edge in face.edges)
				{
					min = Geometry.AxisAlignedMin(min, edgeValues[edge]);
					max = Geometry.AxisAlignedMax(max, edgeValues[edge]);
				}
				var center = centerValues[face];
				var extent = Geometry.AxisAlignedMax(max - center, center - min);
				minValues[face] = center - extent;
				ranges[face] = extent * 2f;
			}
		}

		/// <summary>
		/// Expands the minimum value and the range between the minimum and maximum values of an arbitrary edge attribute for the neighbors of each face, based on new edge attribute values, with the minimums and ranges adjusted to maintain the center values of the faces.
		/// </summary>
		/// <param name="faces">The faces whose minimum and range of neighboring edge attributes are to be expanded.</param>
		/// <param name="edgeValues">The new edge values to be examined.</param>
		/// <param name="centerValues">The new center values of the faces.</param>
		/// <param name="minValues">The existing minimum values to be updated.  These values will either stay unchanged or will get smaller; none of them will ever increase.</param>
		/// <param name="ranges">The existing ranges of values to be updated.  These values will either stay unchanged or will get larger; none of them will ever decrease.</param>
		/// <remarks><para>Once the minimum and maximum value for each face has been calculated, the maximum
		/// distance from either of them to the face's center value is determined, and the final minimum and
		/// range values are chosen such that the center value remains in the middle of that span.</para></remarks>
		public static void ExpandCenteredFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<Vector3> edgeValues, IFaceAttribute<Vector3> centerValues, IFaceAttribute<Vector3> minValues, IFaceAttribute<Vector3> ranges)
		{
			foreach (var face in faces)
			{
				var min = minValues[face];
				var max = min + ranges[face];
				foreach (var edge in face.edges)
				{
					min = Geometry.AxisAlignedMin(min, edgeValues[edge]);
					max = Geometry.AxisAlignedMax(max, edgeValues[edge]);
				}
				var center = centerValues[face];
				var extent = Geometry.AxisAlignedMax(max - center, center - min);
				minValues[face] = center - extent;
				ranges[face] = extent * 2f;
			}
		}

		/// <summary>
		/// Expands the minimum value and the range between the minimum and maximum values of an arbitrary edge attribute for the neighbors of each face, based on new edge attribute values, with the minimums and ranges adjusted to maintain the center values of the faces.
		/// </summary>
		/// <param name="faces">The faces whose minimum and range of neighboring edge attributes are to be expanded.</param>
		/// <param name="edgeValues">The new edge values to be examined.</param>
		/// <param name="centerValues">The new center values of the faces.</param>
		/// <param name="minValues">The existing minimum values to be updated.  These values will either stay unchanged or will get smaller; none of them will ever increase.</param>
		/// <param name="ranges">The existing ranges of values to be updated.  These values will either stay unchanged or will get larger; none of them will ever decrease.</param>
		/// <remarks><para>Once the minimum and maximum value for each face has been calculated, the maximum
		/// distance from either of them to the face's center value is determined, and the final minimum and
		/// range values are chosen such that the center value remains in the middle of that span.</para></remarks>
		public static void ExpandCenteredFaceEdgeMinAndRangeValues(Topology.FacesIndexer faces, IEdgeAttribute<Vector4> edgeValues, IFaceAttribute<Vector4> centerValues, IFaceAttribute<Vector4> minValues, IFaceAttribute<Vector4> ranges)
		{
			foreach (var face in faces)
			{
				var min = minValues[face];
				var max = min + ranges[face];
				foreach (var edge in face.edges)
				{
					min = Geometry.AxisAlignedMin(min, edgeValues[edge]);
					max = Geometry.AxisAlignedMax(max, edgeValues[edge]);
				}
				var center = centerValues[face];
				var extent = Geometry.AxisAlignedMax(max - center, center - min);
				minValues[face] = center - extent;
				ranges[face] = extent * 2f;
			}
		}

		#endregion

		#region CalculateGlobalUVs...(...)

		#region CalculateGlobalPlanarUVs...(...)

		/// <summary>
		/// Calculates the UV coordinates of each face, based on the planar UV space indicated.
		/// </summary>
		/// <param name="faces">The collection of vertices whose UV coordinates are to be calculated.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <returns>The UV coordinates of the faces.</returns>
		public static IFaceAttribute<Vector2> CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(Topology.FacesIndexer faces, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(faces, facePositions, Vector3.zero, uAxis, vAxis, new Vector2[faces.Count].AsFaceAttribute());
		}

		/// <summary>
		/// Calculates the UV coordinates of each face, based on the planar UV space indicated.
		/// </summary>
		/// <param name="faces">The collection of vertices whose UV coordinates are to be calculated.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="origin">The world space origin of the UV space, where the UV coordinate will be (0, 0).</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <returns>The UV coordinates of the faces.</returns>
		public static IFaceAttribute<Vector2> CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(Topology.FacesIndexer faces, IFaceAttribute<Vector3> facePositions, Vector3 origin, Vector3 uAxis, Vector3 vAxis)
		{
			return CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(faces, facePositions, origin, uAxis, vAxis, new Vector2[faces.Count].AsFaceAttribute());
		}

		/// <summary>
		/// Calculates the UV coordinates of each face, based on the planar UV space indicated.
		/// </summary>
		/// <param name="faces">The collection of vertices whose UV coordinates are to be calculated.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The UV coordinates of the faces.</returns>
		public static IFaceAttribute<Vector2> CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(Topology.FacesIndexer faces, IFaceAttribute<Vector3> facePositions, Vector3 uAxis, Vector3 vAxis, IFaceAttribute<Vector2> uvs)
		{
			return CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(faces, facePositions, Vector3.zero, uAxis, vAxis, uvs);
		}

		/// <summary>
		/// Calculates the UV coordinates of each face, based on the planar UV space indicated.
		/// </summary>
		/// <param name="faces">The collection of vertices whose UV coordinates are to be calculated.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="origin">The world space origin of the UV space, where the UV coordinate will be (0, 0).</param>
		/// <param name="uAxis">The world space axis along which the first UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="vAxis">The world space axis along which the second UV component will increase linearly at rate determined by this vector's magnitude.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The UV coordinates of the faces.</returns>
		public static IFaceAttribute<Vector2> CalculateGlobalPlanarUnnormalizedUVsFromFacePositions(Topology.FacesIndexer faces, IFaceAttribute<Vector3> facePositions, Vector3 origin, Vector3 uAxis, Vector3 vAxis, IFaceAttribute<Vector2> uvs)
		{
			var normal = Vector3.Cross(vAxis, uAxis).normalized;
			var uPlane = new Plane(origin, uAxis + origin, normal + origin);
			var vPlane = new Plane(origin, normal + origin, vAxis + origin);

			var uAxisNeg = -uAxis;
			var vAxisNeg = -vAxis;

			foreach (var face in faces)
			{
				var facePosition = facePositions[face];

				uvs[face] = new Vector2(
					Geometry.GetIntersectionParameter(vPlane, new ScaledRay(facePosition, uAxisNeg)),
					Geometry.GetIntersectionParameter(uPlane, new ScaledRay(facePosition, vAxisNeg)));
			}

			return uvs;
		}

		#endregion

		#region CalculateGlobalSphericalUVs...(...)

		/// <summary>
		/// Calculates the UV coordinates of each face, based on the spherical longitude/latitude UV space indicated.
		/// </summary>
		/// <param name="faces">The collection of faces whose UV coordinates are to be calculated.</param>
		/// <param name="surface">The surface describing the overall shape of the spherical manifold.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="orientation">The world space orientation of the UV space.</param>
		/// <returns>The UV coordinates of the faces.</returns>
		public static IFaceAttribute<Vector2> CalculateGlobalSphericalUVsFromFacePositions(Topology.FacesIndexer faces, SphericalSurface surface, IFaceAttribute<Vector3> facePositions, Quaternion orientation)
		{
			return CalculateGlobalSphericalUVsFromFacePositions(faces, surface, facePositions, orientation, new Vector2[faces.Count].AsFaceAttribute());
		}

		/// <summary>
		/// Calculates the UV coordinates of each face, based on the spherical longitude/latitude UV space indicated.
		/// </summary>
		/// <param name="faces">The collection of faces whose UV coordinates are to be calculated.</param>
		/// <param name="surface">The surface describing the overall shape of the spherical manifold.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="orientation">The world space orientation of the UV space.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The UV coordinates of the faces.</returns>
		public static IFaceAttribute<Vector2> CalculateGlobalSphericalUVsFromFacePositions(Topology.FacesIndexer faces, SphericalSurface surface, IFaceAttribute<Vector3> facePositions, Quaternion orientation, IFaceAttribute<Vector2> uvs)
		{
			var twoPi = Mathf.PI * 2f;
			foreach (var face in faces)
			{
				var normal = (orientation * facePositions[face]).normalized;
				uvs[face] = new Vector2(
					Mathf.Atan2(normal.z, normal.x) / twoPi,
					Mathf.Acos(normal.y) / twoPi);
			}

			return uvs;
		}

		#endregion

		#endregion

		/// <summary>
		/// Calculates the local UV coordinates of each face, based on the positions and already computed UV frames of the faces.
		/// </summary>
		/// <param name="faces">The collection of faces whose UV coordinates are to be calculated.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="uvFrames">The UV frames for the faces, defining for each face independently the origin and axes determining UV values.</param>
		/// <returns>The UV coordinates of the faces.</returns>
		public static IFaceAttribute<Vector2> CalculateUnnormalizedUVsFromFacePositions(Topology.FacesIndexer faces, IFaceAttribute<Vector3> facePositions, IFaceAttribute<UVFrame3> uvFrames)
		{
			return CalculateUnnormalizedUVsFromFacePositions(faces, facePositions, uvFrames, new Vector2[faces.Count].AsFaceAttribute());
		}

		/// <summary>
		/// Calculates the local UV coordinates of each face, based on the positions and already computed UV frames of the faces.
		/// </summary>
		/// <param name="faces">The collection of faces whose UV coordinates are to be calculated.</param>
		/// <param name="facePositions">The positions of the faces.  Typically the face centroids, though any other method of mapping faces to individual points is also allowed.</param>
		/// <param name="uvFrames">The UV frames for the faces, defining for each face independently the origin and axes determining UV values.</param>
		/// <param name="uvs">A pre-allocated collection in which the UV coordinates will be stored.</param>
		/// <returns>The UV coordinates of the faces.</returns>
		public static IFaceAttribute<Vector2> CalculateUnnormalizedUVsFromFacePositions(Topology.FacesIndexer faces, IFaceAttribute<Vector3> facePositions, IFaceAttribute<UVFrame3> uvFrames, IFaceAttribute<Vector2> uvs)
		{
			foreach (var face in faces)
			{
				var uvFrame = uvFrames[face];
				var position = facePositions[face];

				uvs[face] = uvFrame.GetUV(position);
			}

			return uvs;
		}

		#region CalculatePerFace...UVsFromFaceUVMinAndRange(...)

		/// <summary>
		/// Calculates the local UV coordinates of each face, based on the minimum and range of unnormalized UV coordinates already calculated, such that the UVs of every face have the same scale.
		/// </summary>
		/// <param name="faces">The collection of faces whose UV coordinates are to be calculated.</param>
		/// <param name="faceMinUVs">The minium UV coordinates for each face.</param>
		/// <param name="faceRangeUVs">The range of UV coordinates for each face.</param>
		/// <param name="uvs">The unnormalized UV coordinates already calculated, and which will be updated with the new normalized values once this function is complete.</param>
		/// <returns>The UV coordinates of the faces.</returns>
		public static IFaceAttribute<Vector2> CalculateUniformlyNormalizedUVsFromFaceUVMinAndRange(Topology.FacesIndexer faces, IFaceAttribute<Vector2> faceMinUVs, IFaceAttribute<Vector2> faceRangeUVs, IFaceAttribute<Vector2> uvs)
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
				uvs[face] = Vector2.Scale(uvs[face] - faceOffsetUV, faceMaxUVRangeReciprocal);
			}

			return uvs;
		}

		#endregion

		/// <summary>
		/// Calculates orthonormal UV frames of the faces, based on the normals of the faces relative to a spherical longitiude/latitude frame of reference.
		/// </summary>
		/// <param name="faces">The collection of faces whose UV coordinates are to be calculated.</param>
		/// <param name="faceNormals">The surface normals of the faces.</param>
		/// <param name="orientation">The orientation of the spherical longitiude/latitude frame of reference.</param>
		/// <returns>The orthonormal UV frames of the faces.</returns>
		public static IFaceAttribute<UVFrame3> CalculatePerFaceSphericalUVFramesFromFaceNormals(Topology.FacesIndexer faces, IFaceAttribute<Vector3> faceNormals, Quaternion orientation)
		{
			return CalculatePerFaceSphericalUVFramesFromFaceNormals(faces, faceNormals, orientation, new UVFrame3[faces.Count].AsFaceAttribute());
		}

		/// <summary>
		/// Calculates orthonormal UV frames of the faces, based on the normals of the faces relative to a spherical longitiude/latitude frame of reference.
		/// </summary>
		/// <param name="faces">The collection of faces whose UV coordinates are to be calculated.</param>
		/// <param name="faceNormals">The surface normals of the faces.</param>
		/// <param name="orientation">The orientation of the spherical longitiude/latitude frame of reference.</param>
		/// <param name="uvFrames">A pre-allocated collection in which the UV frames will be stored.</param>
		/// <returns>The orthonormal UV frames of the faces.</returns>
		public static IFaceAttribute<UVFrame3> CalculatePerFaceSphericalUVFramesFromFaceNormals(Topology.FacesIndexer faces, IFaceAttribute<Vector3> faceNormals, Quaternion orientation, IFaceAttribute<UVFrame3> uvFrames)
		{
			var globalUp = orientation * Vector3.up;
			var globalRight = orientation * Vector3.right;
			foreach (var face in faces)
			{
				var faceNormal = faceNormals[face];
				var faceRight = Vector3.Cross(faceNormal, globalUp);
				var faceRightSqrMag = faceRight.sqrMagnitude;
				Vector3 faceUp;
				if (faceRightSqrMag > 0.01f)
				{
					faceRight /= Mathf.Sqrt(faceRightSqrMag);
					faceUp = Vector3.Cross(faceRight, faceNormal);
				}
				else
				{
					faceUp = Vector3.Cross(globalRight, faceNormal).normalized;
					faceRight = Vector3.Cross(faceNormal, faceUp);
				}

				uvFrames[face] = new UVFrame3(
					new Plane(Vector3.zero, faceRight, faceNormal),
					new Plane(Vector3.zero, faceNormal, faceUp),
					-faceRight,
					-faceUp);
			}

			return uvFrames;
		}
	}
}
