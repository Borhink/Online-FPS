using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeMenu : MonoBehaviour {

	[SerializeField] private Text	_loginText;

	void OnEnable()
    {
        Debug.Log("HomeMenu: script was enabled");
		_loginText.text = ClientManager.instance.account.login;
    }

	public void ButtonPlay()
	{
		if (!GameManager.instance.IsConnected())
		{
			if (UICanvasPopup.instance)
			{
				UICanvasPopup.instance.AddPopup("Error", "Vous n'êtes pas connecté au serveur.");
			}
			return ;
		}
		ClientManager cm = GameManager.instance.GetComponent<ClientManager>();
		cm.Send(
			PacketHandler.newPacket(PacketID.Play)
		);
	}
}
