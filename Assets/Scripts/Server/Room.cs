using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room {
	private float _roomOffset = 100f;
	public int	id;
	private int	_maxCapacity;

	public Dictionary<int, Client> _players = new Dictionary<int, Client>();

	private GameObject	_level;
	private string		_levelName;

	private List<Spawn>	_spawnPoints = new List<Spawn>();
	private int				_spawnIndex = -1;

	public string LevelName { get { return _levelName; }}

	public Room(int id, int maxCapacity, string levelName)
	{
		this.id = id;
		this._maxCapacity = maxCapacity;
		this._levelName = levelName;
	}

	public Spawn NextSpawn()
	{
		_spawnIndex++;
		if (_spawnIndex >= _spawnPoints.Count)
			_spawnIndex = 0;
		Spawn spawn = _spawnPoints[_spawnIndex];
		spawn.position.y -= _roomOffset * id;
		return (_spawnPoints[_spawnIndex]);
	}

	public void SpawnPlayer(Client client)
	{
		if (_players.ContainsKey(client.account.id))
		{
			foreach (Client other in _players.Values)
			{
				Debug.Log("other : " + other.ID + ", client : " + other.ID);
				if (other.account.id != client.account.id)
				{
					Debug.Log("other.account.id != client.account.id");
					Player otherPlayer = other.account.player;
					Packet packet = PacketHandler.newPacket(
						(int)PacketID.Instantiate,
						"Prefabs/Player",
						otherPlayer.NetworkID,
						false,
						otherPlayer.transform.position,
						otherPlayer.transform.rotation);
					client.Send(packet);

					Player clientPlayer = client.account.player;
					packet = PacketHandler.newPacket(
						(int)PacketID.Instantiate,
						"Prefabs/Player",
						clientPlayer.NetworkID,
						false,
						clientPlayer.transform.position,
						clientPlayer.transform.rotation);
					other.Send(packet);
				}
			}
		}
	}

	private void LoadLevel()
	{
		_level = GameObject.Instantiate(Resources.Load<GameObject>("Levels/"+_levelName), new Vector3(0f, _roomOffset * id, 0f), Quaternion.identity);

		Transform spawnPoints =_level.transform.Find("SpawnPoints");
		foreach (Transform child in spawnPoints)
		{
			if (child.tag == "Spawn")
				_spawnPoints.Add(new Spawn(child.position, child.rotation));
		}
		Debug.Log("SpawnPoints size : " + _spawnPoints.Count);
	}

	public void StartGame()
	{
		LoadLevel();
		foreach (Client client in _players.Values)
		{
			Spawn spawn = NextSpawn();
			Packet packet = PacketHandler.newPacket(
				(int)PacketID.LoadScene,
				_levelName);
			spawn.Write(packet);
			client.Send(packet);
		}
	}

	public int Capacity()
	{
		return (_maxCapacity);
	}

	public int PlayerCount()
	{
		return (_players.Count);
	}

	public int CapacityLeft()
	{
		return (_maxCapacity - _players.Count);
	}

	public bool CanJoin(Client client)
	{
		if (CapacityLeft() > 0)
			return (true);
		return (false);
	}

	public void Join(Client client)
	{
		_players.Add(client.account.id, client);
		client.room = this;
	}
}