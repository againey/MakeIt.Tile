using UnityEngine;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Experilous.Topological;

public class CylindricalManifoldGenerationTests
{
	[Test]
	public void MinimalPointyTopTriGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifold.CreatePointyTopTriGridCylinder(2, 1, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold.topology);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold.topology);
	}

	[Test]
	public void MinimalFlatTopTriGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifold.CreateFlatTopTriGridCylinder(3, 1, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold.topology);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold.topology);
	}

	[Test]
	public void MinimalQuadGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifold.CreateQuadGridCylinder(3, 1, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold.topology);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold.topology);
	}

	[Test]
	public void MinimalPointyTopHexGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifold.CreatePointyTopHexGridCylinder(3, 1, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold.topology);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold.topology);
	}

	[Test]
	public void MinimalFlatTopHexGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifold.CreateFlatTopHexGridCylinder(4, 2, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold.topology);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold.topology);
	}

	[Test]
	public void SmallPointyTopTriGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifold.CreatePointyTopTriGridCylinder(3, 2, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold.topology);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold.topology);
	}

	[Test]
	public void SmallFlatTopTriGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifold.CreateFlatTopTriGridCylinder(4, 2, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold.topology);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold.topology);
	}

	[Test]
	public void SmallQuadGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifold.CreateQuadGridCylinder(4, 2, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold.topology);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold.topology);
	}

	[Test]
	public void SmallPointyTopHexGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifold.CreatePointyTopHexGridCylinder(3, 2, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold.topology);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold.topology);
	}

	[Test]
	public void SmallFlatTopHexGridCylinderEdgeCycleTest()
	{
		var manifold = CylindricalManifold.CreateFlatTopHexGridCylinder(4, 4, 60, 0.5f);
		TopologyTests.CheckVerticesForInvalidEdgeCycles(manifold.topology);
		TopologyTests.CheckFacesForInvalidEdgeCycles(manifold.topology);
	}
}
