using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginMenu : MonoBehaviour {

	[SerializeField] private InputField	_loginField;
	[SerializeField] private InputField	_passwordField;

	public void ButtonLogin()
	{
		
		if (!GameManager.instance.IsConnected())
		{
			if (UICanvasPopup.instance)
			{
				UICanvasPopup.instance.AddPopup("Error", "Vous n'êtes pas connecté au serveur.");
			}
			return ;
		}
		if (_loginField.text.Length >= Constant.MIN_LOGIN_LENGTH && _loginField.text.Length <= Constant.MAX_LOGIN_LENGTH)
		{
			if (_passwordField.text.Length >= Constant.MIN_PASSWORD_LENGTH && _passwordField.text.Length <= Constant.MAX_PASSWORD_LENGTH)
			{
				
				Debug.Log("SEND CONNECT PACKET");
				ClientManager cm = GameManager.instance.GetComponent<ClientManager>();
				cm.Send(
					PacketHandler.newPacket((int)PacketID.Login,
						_loginField.text,
						_passwordField.text
					)
				);
			}
			else
			{
				if (UICanvasPopup.instance)
				{
					UICanvasPopup.instance.AddPopup("Warning", "Mot de passe invalide");
				}
			}
		}
		else
		{
			if (UICanvasPopup.instance)
			{
				UICanvasPopup.instance.AddPopup("Warning", "Login invalide");
			}
		}
	}


}
