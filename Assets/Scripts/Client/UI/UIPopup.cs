using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPopup : MonoBehaviour {

	public Text title;
	public Text message;

	public void ButtonCancel()
	{
		Destroy(gameObject);
	}
}
