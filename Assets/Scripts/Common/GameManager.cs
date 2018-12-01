using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
	public enum Side {None, Client, Server};
	public static GameManager instance  {get; private set;}
	public Side side {get; private set;}

	private ClientManager _clientManager = null; 
	private ServerManager _serverManager = null;

	void Awake()
	{
		Debug.developerConsoleVisible = true;
		if (instance == null)
		{
			instance = this;
			side = Side.Client;
			_clientManager = gameObject.AddComponent<ClientManager>();
			_clientManager.Run();
			DontDestroyOnLoad(gameObject);
		}
		else
			Destroy(this);
	}

	public bool IsConnected()
	{
		bool connected = false;
		if (side == Side.Client && _clientManager)
			connected = _clientManager.IsConnected();
		else if (side == Side.Server && _serverManager)
			connected = _serverManager.IsConnected();
		return connected;
	}

	public void ChangeSide()
	{
		if (side == Side.Client)
		{
			side = Side.Server;
			if (_clientManager != null)
				Destroy(_clientManager);
			_serverManager = gameObject.AddComponent<ServerManager>();
			_serverManager.Run();
		}
		else
		{
			side = Side.Client;
			if (_serverManager != null)
				Destroy(_serverManager);
			_clientManager = gameObject.AddComponent<ClientManager>();
			_clientManager.Run();
		}
	}

	// DATA In Game (User connecter au jeu)
	public Dictionary<int, Client> players = new Dictionary<int, Client>();

	// DATA Du JEU
	// public Dictionary<int, Item> itemTemplates = new Dictionary<int, Item>();
	// public Dictionary<int, EffectType> effectTypes = new Dictionary<int, EffectType>();

	void Start()
	{
		// for (int i = 1; i <= 2; i++)
		// {
		// 	Sprite[] list = Resources.LoadAll<Sprite>("Sprites/Body/walk/Body (" + i + ")");
		// 	if (list.Length > 0)
		// 		bodys.Add(list);
		// }
		// Debug.Log(bodys.Count + " bodys loaded!");
	}

	// public Item GetItemTemplate(int id)
    // {
    //     Item item;
    //     itemTemplates.TryGetValue(id, out item);
    //     return (item);
    // }

	void OnDestroy()
	{
		DatabaseManager.CloseDatabase();
	}

	public void LoadLevel(string name)
	{
		StartCoroutine(LoadLevelAsync(name));
	}

	IEnumerator LoadLevelAsync(string name)
	{
		AsyncOperation operation = SceneManager.LoadSceneAsync(name);

		while (!operation.isDone)
		{
			float progress = Mathf.Clamp01(operation.progress / 0.9f);

			yield return null;
		}
		Debug.Log("SEND LOAD COMPLETE PACKET");
		ClientManager cm = GameManager.instance.GetComponent<ClientManager>();
		cm.Send(
			PacketHandler.newPacket((int)PacketID.LoadComplete
			)
		);
	}
}
