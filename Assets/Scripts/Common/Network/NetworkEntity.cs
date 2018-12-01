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
	// public int			clientID;
	public bool			isLocalPlayer = false;
	// public string		prefabName;
	// public SocketScript	ss;

	public Vector3		Position { get { return transform.position; }}
	public Quaternion	Rotation { get { return transform.rotation; }}
}