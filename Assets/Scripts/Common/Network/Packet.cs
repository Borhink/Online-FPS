using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
public class Packet {
	public const int Max_Packet_Size = 65531; // 65535 - 4 octet de taille

	public bool	isWriting {get; private set;}
	List<byte>	_byteBuffer;
	int			_readBufferIndex = 0;

	public Packet()
	{
		_byteBuffer = new List<byte>();
		isWriting = true;
	}

	public Packet(byte[] buffer)
	{
		_byteBuffer = new List<byte>();
		_byteBuffer.AddRange(buffer);
		isWriting = false;
	}

	public byte[]	GetBuffer()
	{
		byte[] array = new byte[Size()];
		byte[] size = BitConverter.GetBytes(_byteBuffer.Count);
		for (int i = 0; i < 4; i++)
			array[i] = size[i];
		for (int i = 0; i < _byteBuffer.Count; i++)
			array[i + 4] = _byteBuffer[i];
		return array;//_byteBuffer.ToArray();
	}

	public int		Size()
	{
		return _byteBuffer.Count + 4;
	}

	public Packet	Add(float value)
	{
		if (!isWriting)
		{
			Debug.Log("Packet is Read Only!");
			return this;
		}
		if (_byteBuffer.Count + 4 >= Max_Packet_Size)
		{
			Debug.Log("Packet size limit!");
			return this;
		}
		_byteBuffer.AddRange(BitConverter.GetBytes(value));
		return this;
	}

	public float	ReadFloat()
	{
		float value = BitConverter.ToSingle(_byteBuffer.ToArray(), _readBufferIndex);
		_readBufferIndex += 4;
		return (value);
	}

	public Packet Add(Vector3 value)
	{
		if (_byteBuffer.Count + 12 >= Max_Packet_Size)
		{
			Debug.Log("Packet size limit!");
			return this;
		}
		return Add(value.x).Add(value.y).Add(value.z);
	}

	public Vector3 ReadVector3()
	{
		return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
	}

	public Packet Add(Vector2 value)
	{
		if (_byteBuffer.Count + 8 >= Max_Packet_Size)
		{
			Debug.Log("Packet size limit!");
			return this;
		}
		return Add(value.x).Add(value.y);
	}

	public Vector2 ReadVector2()
	{
		return new Vector2(ReadFloat(), ReadFloat());
	}

	public Packet Add(Quaternion value)
	{
		if (_byteBuffer.Count + 16 >= Max_Packet_Size)
		{
			Debug.Log("Packet size limit!");
			return this;
		}
		return Add(value.x).Add(value.y).Add(value.z).Add(value.w);
	}

	public Quaternion ReadQuaternion()
	{
		return new Quaternion(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
	}

	public Packet Add(int value)
	{
		if (!isWriting)
		{
			Debug.Log("Packet is Read Only!");
			return this;
		}
		if (_byteBuffer.Count + 4 >= Max_Packet_Size)
		{
			Debug.Log("Packet size limit!");
			return this;
		}
		_byteBuffer.AddRange(BitConverter.GetBytes(value));
		return this;
	}

	public int ReadInt()
	{
		int value = BitConverter.ToInt32(_byteBuffer.ToArray(), _readBufferIndex);
		_readBufferIndex += 4;
		return (value);
	}

	public Packet Add(string value)
	{
		if (!isWriting)
		{
			Debug.Log("Packet is Read Only!");
			return this;
		}
		byte[] valueByte = Encoding.UTF8.GetBytes(value);
		if (_byteBuffer.Count + 4 + valueByte.Length >= Max_Packet_Size)
		{
			Debug.Log("Packet size limit!");
			return this;
		}
		Add(valueByte.Length);
		_byteBuffer.AddRange(valueByte);
		return this;
	}

	public string ReadString()
	{
		int count = ReadInt();
		if (count <= 0)
			return "";
		string value = Encoding.UTF8.GetString(_byteBuffer.ToArray(), _readBufferIndex, count);
		_readBufferIndex += count;
		return (value);
	}

	public Packet Add(bool value)
	{
		if (!isWriting)
		{
			Debug.Log("Packet is Read Only!");
			return this;
		}
		if (_byteBuffer.Count + 1 >= Max_Packet_Size)
		{
			Debug.Log("Packet size limit!");
			return this;
		}
		_byteBuffer.AddRange(BitConverter.GetBytes(value));
		return this;
	}

	public bool ReadBool()
	{
		bool value = BitConverter.ToBoolean(_byteBuffer.ToArray(), _readBufferIndex);
		_readBufferIndex += 1;
		return (value);
	}

	public Packet Add(object value)
	{
		if (value is float)
			return Add((float)value);
		if (value is int)
			return Add((int)value);
		if (value is string)
			return Add((string)value);
		if (value is bool)
			return Add((bool)value);
		if (value is Vector3)
			return Add((Vector3)value);
		if (value is Vector2)
			return Add((Vector2)value);
		if (value is Quaternion)
			return Add((Quaternion)value);
		return this;
	}
	
}