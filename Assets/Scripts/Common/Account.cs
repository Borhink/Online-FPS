using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Account : MonoBehaviour
{
    public int       id = -1;
    public string   login = "Not Found";
    public Player   player = null;

   public Account(int id, string login)
   {
      this.id = id;
      this.login = login;
   }

   public Account(Packet reader)
    {
        id = reader.ReadInt();
        login = reader.ReadString();
    }

    public void Write(Packet writer)
    {
        writer.Add(id);
        writer.Add(login);
    }
}