using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Security.Cryptography;
using System;
using System.Data;
using System.Text;
using UnityEngine;

public class Client {
	private ServerManager	_sm;
	public Socket			socket {get; private set;}
	public Account	account;
	public Room		room = null;
	private int		_id = -1;
	private string	_login = "Not Found";
	private bool 	_connected = false;

	public bool		Connected { get { return _connected; }}
	public int		ID { get { return _id; }}
	public string	Login { get { return _login; }}

	public Client(ServerManager sm, Socket socket)
	{
		this._sm = sm;
		this.socket = socket;
	}

	public void Send(Packet packet)
	{
		if (socket == null)
			return ;
		if (!socket.Connected)
			return ;
		socket.Send(packet.GetBuffer(), packet.Size(), SocketFlags.None);
	}

	public void Disconnect()
	{
		//SAVE DATAS ON DB
		_connected = false;
		Log("déconnecté!");
	}

	public void Log(object message)
	{
		string info = "";
		if (_connected)
		{
			info += _login;
			if (_id > 0)
				info += "#" + _id;
		}
		else
			info += socket.GetHashCode();
		
		Debug.Log("[Server: " + info + "] " + message);
		_sm.logServer.Add("[" + info + "] " + message.ToString());
		_sm.checkClearMessage();
	}

	private string HashPassword(string mdp)
	{
		mdp = "borhink" + mdp + "subutek";
		SHA256Managed crypt = new SHA256Managed();
		string hash = string.Empty;
		byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(mdp));
		foreach (byte theByte in crypto)
		{
			hash += theByte.ToString("x2");
		}
		return hash;
	}

	//Charge les données du compte, depuis la bdd, en mémoire
	private void InitAccount()
	{
		_id = -1;
		IDataReader reader = DatabaseManager.Select("id", "account", "login='"+_login+"'");
		if (reader.Read())
		{
			_id = reader.GetInt32(0);
		}
		account = new Account(_id, _login);
		reader.Close();
		reader = null;
		Log("Chargement des données du compte effectué.");
	}

	public void AccountConnect(string login, string mdp)
	{
		Debug.Log("1");
		login = login.ToLower();
		mdp = HashPassword(mdp); // ICI On Cript le MDP avec notre Sel
		if (DatabaseHandler.AccountConnection(login, mdp))
		{
			Log("S'est connecté à son compte : " + login);
			_connected = true;
			_login = login;
			InitAccount();

			//Send Player Data
			Packet packet = PacketHandler.newPacket(
				(int)PacketID.AccountData
			);
			account.Write(packet);
			Log("PlayerData Packet Size: " + packet.Size());
			_sm.SendTo(socket, packet);

			// Vers le home
			_sm.SendTo(socket,
				PacketHandler.newPacket(
					(int)PacketID.OpenMenu,
					(int)MenuID.Home
				)
			);
		}
		else
		{
		Debug.Log("2");
			// Error Popup
			Log(DatabaseHandler.Error);
			_sm.SendTo(socket,
				PacketHandler.newPacket(
					(int)PacketID.Popup,
					1,
					DatabaseHandler.Error
				)
			);
		}
	}

	public void AccountRegister(string login, string mdp)
	{
		login = login.ToLower();
		string hash = HashPassword(mdp); // ICI On Cript le MDP avec notre Sel
		if (DatabaseHandler.AccountRegister(login, mdp, hash))
		{
			Log("Viens de créer son compte : " + login.ToLower());
			_connected = true;
			_login = login;
			InitAccount();

			//Send Player Data
			Packet packet = PacketHandler.newPacket(
				(int)PacketID.AccountData
			);
			account.Write(packet);
			Log("PlayerData Packet Size: " + packet.Size());
			_sm.SendTo(socket, packet);

			// Vers le home
			_sm.SendTo(socket,
				PacketHandler.newPacket(
					(int)PacketID.OpenMenu,
					(int)MenuID.Home
				)
			);
		}
		else
		{
			// Error Popup
			Log(DatabaseHandler.Error);
			_sm.SendTo(socket,
				PacketHandler.newPacket(
					(int)PacketID.Popup,
					1,
					DatabaseHandler.Error
				)
			);
		}
	}

	public void LoadAndSendDataPlayer()
	{
		// Packet packet;

		// Envoie de l'inventaire
		// packet = PacketHandler.newPacket(
		// 	PacketHandler.PacketID_CharacterInventory,
		// 	characterIndex
		// );
		// characterSelect.inventory.Write(packet);
		// Log("Inventory Packet Size: " + packet.Size());
		// _sm.SendTo(socket, packet);
	}
	
	// public void CreateAndSendItem(int templateID, int quantity = 1)
	// {
	// 	Character charac = characterSelect;
	// 	if (charac != null)
	// 	{
	// 		Item item = ItemManager.CreateNewItem(templateID, charac, quantity);
	// 		if (item != null)
	// 		{
	// 			Packet packet = PacketHandler.newPacket(
	// 				PacketHandler.PacketID_AddItem,
	// 				characterIndex
	// 			);
	// 			item.Write(packet);
	// 			Log("Add Item Packet Size: " + packet.Size());
	// 			_sm.SendTo(socket, packet);
	// 			Log("SUCCESS Add Item " + templateID + " to " + charac.name);

	// 		}
	// 		else
	// 			Log("FAIL Add Item : item creation failed");
	// 	}
	// 	else
	// 		Log("FAIL Add Item : charac == null");
	// }

	// Permet de faire une sorte de BBCode
	public string ParseMsg(string msg)
	{
		msg = msg.Replace("%login%", _login);
		msg = msg.Replace("%test%", "42");
		return msg;
	} 
}