using UnityEngine;
using System.Collections.Generic;

namespace Tiling
{
	using VertexPositions = Topology.VertexAttribute<Vector3>;

	public static class SphereTopology
	{
		public static Topology CreateIcosahedron(out VertexPositions vertexPositions)
		{
			var builder = new Topology.Builder(12, 30, 20);

			var latitude = Mathf.Atan2(1, 2);
			var longitude = Mathf.PI * 0.2f;
			var cosLat = Mathf.Cos(latitude);

			var x0 = cosLat * Mathf.Sin(longitude * 2.0f);
			var x1 = cosLat * Mathf.Sin(longitude);
			var x2 = 0.0f;
			var y0 = +1.0f;
			var y1 = Mathf.Sin(latitude);
			var y2 = -1.0f;
			var z0 = cosLat;
			var z1 = cosLat * Mathf.Cos(longitude);
			var z2 = cosLat * Mathf.Cos(longitude * 2.0f);

			vertexPositions = new VertexPositions(12);;
			vertexPositions[ 0] = new Vector3(+x2,  y0, 0.0f);
			vertexPositions[ 1] = new Vector3(+x2, +y1, +z0);
			vertexPositions[ 2] = new Vector3(+x2, -y1, -z0);
			vertexPositions[ 3] = new Vector3(+x2,  y2, 0.0f);
			vertexPositions[ 4] = new Vector3(-x1, +y1, -z1);
			vertexPositions[ 5] = new Vector3(+x1, +y1, -z1);
			vertexPositions[ 6] = new Vector3(-x1, -y1, +z1);
			vertexPositions[ 7] = new Vector3(+x1, -y1, +z1);
			vertexPositions[ 8] = new Vector3(-x0, +y1, +z2);
			vertexPositions[ 9] = new Vector3(-x0, -y1, -z2);
			vertexPositions[10] = new Vector3(+x0, +y1, +z2);
			vertexPositions[11] = new Vector3(+x0, -y1, -z2);

			var topology = new Topology();

			builder.AddVertex( 1,  8,  4,  5, 10);
			builder.AddVertex( 0, 10,  7,  6,  8);
			builder.AddVertex( 3, 11,  5,  4,  9);
			builder.AddVertex( 2,  9,  6,  7, 11);
			builder.AddVertex( 0,  8,  9,  2,  5);
			builder.AddVertex( 0,  4,  2, 11, 10);
			builder.AddVertex( 1,  7,  3,  9,  8);
			builder.AddVertex( 1, 10, 11,  3,  6);
			builder.AddVertex( 0,  1,  6,  9,  4);
			builder.AddVertex( 2,  4,  8,  6,  3);
			builder.AddVertex( 0,  5, 11,  7,  1);
			builder.AddVertex( 2,  3,  7, 10,  5);

			return builder.BuildTopology();
		}
	}
}
