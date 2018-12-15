using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Data;
using UnityEngine;
using System.Linq;

public class MatchMaker {
	private static int _roomID = 0;

	private ServerManager	_sm;

	private Dictionary<int, Room> _startingRooms = new Dictionary<int, Room>();
	private Dictionary<int, Room> _rooms = new Dictionary<int, Room>();

	public MatchMaker(ServerManager sm)
	{
		_sm = sm;
	}

	//On regarde si il y a des parties à démarrer
	public void CheckStartingRooms()
	{
		var roomsToStart = _startingRooms.Where(r => (float)r.Value.PlayerCount() >= (float)r.Value.Capacity() / 4f).ToArray();

		foreach (var item in roomsToStart)
		{
			_rooms.Add(item.Value.id, item.Value);
			item.Value.StartGame();
			_startingRooms.Remove(item.Key);

			foreach (var room in _startingRooms)
				Debug.Log("startingRooms : " + room.Value.id);
			foreach (var room in _rooms)
				Debug.Log("rooms : " + room.Value.id);
		}
	}

	public void FindMatch(Client client)
	{
		//On cherche une room joignable
		foreach(Room room in _rooms.Values)
		{
			if (room.CanJoin(client))
			{
				room.Join(client);

				// Charge le niveau
				Packet packet = PacketHandler.newPacket(
					PacketID.LoadScene,
					room.LevelName);
				client.Send(packet);
				return;
			}
		}

		//Si on en trouve pas, on en créé une
		Room newRoom = new Room(_roomID, 4, "Level1");
		_startingRooms.Add(_roomID, newRoom);
		newRoom.Join(client);
		_roomID++;
	}

	public void CloseAllRooms()
	{
		foreach (Room room in _rooms.Values)
		{
			room.Close();
		}
	}
}