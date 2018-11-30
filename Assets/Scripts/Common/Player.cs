using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Player : NetworkEntity
{
	private int         _networkid = -1;
	private string      _username = "Not Found";
    
	public int		    NetworkID { get { return _networkid; }}
	public string		Username { get { return _username; }}

    void Start()
    {
        Debug.Log("Player Start");
        if (isMine)
        {
            Debug.Log("SEND SPAWN PACKET");
            ClientManager cm = GameManager.instance.GetComponent<ClientManager>();
            cm.Send(
                PacketHandler.newPacket((int)PacketID.HasSpawn
                )
            );
        }
    }

   public Player(int id, string username, int networkid)
   {
      this._networkid = networkid;
      this._username = username;
   }

   public Player(Packet reader)
    {
        _networkid = reader.ReadInt();
        _username = reader.ReadString();
    }

    public void Write(Packet writer)
    {
        writer.Add(_networkid);
        writer.Add(_username);
    }
}