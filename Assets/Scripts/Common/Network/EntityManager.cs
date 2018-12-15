using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager {

	protected Dictionary<int, NetworkEntity> _entities = new Dictionary<int, NetworkEntity>();

	public NetworkEntity	Instantiate(string prefabName, int networkID, int ownerID, bool isLocalPlayer, Vector3 position, Quaternion rotation, Transform parent = null)
	{
		GameObject prefab = Resources.Load<GameObject>(prefabName);
		if (!prefab)
			return null;
		GameObject go = GameObject.Instantiate(prefab, position, rotation);
		NetworkEntity entity = go.GetComponent<NetworkEntity>();
		entity.networkID = networkID;
		entity.ownerID = ownerID;
		entity.isLocalPlayer = isLocalPlayer;
		if (parent)
			go.transform.parent = parent;
		_entities.Add(networkID, entity);
		return entity;
	}

	public void		Destroy(NetworkEntity entity)
	{
		_entities.Remove(entity.networkID);
		if (entity != null && entity.gameObject != null)
			GameObject.Destroy(entity.gameObject);
	}

	public void		DestroyAllOf(int ownerID)
	{
		foreach(NetworkEntity entity in _entities.Values)
		{
			if (entity.ownerID == ownerID)
			{
				Destroy(entity);
			}
		}
	}

	public NetworkEntity	GetEntity(int id)
	{
		if (!_entities.ContainsKey(id))
			return null;
		return _entities[id];
	}
}