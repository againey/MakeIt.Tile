/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;

namespace Experilous.MakeItCurve
{
	public enum ContourState
	{
		Normal,
		Breaking,
		Broken,
		Resuming,
	}

	public interface IContour
	{
		Vector2 position { get; }

		float targetDistance { get; }
		float actualDistance { get; }

		float GetScaledDistance(IContour other);

		ContourState state { get; }

		int index { get; }
		bool hasIndex { get; }
		int SetIndex(int index);
	}

	public class Contour : IContour
	{
		public IContour Track(float distance)
		{
			return new DependentContour(this, distance);
		}

		public bool MoveNext()
		{
		}

		public Vector2 position
		{
			get
			{
			}
		}

		public float targetDistance
		{
			get
			{
			}
		}

		public float actualDistance
		{
			get
			{
			}
		}

		public float GetScaledDistance(IContour other)
		{
		}

		public ContourState state
		{
			get
			{
			}
		}

		public bool hasIndex
		{
			get
			{
			}
		}

		public int index
		{
			get
			{
			}
		}

		public int SetIndex(int index)
		{
		}

		private class DependentContour : IContour
		{
			private Contour _baseContour;
			private float _distance;

			public DependentContour(Contour baseContour, float distance)
			{
				_baseContour = baseContour;
				_distance = distance;
			}

			public Vector2 position
			{
				get
				{
				}
			}

			public float targetDistance
			{
				get
				{
				}
			}

			public float actualDistance
			{
				get
				{
				}
			}

			public float GetScaledDistance(IContour other)
			{
			}

			public ContourState state
			{
				get
				{
				}
			}

			public bool hasIndex
			{
				get
				{
				}
			}

			public int index
			{
				get
				{
				}
			}

			public int SetIndex(int index)
			{
			}
		}
	}
}
