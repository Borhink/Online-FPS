using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using UnityEngine;

public enum PacketID {
	Login,
	Register,

	OpenMenu,
	LoadScene,
	Play,

	HasSpawn,

	AccountData,

	Chat,
	Popup,

	Kick,
	Ban,

	Instantiate,
	Destroy,
	UpdatePosition
};

class PacketHandler {

	public static Packet	newPacket(int packet_id, params object[] list)
	{
		Packet packet = new Packet();
		packet.Add(packet_id);
		for (int i = 0; i < list.Length; i++)
			packet.Add(list[i]);
		return packet;
	}

	
	public delegate void PacketReceive(Socket sender, Packet packet);
	public static Dictionary<int, PacketReceive> packetList = new Dictionary<int, PacketReceive>();

	public static bool Parses(Socket sender, Packet packet)
	{
		int packetID = packet.ReadInt();
		Debug.Log("PacketHandler Parsing: " + packetID);
		if (packetList.ContainsKey(packetID))
		{
			packetList[packetID](sender, packet);
			return true;
		}
		return false;
	}
}