using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICanvasPopup : MonoBehaviour {
	public static UICanvasPopup instance {get; private set;}

	void Awake()
	{
		instance = this;
	}

	public void AddPopup(string title, string message)
	{
		GameObject go = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Popup"), Vector3.zero, Quaternion.identity, transform);
		go.transform.localPosition = Vector3.zero;
		UIPopup pop = go.GetComponent<UIPopup>();
		pop.title.text = title.ToUpper();
		pop.message.text = message;
	}
}
