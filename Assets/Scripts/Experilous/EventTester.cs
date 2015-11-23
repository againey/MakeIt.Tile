using UnityEngine;
using UnityEditor;
using System;

namespace Experilous
{
	[ExecuteInEditMode]
	public class EventTester : MonoBehaviour, ISerializationCallbackReceiver
	{
		public GameObject Child;

		private void Awake()
		{
			Debug.Log("Awake()");
		}

		private void Start()
		{
			Debug.Log("Start()");
		}

		private void OnEnable()
		{
			Debug.Log("OnEnable()");
		}

		private void OnDisable()
		{
			Debug.Log("OnDisable()");
		}

		private void Update()
		{
			Debug.Log("Update()");
		}

		private void LateUpdate()
		{
			Debug.Log("LateUpdate()");
		}

		public void OnAfterDeserialize()
		{
			Debug.Log("OnAfterDeserialize()");
		}

		public void OnBeforeSerialize()
		{
			//Debug.Log("OnBeforeSerialize()");
		}
	}
}
