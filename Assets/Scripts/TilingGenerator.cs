using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Tiling;

[ExecuteInEditMode]
public abstract class TilingGenerator : MonoBehaviour
{
	private Topology _topology;
	private TileAttribute<Vector3> _tilePositions;
	private CornerAttribute<Vector3> _cornerPositions;

	private bool _invalidated = true;

	public Topology Topology { get { return _topology; } }
	public TileAttribute<Vector3> TilePositions { get { return _tilePositions; } }
	public CornerAttribute<Vector3> CornerPositions { get { return _cornerPositions; } }

	public void Invalidate()
	{
		_invalidated = true;
	}

	void Awake()
	{
		Invalidate();
	}

	void OnValidate()
	{
		Invalidate();
	}

	void Update()
	{
		if (_invalidated)
		{
			RebuildTiling(out _topology, out _tilePositions, out _cornerPositions);
			_invalidated = false;
		}
	}

	protected abstract void RebuildTiling(out Topology topology, out TileAttribute<Vector3> tilePositions, out CornerAttribute<Vector3> cornerPositions);
}
