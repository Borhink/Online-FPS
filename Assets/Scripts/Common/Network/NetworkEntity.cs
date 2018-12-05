using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using UnityEngine;

public class NetworkEntity : MonoBehaviour
{
	public int       	networkID;
	public int			ownerID;
	public bool			isLocalPlayer = false;
	// public string		prefabName;
	// public SocketScript	ss;

	public Vector3		Position { get { return transform.position; }}
	public Quaternion	Rotation { get { return transform.rotation; }}

	public NetworkEntity(int networkID, int ownerID, bool isLocalPlayer)
	{
		this.networkID = networkID;
		this.ownerID = ownerID;
		this.isLocalPlayer = isLocalPlayer;
	}

	public NetworkEntity(Packet reader)
    {
        networkID = reader.ReadInt();
        ownerID = reader.ReadInt();
        isLocalPlayer = reader.ReadBool();
    }

    public virtual void Write(Packet writer)
    {
        writer.Add(networkID);
        writer.Add(ownerID);
        writer.Add(isLocalPlayer);
    }
}