using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TagFind.Interfaces;
using Windows.Storage;

namespace TagFind.Classes.DB
{
    public class DBListManager
    {
        //Global service

        private MessageManager MessageManager = ((App)App.Current)?.ServiceProvider?.GetRequiredService<MessageManager>() ?? new();

        // Runtime values

        public string MetaListDBPath = string.Empty;
        public string MetaListDBName = "DBMetaList.db";
        public bool Connected = false;
        SqliteConnection dbconnection = new();
        private readonly object _lock = new object();

        public DBListManager()
        {
            try
            {
                MetaListDBPath = ApplicationData.Current.LocalFolder.Path;
            }
            catch
            {
                MetaListDBPath = Environment.ProcessPath ?? "";
            }
            MetaListDBPath = System.IO.Path.Combine(MetaListDBPath, MetaListDBName); 
            ConnectListDB();
        }

        ~DBListManager()
        {
            if (!Connected)
            {
                return;
            }
            try
            {
                dbconnection.Close();
            }
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
            }
        }

        public async void ConnectListDB()
        {
            try
            {
                if (!File.Exists(MetaListDBPath))
                {
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(System.IO.Path.GetDirectoryName(MetaListDBPath));
                    await folder.CreateFileAsync(System.IO.Path.GetFileName(MetaListDBPath));

                    dbconnection = new SqliteConnection($"Filename={MetaListDBPath};");
                    dbconnection.Open();

                    string createTableSql = @"CREATE TABLE IF NOT EXISTS DbMeta"
                        + @"(Id INTEGER PRIMARY KEY AUTOINCREMENT,"
                        + @"Path TEXT NOT NULL,"
                        + @"IsValid INTEGER NOT NULL,"
                        + @"Name TEXT,"
                        + @"Description TEXT"
                        + @")";

                    var createTable = new SqliteCommand(createTableSql, dbconnection);
                    createTable.ExecuteReader();
                    Connected = true;
                }
                else
                {
                    dbconnection = new SqliteConnection($"Data Source={MetaListDBPath};");
                    dbconnection.Open();
                    Connected = true;
                }
            }
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
            }
        }

        public void Add(string dbPath, string description)
        {
            lock (_lock)
            {
                try
                {
                    if (Connected)
                    {
                        string command =
                            "INSERT INTO DbMeta " +
                            "VALUES (" +
                            "NULL," +
                            "@Path," +
                            "@IsValid," +
                            "@Name," +
                            "@Description" +
                            ")";
                        SqliteCommand SqliteCommand = new(command, dbconnection);
                        SqliteCommand.Parameters.AddWithValue("@Path", dbPath);
                        SqliteCommand.Parameters.AddWithValue("@IsValid", File.Exists(MetaListDBPath) ? 1 : 0);
                        SqliteCommand.Parameters.AddWithValue("@Name", System.IO.Path.GetFileNameWithoutExtension(dbPath));
                        SqliteCommand.Parameters.AddWithValue("@Description", description);
                        SqliteCommand.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageManager.PushMessage(MessageType.Error, ex.Message);
                }
            }
        }

        public void Remove(long ID)
        {
            lock (_lock)
            {
                try
                {
                    if (Connected)
                    {
                        string command =
                            "DELETE FROM DbMeta " +
                            "WHERE ID = @ID";
                        SqliteCommand SqliteCommand = new(command, dbconnection);
                        SqliteCommand.Parameters.AddWithValue("@ID", ID);
                        SqliteCommand.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageManager.PushMessage(MessageType.Error, ex.Message);
                }
            }
        }

        public void GetList(ref List<MetaData> DBInfoList)
        {
            lock (_lock)
            {
                try
                {
                    string command =
                        "SELECT * FROM DbMeta";
                    SqliteCommand SqliteCommand = new(command, dbconnection);
                    SqliteDataReader SqliteDataReader = SqliteCommand.ExecuteReader();
                    DBInfoList.Clear();
                    while (SqliteDataReader.Read())
                    {
                        MetaData DBInfo = new();
                        DBInfo.ID = SqliteDataReader.GetInt32(0);
                        DBInfo.Path = SqliteDataReader.GetString(1);
                        DBInfo.IsValid = SqliteDataReader.GetInt32(2) == 1;
                        DBInfo.Name = SqliteDataReader.GetString(3);
                        DBInfo.Description = SqliteDataReader.GetString(4);
                        DBInfoList.Add(DBInfo);
                    }
                }
                catch (Exception ex)
                {
                    MessageManager.PushMessage(MessageType.Error, ex.Message);
                }
            }
        }
    }
}
