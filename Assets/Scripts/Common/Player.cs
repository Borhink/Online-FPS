using System.Collections;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using UnityEngine;

public class Player : NetworkEntity
{
	private string      _username = "Not Found";
	public string		Username { get { return _username; }}

    void Start()
    {
        Debug.Log("Player Start");
    }

   public Player(int networkid, bool isLocalPlayer, string username)
   {
      this.networkID = networkid;
      this.isLocalPlayer = isLocalPlayer;
      this._username = username;
   }

   public Player(Packet reader)
    {
        networkID = reader.ReadInt();
        _username = reader.ReadString();
    }

    public void Write(Packet writer)
    {
        writer.Add(networkID);
        writer.Add(_username);
    }
}