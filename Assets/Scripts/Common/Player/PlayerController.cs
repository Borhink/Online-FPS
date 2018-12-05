using System.Collections;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerController : MonoBehaviour
{
    private Player _player = null;

    void Awake()
	{
		_player = GetComponent<Player>();
        Debug.Log("PlayerController Awake");
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            _player.Move(new Vector3(0f, 1f, 0f));
        }
    }
}