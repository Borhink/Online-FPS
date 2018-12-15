using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room {
	private EntityManager	_em = new EntityManager();
	public Dictionary<int, Client> _clients = new Dictionary<int, Client>();

	private float _roomOffset = 100f;
	public int	id;
	private int	_maxCapacity;

	private GameObject	_level;
	private string		_levelName;

	private List<Spawn>	_spawnPoints = new List<Spawn>();
	private int			_spawnIndex = -1;

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
		if (_clients.ContainsKey(client.ID))
		{
			//Création du personnage
			Spawn spawn = NextSpawn();
			NetworkEntity entity = _em.Instantiate(
				"Prefabs/Player",
				ServerManager.GetNetworkID(),
				client.ID,
				false,
				spawn.position,
				spawn.rotation,
				_level.transform
			);
			Player newPlayer =  entity.gameObject.GetComponent<Player>();
			client.account.player = newPlayer;

			//Envoi du personnage à son client
			Packet packet = PacketHandler.newPacket(
				PacketID.Instantiate,
				"Prefabs/Player",
				newPlayer.networkID,
				newPlayer.ownerID,
				true,
				spawn.position,
				spawn.rotation
			);
			client.Send(packet);

			foreach (Client other in _clients.Values)
			{
				if (other.ID != client.ID)
				{
					//Envois des autres personnages au client
					Player otherPlayer = other.account.player;
					packet = PacketHandler.newPacket(
						PacketID.Instantiate,
						"Prefabs/Player",
						otherPlayer.networkID,
						newPlayer.ownerID,
						false,
						otherPlayer.Position,
						otherPlayer.Rotation
					);
					client.Send(packet);

					//Envois du nouveau personnage aux autres clients
					packet = PacketHandler.newPacket(
						PacketID.Instantiate,
						"Prefabs/Player",
						newPlayer.networkID,
						newPlayer.ownerID,
						false,
						spawn.position,
						spawn.rotation
					);
					other.Send(packet);
				}
			}
		}
	}

	public void RemovePlayer(Client client)
	{
		if (_clients.ContainsKey(client.ID))
		{
			foreach (Client other in _clients.Values)
			{
				if (other.ID != client.ID)
				{
					//Supprimme le personnage pour les autres clients
					Packet packet = PacketHandler.newPacket(
						PacketID.Destroy,
						client.account.player.networkID
					);
					other.Send(packet);
				}
			}
			
			//Destruction du personnage
			_em.Destroy(client.account.player);
			client.account.player = null;
		}
	}

	private void LoadLevel()
	{
		//Création du niveau
		_level = GameObject.Instantiate(Resources.Load<GameObject>("Levels/"+_levelName), new Vector3(0f, _roomOffset * id, 0f), Quaternion.identity);
		_level.name = "Room " + id + " - " + _levelName;

		//Récupération des points de spawn
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
		foreach (Client client in _clients.Values)
		{
			//Envoi la demande de chargement de la map aux clients
			Packet packet = PacketHandler.newPacket(
				PacketID.LoadScene,
				_levelName);
			client.Send(packet);
		}
	}

	public void UpdateTransform(Client client, int networkID, Vector3 position, Quaternion rotation)
	{
		NetworkEntity entity = _em.GetEntity(networkID);
		if (!entity || entity.ownerID != client.ID)
			return;

		entity.transform.position = position;
		entity.transform.rotation = rotation;

		foreach (Client other in _clients.Values)
		{
			if (other.ID != client.ID)
			{
				//Update le transform pour les autres clients
				Packet packet = PacketHandler.newPacket(
					PacketID.UpdateTransform,
					networkID,
					entity.transform.position,
					entity.transform.rotation
				);
				other.Send(packet);
			}
		}
	}

	public int Capacity()
	{
		return (_maxCapacity);
	}

	public int PlayerCount()
	{
		return (_clients.Count);
	}

	public int CapacityLeft()
	{
		return (_maxCapacity - _clients.Count);
	}

	public bool CanJoin(Client client)
	{
		if (CapacityLeft() > 0)
			return (true);
		return (false);
	}

	public void Join(Client client)
	{
		_clients.Add(client.ID, client);
		client.room = this;
	}

	public void Leave(Client client)
	{
		if (client.account.player != null)
			RemovePlayer(client);
		_clients.Remove(client.ID);
		// _em.DestroyAllOf(client.ID); TODO detruires les objets associés si utile (+ MàJ autres clients)
		client.room = null;
	}

	public void Close()
	{
		foreach(Client client in _clients.Values)
		{
			//Renvoi des clients au MainMenu
			client.Send(PacketHandler.newPacket(
				PacketID.LoadScene,
				"MainMenu"
			));
			//Vers le home
			client.Send(PacketHandler.newPacket(
				PacketID.OpenMenu,
				(int)MenuID.Home
			));
			Debug.Log("ERROR HERE, need arg to loadScene for home");

			//Destruction du personnage
			_em.Destroy(client.account.player);
			client.account.player = null;
		}
		//Destruction du niveau
		GameObject.Destroy(_level);
	}
}