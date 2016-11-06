﻿/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using ScaledRay = Experilous.Numerics.ScaledRay;

namespace Experilous.MakeItTile
{
	public interface IFaceSpatialPartitioning
	{
		Topology.Face FindFace(Vector3 point);
		Topology.Face FindFace(Ray ray);
		Topology.Face FindFace(ScaledRay ray);
	}
}