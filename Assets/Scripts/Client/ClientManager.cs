using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Data;
using UnityEngine;

public class ClientManager : SocketScript {

	public static new ClientManager instance {get; private set;}
	public static bool	readMutex = false;
	
	public Account		account;

	void Awake()
	{
		instance = this;
	}

	override public void Run()
	{
		Debug.Log("run");
		StartCoroutine(AutoConnect());
	}

	IEnumerator AutoConnect()
	{
		bool autoconnect = true;
		while (autoconnect)
		{
			try {
				_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, type);
				_socket.Connect(GetAddress());
				if (_socket.Connected)
				{
					Debug.Log("Socket connectée : " + _socket.GetHashCode() );
					Thread receiveThread = new Thread(new ThreadStart(ThreadReceive));
					receiveThread.Start();
					Thread checkIsConnectThread = new Thread(new ThreadStart(ThreadCheckIsConnected));
					checkIsConnectThread.Start();
					autoconnect = false;
				}
			} catch(SocketException e) {
				_socket = null;
				Debug.Log("Erreur connection au serveur: " + e.Message);
			}
			yield return new WaitForSeconds(5.0f);
		}
	}

#region Threads
	void ThreadReceive()
	{
		byte[] byteBuffer;
		List<byte> arrayByte = new List<byte>();
		while(true)
		{
			if (_socket == null)
				return;
			while(_socket.Available > 0)
			{
				readMutex = true;
				Log("Received: " + _socket.Available);
				byteBuffer = new byte[_socket.Available];
				_socket.Receive(byteBuffer, 0, _socket.Available, SocketFlags.None);
				arrayByte.AddRange(byteBuffer);
			}
			while (arrayByte.Count >= 4)
			{
				int size = BitConverter.ToInt32(arrayByte.ToArray(), 0);
				if (arrayByte.Count >= size + 4)
				{
					arrayByte.RemoveRange(0, 4);
					byte[] packetArray = new byte[size];
					Buffer.BlockCopy(arrayByte.ToArray(), 0, packetArray, 0, size);
					arrayByte.RemoveRange(0, size);
					Packet readPacket = new Packet(packetArray);
					if (!PacketHandler.Parses(_socket, readPacket))
					{
						Log("Error Packet!");
					}
				}
				else
					break;
			}
			readMutex = false;
			Thread.Sleep(10);
		}
	}

	void ThreadCheckIsConnected()
	{
		while(true)
		{
			Debug.Log("ISConnected");
			if (_socket.Poll(10, SelectMode.SelectRead) && _socket.Available == 0 && !readMutex)
			{
			Debug.Log("NOT connected");
				Disconnect();
				return;
			}
			Thread.Sleep(5);
		}
	}
#endregion

	public void Disconnect()
	{
		Debug.Log("Disconnected");
		_socket.Close();
		_socket = null;
		_dispatcher.Invoke(() => {
			GameManager.instance.LoadLevel("MainMenu");
			Run();
		});
	}

	public void Send(Packet packet)
	{
		if (_socket == null)
			return ;
		//_socket.SendBufferSize = packet.Size();
		_socket.Send(packet.GetBuffer(), packet.Size(), SocketFlags.None);
	}

	override protected void Close()
	{

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
		//PacketHandler.packetList.Add((int)PacketHandler.PacketID.Login, Packet_AccountConnect); // Serv <= Client (string, string)
		//PacketHandler.packetList.Add((int)PacketHandler.PacketID.Register, Packet_AccountRegister); // Serv <= Client (string, string)
		PacketHandler.packetList.Add((int)PacketID.AccountData, Packet_Account); // Serv => Client (int, ...)

		// Menu - Scene
		PacketHandler.packetList.Add((int)PacketID.OpenMenu, Packet_OpenMenu); // Serv <=> Client (int, ...)
		PacketHandler.packetList.Add((int)PacketID.LoadScene, Packet_LoadScene); // Serv => Client (string, Vec3)
		// PacketHandler.packetList.Add((int)PacketID.Play, Packet_Play); // Serv <= Client ()
		// PacketHandler.packetList.Add((int)PacketID.LoadComplete, Packet_LoadComplete); // Serv <= Client ()

		// Chat/Message/Popup
		PacketHandler.packetList.Add((int)PacketID.Chat, Packet_Chat); // Serv <=> Client (int, [int], string)
		PacketHandler.packetList.Add((int)PacketID.Popup, Packet_Popup); // Serv => Client (int, string)

		// GameObject
		PacketHandler.packetList.Add((int)PacketID.Instantiate, Packet_Instantiate); // Serv => Client (str, int, int, bool, Vec3, Quat)
		PacketHandler.packetList.Add((int)PacketID.Destroy, Packet_Destroy); // Serv => Client (int)
		PacketHandler.packetList.Add((int)PacketID.UpdatePosition, Packet_UpdatePosition); // Serv => Client (int, Vec3)

	}

	/* *** Account Packet *** */
	void Packet_Account(Socket sender, Packet packet)
	{
		account = new Account(packet);
	}

	/* *** Scene - Menu *** */
	void Packet_OpenMenu(Socket sender, Packet packet)
	{
		int menu = packet.ReadInt();
		_dispatcher.Invoke(() => PanelManager.instance.OpenMenu(menu));
	}

	void Packet_LoadScene(Socket sender, Packet packet)
	{
		string scene = packet.ReadString();
		_dispatcher.Invoke(() => GameManager.instance.LoadLevel(scene));
	}

	/* *** Chat/Message/Popup *** */
	void Packet_Chat(Socket sender, Packet packet)
	{
		int type = packet.ReadInt();
		if (type == 2)
		{
			/*string toName = */packet.ReadString();
			string msg = packet.ReadString();

		}
		else
		{
			string msg = packet.ReadString();

		}
	}

	void Packet_Popup(Socket sender, Packet packet)
	{
		int type = packet.ReadInt();
		string msg = packet.ReadString();
		_dispatcher.Invoke(
			() => {
				if (UICanvasPopup.instance)
				{
					string title = (type == 2) ? "Error" : (type == 1) ? "Warning" : "Information";
					UICanvasPopup.instance.AddPopup(title, msg);
				}
			}
		);
	}

	/* *** GameObject *** */
	void Packet_Instantiate(Socket sender, Packet packet)
	{
		string prefabName = packet.ReadString();
		int networkID = packet.ReadInt();
		int ownerID = packet.ReadInt();
		bool isLocalPlayer = packet.ReadBool();
		Vector3 position = packet.ReadVector3();
		Quaternion rotation = packet.ReadQuaternion();
		_dispatcher.Invoke(
			() => {
				NetworkEntity entity = Instantiate(prefabName, networkID, ownerID, isLocalPlayer, position, rotation);
			}
		);
	}

	void Packet_Destroy(Socket sender, Packet packet)
	{
		int index = packet.ReadInt();
		NetworkEntity entity = GetEntity(index);
		if (entity != null)
		{
			_dispatcher.Invoke(
				() => Destroy(entity)
			);
		}
	}

	void Packet_UpdatePosition(Socket sender, Packet packet)
	{
		int index = packet.ReadInt();
		Vector3 position = packet.ReadVector3();
		NetworkEntity entity = GetEntity(index);
		if (entity != null)
		{
			_dispatcher.Invoke(
				() => {
					entity.gameObject.transform.position = position;
				}
			);
		}
	}

}