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

    public void Move(Vector3 dir)
    {
        //TODO
    }

   public Player(int networkID, int ownerID, bool isLocalPlayer, string username)
   : base(networkID, ownerID, isLocalPlayer)
   {
      this._username = username;
   }

   public Player(Packet reader)
   : base(reader)
    {
        _username = reader.ReadString();
    }

    public override void Write(Packet writer)
    {
        base.Write(writer);
        writer.Add(_username);
    }
}