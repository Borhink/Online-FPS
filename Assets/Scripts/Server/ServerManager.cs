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

	Dictionary<Socket, Client> _clientsTable = new Dictionary<Socket, Client>();
	List<Socket> _clientsSocket = new List<Socket>();
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
			Log("Erreur creation de serveur socket: " + e.Message);
		}
	}

	private void Update()
	{
		_matchMaker.CheckStartingRooms();
	}

	// public void LoadEffectTypes()
    // {
    //     IDataReader buffer = DatabaseManager.Select("*", "EffectType");
    //     while (buffer.Read())
    //     {
    //         int id = buffer.GetInt32(0);
    //         string name = buffer.GetString(1);
    //         string color = buffer.GetString(2);
    //         int category = buffer.GetInt32(3);
    //         GameManager.instance.effectTypes.Add(id, new EffectType(id, name, color, category));
    //     }
    //     Log(GameManager.instance.effectTypes.Count + " EffectType chargés.");
    //     buffer.Close();
    //     buffer = null;
    // }

	public static int GetNetworkID()
	{
		return (++NetworkID);

	}

	void InitData()
	{
		// DatabaseManager.OpenDatabase();
		// LoadEffectTypes();
	}

	void ThreadConnect()
	{
		Socket currentClient;
		Log("Le Serveur est a l'ecoute du port: <b>" + port + "</b>");
		while(true)
		{
			currentClient = _socket.Accept();
			Log("Nouveau client: " + currentClient.GetHashCode());
			_clientsSocket.Add(currentClient);
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
			readList.AddRange(_clientsSocket);
			Debug.Log("WAITING PACKET");
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

	void	ClientDisconnect(Socket client)
	{
		foreach(NetworkEntity remote in _entities.Values)
		{
			// if (remote.clientID == client.GetHashCode())
			{
				_dispatcher.Invoke(() => Destroy(remote));
			}
		}
	}

	void ThreadCheckClientIsConnected()
	{
		while(true)
		{
			for(int i = 0; i < _clientsSocket.Count; i++)
			{
				Socket client = _clientsSocket[i];
				if (client.Poll(10, SelectMode.SelectRead) && client.Available == 0 && !readMutex)
				{
					_clientsSocket.Remove(client);
					_clientsTable[client].Disconnect();
					_clientsTable.Remove(client);
					_dispatcher.Invoke(() => {
							ClientDisconnect(client);
							client.Close();
						}
					);
					i--;
				}
			}
			Thread.Sleep(5);
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
		foreach(Socket client in _clientsSocket)
		{
			SendTo(client, packet);
		}
	}

	public void SendToOther(Socket sender, Packet packet)
	{
		foreach(Socket client in _clientsSocket)
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
		foreach(Socket client in _clientsSocket)
		{
			client.Close();
		}
		_clientsSocket.Clear();
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

		// Chat/Message/Popup
		PacketHandler.packetList.Add((int)PacketID.Chat, Packet_Chat); // Serv <=> Client (int, [int], string)
		//PacketHandler.packetList.Add((int)PacketHandler.PacketID.Popup, Packet_Popup); // Serv => Client (int, string)

		// GameObject
		//PacketHandler.packetList.Add((int)PacketHandler.PacketID.Instantiate, Packet_Instantiate); // Serv => Client (str, int, bool, Vec3, Quat)
		//PacketHandler.packetList.Add((int)PacketHandler.PacketID.Destroy, Packet_Destroy); // Serv => Client (int)
		//PacketHandler.packetList.Add((int)PacketHandler.PacketID.UpdatePosition, Packet_UpdatePosition); // Serv => Client (int, Vec3)
		
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
						 (int)PacketID.Popup,
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
						(int)PacketID.Chat,
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
							(int)PacketID.Chat,
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
							(int)PacketID.Chat,
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
						(int)PacketID.Chat,
						1,
						"<color=red><b>Le message n'a pas pu etre envoyer:</b> " + _clientsTable[sender].ParseMsg(msg) + "</color>"
					)
				);
				break;
		}
	}
	/* *** GameObject *** */
}
