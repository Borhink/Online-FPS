using System.Collections;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{
	[SerializeField] private float	_speed = 5f;
	[SerializeField] private float	_sensivity = 3f;


	private PlayerMotor 		_motor;

    private void Start()
	{
		_motor = GetComponent<PlayerMotor>();
	}

    void Update()
    {
        //Déplacements
		float xMov = Input.GetAxis("Horizontal");
		float zMov = Input.GetAxis("Vertical");
		Vector3 moveHorizontal = transform.right * xMov;
		Vector3 moveVertical = transform.forward * zMov;
		Vector3 velocity = (moveHorizontal + moveVertical) * _speed;
		_motor.Move(velocity);

		//Rotations
		float yRot = Input.GetAxisRaw("Mouse X");
		Vector3 rotation = new Vector3(0, yRot, 0) * _sensivity;
		_motor.Rotate(rotation);
		float xRot = Input.GetAxisRaw("Mouse Y");
		float cameraRotationX = xRot * _sensivity;
		_motor.RotateCamera(cameraRotationX);
    }
}