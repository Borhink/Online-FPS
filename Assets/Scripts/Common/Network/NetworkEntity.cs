using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using UnityEngine;

public class NetworkEntity : MonoBehaviour
{
	public string		prefabName;
	public int			index;
	public Socket		socket;
	public SocketScript	ss;
	public bool			isMine;
}