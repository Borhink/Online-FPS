using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MenuID
{
	Background,
	Login,
	Register,
	Home
}

public class PanelManager : MonoBehaviour {

	public static PanelManager instance  {get; private set;}

	[SerializeField] private GameObject _panelClient;
	[SerializeField] private GameObject _panelServer;
	[SerializeField] private GameObject _clientSideButton;
	[SerializeField] private GameObject _serverSideButton;

	[SerializeField] private GameObject[] _panels;

	void Awake()
	{
		Debug.developerConsoleVisible = true;
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
			Destroy(this);
	}

	private void Start()
	{
		DesactiveAllPanels();
		ActivateLoginPanel();
		_clientSideButton.SetActive(true);
		_serverSideButton.SetActive(true);
		_panelClient.SetActive(true);
		_panelServer.SetActive(false);
	}

	public void DesactiveAllPanels()
	{
		foreach (GameObject panel in _panels)
		{
			panel.SetActive(false);
		}
	}

	public void ActivePanel(MenuID id, bool activate = true)
	{
		_panels[(int)id].SetActive(activate);
	}

	public void OpenMenu(int id)
	{
		switch(id)
		{
			case (int)MenuID.Login:
				ActivateLoginPanel();
				break;
			case (int)MenuID.Register:
				ActivateRegisterPanel();
			break;
			case (int)MenuID.Home:
				ActivateHomePanel();
			break;
			default:
				break;
		}
	}

	public void ActivateLoginPanel()
	{
		DesactiveAllPanels();
		ActivePanel(MenuID.Background);
		ActivePanel(MenuID.Login);
		_serverSideButton.SetActive(true);
		MenuNavigation.instance.Refresh();
	}

	public void ActivateRegisterPanel()
	{
		DesactiveAllPanels();
		ActivePanel(MenuID.Background);
		ActivePanel(MenuID.Register);
		MenuNavigation.instance.Refresh();
	}

	public void ActivateHomePanel()
	{
		DesactiveAllPanels();
		ActivePanel(MenuID.Background);
		ActivePanel(MenuID.Home);
		_serverSideButton.SetActive(false);
		MenuNavigation.instance.Refresh();
	}

	public void ChangeGameSide()
	{
		GameManager.instance.ChangeSide();
		_panelClient.SetActive(!_panelClient.activeSelf);
		_panelServer.SetActive(!_panelServer.activeSelf);
		MenuNavigation.instance.Refresh();
	}
}
