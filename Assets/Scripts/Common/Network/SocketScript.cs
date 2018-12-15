using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(Dispatcher))]
abstract public class SocketScript : MonoBehaviour {
	public static SocketScript instance {get; private set;}
	public string			address = "127.0.0.1";
	public int				port = 4221;
	protected Dispatcher	_dispatcher;
	protected Socket		_socket = null;
	public ProtocolType		type = ProtocolType.Tcp;

	
	public Socket			Socket { get { return _socket; }}

	void Awake()
	{
		instance = this;
	}

	private void Start() {
		_dispatcher = GetComponent<Dispatcher>();
		InitPacket();
	}

	public IPEndPoint GetAddress()
	{
		IPAddress ipAddress = IPAddress.Parse(address);
		return (new IPEndPoint(ipAddress, port));
	}
	abstract public void Run();
	abstract protected void InitPacket();
	abstract protected void Close();
	public bool IsServer()
	{
		if (GameManager.instance != null)
			return GameManager.instance.side == GameManager.Side.Server;
		return false;
	}

	public bool IsConnected()
	{
		if (_socket != null)
			return _socket.Connected;
		return false;
	}

	virtual public void Log(object message)
	{
		Debug.Log("[Client] " + message);
	}
	
	void OnDestroy()
	{
		Close();
		if (_socket == null)
			return;
		_socket.Close();
	}
}