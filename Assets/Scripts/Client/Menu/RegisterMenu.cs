using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegisterMenu : MonoBehaviour {

	[SerializeField] private InputField	_loginField;
	[SerializeField] private InputField	_passwordField;
	[SerializeField] private InputField	_confirmPasswordField;

	public void ButtonRegister()
	{
		if (!GameManager.instance.IsConnected())
		{
			if (UICanvasPopup.instance)
			{
				UICanvasPopup.instance.AddPopup("Error", "Vous n'êtes pas connecté au serveur.");
			}
			return ;
		}

		if (_loginField.text.Length >= 4)
		{
			if (_passwordField.text.Length >= 6 && _passwordField.text.Length <= 32)
			{
				if (_passwordField.text == _confirmPasswordField.text)
				{
					ClientManager cm = GameManager.instance.GetComponent<ClientManager>();
					cm.Send(
						PacketHandler.newPacket((int)PacketID.Register,
							_loginField.text,
							_passwordField.text
						)
					);
				}
				else
				{
					if (UICanvasPopup.instance)
					{
						UICanvasPopup.instance.AddPopup("Warning", "La confirmation du mot de passe ne correspond pas");
					}
				}
			}
			else
			{
				if (UICanvasPopup.instance)
				{
					UICanvasPopup.instance.AddPopup("Warning", "Le mot de passe doit faire entre "+Constant.MIN_PASSWORD_LENGTH+" et "+Constant.MAX_PASSWORD_LENGTH+" caractères");
				}
			}
		}
		else
		{
			if (UICanvasPopup.instance)
			{
				UICanvasPopup.instance.AddPopup("Warning", "Le login doit faire entre "+Constant.MIN_LOGIN_LENGTH+" et "+Constant.MAX_LOGIN_LENGTH+" caractères");
			}
		}
	}
}
