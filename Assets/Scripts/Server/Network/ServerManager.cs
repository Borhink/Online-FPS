using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Data;
using UnityEngine;

public class ServerManager : SocketScript {
	public static int NetworkID = 0;

	public List<string> logServer = new List<string>();

	Dictionary<Socket, Client>	_clientsTable = new Dictionary<Socket, Client>();
	List<Socket>				_socketsTable = new List<Socket>();

	public static bool	readMutex = false;

	private MatchMaker	_matchMaker = null;

	override public void Run()
	{
		_matchMaker = new MatchMaker(this);
		try {
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, type);
			_socket.Bind(GetAddress());
			_socket.Listen(10);
			Thread connectThread = new Thread(new ThreadStart(ThreadConnect));
			connectThread.Start();
			Thread receiveThread = new Thread(new ThreadStart(ThreadReceive));
			receiveThread.Start();
			Thread isConnectedThread = new Thread(new ThreadStart(ThreadCheckClientIsConnected));
			isConnectedThread.Start();
			InitData();
		} catch(SocketException e) {
			Log("Erreur à la création du serveur : " + e.Message);
		}
	}

#region Threads
	void ThreadConnect()
	{
		Socket currentClient;
		Log("Le Serveur est à l'écoute du port: <b>" + port + "</b>");
		while(true)
		{
			currentClient = _socket.Accept();
			Log("Nouveau client: " + currentClient.GetHashCode());
			_socketsTable.Add(currentClient);
			_clientsTable.Add(currentClient, new Client(this, currentClient));	
		}
	}

	void ThreadReceive()
	{
		List<Socket> readList = new List<Socket>();
		byte[] byteBuffer;
		Dictionary<Socket, List<byte>> arrayByte = new Dictionary<Socket, List<byte>>();
		while(true)
		{
			readList.Clear();
			readList.AddRange(_socketsTable);
			if (readList.Count > 0)
			{
				Debug.Log("RECEIVED PACKET");
				Socket.Select(readList, null, null, 1000);
				foreach(Socket client in readList)
				{
					if (!arrayByte.ContainsKey(client))
					{
						arrayByte.Add(client, new List<byte>());
					}
					while(client.Available > 0)
					{
						readMutex = true;
						byteBuffer = new byte[client.Available];
						Log("Packet size received: " + client.Available);
						client.Receive(byteBuffer, 0, client.Available, SocketFlags.None);
						arrayByte[client].AddRange(byteBuffer);
					}
					while (arrayByte[client].Count >= 4)
					{
						int size = BitConverter.ToInt32(arrayByte[client].ToArray(), 0);
						if (arrayByte[client].Count >= size + 4)
						{
							arrayByte[client].RemoveRange(0, 4);
							byte[] packetArray = new byte[size];
							Buffer.BlockCopy(arrayByte[client].ToArray(), 0, packetArray, 0, size);
							arrayByte[client].RemoveRange(0, size);
							Packet readPacket = new Packet(packetArray);
							if (!PacketHandler.Parses(client, readPacket))
							{
								Log("Error Packet!");
							}
						}
						else
							break;
					}
					readMutex = false;
				}
			}
			Thread.Sleep(10);
		}
	}

	void ThreadCheckClientIsConnected()
	{
		while(true)
		{
			for(int i = 0; i < _socketsTable.Count; i++)
			{
				Socket socket = _socketsTable[i];
				if (socket.Poll(10, SelectMode.SelectRead) && socket.Available == 0 && !readMutex)
				{
					Client client = _clientsTable[socket];
					_socketsTable.Remove(socket);
					_clientsTable[socket].Disconnect();
					_clientsTable.Remove(socket);
					_dispatcher.Invoke(() => {
							ClientDisconnect(client);
							socket.Close();
						}
					);
					i--;
				}
			}
			Thread.Sleep(5);
		}
	}
