using UnityEngine;
using System.Collections.Generic;
using Tiling;

public class SphereTopology
{
	public static void Dodecahedron(out MinimalTopology topology, out TileAttribute<Vector3> tilePositions)
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

		tilePositions = new TileAttribute<Vector3>(12);;
		tilePositions[ 0] = new Vector3(+x2,  y0, 0.0f);
		tilePositions[ 1] = new Vector3(+x2, +y1, +z0);
		tilePositions[ 2] = new Vector3(+x2, -y1, -z0);
		tilePositions[ 3] = new Vector3(+x2,  y2, 0.0f);
		tilePositions[ 4] = new Vector3(-x1, +y1, -z1);
		tilePositions[ 5] = new Vector3(+x1, +y1, -z1);
		tilePositions[ 6] = new Vector3(-x1, -y1, +z1);
		tilePositions[ 7] = new Vector3(+x1, -y1, +z1);
		tilePositions[ 8] = new Vector3(-x0, +y1, +z2);
		tilePositions[ 9] = new Vector3(-x0, -y1, -z2);
		tilePositions[10] = new Vector3(+x0, +y1, +z2);
		tilePositions[11] = new Vector3(+x0, -y1, -z2);

		topology = new MinimalTopology();

		topology._edgeTiles = new int[30,2];
		topology._edgeTiles[ 0, 0] =  0; topology._edgeTiles[ 0, 1] =  1;
		topology._edgeTiles[ 1, 0] =  0; topology._edgeTiles[ 1, 1] =  8;
		topology._edgeTiles[ 2, 0] =  0; topology._edgeTiles[ 2, 1] =  4;
		topology._edgeTiles[ 3, 0] =  0; topology._edgeTiles[ 3, 1] =  5;
		topology._edgeTiles[ 4, 0] =  0; topology._edgeTiles[ 4, 1] = 10;
		topology._edgeTiles[ 5, 0] =  1; topology._edgeTiles[ 5, 1] = 10;
		topology._edgeTiles[ 6, 0] =  1; topology._edgeTiles[ 6, 1] =  7;
		topology._edgeTiles[ 7, 0] =  1; topology._edgeTiles[ 7, 1] =  6;
		topology._edgeTiles[ 8, 0] =  1; topology._edgeTiles[ 8, 1] =  8;
		topology._edgeTiles[ 9, 0] =  2; topology._edgeTiles[ 9, 1] =  3;
		topology._edgeTiles[10, 0] =  2; topology._edgeTiles[10, 1] = 11;
		topology._edgeTiles[11, 0] =  2; topology._edgeTiles[11, 1] =  5;
		topology._edgeTiles[12, 0] =  2; topology._edgeTiles[12, 1] =  4;
		topology._edgeTiles[13, 0] =  2; topology._edgeTiles[13, 1] =  9;
		topology._edgeTiles[14, 0] =  3; topology._edgeTiles[14, 1] =  9;
		topology._edgeTiles[15, 0] =  3; topology._edgeTiles[15, 1] =  6;
		topology._edgeTiles[16, 0] =  3; topology._edgeTiles[16, 1] =  7;
		topology._edgeTiles[17, 0] =  3; topology._edgeTiles[17, 1] = 11;
		topology._edgeTiles[18, 0] =  4; topology._edgeTiles[18, 1] =  8;
		topology._edgeTiles[19, 0] =  4; topology._edgeTiles[19, 1] =  9;
		topology._edgeTiles[20, 0] =  4; topology._edgeTiles[20, 1] =  5;
		topology._edgeTiles[21, 0] =  5; topology._edgeTiles[21, 1] = 11;
		topology._edgeTiles[22, 0] =  5; topology._edgeTiles[22, 1] = 10;
		topology._edgeTiles[23, 0] =  6; topology._edgeTiles[23, 1] =  7;
		topology._edgeTiles[24, 0] =  6; topology._edgeTiles[24, 1] =  9;
		topology._edgeTiles[25, 0] =  6; topology._edgeTiles[25, 1] =  8;
		topology._edgeTiles[26, 0] =  7; topology._edgeTiles[26, 1] = 10;
		topology._edgeTiles[27, 0] =  7; topology._edgeTiles[27, 1] = 11;
		topology._edgeTiles[28, 0] =  8; topology._edgeTiles[28, 1] =  9;
		topology._edgeTiles[29, 0] = 10; topology._edgeTiles[29, 1] = 11;

		topology._cornerTiles = new int[20, 3];
		topology._cornerTiles[ 0, 0] =  0; topology._cornerTiles[ 0, 1] =  1; topology._cornerTiles[ 0, 2] =  8;
		topology._cornerTiles[ 1, 0] =  0; topology._cornerTiles[ 1, 1] =  8; topology._cornerTiles[ 1, 2] =  4;
		topology._cornerTiles[ 2, 0] =  0; topology._cornerTiles[ 2, 1] =  4; topology._cornerTiles[ 2, 2] =  5;
		topology._cornerTiles[ 3, 0] =  0; topology._cornerTiles[ 3, 1] =  5; topology._cornerTiles[ 3, 2] = 10;
		topology._cornerTiles[ 4, 0] =  0; topology._cornerTiles[ 4, 1] = 10; topology._cornerTiles[ 4, 2] =  1;
		topology._cornerTiles[ 5, 0] =  1; topology._cornerTiles[ 5, 1] = 10; topology._cornerTiles[ 5, 2] =  7;
		topology._cornerTiles[ 6, 0] =  1; topology._cornerTiles[ 6, 1] =  7; topology._cornerTiles[ 6, 2] =  6;
		topology._cornerTiles[ 7, 0] =  1; topology._cornerTiles[ 7, 1] =  6; topology._cornerTiles[ 7, 2] =  8;
		topology._cornerTiles[ 8, 0] =  2; topology._cornerTiles[ 8, 1] =  3; topology._cornerTiles[ 8, 2] = 11;
		topology._cornerTiles[ 9, 0] =  2; topology._cornerTiles[ 9, 1] = 11; topology._cornerTiles[ 9, 2] =  5;
		topology._cornerTiles[10, 0] =  2; topology._cornerTiles[10, 1] =  5; topology._cornerTiles[10, 2] =  4;
		topology._cornerTiles[11, 0] =  2; topology._cornerTiles[11, 1] =  4; topology._cornerTiles[11, 2] =  9;
		topology._cornerTiles[12, 0] =  2; topology._cornerTiles[12, 1] =  9; topology._cornerTiles[12, 2] =  3;
		topology._cornerTiles[13, 0] =  3; topology._cornerTiles[13, 1] =  9; topology._cornerTiles[13, 2] =  6;
		topology._cornerTiles[14, 0] =  3; topology._cornerTiles[14, 1] =  6; topology._cornerTiles[14, 2] =  7;
		topology._cornerTiles[15, 0] =  3; topology._cornerTiles[15, 1] =  7; topology._cornerTiles[15, 2] = 11;
		topology._cornerTiles[16, 0] =  4; topology._cornerTiles[16, 1] =  8; topology._cornerTiles[16, 2] =  9;
		topology._cornerTiles[17, 0] =  5; topology._cornerTiles[17, 1] = 11; topology._cornerTiles[17, 2] = 10;
		topology._cornerTiles[18, 0] =  6; topology._cornerTiles[18, 1] =  9; topology._cornerTiles[18, 2] =  8;
		topology._cornerTiles[19, 0] =  7; topology._cornerTiles[19, 1] = 10; topology._cornerTiles[19, 2] = 11;
	}
}
