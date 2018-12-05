using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;

public struct KeyValue
{
    public decimal price;
    public string title;
    public string author;
}

public static class DatabaseManager {

	private static MySqlConnection	_conn = null;
	private static MySqlTransaction _transaction = null;

	private static long	_lastInsertID = -1;
	public  static long	LastInsertID { get { return _lastInsertID; }}	

	public static void OpenDatabase()
	{
		if (_conn != null)
			return;

		_conn = new MySqlConnection("SERVER=127.0.0.1; DATABASE=fps; UID=root; PASSWORD=");
		_conn.Open();
		Debug.Log("MySQL version : " + _conn.ServerVersion);
	}

	public static void CloseDatabase()
	{
		if (_conn == null)
			return;

		_conn.Close();
		_conn = null;
	}

	/// <summary>
	/// Select from table in database
	/// <example> Example: Select("col1, col2", "table", "col1='test'")</example>
	/// <param name="columns"> "col1, col2"</param>
	/// <param name="table"> "table"</param>
	/// <param name="where"> "col1='test'"</param>
	/// </summary>
	public static MySqlDataReader Select(string columns, string table, string where = "")
	{
		OpenDatabase();

		string query = "SELECT " + columns + " FROM " + table;
		if (where != "")
			query += " WHERE " + where;
		MySqlCommand cmd = new MySqlCommand(query, _conn);
		Debug.Log(cmd.CommandText);
		MySqlDataReader reader = cmd.ExecuteReader();
		cmd.Dispose();
		return (reader);
	}

	/// <summary>
	/// Insert in table in database
	/// <example> Example: Insert("table", "col1, col2", "1, 'test'")</example>
	/// <param name="table"> "table"</param>
	/// <param name="columns"> "col1, col2"</param>
	/// <param name="values"> "1, 'test'"</param>
	/// </summary>
	public static void Insert(string table, string columns, string values)
	{
		OpenDatabase();

		string query = "INSERT INTO " + table + " (" + columns + ") VALUES (" + values + ")";
		MySqlCommand cmd = new MySqlCommand(query, _conn);
		Debug.Log(cmd.CommandText);
		if (_transaction != null)
			cmd.ExecuteNonQuery();
		else
		{
			IDataReader reader = cmd.ExecuteReader();
			reader.Close();
		}
		_lastInsertID = cmd.LastInsertedId;
		cmd.Dispose();
	}

	public static void Update(string table, string values, string where)
	{
		OpenDatabase();

		string query = "UPDATE " + table + " SET " + values + " WHERE " + where;
		MySqlCommand cmd = new MySqlCommand(query, _conn);
		Debug.Log(cmd.CommandText);
		if (_transaction != null)
			cmd.ExecuteNonQuery();
		else
		{
			IDataReader reader = cmd.ExecuteReader();
			reader.Close();
		}
		cmd.Dispose();
	}

	public static void Replace(string table, string columns, string values)
	{
		OpenDatabase();

		string query = "REPLACE INTO " + table + " (" + columns + ") VALUES (" + values + ")";
		MySqlCommand cmd = new MySqlCommand(query, _conn);
		Debug.Log(cmd.CommandText);
		if (_transaction != null)
			cmd.ExecuteNonQuery();
		else
		{
			IDataReader reader = cmd.ExecuteReader();
			reader.Close();
		}
		cmd.Dispose();
	}

	public static void Delete(string table, string where)
	{
		OpenDatabase();

		string query = "DELETE FROM " + table + " WHERE " + where;
		MySqlCommand cmd = new MySqlCommand(query, _conn);
		Debug.Log(cmd.CommandText);
		if (_transaction != null)
			cmd.ExecuteNonQuery();
		else
		{
			IDataReader reader = cmd.ExecuteReader();
			reader.Close();
		}
		cmd.Dispose();
	}

	public static void Prepare()
	{
		if (_transaction != null)
			return;

		OpenDatabase();
		_transaction = _conn.BeginTransaction();
	}

	public static void Commit()
	{
		 if (_transaction == null)
		 	return;

		_transaction.Commit();
		_transaction.Dispose();
        _transaction = null;
	}

	
	// while (reader.Read())
	// {
	// 	test = reader.GetString("login");
	// }
	// reader.Close();

}