#endregion

	private void Update()
	{
		// if (Input.GetKeyDown(KeyCode.A))
		// 	_matchMaker.CloseAllRooms();
		_matchMaker.CheckStartingRooms();
	}

	void OnDestroy()
	{
		_matchMaker.CloseAllRooms();
		Close();
	}

	public static int GetNetworkID()
	{
		return (++NetworkID);
	}

	void InitData()
	{
		// LoadEffectTypes();
	}

	void	ClientDisconnect(Client client)
	{
		Debug.Log("Client disconnect : " + client);
		if (client.room != null)
		{
			_dispatcher.Invoke(() => client.room.Leave(client));
		}
	}

	public void SendTo(Socket sender, Packet packet)
	{
		if (_clientsTable.ContainsKey(sender))
		{
			_clientsTable[sender].Send(packet);
		}
	}

	public void SendToAll(Packet packet)
	{
		foreach(Socket client in _socketsTable)
		{
			SendTo(client, packet);
		}
	}

	public void SendToOther(Socket sender, Packet packet)
	{
		foreach(Socket client in _socketsTable)
		{
			if (client != sender)
				SendTo(client, packet);
		}
	}

	override protected void Close()
	{
		foreach(KeyValuePair<Socket, Client> client in _clientsTable)
		{
			client.Value.Disconnect();
		}
		_clientsTable.Clear();
		foreach(Socket client in _socketsTable)
		{
			client.Close();
		}
		_socketsTable.Clear();
		if (_socket != null)
			_socket.Close();
	}

	public void checkClearMessage()
	{
		if (logServer.Count > 5000)
		{
			logServer.RemoveAt(0);
		}
	}

	override public void Log(object message)
	{
		logServer.Add(message.ToString());
		Debug.Log("[Server] " + message);
		checkClearMessage();
	}

	public bool ClientIsConnected(string login)
	{
		Debug.Log("Clients :");
		login = login.ToLower();
		foreach(Client client in _clientsTable.Values)
		{
			Debug.Log("Connected : "+client.Connected+", Login : "+client.Login );
			if (client.Connected && client.Login == login)
				return (true);
		}
		Debug.Log("Not connected");
		return (false);
	}

	public bool IsConnected(Socket socket)
	{
		if (_clientsTable.ContainsKey(socket))
			return _clientsTable[socket].Connected;
		return false;
	}

	public Socket GetSocketByLogin(string login)
	{
		foreach(KeyValuePair<Socket, Client> client in _clientsTable)
		{
			if (client.Value.Connected && client.Value.Login == login)
			{
				return client.Key;
			}
		}
		return null;
	}

	public Client GetClientByLogin(string login)
	{
		if (name != null)
		{
			foreach(Client sc in _clientsTable.Values)
			{
				if (sc.Login == login)
					return (sc);
			}
		}
		return (null);
	}
	
	/*
	 * ************************** *
	 * *** INITIALIZE PACKETS *** *
	 * ************************** *
	 */
	override protected void InitPacket()
	{
		PacketHandler.packetList.Clear();

		// Account Packet
		PacketHandler.packetList.Add((int)PacketID.Login, Packet_AccountConnect); // Serv <= Client (string, string)
		PacketHandler.packetList.Add((int)PacketID.Register, Packet_AccountRegister); // Serv <= Client (string, string)
		// PacketHandler.packetList.Add((int)PacketID.AccountData, Packet_Account); // Serv => Client (int, ...)

		// Menu - Scene
		PacketHandler.packetList.Add((int)PacketID.OpenMenu, Packet_OpenMenu); // Serv <=> Client (int, ...)
		// PacketHandler.packetList.Add((int)PacketID.LoadScene, Packet_LoadScene); // Serv => Client (string)
		PacketHandler.packetList.Add((int)PacketID.Play, Packet_Play); // Serv <= Client ()
		PacketHandler.packetList.Add((int)PacketID.LoadComplete, Packet_LoadComplete); // Serv <= Client ()

		// GameObject
		//PacketHandler.packetList.Add((int)PacketHandler.PacketID.Instantiate, Packet_Instantiate); // Serv => Client (str, int, int, bool, Vec3, Quat)
		//PacketHandler.packetList.Add((int)PacketHandler.PacketID.Destroy, Packet_Destroy); // Serv => Client (int)
		PacketHandler.packetList.Add((int)PacketID.UpdateTransform, Packet_UpdateTransform); // Serv <=> Client (int, Vec3, Quat)

		// Chat/Message/Popup
		PacketHandler.packetList.Add((int)PacketID.Chat, Packet_Chat); // Serv <=> Client (int, [int], string)
		//PacketHandler.packetList.Add((int)PacketHandler.PacketID.Popup, Packet_Popup); // Serv => Client (int, string)
		
	}

	/* *** Account Packet *** */
	void Packet_AccountConnect(Socket sender, Packet packet)
	{
		string login = packet.ReadString();
		string mdp = packet.ReadString();
		if (_clientsTable.ContainsKey(sender))
		{
			if (ClientIsConnected(login))
			{
				_clientsTable[sender].Log("<color=red>Essaye de se connecter au compte: <b>" + login + "</b> qui est déjà connecté !</color>");
				 SendTo(sender,
					 PacketHandler.newPacket(
						 PacketID.Popup,
						 2,
						 "Ce compte est déjà connecté au jeu !"
					 )
				);
			}
			else
				_clientsTable[sender].AccountConnect(login, mdp);
		}
	}

	void Packet_AccountRegister(Socket sender, Packet packet)
	{
		string login = packet.ReadString();
		string mdp = packet.ReadString();
		if (_clientsTable.ContainsKey(sender))
		{
			_clientsTable[sender].AccountRegister(login, mdp);
		}
	}

	void Packet_OpenMenu(Socket sender, Packet packet)
	{
		int menu = packet.ReadInt();
		switch(menu)
		{
			default:
				break;
		}
	}	

	void Packet_Play(Socket sender, Packet packet)
	{
		Client client = _clientsTable[sender];
		if (client != null)
			_matchMaker.FindMatch(client);
	}

	void Packet_LoadComplete(Socket sender, Packet packet)
	{
		Client client = _clientsTable[sender];
		if (client != null && client.room != null)
		{
			_dispatcher.Invoke(
				() => {	client.room.SpawnPlayer(client); }
			);
		}
	}

	void Packet_UpdateTransform(Socket sender, Packet packet)
	{
		int networkID = packet.ReadInt();
		Vector3 position = packet.ReadVector3();
		Quaternion rotation = packet.ReadQuaternion();

		Client client = _clientsTable[sender];
		if (client != null && client.room != null)
		{
			Debug.Log("Received packet transform from " + client.ID + ", pos: " + position);
			_dispatcher.Invoke(
				() => {	client.room.UpdateTransform(client, networkID, position, rotation); }
			);
		}
	}

	/* *** Chat/Message/Popup *** */
	void Packet_Chat(Socket sender, Packet packet)
	{
		if (!IsConnected(sender))
			return ;
		int type = packet.ReadInt();
		string msg;
		switch(type)
		{
			case 1: // All
				msg = packet.ReadString();
				if (msg.Length <= 0)
					return ;
				Log("<b>" + _clientsTable[sender].Login + ":</b> " + _clientsTable[sender].ParseMsg(msg));
				SendToAll(
					PacketHandler.newPacket(
						PacketID.Chat,
						1,
						"<b>" + _clientsTable[sender].Login + ":</b> " + _clientsTable[sender].ParseMsg(msg)
					)
				);
				break;
			case 2: // To
				string toName = packet.ReadString();
				if (toName.Length <= 0)
					return ;
				msg = packet.ReadString();
				if (msg.Length <= 0)
					return ;
				Socket toSocket = GetSocketByLogin(toName);
				if (toSocket != null)
				{
					Log("<b>" + _clientsTable[sender].Login + " chuchote a " + toName + ":</b> " + _clientsTable[sender].ParseMsg(msg));
					SendTo(toSocket,
						PacketHandler.newPacket(
							PacketID.Chat,
							2,
							_clientsTable[sender].Login,
							"<b>" + _clientsTable[sender].Login + " vous chuchote:</b> " + _clientsTable[sender].ParseMsg(msg)
						)
					);
				}
				else
				{
					Log("<color=orange><b>Le message n'a pas pu etre envoyer:</b> " + toName + " n'existe pas ou n'est pas connecte</color>");
					SendTo(sender,
						PacketHandler.newPacket(
							PacketID.Chat,
							1,
							"<color=orange><b>Le message n'a pas pu etre envoyer:</b> " + toName + " n'existe pas ou n'est pas connecte</color>"
						)
					);
				}
				break;
			default: // Senser jamais arriver
				msg = packet.ReadString();
				if (msg.Length <= 0)
					return ;
				Log("<color=red><b>Le message n'a pas pu etre envoyer:</b> " + _clientsTable[sender].ParseMsg(msg) + "</color>");
				SendTo(sender,
					PacketHandler.newPacket(
						PacketID.Chat,
						1,
						"<color=red><b>Le message n'a pas pu etre envoyer:</b> " + _clientsTable[sender].ParseMsg(msg) + "</color>"
					)
				);
				break;
		}
	}
	/* *** GameObject *** */
}
