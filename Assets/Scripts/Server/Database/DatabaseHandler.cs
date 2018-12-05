using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;

public class DatabaseHandler
{
	public static string Error {get; private set;}

	public static bool AccountConnection(string login, string password)
	{
		MySqlDataReader reader = DatabaseManager.Select("1", "account", "login='" + login + "' AND password='" + password + "'");
		bool ok = reader.HasRows;
		if (!ok)
			Error = "Erreur login ou mot de passe !";
		reader.Close();
		// reader = null;
		return ok;
	}

	public static bool AccountExist(string login)
	{
		MySqlDataReader reader = DatabaseManager.Select("1", "account", "login='" + login + "'");
		bool ok = reader.HasRows;
		reader.Close();
		// reader = null;
		return ok;
	}

	public static bool AccountRegister(string login, string password, string hash)
	{
		if (login.Length >= Constant.MIN_LOGIN_LENGTH && login.Length <= Constant.MAX_LOGIN_LENGTH)
		{
			if (password.Length >= Constant.MIN_PASSWORD_LENGTH && password.Length <= Constant.MAX_PASSWORD_LENGTH)
			{
				if (AccountExist(login) == false)
				{
					DatabaseManager.Insert("account", "login, password", "'" + login + "', '" + hash + "'");
					return true;
				}
				else
					Error = "Le login existe déjà !";
			}
			else
				Error = "Le mot de passe doit faire entre "+Constant.MIN_PASSWORD_LENGTH+" et "+Constant.MAX_PASSWORD_LENGTH+" caractères";
		}
		else
			Error = "Le login doit faire entre "+Constant.MIN_LOGIN_LENGTH+" et "+Constant.MAX_LOGIN_LENGTH+" caractères";
		return false;
	}
}