using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerSetup : MonoBehaviour {
	[SerializeField] private Behaviour[]	_componentsToDisable;

	private Player _player = null;

	void Awake()
	{
		_player = GetComponent<Player>();
        Debug.Log("PlayerSetup Awake");
	}

	void Start ()
	{
        Debug.Log("PlayerSetup Start");
		if (!_player.isLocalPlayer)
		{
			DisableComponents();
		}
	}
	
	private void DisableComponents()
	{
		foreach(Behaviour c in _componentsToDisable)
		{
			c.enabled = false;
		}
	}
}
