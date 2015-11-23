using UnityEngine;
using System.ComponentModel;

namespace Experilous
{
	public class PropertyChangedNotifier : MonoBehaviour, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void NotifyPropertyChanged(string propertyName)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
