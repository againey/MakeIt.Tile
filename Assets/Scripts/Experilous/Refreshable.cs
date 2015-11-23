namespace Experilous
{
	public static class Refreshable
	{
		public static IRefreshable Chain(IRefreshable upstream, IRefreshable downstream)
		{
			upstream.Refreshed += downstream.PropagateRefresh;

			return upstream;
		}

		public static T Rechain<T>(T oldUpstream, T newUpstream, IRefreshable downstream)
		{
			if (ReferenceEquals(oldUpstream, newUpstream)) return newUpstream;

			if (oldUpstream != null)
			{
				((IRefreshable)oldUpstream).Refreshed -= downstream.PropagateRefresh;
			}

			if (newUpstream != null)
			{
				((IRefreshable)newUpstream).Refreshed += downstream.PropagateRefresh;
			}

			oldUpstream = newUpstream;

			downstream.Refresh();

#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty((UnityEngine.Object)downstream);
#endif

			return newUpstream;
		}

		public static bool Rechain<T>(ref T oldUpstream, T newUpstream, IRefreshable downstream) where T : IRefreshable
		{
			if (ReferenceEquals(oldUpstream, newUpstream)) return false;

			if (oldUpstream != null)
			{
				oldUpstream.Refreshed -= downstream.PropagateRefresh;
			}

			if (newUpstream != null)
			{
				newUpstream.Refreshed += downstream.PropagateRefresh;
			}

			oldUpstream = newUpstream;

			downstream.Refresh();

#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty((UnityEngine.Object)downstream);
#endif

			return true;
		}
	}
}
