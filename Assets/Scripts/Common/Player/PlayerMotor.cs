using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour {
	[SerializeField] private Camera _cam;
	[SerializeField] private float _camRotationLimit = 85f;

	private Vector3		_velocity;
	private Vector3		_rotation;
	private float		_cameraRotationX;
	private float		_currentCameraRotationX;
	private Vector3		_thrusterForce;
	private Rigidbody	_rb;

	private void Start()
	{
		_rb = GetComponent<Rigidbody>();
	}

	public void Move(Vector3 velocity)
	{
		_velocity = velocity;
	}

	public void Rotate(Vector3 rotation)
	{
		_rotation = rotation;
	}

	public void RotateCamera(float rotationX)
	{
		_cameraRotationX = rotationX;
	}

	public void ApplyThruster(Vector3 thrusterForce)
	{
		_thrusterForce = thrusterForce;
	}

	private void FixedUpdate()
	{
		PerfomMovement();
		PerformRotation();
	}

	private void PerfomMovement()
	{
		if (_velocity != Vector3.zero)
			_rb.MovePosition(_rb.position + _velocity * Time.fixedDeltaTime);
		if (_thrusterForce != Vector3.zero)
			_rb.AddForce(_thrusterForce * Time.fixedDeltaTime, ForceMode.Acceleration);
	}

	private void PerformRotation()
	{
		_rb.MoveRotation(_rb.rotation * Quaternion.Euler(_rotation));
		_currentCameraRotationX -= _cameraRotationX;
		_currentCameraRotationX = Mathf.Clamp(_currentCameraRotationX, -_camRotationLimit, _camRotationLimit);
		_cam.transform.localEulerAngles = new Vector3(_currentCameraRotationX, 0f , 0f);
	}
}
