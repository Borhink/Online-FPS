using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Spawn {
	public Vector3 position;
	public Quaternion rotation;

    public Spawn(Vector3 position, Quaternion rotation)
   {
      this.position = position;
      this.rotation = rotation;
   }

   public Spawn(Packet reader)
    {
        position = reader.ReadVector3();
        rotation = reader.ReadQuaternion();
    }

    public void Write(Packet writer)
    {
        writer.Add(position);
        writer.Add(rotation);
    }
}