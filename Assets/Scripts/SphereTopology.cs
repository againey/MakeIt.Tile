using UnityEngine;
using System.Collections.Generic;

public class SphereTopology
{
	public static void Icosahedron(out BasicMeshTopology topology, out Vector3[] vertexPositions)
	{
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

		vertexPositions = new Vector3[12];
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

		topology = new BasicMeshTopology();

		topology._edgeVertices = new int[30,2];
		topology._edgeVertices[ 0, 0] =  0; topology._edgeVertices[ 0, 1] =  1;
		topology._edgeVertices[ 1, 0] =  0; topology._edgeVertices[ 1, 1] =  8;
		topology._edgeVertices[ 2, 0] =  0; topology._edgeVertices[ 2, 1] =  4;
		topology._edgeVertices[ 3, 0] =  0; topology._edgeVertices[ 3, 1] =  5;
		topology._edgeVertices[ 4, 0] =  0; topology._edgeVertices[ 4, 1] = 10;
		topology._edgeVertices[ 5, 0] =  1; topology._edgeVertices[ 5, 1] = 10;
		topology._edgeVertices[ 6, 0] =  1; topology._edgeVertices[ 6, 1] =  7;
		topology._edgeVertices[ 7, 0] =  1; topology._edgeVertices[ 7, 1] =  6;
		topology._edgeVertices[ 8, 0] =  1; topology._edgeVertices[ 8, 1] =  8;
		topology._edgeVertices[ 9, 0] =  2; topology._edgeVertices[ 9, 1] =  3;
		topology._edgeVertices[10, 0] =  2; topology._edgeVertices[10, 1] = 11;
		topology._edgeVertices[11, 0] =  2; topology._edgeVertices[11, 1] =  5;
		topology._edgeVertices[12, 0] =  2; topology._edgeVertices[12, 1] =  4;
		topology._edgeVertices[13, 0] =  2; topology._edgeVertices[13, 1] =  9;
		topology._edgeVertices[14, 0] =  3; topology._edgeVertices[14, 1] =  9;
		topology._edgeVertices[15, 0] =  3; topology._edgeVertices[15, 1] =  6;
		topology._edgeVertices[16, 0] =  3; topology._edgeVertices[16, 1] =  7;
		topology._edgeVertices[17, 0] =  3; topology._edgeVertices[17, 1] = 11;
		topology._edgeVertices[18, 0] =  4; topology._edgeVertices[18, 1] =  8;
		topology._edgeVertices[19, 0] =  4; topology._edgeVertices[19, 1] =  9;
		topology._edgeVertices[20, 0] =  4; topology._edgeVertices[20, 1] =  5;
		topology._edgeVertices[21, 0] =  5; topology._edgeVertices[21, 1] = 11;
		topology._edgeVertices[22, 0] =  5; topology._edgeVertices[22, 1] = 10;
		topology._edgeVertices[23, 0] =  6; topology._edgeVertices[23, 1] =  7;
		topology._edgeVertices[24, 0] =  6; topology._edgeVertices[24, 1] =  9;
		topology._edgeVertices[25, 0] =  6; topology._edgeVertices[25, 1] =  8;
		topology._edgeVertices[26, 0] =  7; topology._edgeVertices[26, 1] = 10;
		topology._edgeVertices[27, 0] =  7; topology._edgeVertices[27, 1] = 11;
		topology._edgeVertices[28, 0] =  8; topology._edgeVertices[28, 1] =  9;
		topology._edgeVertices[29, 0] = 10; topology._edgeVertices[29, 1] = 11;

		topology._triangleVertices = new int[20, 3];
		topology._triangleVertices[ 0, 0] =  0; topology._triangleVertices[ 0, 1] =  1; topology._triangleVertices[ 0, 2] =  8;
		topology._triangleVertices[ 1, 0] =  0; topology._triangleVertices[ 1, 1] =  8; topology._triangleVertices[ 1, 2] =  4;
		topology._triangleVertices[ 2, 0] =  0; topology._triangleVertices[ 2, 1] =  4; topology._triangleVertices[ 2, 2] =  5;
		topology._triangleVertices[ 3, 0] =  0; topology._triangleVertices[ 3, 1] =  5; topology._triangleVertices[ 3, 2] = 10;
		topology._triangleVertices[ 4, 0] =  0; topology._triangleVertices[ 4, 1] = 10; topology._triangleVertices[ 4, 2] =  1;
		topology._triangleVertices[ 5, 0] =  1; topology._triangleVertices[ 5, 1] = 10; topology._triangleVertices[ 5, 2] =  7;
		topology._triangleVertices[ 6, 0] =  1; topology._triangleVertices[ 6, 1] =  7; topology._triangleVertices[ 6, 2] =  6;
		topology._triangleVertices[ 7, 0] =  1; topology._triangleVertices[ 7, 1] =  6; topology._triangleVertices[ 7, 2] =  8;
		topology._triangleVertices[ 8, 0] =  2; topology._triangleVertices[ 8, 1] =  3; topology._triangleVertices[ 8, 2] = 11;
		topology._triangleVertices[ 9, 0] =  2; topology._triangleVertices[ 9, 1] = 11; topology._triangleVertices[ 9, 2] =  5;
		topology._triangleVertices[10, 0] =  2; topology._triangleVertices[10, 1] =  5; topology._triangleVertices[10, 2] =  4;
		topology._triangleVertices[11, 0] =  2; topology._triangleVertices[11, 1] =  4; topology._triangleVertices[11, 2] =  9;
		topology._triangleVertices[12, 0] =  2; topology._triangleVertices[12, 1] =  9; topology._triangleVertices[12, 2] =  3;
		topology._triangleVertices[13, 0] =  3; topology._triangleVertices[13, 1] =  9; topology._triangleVertices[13, 2] =  6;
		topology._triangleVertices[14, 0] =  3; topology._triangleVertices[14, 1] =  6; topology._triangleVertices[14, 2] =  7;
		topology._triangleVertices[15, 0] =  3; topology._triangleVertices[15, 1] =  7; topology._triangleVertices[15, 2] = 11;
		topology._triangleVertices[16, 0] =  4; topology._triangleVertices[16, 1] =  8; topology._triangleVertices[16, 2] =  9;
		topology._triangleVertices[17, 0] =  5; topology._triangleVertices[17, 1] = 11; topology._triangleVertices[17, 2] = 10;
		topology._triangleVertices[18, 0] =  6; topology._triangleVertices[18, 1] =  9; topology._triangleVertices[18, 2] =  8;
		topology._triangleVertices[19, 0] =  7; topology._triangleVertices[19, 1] = 10; topology._triangleVertices[19, 2] = 11;
	}
}
