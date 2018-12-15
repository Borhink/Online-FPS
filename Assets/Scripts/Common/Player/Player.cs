using System.Collections;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : NetworkEntity
{
	private string      _username = "Not Found";
	public string		Username { get { return _username; }}

    private float       _timerRefresh = 0f;

    public Player(int networkID, int ownerID, bool isLocalPlayer, string username)
    : base(networkID, ownerID, isLocalPlayer)
    {
        this._username = username;
    }

    void Start()
    {
        Debug.Log("Player Start");
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            Debug.Log("Player local");
            _timerRefresh += Time.deltaTime;

            if (_timerRefresh > Constant.MEDIUM_REFRESH_RATE)
            {
                UpdateTransform();
                _timerRefresh = 0f;
            }
        }
    }

    public void UpdateTransform()
    {
        ClientManager cm = GameManager.instance.GetComponent<ClientManager>();

        //Envoi du transform
        cm.Send(PacketHandler.newPacket(
            PacketID.UpdateTransform,
            networkID,
            transform.position,
            transform.rotation
        ));
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