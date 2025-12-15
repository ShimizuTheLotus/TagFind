using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TagFind.Classes.DataTypes;
using TagFind.Classes.Extensions;

namespace TagFind.Classes.DB
{
    using static TagFind.Classes.Consts.DB.UserDB;
    using static TagFind.Classes.DataTypes.SearchCondition;
    using DataItemFastSearch = Consts.DB.UserDB.DataItemFastSearch;
    using DataItems = Consts.DB.UserDB.DataItems;
    using ItemTags = Consts.DB.UserDB.ItemTags;
    // Surname of DB property names
    using Meta = Consts.DB.UserDB.Meta;
    using PropertyTemplates = Consts.DB.UserDB.PropertyTemplates;
    using TagData = Consts.DB.UserDB.TagData;
    using TagPool = Consts.DB.UserDB.TagPool;

    public class MetaItem
    {
        public string Property { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class DBContentManager
    {
        //Global service
        MessageManager MessageManager = ((App)App.Current)?.ServiceProvider?.GetRequiredService<MessageManager>() ?? new();

        private SemaphoreSlim _lock = new(3, 3);

        //Reference
        public string DBPath = string.Empty;
        public bool Connected = false;
        SqliteConnection? dbConnection = new();

        public DBContentManager()
        {

        }
        DBContentManager(string DBPath)
        {
            OpenDB(DBPath);
        }

        ~DBContentManager()
        {
            CloseDB();
        }


        // DB options

        public void OpenDB(string dbpath)
        {
            DBPath = dbpath;
            OpenDB();
        }

        public void OpenDB()
        {
            if (DBPath == string.Empty)
            {
                return;
            }
            // If not exists db, return.
            if (!File.Exists(DBPath))
            {
                return;
            }

            try
            {
                dbConnection = new SqliteConnection($"Filename = {DBPath};");
                dbConnection.Open();
                Connected = true;
            }
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
            }
        }

        public void CloseDB()
        {
            try
            {
                if (dbConnection != null && dbConnection.State != System.Data.ConnectionState.Closed)
                {
                    dbConnection.Close();
                }
            }
            catch (Exception ex)
            {
                if (ex is ObjectDisposedException)
                {
                    Connected = false;
                    return;
                }
                MessageManager.PushMessage(MessageType.Error, ex.Message);
            }
            finally
            {
                _lock = new(3, 3);
                Connected = false;
            }
        }

        public void CreateNewDB(string dbpath)
        {
            OpenDB(dbpath);
            InitializeDB();
            CloseDB();
        }

        /// <summary>
        /// Create tables for database.
        /// </summary>
        public void InitializeDB()
        {
            try
            {
                string command = "CREATE TABLE IF NOT EXISTS " +
                    $"{nameof(Meta)} (" +
                    $"{new Meta().Property} TEXT NOT NULL," +
                    $"{new Meta().Value} TEXT NOT NULL)";
                SqliteCommand SqliteCommand = new(command, dbConnection);
                SqliteCommand.ExecuteNonQuery();

                command =
                    "CREATE TABLE IF NOT EXISTS " +
                    $"{nameof(TagPool)} (" +
                    $"{new TagPool().ID} INTEGER PRIMARY KEY," +
                    $"{new TagPool().MainName} TEXT NOT NULL," +
                    $"{new TagPool().Description} TEXT," +
                    $"{new TagPool().CreatedTime} INTEGER," +
                    $"{new TagPool().ModifiedTime} INTEGER" +
                    ")";
                SqliteCommand = new(command, dbConnection);
                SqliteCommand.ExecuteNonQuery();

                command =
                    "CREATE TABLE IF NOT EXISTS " +
                    $"{nameof(TagData)} (" +
                    $"{new TagData().ID} INTEGER PRIMARY KEY," +
                    $"{new TagData().TagID} INTEGER NOT NULL," +
                    $"{new TagData().Seq} TEXT NOT NULL," +
                    $"{new TagData().Type} TEXT NOT NULL," +
                    $"{new TagData().Value} TEXT NOT NULL" +
                    ")";
                SqliteCommand = new(command, dbConnection);
                SqliteCommand.ExecuteNonQuery();

                command =
                    "CREATE TABLE IF NOT EXISTS " +
                    $"{nameof(PropertyTemplates)} (" +
                    $"{new PropertyTemplates().ID} INTEGER PRIMARY KEY," +
                    $"{new PropertyTemplates().TemplateName} TEXT," +
                    $"{new PropertyTemplates().PropertyName} TEXT NOT NULL," +
                    $"{new PropertyTemplates().Seq} INTEGER NOT NULL," +
                    $"{new PropertyTemplates().TagParentRestriction} INTEGER NOT NULL" +
                    ")";
                SqliteCommand = new(command, dbConnection);
                SqliteCommand.ExecuteNonQuery();

                command =
                    "CREATE TABLE IF NOT EXISTS " +
                    $"{nameof(DataItems)} (" +
                    $"{new DataItems().ID} INTEGER PRIMARY KEY," +
                    $"{new DataItems().ParentItemID} INTEGER NOT NULL," +
                    $"{new DataItems().Type} TEXT NOT NULL," +
                    $"{new DataItems().CreatedTime} INTEGER NOT NULL," +
                    $"{new DataItems().ModifiedTime} INTEGER NOT NULL," +
                    $"{new DataItems().Title} TEXT" +
                    ")";
                SqliteCommand = new(command, dbConnection);
                SqliteCommand.ExecuteNonQuery();

                command =
                    "CREATE VIRTUAL TABLE IF NOT EXISTS " +
                    $"{nameof(DataItemFastSearch)} " +
                    $"USING FTS5(" +
                    $"{new DataItemFastSearch().DataItemID}," +
                    $"{new DataItemFastSearch().Title}," +
                    $"{new DataItemFastSearch().Description}," +
                    $"{new DataItemFastSearch().RefPath}," +
                    $"{new DataItemFastSearch().SearchText}," +
                    $")";
                SqliteCommand = new(command, dbConnection);
                SqliteCommand.ExecuteNonQuery();

                command =
                    "CREATE TABLE IF NOT EXISTS " +
                    $"{nameof(ItemTags)} (" +
                    $"{new ItemTags().ItemID} INTEGER," +
                    $"{new ItemTags().PropertyID} INTEGER NOT NULL," +
                    $"{new ItemTags().TagID} INTEGER NOT NULL," +
                    $"{new ItemTags().ParentTagID} INTEGER NOT NULL" +
                    ")";
                SqliteCommand = new(command, dbConnection);
                SqliteCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
            }
        }

        /// <summary>
        /// Edit metadata of the database, if there's no such a property, it will be created.
        /// </summary>
        /// <param name="Property">Property name.</param>
        /// <param name="Value">Text value.</param>
        public async Task EditMeta(string Property, string Value)
        {
            await _lock.WaitAsync();
#if !DEBUG
            try
#endif
            {
                if (Property == string.Empty)
                {
                    return;
                }
                string command =
                    $"SELECT * FROM {nameof(Meta)} " +
                    $"WHERE {new Meta().Property} = @{new Meta().Property} " +
                    $"LIMIT 1";
                SqliteCommand SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new Meta().Property}", Property);
                SqliteDataReader reader = SqliteCommand.ExecuteReader();
                MetaItem metaItem = new();
                while (reader.Read())
                {
                    metaItem.Property = reader.GetString(0);
                    metaItem.Value = reader.GetString(1);
                    break;
                }
                if (metaItem.Property != string.Empty)
                {
                    command =
                        $"UPDATE {nameof(Meta)} " +
                        $"SET {new Meta().Value} = @{new Meta().Value} " +
                        $"WHERE {new Meta().Property} = @{new Meta().Property}";
                    SqliteCommand = new(command, dbConnection);
                    SqliteCommand.Parameters.AddWithValue($"@{new Meta().Value}", Value);
                    SqliteCommand.Parameters.AddWithValue($"@{new Meta().Property}", Property);
                    SqliteCommand.ExecuteNonQuery();
                }
                else
                {
                    command =
                        $"INSERT INTO {nameof(Meta)} " +
                        $"VALUES (" +
                        $"@{new Meta().Property}," +
                        $"@{new Meta().Value}" +
                        ")";
                    SqliteCommand = new(command, dbConnection);
                    SqliteCommand.Parameters.AddWithValue($"@{new Meta().Property}", Property);
                    SqliteCommand.Parameters.AddWithValue($"@{new Meta().Value}", Value);
                    SqliteCommand.ExecuteNonQuery();
                }
            }
#if !DEBUG
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
            }
            finally
#endif
            {
                _lock.Release();
            }
        }

        public async Task<string> GetMeta(string Property)
        {
            await _lock.WaitAsync();

            try
            {
                string result = string.Empty;
                string command =
                    $"SELECT * FROM {nameof(Meta)} " +
                    $"WHERE {new Meta().Property} = @{new Meta().Property} " +
                    $"LIMIT 1";
                SqliteCommand SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new Meta().Property}", Property);
                SqliteDataReader reader = SqliteCommand.ExecuteReader();
                while (reader.Read())
                {
                    result = reader.GetString(1);
                }
                return result;
            }
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
                return string.Empty;
            }
            finally
            {
                _lock.Release();
            }
        }

        // DataItem options
        public async Task<ObservableCollection<DataItem>> DataItemsFastSearchGetList(string searchString, bool searchDescription, bool searchSearchText, long ParentID = -1)
        {
            await _lock.WaitAsync();
#if !DEBUG
            try
#endif
            {
                List<long> itemIDs = [];
                HashSet<DataItem> dataItems = new(new DataItemEqualityComparer());
                if (searchString == string.Empty)
                {
                    if (ParentID != -1)
                    {
                        itemIDs = await DataItemGetChildDataItemIDs(ParentID);
                    }
                    string cmd =
                        $"SELECT * FROM {nameof(DataItems)} " +
                        (ParentID == -1
                        ? $""
                        : $"WHERE {new DataItems().ParentItemID} = @{new DataItems().ID}");
                    SqliteCommand sqlCmd = new(cmd, dbConnection);
                    SqliteDataReader reader1 = sqlCmd.ExecuteReader();
                    reader1.DataItemsAddDataItemsFromReader(ref dataItems, dbConnection, MessageManager);
                    return new ObservableCollection<DataItem>(dataItems);
                }
                // ParentID is -1 means search all items

                if (ParentID != -1)
                {
                    itemIDs = await DataItemGetChildDataItemIDs(ParentID);
                }
                string placeholders = string.Join(",", itemIDs.Select((_, i) => $"@p{i}"));

                string command =
                    $"SELECT * FROM {nameof(DataItemFastSearch)} " +
                    $"WHERE (" +
                    $"{new DataItemFastSearch().Title} MATCH @{new DataItemFastSearch().Title} " +
                    (searchDescription
                    ? $"OR {new DataItemFastSearch().Description} MATCH @{new DataItemFastSearch().Description} "
                    : $"") +
                    (searchSearchText
                    ? $"OR {new DataItemFastSearch().SearchText} MATCH @{new DataItemFastSearch().SearchText} "
                    : $"") +
                    $")"
                    + (ParentID == -1
                    ? $""
                    : $"AND {new DataItems().ID} IN ({placeholders})");
                SqliteCommand SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().Title}", searchString);
                if (searchDescription)
                {
                    SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().Description}", searchString);
                }
                if (searchSearchText)
                {
                    SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().SearchText}", searchString);
                }
                if (ParentID != default)
                {
                    int index = 0;
                    foreach (long ID in itemIDs)
                    {
                        SqliteCommand.Parameters.AddWithValue($"@p{index}", ID);
                        index++;
                    }
                }
                SqliteDataReader reader = SqliteCommand.ExecuteReader();
                reader.DataItemsAddDataItemsFromReader(ref dataItems, dbConnection, MessageManager);
                _lock.Release();
                return new ObservableCollection<DataItem>(dataItems);
            }
#if !DEBUG
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
                return [];
            }
            finally
            {
                _lock.Release();
            }
#endif
        }

        public async Task<ObservableCollection<DataItem>> DataItemsGetChildOfParentItemAsync(long parentDataItemID, bool useLock = true)
        {
            if (useLock)
            {
                await _lock.WaitAsync();
            }
            return await Task.Run(() =>
            {
                try
                {
                    HashSet<DataItem> result = new(new DataItemEqualityComparer());
                    StringBuilder commandBuilder = new();
                    commandBuilder.Append($"SELECT * FROM {nameof(DataItems)} ");
                    if (parentDataItemID != -1)
                    {
                        commandBuilder.Append($"WHERE {new DataItems().ParentItemID} = @{new DataItems().ParentItemID}");
                    }
                    SqliteCommand SqliteCommand = new(commandBuilder.ToString(), dbConnection);
                    if (parentDataItemID != -1)
                    {
                        SqliteCommand.Parameters.AddWithValue($"@{new DataItems().ParentItemID}", parentDataItemID);
                    }
                    SqliteDataReader SqliteDataReader = SqliteCommand.ExecuteReader();
                    SqliteDataReader.DataItemsAddDataItemsFromReader(ref result, dbConnection, MessageManager);
                    return new ObservableCollection<DataItem>(result);
                }
                catch (Exception ex)
                {
                    MessageManager.PushMessage(MessageType.Error, ex.Message);
                    return [];
                }
                finally
                {
                    if (useLock)
                    {
                        _lock.Release();
                    }
                }
            }
            );
        }

        private async IAsyncEnumerable<DataItem> DataItemsGetChildOfParentItemIterativeAsync(long parentDataItemID, SearchAndSortModeInfo sortMode)
        {
            SqliteDataReader? SqliteDataReader = null;
            try
            {
                StringBuilder commandBuilder = new();
                commandBuilder.Append($"SELECT * FROM {nameof(DataItems)} ");
                if (parentDataItemID != -1)
                {
                    commandBuilder.Append($"WHERE {new DataItems().ParentItemID} = @{new DataItems().ParentItemID} ");
                }
                commandBuilder.Append($"ORDER BY {Enum.GetName(sortMode.SortMode)} {Enum.GetName(sortMode.SortDirection)}");

                SqliteCommand SqliteCommand = new(commandBuilder.ToString(), dbConnection);
                if (parentDataItemID != -1)
                {
                    SqliteCommand.Parameters.AddWithValue($"@{new DataItems().ParentItemID}", parentDataItemID);
                }

                SqliteDataReader = SqliteCommand.ExecuteReader();
                await foreach (DataItem dataItem in SqliteDataReader.DataItemsAddDataItemsIterativeFromReader(dbConnection, MessageManager))
                {
                    yield return dataItem;
                }
            }
            finally
            {
                SqliteDataReader?.DisposeAsync();
            }
        }

        private bool DataItemVerifyExists(long ID)
        {
            {
                string command =
                    $"SELECT * FROM {nameof(DataItems)} " +
                    $"WHERE {nameof(DataItems.ID)} = @{nameof(DataItems.ID)} " +
                    $"LIMIT 1";
                SqliteCommand sqliteCommand = new(command, dbConnection);
                sqliteCommand.Parameters.AddWithValue($"@{nameof(DataItems.ID)}", ID);
                SqliteDataReader sqliteDataReader = sqliteCommand.ExecuteReader();
                while (sqliteDataReader.Read())
                {
                    return true;
                }
                return false;
            }
        }

        private long DataItemGetParentID(long ID)
        {
            string command =
                $"SELECT * FROM {nameof(DataItems)} " +
                $"WHERE {nameof(DataItems.ID)} = @{nameof(DataItems.ID)} " +
                $"LIMIT 1";
            SqliteCommand sqliteCommand = new(command, dbConnection);
            sqliteCommand.Parameters.AddWithValue($"@{nameof(DataItems.ID)}", ID);
            SqliteDataReader sqliteDataReader = sqliteCommand.ExecuteReader();
            while (sqliteDataReader.Read())
            {
                return sqliteDataReader.GetInt64(1);
            }
            return -1;
        }

        private string DataItemGetNameByID(long ID)
        {
            string subcommand =
                $"SELECT * FROM {nameof(DataItemFastSearch)} " +
                $"WHERE {new DataItemFastSearch().DataItemID} = @{new DataItemFastSearch().DataItemID} " +
                $"LIMIT 1";
            SqliteCommand SqliteCommand = new(subcommand, dbConnection);
            SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().DataItemID}", ID);
            SqliteDataReader SqliteDataReader = SqliteCommand.ExecuteReader();
            while (SqliteDataReader.Read())
            {
                return SqliteDataReader.GetString(1);
            }
            return string.Empty;
        }

        public async Task<DataItem> DataItemGetByID(long ID)
        {
            await _lock.WaitAsync();
            return await Task.Run(() =>
            {
                try
                {
                    HashSet<DataItem> result = new(new DataItemEqualityComparer());
                    string command =
                        $"SELECT * FROM {nameof(DataItems)} " +
                        $"WHERE {nameof(DataItems.ID)} = @{nameof(DataItems.ID)} " +
                        $"LIMIT 1";
                    SqliteCommand sqliteCommand = new(command, dbConnection);
                    sqliteCommand.Parameters.AddWithValue($"@{nameof(DataItems.ID)}", ID);
                    SqliteDataReader SqliteDataReader = sqliteCommand.ExecuteReader();
                    SqliteDataReader.DataItemsAddDataItemsFromReader(ref result, dbConnection, MessageManager);
                    if (result.Count == 0)
                    {
                        return new();
                    }
                    else
                    {
                        return result.First();
                    }
                }
                catch (Exception ex)
                {
                    MessageManager.PushMessage(MessageType.Error, ex.Message);
                    return new();
                }
                finally
                {
                    _lock.Release();
                }
            });
        }

        public async Task<List<DataItem>> DataItemsSearchViaSearchConditionsAsync(ObservableCollection<SearchCondition> searchConditions, DataItemSearchConfig dataItemSearchConfig)
        {
            await _lock.WaitAsync();
            return await Task.Run(async () =>
            {
                try
                {
                    List<DataItem> emptyResult = new();

                    // No conditions
                    if (searchConditions == null || searchConditions.Count == 0)
                    {
                        if (dataItemSearchConfig.SearchMode == SearchModeEnum.Layer)
                        {
                            emptyResult = (await DataItemsGetChildOfParentItemAsync(dataItemSearchConfig.ParentOrAncestorIDLimit, useLock: false)).ToList();
                            return emptyResult;
                        }
                        else if(dataItemSearchConfig.SearchMode == SearchModeEnum.Folder)
                        {
                            emptyResult = (await GetAllChildDataItemsDFS(dataItemSearchConfig.ParentOrAncestorIDLimit)).ToList();
                            return emptyResult;
                        }
                        else
                        {
                            HashSet<DataItem> results = [];
                            string cmd = $"SELECT * FROM {nameof(DataItems)} ";
                            SqliteCommand sqlCmd = new (cmd, dbConnection);
                            SqliteDataReader reader = sqlCmd.ExecuteReader();
                            reader.DataItemsAddDataItemsFromReader(ref results, dbConnection, MessageManager);
                            return results.ToList();
                        }
                    }

                    List<TextCondition> textConditions = searchConditions.OfType<TextCondition>().ToList();
                    List<TagCondition> tagConditions = searchConditions.OfType<TagCondition>().ToList();

                    // Dictionaries to count matches per DataItemID
                    Dictionary<long, long> textMatchCounts = new();
                    Dictionary<long, long> tagMatchCounts = new();

                    // Process text conditions
                    foreach (TextCondition tcond in textConditions)
                    {
                        HashSet<long> matchID = new();
                        // FTS5 table uses split words, this might be bad for words searching. Try to search using LIKE
                        StringBuilder _commandBuilder = new();
                        _commandBuilder.Append($"SELECT {new DataItemFastSearch().DataItemID} FROM {nameof(DataItemFastSearch)} ");
                        _commandBuilder.Append($"WHERE {new DataItemFastSearch().SearchText} MATCH @{new DataItemFastSearch().SearchText} ");
                        if (dataItemSearchConfig.SearchTitle)
                        {
                            _commandBuilder.Append($"OR {new DataItemFastSearch().Title} MATCH @{new DataItemFastSearch().Title} ");
                        }
                        if (dataItemSearchConfig.SearchDescription)
                        {
                            _commandBuilder.Append($"OR {new DataItemFastSearch().Description} MATCH @{new DataItemFastSearch().Description}");
                        }

                        SqliteCommand _cmd = new(_commandBuilder.ToString(), dbConnection);
                        _cmd.Parameters.AddWithValue($"@{new DataItemFastSearch().SearchText}", tcond.MainName);
                        if (dataItemSearchConfig.SearchTitle)
                        {
                            _cmd.Parameters.AddWithValue($"@{new DataItemFastSearch().Title}", tcond.MainName);
                        }
                        if (dataItemSearchConfig.SearchDescription)
                        {
                            _cmd.Parameters.AddWithValue($"@{new DataItemFastSearch().Description}", tcond.MainName);
                        }

                        using (SqliteDataReader reader = _cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                long id = reader.IsDBNull(0) ? -1 : reader.GetInt64(0);
                                if (id == -1) continue;
                                if (!textMatchCounts.ContainsKey(id)) textMatchCounts[id] = 0;
                                textMatchCounts[id]++;
                                matchID.Add(id);
                            }
                        }

                        _commandBuilder = new();
                        _commandBuilder.Append($"SELECT {new DataItemFastSearch().DataItemID} FROM {nameof(DataItemFastSearch)} ");
                        _commandBuilder.Append($"WHERE {new DataItemFastSearch().SearchText} LIKE @{new DataItemFastSearch().SearchText} ");
                        if (dataItemSearchConfig.SearchTitle)
                        {
                            _commandBuilder.Append($"OR {new DataItemFastSearch().Title} LIKE @{new DataItemFastSearch().Title} ");
                        }
                        if (dataItemSearchConfig.SearchDescription)
                        {
                            _commandBuilder.Append($"OR {new DataItemFastSearch().Description} LIKE @{new DataItemFastSearch().Description}");
                        }

                        _cmd = new(_commandBuilder.ToString(), dbConnection);
                        _cmd.Parameters.AddWithValue($"@{new DataItemFastSearch().SearchText}", $"%{tcond.MainName}%");
                        if (dataItemSearchConfig.SearchTitle)
                        {
                            _cmd.Parameters.AddWithValue($"@{new DataItemFastSearch().Title}", $"%{tcond.MainName}%");
                        }
                        if (dataItemSearchConfig.SearchDescription)
                        {
                            _cmd.Parameters.AddWithValue($"@{new DataItemFastSearch().Description}", $"%{tcond.MainName}%");
                        }

                        using (SqliteDataReader reader = _cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                long id = reader.IsDBNull(0) ? -1 : reader.GetInt64(0);
                                if (id == -1) continue;
                                if (matchID.Contains(id)) continue;
                                if (!textMatchCounts.ContainsKey(id))
                                {
                                    textMatchCounts[id] = 0;
                                    textMatchCounts[id]++;
                                }
                                matchID.Add(id);
                            }
                        }

                    }

                    // Process tag conditions
                    foreach (TagCondition tcond in tagConditions)
                    {
                        string _command =
                            $"SELECT DISTINCT {new ItemTags().ItemID} FROM {nameof(ItemTags)} " +
                            $"WHERE {new ItemTags().TagID} = @{new ItemTags().TagID}";
                        SqliteCommand _cmd = new(_command, dbConnection);
                        _cmd.Parameters.AddWithValue($"@{new ItemTags().TagID}", tcond.TagID);
                        using (SqliteDataReader reader = _cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                long id = reader.IsDBNull(0) ? -1 : reader.GetInt64(0);
                                if (id == -1) continue;
                                if (!tagMatchCounts.ContainsKey(id)) tagMatchCounts[id] = 0;
                                tagMatchCounts[id]++;
                            }
                        }
                    }

                    // Determine final set of IDs that satisfy all conditions
                    HashSet<long> finalIDs = new();

                    bool hasText = textConditions.Count > 0;
                    bool hasTag = tagConditions.Count > 0;

                    if (hasText && hasTag)
                    {
                        // IDs present in both dictionaries and meeting required counts
                        foreach (var kv in textMatchCounts)
                        {
                            long id = kv.Key;
                            if (kv.Value == textConditions.Count && tagMatchCounts.TryGetValue(id, out var tagCount) && tagCount == tagConditions.Count)
                            {
                                finalIDs.Add(id);
                            }
                        }
                    }
                    else if (hasText)
                    {
                        foreach (var kv in textMatchCounts)
                        {
                            if (kv.Value == textConditions.Count) finalIDs.Add(kv.Key);
                        }
                    }
                    else if (hasTag)
                    {
                        foreach (var kv in tagMatchCounts)
                        {
                            if (kv.Value == tagConditions.Count) finalIDs.Add(kv.Key);
                        }
                    }
                    else
                    {
                        // No recognizable conditions -> return empty
                        return emptyResult;
                    }

                    if (finalIDs.Count == 0) return emptyResult;

                    // Retrieve DataItems for the final IDs
                    // Build placeholders and parameters
                    var idList = finalIDs.ToList();
                    string[] paramNames = idList.Select((_, i) => $"@p{i}").ToArray();
                    string placeholders = string.Join(",", paramNames);

                    string selectCmd =
                        $"SELECT * FROM {nameof(DataItems)} " +
                        $"WHERE {new DataItems().ID} IN ({placeholders})";
                    SqliteCommand selectCommand = new(selectCmd, dbConnection);
                    for (int i = 0; i < idList.Count; i++)
                    {
                        selectCommand.Parameters.AddWithValue(paramNames[i], idList[i]);
                    }

                    HashSet<DataItem> dataItemsSet = new(new DataItemEqualityComparer());
                    using (SqliteDataReader reader = selectCommand.ExecuteReader())
                    {
                        // Reuse existing extension to populate DataItem objects
                        reader.DataItemsAddDataItemsFromReader(ref dataItemsSet, dbConnection, MessageManager);
                    }

                    if (dataItemSearchConfig.SearchMode == SearchModeEnum.Global)
                    {
                        return dataItemsSet.ToList();
                    }
                    else if (dataItemSearchConfig.SearchMode == SearchModeEnum.Layer)
                    {
                        return dataItemsSet.Where(x => x.ParentID == dataItemSearchConfig.ParentOrAncestorIDLimit).ToList();
                    }
                    else
                    {
                        HashSet<long> ancestors = new(await GetAllChildDataItemIDsDFS(dataItemSearchConfig.ParentOrAncestorIDLimit));
                        ancestors.Add(dataItemSearchConfig.ParentOrAncestorIDLimit);
                        return dataItemsSet.Where(x => ancestors.Contains(x.ParentID)).ToList();
                    }
                }
                catch (Exception ex)
                {
                    MessageManager.PushMessage(MessageType.Error, ex.Message);
                    return [];
                }
                finally
                {
                    _lock.Release();
                }
            });
        }

        private async Task DataItemGetChildSetsIterative(long tagID, List<long> EqualValueList, HashSet<long> matchIDHashSet)
        {
            Tag tag = await TagPoolGetTagByID(tagID);
            foreach (PropertyItem propertyItem in tag.PropertyItems)
            {
                if (propertyItem.IsContainsRelation)
                {
                    // Try get subItems.

                    // Find tag that contains this propertyItem in its logic chains
                    HashSet<long> tagIDsOfContainPropertyItem = [];
                    string seq = $"% % {propertyItem.ID}";
                    string command =
                        $"SELECT * FROM {nameof(TagData)} " +
                        $"WHERE {nameof(TagData.Seq)} LIKE @{nameof(TagData.Seq)} " +
                        $"AND {nameof(TagData.Type)} = @{nameof(TagData.Type)}";
                    SqliteCommand sqliteCommand = new(command, dbConnection);
                    sqliteCommand.Parameters.AddWithValue($"@{nameof(TagData.Seq)}", seq);
                    sqliteCommand.Parameters.AddWithValue($"@{nameof(TagData.Type)}", nameof(TagData.LogicChainItem));
                    SqliteDataReader reader = sqliteCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        tagIDsOfContainPropertyItem.Add(reader.GetInt64(1));
                    }


                    foreach (long id in tagIDsOfContainPropertyItem)
                    {
                        if (EqualValueList.Contains(id)) continue;
                        Tag _tag = await TagPoolGetTagByID(id);
                        if (matchIDHashSet.Contains(id)) return;
                        List<LogicChain> logicChains = _tag.LogicChains.Where(x => x.LogicChainData.Any(y => y.ParentPropertyItemID == propertyItem.ID)).ToList();
                        foreach (LogicChain logicChain in logicChains)
                        {
                            bool getStartNode = false;
                            foreach (LogicChainItem logicChainItem in logicChain.LogicChainData)
                            {
                                if (logicChainItem.ParentPropertyItemID == propertyItem.ID)
                                {
                                    getStartNode = true;
                                }
                                if (getStartNode)
                                {
                                    // Set condition haven't spread to the tag on this chain.
                                    if (!PropertyItemGetIsContainRelationByID(logicChainItem.ParentPropertyItemID))
                                        break;
                                    // Set relation still true.
                                    if (logicChainItem.OnChainTagID == _tag.ID
                                        || logicChainItem.OnChainTagID == -1)
                                    {
                                        matchIDHashSet.Add(id);
                                        EqualValueList.Add(id);
                                        await DataItemGetChildSetsIterative(id, EqualValueList, matchIDHashSet);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private async Task DataItemSearchHelperGetChildSets(HashSet<long> matchIDHashSet, List<long> EqualValueList, HashSet<long> tagIDsOfContainPropertyItem, long startPropertyID)
        {
            // Find property items having the property ID in their logic chains.
            foreach (long id in tagIDsOfContainPropertyItem)
            {
                if (matchIDHashSet.Contains(id)) continue;
                Tag _tag = await TagPoolGetTagByID(id);
                List<LogicChain> logicChains = _tag.LogicChains.Where(x => x.LogicChainData.Any(y => y.ParentPropertyItemID == startPropertyID)).ToList();
                foreach (LogicChain logicChain in logicChains)
                {
                    bool getStartNode = false;
                    foreach (LogicChainItem logicChainItem in logicChain.LogicChainData)
                    {
                        if (logicChainItem.ParentPropertyItemID == startPropertyID)
                        {
                            getStartNode = true;
                        }
                        if (getStartNode)
                        {
                            // Set condition haven't spread to the tag on this chain.
                            if (!PropertyItemGetIsContainRelationByID(logicChainItem.ParentPropertyItemID))
                                break;
                            // Set relation still true.
                            if (logicChainItem.OnChainTagID == _tag.ID
                                || logicChainItem.OnChainTagID == -1)
                            {

                                if (!matchIDHashSet.Contains(id))
                                {
                                    matchIDHashSet.Add(id);
                                    EqualValueList.Add(id);
                                    await DataItemGetChildSetsIterative(id, EqualValueList, matchIDHashSet);
                                }
                                else
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        public async IAsyncEnumerable<DataItem> DataItemSearchViaSearchConditionsIterativeAsync(ObservableCollection<SearchCondition> searchConditions, DataItemSearchConfig dataItemSearchConfig, SearchAndSortModeInfo SearchAndSortMode, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync();

            try
            {
                if (dataItemSearchConfig.SetSearch)
                {
                    foreach (SearchCondition condition in searchConditions)
                    {
                        if (condition is TagCondition tagCondition)
                        {
                            tagCondition.EqualValueList.Clear();
                            tagCondition.EqualValueList.Add(tagCondition.TagID);
                            // Get properties with "contains" relation.
                            Tag tag = await TagPoolGetTagByID(tagCondition.TagID);
                            // Examine properties in the tag
                            foreach (PropertyItem propertyItem in tag.PropertyItems)
                            {
                                if (propertyItem.IsContainsRelation)
                                {
                                    // Try get subItems.

                                    // Find tag that contains this propertyItem in its logic chains
                                    HashSet<long> tagIDsOfContainPropertyItem = [];
                                    string seq = $"% % {propertyItem.ID}";
                                    string command =
                                        $"SELECT * FROM {nameof(TagData)} " +
                                        $"WHERE {nameof(TagData.Seq)} LIKE @{nameof(TagData.Seq)} " +
                                        $"AND {nameof(TagData.Type)} = @{nameof(TagData.Type)}";
                                    SqliteCommand sqliteCommand = new(command, dbConnection);
                                    sqliteCommand.Parameters.AddWithValue($"@{nameof(TagData.Seq)}", seq);
                                    sqliteCommand.Parameters.AddWithValue($"@{nameof(TagData.Type)}", nameof(TagData.LogicChainItem));
                                    SqliteDataReader reader = sqliteCommand.ExecuteReader();
                                    while (reader.Read())
                                    {
                                        tagIDsOfContainPropertyItem.Add(reader.GetInt64(1));
                                    }

                                    // Try find the tags that is truly a subset.
                                    if (dataItemSearchConfig.FindSetStoredIndifferentTags)
                                    {
                                        HashSet<long> _matchIDs = [];
                                        await DataItemSearchHelperGetChildSets(_matchIDs, tagCondition.EqualValueList, tagIDsOfContainPropertyItem, propertyItem.ID);

                                    }
                                    else
                                    {
                                        foreach (long id in tagIDsOfContainPropertyItem)
                                        {
                                            if (tagCondition.EqualValueList.Contains(id)) continue;
                                            Tag _tag = await TagPoolGetTagByID(id);
                                            List<LogicChain> logicChains = _tag.LogicChains.Where(x => x.LogicChainData.Any(y => y.ParentPropertyItemID == propertyItem.ID)).ToList();
                                            bool isSubset = false;
                                            foreach (LogicChain logicChain in logicChains)
                                            {
                                                bool getStartNode = false;
                                                foreach (LogicChainItem logicChainItem in logicChain.LogicChainData)
                                                {
                                                    if (logicChainItem.ParentPropertyItemID == propertyItem.ID)
                                                    {
                                                        getStartNode = true;
                                                    }
                                                    if (getStartNode)
                                                    {
                                                        // Set condition haven't spread to the tag on this chain.
                                                        if (!PropertyItemGetIsContainRelationByID(logicChainItem.ParentPropertyItemID))
                                                            break;
                                                        // Set relation still true.
                                                        if (logicChainItem.OnChainTagID == _tag.ID
                                                            || logicChainItem.OnChainTagID == -1)
                                                        {
                                                            isSubset = true;
                                                            break;
                                                        }
                                                    }
                                                }
                                                if (isSubset)
                                                {
                                                    tagCondition.EqualValueList.Add(id);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                List<DataItem> emptyResult = [];

                //No conditions
                if (searchConditions == null || searchConditions.Count == 0)
                {
                    if (dataItemSearchConfig.SearchMode == SearchModeEnum.Layer)
                    {
                        await foreach (DataItem item in DataItemsGetChildOfParentItemIterativeAsync(dataItemSearchConfig.ParentOrAncestorIDLimit, SearchAndSortMode))
                        {
                            yield return item;
                        }
                    }
                    else if (dataItemSearchConfig.SearchMode == SearchModeEnum.Folder)
                    {
                        emptyResult = (await GetAllChildDataItemsDFS(dataItemSearchConfig.ParentOrAncestorIDLimit)).ToList();
                        List<DataItem> sortedResult = [];
                        if (SearchAndSortMode.SortDirection == SortDirectionEnum.ASC)
                        {
                            switch (SearchAndSortMode.SortMode)
                            {
                                case SortModeEnum.ID:
                                    sortedResult = emptyResult.OrderBy(x => x.ID).ToList();
                                    break;
                                case SortModeEnum.Title:
                                    sortedResult = emptyResult.OrderBy(x => x.Title).ToList();
                                    break;
                                case SortModeEnum.CreatedTime:
                                    sortedResult = emptyResult.OrderBy(x => x.CreatedTime).ToList();
                                    break;
                                case SortModeEnum.ModifiedTime:
                                    sortedResult = emptyResult.OrderBy(x => x.ModifiedTime).ToList();
                                    break;
                            }
                        }
                        else
                        {
                            switch (SearchAndSortMode.SortMode)
                            {
                                case SortModeEnum.ID:
                                    sortedResult = emptyResult.OrderByDescending(x => x.ID).ToList();
                                    break;
                                case SortModeEnum.Title:
                                    sortedResult = emptyResult.OrderByDescending(x => x.Title).ToList();
                                    break;
                                case SortModeEnum.CreatedTime:
                                    sortedResult = emptyResult.OrderByDescending(x => x.CreatedTime).ToList();
                                    break;
                                case SortModeEnum.ModifiedTime:
                                    sortedResult = emptyResult.OrderByDescending(x => x.ModifiedTime).ToList();
                                    break;
                            }
                        }

                        foreach (DataItem item in sortedResult)
                        {
                            yield return item;
                        }
                    }
                    else
                    {
                        HashSet<DataItem> results = [];
                        string cmd =
                            $"SELECT * FROM {nameof(DataItems)} " +
                            $"ORDER BY {Enum.GetName(SearchAndSortMode.SortMode)} {Enum.GetName(SearchAndSortMode.SortDirection)}";
                        SqliteCommand sqlCmd = new(cmd, dbConnection);
                        SqliteDataReader reader = sqlCmd.ExecuteReader();
                        await foreach (DataItem dataItem in reader.DataItemsAddDataItemsIterativeFromReader(dbConnection, MessageManager))
                        {
                            yield return dataItem;
                        }
                    }
                    yield break;
                }

                List<TextCondition> textConditions = searchConditions.OfType<TextCondition>().ToList();
                List<TagCondition> tagConditions = searchConditions.OfType<TagCondition>().ToList();

                //Dictionaries to count matches per DataItemID
                Dictionary<long, long> textMatchCounts = new();
                Dictionary<long, long> tagMatchCounts = new();

                HashSet<long> ancestors = new(await GetAllChildDataItemIDsDFS(dataItemSearchConfig.ParentOrAncestorIDLimit));
                ancestors.Add(dataItemSearchConfig.ParentOrAncestorIDLimit);

                bool hasText = textConditions.Count > 0;
                bool hasTag = tagConditions.Count > 0;

                if (hasText && !hasTag)
                {
                    //Process text conditions
                    foreach (TextCondition tcond in textConditions)
                    {
                        HashSet<long> matchID = [];
                        if (SearchAndSortMode.TextMatchMode == TextMatchModeEnum.Fast)
                        {
                            StringBuilder _commandBuilder = new();
                            _commandBuilder.Append($"SELECT DISTINCT {new DataItemFastSearch().DataItemID} FROM {nameof(DataItemFastSearch)} ");
                            _commandBuilder.Append($"WHERE {new DataItemFastSearch().SearchText} MATCH @{new DataItemFastSearch().SearchText} ");
                            if (dataItemSearchConfig.SearchTitle)
                            {
                                _commandBuilder.Append($"OR {new DataItemFastSearch().Title} MATCH @{new DataItemFastSearch().Title} ");
                            }
                            if (dataItemSearchConfig.SearchDescription)
                            {
                                _commandBuilder.Append($"OR {new DataItemFastSearch().Description} MATCH @{new DataItemFastSearch().Description} ");
                            }

                            SqliteCommand _cmd = new(_commandBuilder.ToString(), dbConnection);
                            _cmd.Parameters.AddWithValue($"@{new DataItemFastSearch().SearchText}", tcond.MainName);
                            if (dataItemSearchConfig.SearchTitle)
                            {
                                _cmd.Parameters.AddWithValue($"@{new DataItemFastSearch().Title}", tcond.MainName);
                            }
                            if (dataItemSearchConfig.SearchDescription)
                            {
                                _cmd.Parameters.AddWithValue($"@{new DataItemFastSearch().Description}", tcond.MainName);
                            }

                            using (SqliteDataReader reader = _cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    long id = reader.IsDBNull(0) ? -1 : reader.GetInt64(0);
                                    if (id == -1) continue;
                                    if (!textMatchCounts.ContainsKey(id)) textMatchCounts[id] = 0;
                                    textMatchCounts[id]++;
                                }
                            }
                        }
                        else
                        {
                            StringBuilder _commandBuilder = new();
                            _commandBuilder.Append($"SELECT DISTINCT {new DataItemFastSearch().DataItemID} FROM {nameof(DataItemFastSearch)} ");
                            _commandBuilder.Append($"WHERE {new DataItemFastSearch().SearchText} LIKE @{new DataItemFastSearch().SearchText} ");
                            if (dataItemSearchConfig.SearchTitle)
                            {
                                _commandBuilder.Append($"OR {new DataItemFastSearch().Title} LIKE @{new DataItemFastSearch().Title} ");
                            }
                            if (dataItemSearchConfig.SearchDescription)
                            {
                                _commandBuilder.Append($"OR {new DataItemFastSearch().Description} LIKE @{new DataItemFastSearch().Description} ");
                            }

                            SqliteCommand _cmd = new(_commandBuilder.ToString(), dbConnection);
                            _cmd.Parameters.AddWithValue($"@{new DataItemFastSearch().SearchText}", $"%{tcond.MainName}%");
                            if (dataItemSearchConfig.SearchTitle)
                            {
                                _cmd.Parameters.AddWithValue($"@{new DataItemFastSearch().Title}", $"%{tcond.MainName}%");
                            }
                            if (dataItemSearchConfig.SearchDescription)
                            {
                                _cmd.Parameters.AddWithValue($"@{new DataItemFastSearch().Description}", $"%{tcond.MainName}%");
                            }

                            using (SqliteDataReader reader = _cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    long id = reader.IsDBNull(0) ? -1 : reader.GetInt64(0);
                                    if (id == -1) continue;
                                    if (matchID.Contains(id)) continue;
                                    if (!textMatchCounts.ContainsKey(id))
                                    {
                                        textMatchCounts[id] = 0;
                                        textMatchCounts[id]++;
                                    }
                                }
                            }
                        }
                        foreach (var kv in textMatchCounts)
                        {
                            if (kv.Value == textConditions.Count) matchID.Add(kv.Key);
                        }

                        if (dataItemSearchConfig.SearchMode == SearchModeEnum.Global)
                            foreach (long id in matchID)
                            {
                                yield return await DataItemGetByID(id);
                            }
                        else if (dataItemSearchConfig.SearchMode == SearchModeEnum.Layer)
                        {
                            foreach (long id in matchID)
                            {
                                long parentID = DataItemGetParentID(id);
                                if (parentID != -1 && parentID == dataItemSearchConfig.ParentOrAncestorIDLimit)
                                {
                                    yield return await DataItemGetByID(id);
                                }
                            }
                        }
                        else
                        {
                            foreach (long id in matchID)
                            {
                                long parentID = DataItemGetParentID(id);
                                if (ancestors.Contains(parentID))
                                {
                                    yield return await DataItemGetByID(id);
                                }
                            }
                        }
                        yield break;
                    }
                }

                //Process tag conditions
                foreach (TagCondition tcond in tagConditions)
                {
                    if (dataItemSearchConfig.SetSearch)
                    {
                        StringBuilder command = new();
                        command.Append($"SELECT DISTINCT {new ItemTags().ItemID} FROM {nameof(ItemTags)} ");
                        string[] tagIDInListPlaceHolders = tcond.EqualValueList.Select((_, i) => $"@p{i}").ToArray();
                        string tagIDInListPlaceHolderText = string.Join(",", tagIDInListPlaceHolders);
                        command.Append($"WHERE {nameof(ItemTags.TagID)} IN ({tagIDInListPlaceHolderText})");
                        SqliteCommand sqliteCommand = new(command.ToString(), dbConnection);
                        if (tagIDInListPlaceHolders.Count() > 0)
                        {
                            for (int i = 0; i < tagIDInListPlaceHolders.Count(); i++)
                                sqliteCommand.Parameters.AddWithValue(tagIDInListPlaceHolders[i], tcond.EqualValueList[i]);
                            SqliteDataReader reader = sqliteCommand.ExecuteReader();
                            while (reader.Read())
                            {
                                long id = reader.IsDBNull(0) ? -1 : reader.GetInt64(0);
                                if (id == -1) continue;
                                if (!tagMatchCounts.ContainsKey(id)) tagMatchCounts[id] = 0;
                                tagMatchCounts[id]++;
                            }
                        }
                    }

                    else
                    {
                        string _command =
                            $"SELECT DISTINCT {new ItemTags().ItemID} FROM {nameof(ItemTags)} " +
                            $"WHERE {new ItemTags().TagID} = @{new ItemTags().TagID}";
                        SqliteCommand _cmd = new(_command, dbConnection);
                        _cmd.Parameters.AddWithValue($"@{new ItemTags().TagID}", tcond.TagID);
                        using (SqliteDataReader reader = _cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                long id = reader.IsDBNull(0) ? -1 : reader.GetInt64(0);
                                if (id == -1) continue;
                                if (!tagMatchCounts.ContainsKey(id)) tagMatchCounts[id] = 0;
                                tagMatchCounts[id]++;
                            }
                        }
                    }
                }

                //Determine final set of IDs that satisfy all conditions
                HashSet<long> finalIDs = new();

                if (hasText)
                {
                    if (hasTag)
                    {
                        List<long> itemIDsFitTagConditions = [];
                        foreach (var kv in tagMatchCounts)
                        {
                            if (kv.Value == tagConditions.Count) itemIDsFitTagConditions.Add(kv.Key);
                        }
                        string[] itemIDPlaceholders = itemIDsFitTagConditions.Select((_, i) => $"@p{i}").ToArray();
                    }
                }

                if (hasText)
                {
                    List<long> itemIDsFitTagConditions = [];
                    foreach (var kv in tagMatchCounts)
                    {
                        if (kv.Value == tagConditions.Count) itemIDsFitTagConditions.Add(kv.Key);
                    }
                    string[] itemIDPlaceholders = itemIDsFitTagConditions.Select((_, i) => $"@p{i}").ToArray();
                    string itemIDPlaceholderText = string.Join(",", itemIDPlaceholders);

                    foreach (TextCondition tcond in textConditions)
                    {
                        HashSet<long> matchID = [];
                        if (SearchAndSortMode.TextMatchMode == TextMatchModeEnum.Fast)
                        {
                            StringBuilder _commandBuilder = new();
                            _commandBuilder.Append($"SELECT {new DataItemFastSearch().DataItemID} FROM {nameof(DataItemFastSearch)} ");
                            _commandBuilder.Append($"WHERE {new DataItemFastSearch().SearchText} MATCH @{new DataItemFastSearch().SearchText} ");
                            if (dataItemSearchConfig.SearchTitle)
                            {
                                _commandBuilder.Append($"OR {new DataItemFastSearch().Title} MATCH @{new DataItemFastSearch().Title} ");
                            }
                            if (dataItemSearchConfig.SearchDescription)
                            {
                                _commandBuilder.Append($"OR {new DataItemFastSearch().Description} MATCH @{new DataItemFastSearch().Description} ");
                            }
                            if (hasTag)
                            {
                                _commandBuilder.Append($"AND {nameof(DataItemFastSearch.DataItemID)} IN ({itemIDPlaceholderText})");
                            }

                            SqliteCommand _cmd = new(_commandBuilder.ToString(), dbConnection);
                            _cmd.Parameters.AddWithValue($"@{new DataItemFastSearch().SearchText}", tcond.MainName);
                            if (dataItemSearchConfig.SearchTitle)
                            {
                                _cmd.Parameters.AddWithValue($"@{new DataItemFastSearch().Title}", tcond.MainName);
                            }
                            if (dataItemSearchConfig.SearchDescription)
                            {
                                _cmd.Parameters.AddWithValue($"@{new DataItemFastSearch().Description}", tcond.MainName);
                            }
                            if (hasTag)
                            {
                                if (itemIDPlaceholders.Count() > 0)
                                {
                                    for (int i = 0; i < itemIDPlaceholders.Count(); i++)
                                        _cmd.Parameters.AddWithValue(itemIDPlaceholders[i], itemIDsFitTagConditions[i]);
                                }
                            }

                            using (SqliteDataReader subReader = _cmd.ExecuteReader())
                            {
                                while (subReader.Read())
                                {
                                    long id = subReader.IsDBNull(0) ? -1 : subReader.GetInt64(0);
                                    if (id == -1) continue;
                                    if (!textMatchCounts.ContainsKey(id)) textMatchCounts[id] = 0;
                                    textMatchCounts[id]++;
                                    if (!matchID.Contains(id))
                                    {
                                        matchID.Add(id);
                                    }
                                }
                            }
                        }
                        else
                        {
                            StringBuilder _commandBuilder = new();
                            _commandBuilder.Append($"SELECT {new DataItemFastSearch().DataItemID} FROM {nameof(DataItemFastSearch)} ");
                            _commandBuilder.Append($"WHERE {new DataItemFastSearch().SearchText} LIKE @{new DataItemFastSearch().SearchText} ");
                            if (dataItemSearchConfig.SearchTitle)
                            {
                                _commandBuilder.Append($"OR {new DataItemFastSearch().Title} LIKE @{new DataItemFastSearch().Title} ");
                            }
                            if (dataItemSearchConfig.SearchDescription)
                            {
                                _commandBuilder.Append($"OR {new DataItemFastSearch().Description} LIKE @{new DataItemFastSearch().Description} ");
                            }
                            if (hasTag)
                            {
                                _commandBuilder.Append($"AND {nameof(DataItemFastSearch.DataItemID)} IN ({itemIDPlaceholderText})");
                            }

                            SqliteCommand _cmd = new(_commandBuilder.ToString(), dbConnection);
                            _cmd.Parameters.AddWithValue($"@{new DataItemFastSearch().SearchText}", $"%{tcond.MainName}%");
                            if (dataItemSearchConfig.SearchTitle)
                            {
                                _cmd.Parameters.AddWithValue($"@{new DataItemFastSearch().Title}", $"%{tcond.MainName}%");
                            }
                            if (dataItemSearchConfig.SearchDescription)
                            {
                                _cmd.Parameters.AddWithValue($"@{new DataItemFastSearch().Description}", $"%{tcond.MainName}%");
                            }
                            if (hasTag)
                            {
                                if (itemIDPlaceholders.Count() > 0)
                                {
                                    for (int i = 0; i < itemIDPlaceholders.Count(); i++)
                                        _cmd.Parameters.AddWithValue(itemIDPlaceholders[i], itemIDsFitTagConditions[i]);
                                }
                            }

                            using (SqliteDataReader subReader = _cmd.ExecuteReader())
                            {
                                while (subReader.Read())
                                {
                                    long id = subReader.IsDBNull(0) ? -1 : subReader.GetInt64(0);
                                    if (id == -1) continue;
                                    if (matchID.Contains(id)) continue;
                                    if (!textMatchCounts.ContainsKey(id))
                                    {
                                        textMatchCounts[id] = 0;
                                        textMatchCounts[id]++;
                                    }
                                    if (!matchID.Contains(id))
                                    {
                                        matchID.Add(id);
                                    }
                                }
                            }
                        }
                    }

                    HashSet<long> textMatchID = [];
                    foreach (var kv in textMatchCounts)
                    {
                        if (kv.Value == textConditions.Count) textMatchID.Add(kv.Key);
                    }
                    List<long> pathMatchID = [];

                    if (dataItemSearchConfig.SearchMode == SearchModeEnum.Global)
                        foreach (long id in textMatchID)
                        {
                            pathMatchID.Add(id);
                        }
                    else if (dataItemSearchConfig.SearchMode == SearchModeEnum.Layer)
                    {
                        foreach (long id in textMatchID)
                        {
                            long parentID = DataItemGetParentID(id);
                            if (parentID != -1 && parentID == dataItemSearchConfig.ParentOrAncestorIDLimit)
                            {
                                pathMatchID.Add(id);
                            }
                        }
                    }
                    else
                    {
                        foreach (long id in textMatchID)
                        {
                            long parentID = DataItemGetParentID(id);
                            if (ancestors.Contains(parentID))
                            {
                                pathMatchID.Add(id);
                            }
                        }
                    }

                    string[] pathMatchIDPlaceHolders = pathMatchID.Select((_, i) => $"@p{i}").ToArray();
                    string pathMatchIDPlaceHolderText = string.Join(",", pathMatchIDPlaceHolders);

                    string selectCmd =
                       $"SELECT * FROM {nameof(DataItems)} " +
                       $"WHERE {nameof(DataItems.ID)} IN ({pathMatchIDPlaceHolderText}) " +
                       $"ORDER BY {Enum.GetName(SearchAndSortMode.SortMode)} {Enum.GetName(SearchAndSortMode.SortDirection)}";
                    SqliteCommand sqliteCommand = new(selectCmd, dbConnection);
                    if (pathMatchIDPlaceHolders.Count() > 0)
                    {
                        for (int i = 0; i < pathMatchIDPlaceHolders.Count(); i++)
                            sqliteCommand.Parameters.AddWithValue(pathMatchIDPlaceHolders[i], pathMatchID[i]);
                        SqliteDataReader reader = sqliteCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            yield return await DataItemGetByID(reader.GetInt64(0));
                        }
                        yield break;
                    }
                }
                else
                {
                    List<long> itemIDsFitTagConditions = [];
                    foreach (var kv in tagMatchCounts)
                    {
                        if (kv.Value == tagConditions.Count) itemIDsFitTagConditions.Add(kv.Key);
                    }
                    string[] itemIDPlaceholders = itemIDsFitTagConditions.Select((_, i) => $"@p{i}").ToArray();
                    string itemIDPlaceholderText = string.Join(",", itemIDPlaceholders);

                    List<long> pathMatchID = [];

                    if (dataItemSearchConfig.SearchMode == SearchModeEnum.Global)
                        foreach (long id in itemIDsFitTagConditions)
                        {
                            pathMatchID.Add(id);
                        }
                    else if (dataItemSearchConfig.SearchMode == SearchModeEnum.Layer)
                    {
                        foreach (long id in itemIDsFitTagConditions)
                        {
                            long parentID = DataItemGetParentID(id);
                            if (parentID != -1 && parentID == dataItemSearchConfig.ParentOrAncestorIDLimit)
                            {
                                pathMatchID.Add(id);
                            }
                        }
                    }
                    else
                    {
                        foreach (long id in itemIDsFitTagConditions)
                        {
                            long parentID = DataItemGetParentID(id);
                            if (ancestors.Contains(parentID))
                            {
                                pathMatchID.Add(id);
                            }
                        }
                    }

                    string[] pathMatchIDPlaceHolders = pathMatchID.Select((_, i) => $"@p{i}").ToArray();
                    string pathMatchIDPlaceHolderText = string.Join(",", pathMatchIDPlaceHolders);

                    StringBuilder selectCmd = new();
                    selectCmd.Append($"SELECT * FROM {nameof(DataItems)} ");
                    if (hasTag)
                        selectCmd.Append($"WHERE {new DataItems().ID} IN ({pathMatchIDPlaceHolderText}) ");
                    selectCmd.Append($"ORDER BY {Enum.GetName(SearchAndSortMode.SortMode)} {Enum.GetName(SearchAndSortMode.SortDirection)}");
                    SqliteCommand sqliteCommand = new(selectCmd.ToString(), dbConnection);
                    if (pathMatchIDPlaceHolders.Count() > 0)
                    {
                        for (int i = 0; i < itemIDPlaceholders.Count(); i++)
                            sqliteCommand.Parameters.AddWithValue(pathMatchIDPlaceHolders[i], pathMatchID[i]);
                        SqliteDataReader reader = sqliteCommand.ExecuteReader();
                        while (reader.Read())
                        {
                            yield return await DataItemGetByID(reader.GetInt64(0));
                        }
                    }
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Add new data item.
        /// </summary>
        /// <param name="dataItem">Data item instance.</param>
        public async void DataItemAdd(DataItem dataItem)
        {
            await _lock.WaitAsync();
#if !DEBUG 
            try
#endif
            {
                string command =
                    $"INSERT INTO {nameof(DataItems)} (" +
                    $"{new DataItems().ParentItemID}," +
                    $"{new DataItems().Type}," +
                    $"{new DataItems().CreatedTime}," +
                    $"{new DataItems().ModifiedTime}," +
                    $"{new DataItems().Title}" +
                    $")" +
                    $"VALUES (" +
                    $"@{new DataItems().ParentItemID}," +
                    $"@{new DataItems().Type}," +
                    $"@{new DataItems().CreatedTime}," +
                    $"@{new DataItems().ModifiedTime}," +
                    $"@{new DataItems().Title}" +
                    $")";
                SqliteCommand SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItems().ParentItemID}", dataItem.ParentID);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItems().Type}", dataItem.ItemType);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItems().CreatedTime}", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                SqliteCommand.Parameters.AddWithValue($"@{new DataItems().ModifiedTime}", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                SqliteCommand.Parameters.AddWithValue($"@{new DataItems().Title}", dataItem.Title);
                SqliteCommand.ExecuteNonQuery();

                // Get data item ID
                command = "SELECT last_insert_rowid()";
                SqliteCommand = new(command, dbConnection);
                dataItem.ID = (long)(SqliteCommand.ExecuteScalar() ?? -1);

                await DataItemFastSearchClearRecordsOfDataItemID(dataItem.ID);

                command =
                    $"INSERT INTO {nameof(DataItemFastSearch)} (" +
                    $"{new DataItemFastSearch().DataItemID}," +
                    $"{new DataItemFastSearch().Title}," +
                    $"{new DataItemFastSearch().Description}," +
                    $"{new DataItemFastSearch().RefPath}," +
                    $"{new DataItemFastSearch().SearchText}" +
                    $") " +
                    $"VALUES (" +
                    $"@{new DataItemFastSearch().DataItemID}," +
                    $"@{new DataItemFastSearch().Title}," +
                    $"@{new DataItemFastSearch().Description}," +
                    $"@{new DataItemFastSearch().RefPath}," +
                    $"@{new DataItemFastSearch().SearchText}" +
                    $")";
                SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().DataItemID}", dataItem.ID);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().Title}", dataItem.Title);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().Description}", dataItem.Description);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().RefPath}", dataItem.RefPath);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().SearchText}", dataItem.SearchText);
                SqliteCommand.ExecuteNonQuery();

                // Add tags
                dbConnection.SaveDataItemTags(dataItem.ItemTags, dataItem.ID);

                // Remove entire searchStrings
                // Add searchStrings
                string filePath = dataItem.RefPath;
                if (filePath.IsValidFilePath())
                {
                    if (filePath.IsDocumentFile())
                    {
                        await DataItemFastSearchStoreDocumentSearchText(dataItem);
                    }
                }
            }
#if !DEBUG
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
            }
            finally
#endif
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Used after data item finished editing.
        /// </summary>
        /// <param name="dataItem">Data item instance.</param>
        /// <param name="UpdateTags">If tags not edited, set it false for better efficiency.</param>
        public async void DataItemUpdate(DataItem dataItem, bool UpdateTags = true)
        {
            await _lock.WaitAsync();
#if !DEBUG
            try
#endif
            {
                string command =
                    $"UPDATE {nameof(DataItems)} " +
                    $"SET " +
                    $"{new DataItems().ParentItemID} = @{new DataItems().ParentItemID}," +
                    $"{new DataItems().Type} = @{new DataItems().Type}," +
                    $"{new DataItems().ModifiedTime} = @{new DataItems().ModifiedTime}," +
                    $"{new DataItems().Title} = @{new DataItems().Title} " +
                    $"WHERE {new DataItems().ID} = @{new DataItems().ID}";
                SqliteCommand SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItems().ParentItemID}", dataItem.ParentID);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItems().Type}", dataItem.ItemType);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItems().ModifiedTime}", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                SqliteCommand.Parameters.AddWithValue($"@{new DataItems().ID}", dataItem.ID);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItems().Title}", dataItem.Title);
                SqliteCommand.ExecuteNonQuery();

                // Update tags
                if (UpdateTags)
                {
                    // Delete tags related
                    command =
                        $"DELETE FROM {nameof(ItemTags)} " +
                        $"WHERE {new ItemTags().ItemID} = @{new ItemTags().ItemID}";
                    SqliteCommand = new(command, dbConnection);
                    SqliteCommand.Parameters.AddWithValue($"@{new ItemTags().ItemID}", dataItem.ID);
                    SqliteCommand.ExecuteNonQuery();

                    // Re-add tags
                    dbConnection.SaveDataItemTags(dataItem.ItemTags, dataItem.ID);
                }

                // Remove entire searchStrings
                await DataItemFastSearchClearRecordsOfDataItemID(dataItem.ID);

                command =
                    $"INSERT INTO {nameof(DataItemFastSearch)} (" +
                    $"{new DataItemFastSearch().DataItemID}," +
                    $"{new DataItemFastSearch().Title}," +
                    $"{new DataItemFastSearch().Description}," +
                    $"{new DataItemFastSearch().RefPath}," +
                    $"{new DataItemFastSearch().SearchText}" +
                    $")" +
                    $"VALUES (" +
                    $"@{new DataItemFastSearch().DataItemID}," +
                    $"@{new DataItemFastSearch().Title}," +
                    $"@{new DataItemFastSearch().Description}," +
                    $"@{new DataItemFastSearch().RefPath}," +
                    $"@{new DataItemFastSearch().SearchText}" +
                    $")";
                SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().DataItemID}", dataItem.ID);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().Title}", dataItem.Title);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().Description}", dataItem.Description);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().RefPath}", dataItem.RefPath);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().SearchText}", dataItem.SearchText);
                SqliteCommand.ExecuteNonQuery();

                // Add searchStrings
                string filePath = dataItem.RefPath;
                if (filePath.IsValidFilePath())
                {
                    if (filePath.IsDocumentFile())
                    {
                        await DataItemFastSearchStoreDocumentSearchText(dataItem);
                    }
                }
            }
#if !DEBUG
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
            }
            finally
#endif
            {
                _lock.Release();
            }
        }
        
        public async Task DataItemBatchDelete(List<long> rootIDs, bool removeChilds)
        {
            // Do not add lock on this function!

            HashSet<long> allItemsToDelete = [];

            if (removeChilds)
            {
                foreach (long rootID in rootIDs)
                {
                    var allChildItems = await GetAllChildDataItemIDsDFS(rootID);
                    foreach (long childID in allChildItems)
                    {
                        allItemsToDelete.Add(childID);
                    }
                }
            }
            foreach (long rootID in rootIDs)
            {
                allItemsToDelete.Add(rootID);
            }
            foreach (long childID in allItemsToDelete)
            {
                await DataItemRemoveSingle(childID);
            }
        }

        private async Task<List<long>> GetAllChildDataItemIDsDFS(long parentId)
        {
            var allChildIds = new List<long>();
            var queue = new Queue<long>();
            queue.Enqueue(parentId);

            while (queue.Count > 0)
            {
                var currentId = queue.Dequeue();
                var childIds = await DataItemGetChildDataItemIDs(currentId);

                if (childIds == null || childIds.Count == 0)
                    continue;

                allChildIds.AddRange(childIds);

                foreach (var childId in childIds)
                {
                    queue.Enqueue(childId);
                }
            }

            return allChildIds;
        }

        public async Task<ObservableCollection<DataItem>> GetAllChildDataItemsDFS(long parentID)
        {
            var allChildIds = new List<DataItem>();
            var queue = new Queue<DataItem>();
            queue.Enqueue(new DataItem() { ID = parentID});

            while (queue.Count > 0)
            {
                var currentItem = queue.Dequeue();
                var childItems = await DataItemsGetChildOfParentItemAsync(currentItem.ID, false);

                if (childItems == null || childItems.Count == 0)
                    continue;

                allChildIds.AddRange(childItems);

                foreach (var childId in childItems)
                {
                    queue.Enqueue(childId);
                }
            }

            return new ObservableCollection<DataItem>(allChildIds);
        }

        public async Task<ObservableCollection<ExplorerFolder>> GetDataItemPath(long id, bool useLock = true)
        {
            if (useLock)
            {
                await _lock.WaitAsync();
            }

            try
            {
                HashSet<long> verifier = [];
                ObservableCollection<ExplorerFolder> result = [];

                long currentID = id;
                while (true)
                {
                    string command =
                        $"SELECT * FROM {nameof(DataItems)} " +
                        $"WHERE {nameof(DataItems.ID)} = @{nameof(DataItems.ID)} " +
                        $"LIMIT 1";
                    SqliteCommand sqliteCommand = new(command, dbConnection);
                    sqliteCommand.Parameters.AddWithValue($"@{nameof(DataItems.ID)}", currentID);
                    SqliteDataReader reader =
                        sqliteCommand.ExecuteReader();
                    bool hasData = false;
                    while (reader.Read())
                    {
                        hasData = true;
                        long parentID = reader.GetInt64(1);
                        if (!verifier.Contains(currentID))
                        {
                            // In searching
                            verifier.Add(currentID);
                            string dataItemTitle = reader.GetString(5); // DataItemGetNameByID(currentID);

                            if (DataItemVerifyExists(currentID))
                            {
                                result.Insert(0, new ExplorerFolder() { ID = currentID, Name = dataItemTitle });
                            }
                            else
                            {
                                result.Insert(0, new ExplorerFolder() { ID = currentID, Name = "{No Data}" });
                                return result;
                            }
                        }
                        // Path looped
                        else
                        {
                            result.Insert(0, new ExplorerFolder() { ID = -1, Name = "*Wild" });
                            return result;
                        }
                        if (parentID == 0)
                        {
                            // Got root
                            result.Insert(0, new ExplorerFolder() { ID = 0, Name = "Root" });
                            return result;
                        }

                        currentID = parentID;
                    }
                    // No item
                    if (!hasData)
                    {
                        result.Insert(0, new ExplorerFolder() { ID = -1, Name = "*Wild" });
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
                return [];
            }
            finally
            {
                if (useLock)
                {
                    _lock.Release();
                }
            }
        }

        /// <summary>
        /// Remove data item with specific ID.
        /// </summary>
        /// <param name="ID">Specific ID></param>
        public async Task DataItemRemoveSingle(long ID)
        {
            await _lock.WaitAsync();
#if !DEBUG
            try
#endif
            {
                // Delete data from table DataItems
                string command =
                    $"DELETE FROM {nameof(DataItems)} " +
                    $"WHERE {new DataItems().ID} = @{new DataItems().ID}";
                SqliteCommand SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItems().ID}", ID);
                SqliteCommand.ExecuteNonQuery();

                command =
                    $"DELETE FROM {nameof(DataItemFastSearch)} " +
                    $"WHERE {new DataItemFastSearch().DataItemID} = @{new DataItemFastSearch().DataItemID}";
                SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().DataItemID}", ID);
                SqliteCommand.ExecuteNonQuery();

                // Delete tags related
                command =
                    $"DELETE FROM {nameof(ItemTags)} " +
                    $"WHERE {new ItemTags().ItemID} = @{new ItemTags().ItemID}";
                SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new ItemTags().ItemID}", ID);
                SqliteCommand.ExecuteNonQuery();
            }
#if !DEBUG
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
            }
            finally
#endif
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Set a parent for a dataitem.
        /// </summary>
        /// <param name="dataItemID">ID of data item being moved.</param>
        /// <param name="newParentID">ID of target parent.</param>
        public async Task DataItemMoveTo(long dataItemID, int newParentID)
        {
            await _lock.WaitAsync();
#if !DEBUG
            try
#endif
            {
                string command =
                    $"UPDATE {nameof(DataItems)} " +
                    $"SET {new DataItems().ParentItemID} = @{new DataItems().ParentItemID} " +
                    $"WHERE {new DataItems().ID} = @{new DataItems().ID}";
                SqliteCommand SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItems().ParentItemID}", newParentID);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItems().ID}", dataItemID);
                SqliteCommand.ExecuteNonQuery();

                // No need to update fast search table, as it doesn't contain parent info.
            }
#if !DEBUG
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
            }
            finally
#endif
            {
                _lock.Release();
            }
        }

        public async Task DataItemCopyTo(DataItem sourceDataItem, int insertParentID)
        {
            await _lock.WaitAsync();
#if !DEBUG
            try
#endif
            {
                string command =
                    $"INSERT INTO {nameof(DataItems)} (" +
                    $"{new DataItems().ParentItemID}," +
                    $"{new DataItems().Type}," +
                    $"{new DataItems().CreatedTime}," +
                    $"{new DataItems().ModifiedTime}," +
                    $"{new DataItems().Title}" +
                    $")" +
                    $"VALUES (" +
                    $"@{new DataItems().ParentItemID}," +
                    $"@{new DataItems().Type}," +
                    $"@{new DataItems().CreatedTime}," +
                    $"@{new DataItems().ModifiedTime}," +
                    $"@{new DataItems().Title}" +
                    ")";

                SqliteCommand SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItems().ParentItemID}", insertParentID);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItems().Type}", sourceDataItem.ItemType);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItems().CreatedTime}", sourceDataItem.CreatedTime);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItems().ModifiedTime}", sourceDataItem.ModifiedTime);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItems().Title}", sourceDataItem.Title);
                SqliteCommand.ExecuteNonQuery();

                // Get data item ID
                command = "SELECT last_insert_rowid()";
                SqliteCommand = new(command, dbConnection);
                long newDataItemID = (long)(SqliteCommand.ExecuteScalar() ?? -1);
                if (newDataItemID == -1) return;

                command =
                    $"INSERT INTO {nameof(DataItemFastSearch)} (" +
                    $"{new DataItemFastSearch().DataItemID}," +
                    $"{new DataItemFastSearch().Title}," +
                    $"{new DataItemFastSearch().Description}," +
                    $"{new DataItemFastSearch().RefPath}," +
                    $"{new DataItemFastSearch().SearchText}" +
                    $")" +
                    $"VALUES (" +
                    $"@{new DataItemFastSearch().DataItemID}," +
                    $"@{new DataItemFastSearch().Title}," +
                    $"@{new DataItemFastSearch().Description}," +
                    $"@{new DataItemFastSearch().RefPath}," +
                    $"@{new DataItemFastSearch().SearchText}" +
                    ")";
                SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().DataItemID}", newDataItemID);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().Title}", sourceDataItem.Title);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().Description}", sourceDataItem.Description);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().RefPath}", sourceDataItem.RefPath);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().SearchText}", sourceDataItem.SearchText);

                // Add tags
                dbConnection.SaveDataItemTags(sourceDataItem.ItemTags, sourceDataItem.ID);
            }
#if !DEBUG
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
            }
            finally
#endif
            {
                _lock.Release();
            }
        }
        private async Task<List<long>> DataItemGetChildDataItemIDs(long ID)
        {
            try
            {
                List<long> IDs = [];
                string command =
                    $"SELECT * FROM {nameof(DataItems)} " +
                    $"WHERE {new DataItems().ParentItemID} = @{new DataItems().ParentItemID}";
                SqliteCommand SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItems().ParentItemID}", ID);
                SqliteDataReader reader = SqliteCommand.ExecuteReader();
                while (reader.Read())
                {
                    IDs.Add(reader.GetInt64(0));
                }
                return IDs;
            }
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
                return [];
            }
        }

        // DataItemFastSearch options
        public async Task DataItemFastSearchStoreDocumentSearchText(DataItem dataItem)
        {
            await _lock.WaitAsync();
            try
            {
                bool IsFirstRecord = true;
                await foreach (string searchText in dataItem.RefPath.GetDocumentFileTextAsync())
                {
                    await DataItemFastSearchStoreSearchText(dataItem, searchText, IsFirstRecord);
                    IsFirstRecord = false;
                }
            }
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task DataItemFastSearchClearRecordsOfDataItemID(long dataItemID)
        {
            try
            {
                string command =
                    $"DELETE FROM {nameof(DataItemFastSearch)} " +
                    $"WHERE {new DataItemFastSearch().DataItemID} = @{new DataItemFastSearch().DataItemID}";
                SqliteCommand SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().DataItemID}", dataItemID);
                SqliteCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
            }
        }

        private async Task DataItemFastSearchStoreSearchText(DataItem dataItem, string searchText, bool IsFirstRecord = false)
        {
            try
            {
                string command =
                    $"INSERT INTO {nameof(DataItemFastSearch)} (" +
                    $"{new DataItemFastSearch().DataItemID}, " +
                    $"{new DataItemFastSearch().Title}, " +
                    $"{new DataItemFastSearch().Description}, " +
                    $"{new DataItemFastSearch().RefPath}, " +
                    $"{new DataItemFastSearch().SearchText}" +
                    $")" +
                    $"VALUES (" +
                    $"@{new DataItemFastSearch().DataItemID}, " +
                    $"@{new DataItemFastSearch().Title}, " +
                    $"@{new DataItemFastSearch().Description}, " +
                    $"@{new DataItemFastSearch().RefPath}, " +
                    $"@{new DataItemFastSearch().SearchText}" +
                    $")";
                SqliteCommand SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().DataItemID}", dataItem.ID);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().Title}", IsFirstRecord ? dataItem.Title : string.Empty);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().Description}", IsFirstRecord ? dataItem.Description : string.Empty);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().RefPath}", IsFirstRecord ? dataItem.RefPath : string.Empty);
                SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().SearchText}", searchText);
                SqliteCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
            }
        }


        // TagPool options

        public async Task<ObservableCollection<Tag>> TagPoolGetTagList(string searchString = "")
        {
            await _lock.WaitAsync();

            Dictionary<long, string> ParentPropertyNameTempDict = [];
            Dictionary<long, string> OnChainTagNameTempDict = [];
            // TagID, (ParentTagID, PropertyID)
            HashSet<Tag> tags = new(new TagIDEqualityComparer());
            HashSet<long> tagIDViaSurnames = [];

            searchString = searchString.Trim();

            // Search tags via main names.
#if !DEBUG
            try
#endif
            {
                string command =
                    $"SELECT * FROM {nameof(TagPool)} " +
                    (searchString == string.Empty
                    ? $""
                    : $"WHERE {new TagPool().MainName} LIKE @{new TagPool().MainName}");
                SqliteCommand SqliteCommand = new(command, dbConnection);
                if (searchString != string.Empty)
                {
                    SqliteCommand.Parameters.AddWithValue($"@{new TagPool().MainName}", searchString);
                }
                SqliteDataReader reader = SqliteCommand.ExecuteReader();

                // Get tags
                reader.TagPoolAddTagsFromReader(ref tags, dbConnection, MessageManager, ref ParentPropertyNameTempDict, ref OnChainTagNameTempDict);
            }
#if !DEBUG
            catch (Exception ex)
            {
              MessageManager.PushMessage(MessageType.Error, ex.Message);
            }
#endif


            if (searchString == string.Empty)
            {
                _lock.Release();
                return new ObservableCollection<Tag>(tags);
            }
#if !DEBUG
            try
#endif
            // Search tags
            {
                string command =
                    $"SELECT * FROM {nameof(TagData)} " +
                    $"WHERE {new TagData().Type} = @{new TagData().Type} " +
                    $"AND {new TagData().Value} LIKE @{new TagData().Value}";
                SqliteCommand SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new TagData().Type}", new TagData().Surname);
                SqliteCommand.Parameters.AddWithValue($"@{new TagData().Value}", searchString);
                SqliteDataReader reader = SqliteCommand.ExecuteReader();
                while (reader.Read())
                {
                    tagIDViaSurnames.Add(reader.GetInt64(1));
                }

                string placeholders = string.Join(",", tagIDViaSurnames.ToList().Select((_, i) => $"@p{i}"));
                command =
                    $"SELECT * FROM {nameof(TagPool)} " +
                    $"WHERE {new TagPool().ID} IN ({placeholders})";
                SqliteCommand = new(command, dbConnection);
                int index = 0;
                foreach (long ID in tagIDViaSurnames)
                {
                    SqliteCommand.Parameters.AddWithValue($"@p{index}", ID);
                    index++;
                }
                reader = SqliteCommand.ExecuteReader();

                // Get tags.
                reader.TagPoolAddTagsFromReader(ref tags, dbConnection, MessageManager, ref ParentPropertyNameTempDict, ref OnChainTagNameTempDict);
            }
#if !DEBUG
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
            }
            finally
#endif
            {
                _lock.Release();
            }

            return new ObservableCollection<Tag>(tags);
        }

        public async Task<Tag> TagPoolGetTagByID(long tagID)
        {
            await _lock.WaitAsync();

            Dictionary<long, string> ParentPropertyNameTempDict = [];
            Dictionary<long, string> OnChainTagNameTempDict = [];
            // TagID, (ParentTagID, PropertyID)
            HashSet<Tag> tags = new(new TagIDEqualityComparer());
            HashSet<long> tagIDViaSurnames = [];
#if !DEBUG
            try
#endif
            {
                string command =
                    $"SELECT * FROM {nameof(TagPool)} " +
                    $"WHERE {new TagPool().ID} = @{new TagPool().ID} " +
                    $"LIMIT 1";
                SqliteCommand SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new TagPool().ID}", tagID);
                SqliteDataReader reader = SqliteCommand.ExecuteReader();

                // Get tags
                reader.TagPoolAddTagsFromReader(ref tags, dbConnection, MessageManager, ref ParentPropertyNameTempDict, ref OnChainTagNameTempDict);
            }
#if !DEBUG
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
            }
            finally
#endif
            {
                _lock.Release();
            }
            if (tags.Count == 0) return new();
            return tags.First();
        }

        public async Task<Tag> TagPoolGetTagByPropertyID(long propertyID)
        {
            await _lock.WaitAsync();

            Dictionary<long, string> ParentPropertyNameTempDict = [];
            Dictionary<long, string> OnChainTagNameTempDict = [];
            // TagID, (ParentTagID, PropertyID)
            HashSet<Tag> tags = new(new TagIDEqualityComparer());
            HashSet<long> tagIDViaSurnames = [];
#if !DEBUG
            try
#endif
            {
                string command =
                    $"SELECT * FROM {nameof(TagData)} " +
                    $"WHERE {new TagData().ID} = @{new TagData().ID} " +
                    $"LIMIT 1";
                SqliteCommand SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new TagPool().ID}", propertyID);
                SqliteDataReader reader = SqliteCommand.ExecuteReader();
                long tagID = -1;
                while (reader.Read())
                {
                    tagID = reader.GetInt64(1);
                    break;
                }
                // Get tags
                tags.Add(await TagPoolGetTagByID(tagID));
            }
#if !DEBUG
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
            }
            finally
#endif
            {
                _lock.Release();
            }
            if (tags.Count == 0) return new();
            return tags.First();
        }        

        /// <summary>
        /// Add a unique tag to tag pool.
        /// </summary>
        /// <param name="tag"></param>
        public async Task<long> TagPoolAddUniqueTag(Tag tag)
        {
            await _lock.WaitAsync();
#if !DEBUG
            try
#endif
            {
                string command =
                    $"INSERT INTO {nameof(TagPool)}(" +
                    $"{new TagPool().MainName}," +
                    $"{new TagPool().Description}," +
                    $"{new TagPool().CreatedTime}," +
                    $"{new TagPool().ModifiedTime}" +
                    $") " +
                    $"VALUES (" +
                    $"@{new TagPool().MainName}," +
                    $"@{new TagPool().Description}," +
                    $"@{new TagPool().CreatedTime}," +
                    $"@{new TagPool().ModifiedTime}" +
                    ")";
                SqliteCommand SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new TagPool().MainName}", tag.MainName);
                SqliteCommand.Parameters.AddWithValue($"@{new TagPool().Description}", tag.Description);
                SqliteCommand.Parameters.AddWithValue($"@{new TagPool().CreatedTime}", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                SqliteCommand.Parameters.AddWithValue($"@{new TagPool().ModifiedTime}", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                SqliteCommand.ExecuteNonQuery();

                // Get tag ID
                command = "SELECT last_insert_rowid()";
                SqliteCommand = new(command, dbConnection);
                tag.ID = (long)(SqliteCommand.ExecuteScalar() ?? -1);
                if (tag.ID == -1) return tag.ID;

                // Log surnames
                TagDataAddSurnames(tag.ID, tag.Surnames);

                // Log logic chains
                TagDataAddLogicChains(tag.ID, tag.LogicChains);

                // Log property items
                TagDataAddPropertyItems(tag.ID, tag.PropertyItems);
            }
#if !DEBUG
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
            }
            finally
#endif
            {
                _lock.Release();
            }
            return tag.ID;
        }

        public async Task TagPoolUpdateTag(Tag tagInfo)
        {
            await _lock.WaitAsync();

            try
            {
                Tag originalTag = await TagPoolGetTagByID(tagInfo.ID);

                // Update main name
                string command =
                    $"UPDATE {nameof(TagPool)} " +
                    $"SET {new TagPool().MainName} = @{new TagPool().MainName}, " +
                    $"{new TagPool().Description} = @{new TagPool().Description}, " +
                    $"{new TagPool().ModifiedTime} = @{new TagPool().ModifiedTime} " +
                    $"WHERE {new TagPool().ID} = @{new TagPool().ID}";
                SqliteCommand SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new TagPool().MainName}", tagInfo.MainName);
                SqliteCommand.Parameters.AddWithValue($"@{new TagPool().Description}", tagInfo.Description);
                SqliteCommand.Parameters.AddWithValue($"@{new TagPool().ModifiedTime}", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                SqliteCommand.Parameters.AddWithValue($"@{new TagPool().ID}", tagInfo.ID);
                SqliteCommand.ExecuteNonQuery();


                // Update surnames

                // Remove original surnames
                TagDataRemoveSurnames(tagInfo.ID);

                // Add new surnames
                TagDataAddSurnames(tagInfo.ID, tagInfo.Surnames);


                // Update LogicChains
                TagDataRemoveLogicChainsOfTag(tagInfo.ID);
                TagDataAddLogicChains(tagInfo.ID, tagInfo.LogicChains);

                // Update PropertyItems
                // Since PropertyItems can be referenced using ID by other tags, avoid changing its ID if it's still a same property.

                // Delete removed PropertyItems
                foreach (PropertyItem property in originalTag.PropertyItems)
                {
                    // PropertyItem removed
                    if (!tagInfo.PropertyItems.ContainsPropertyItem(property))
                    {
                        TagDataRemovePropertyItem(tagInfo.ID, property);
                    }
                }

                // Add or update PropertyItems
                foreach (PropertyItem property in tagInfo.PropertyItems)
                {
                    // Property item is edited
                    if (originalTag.PropertyItems.ContainsPropertyItem(property))
                    {
                        TagDataUpdatePropertyItemSeq(property.ID, property.Seq, property.PropertyName, property.IsContainsRelation);
                        // Update RestrictionLogicChains
                        TagDataRemoveRestrictionLogicChainsOfPropertyItem(tagInfo.ID, property.ID);

                        TagDataAddRestrictionLogicChains(tagInfo.ID, property.ID, property.RestrictedTagLogicChains);
                    }
                    // Is new PropertyItem
                    else
                    {
                        TagDataAddPropertyItem(tagInfo.ID, property);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
                return;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task TagPoolRemoveTagByID(long ID)
        {
            await _lock.WaitAsync();

#if !DEBUG
            try
#endif
            {
                Tag tag = await TagPoolGetTagByID(ID);
                // Remove PropertyItem and its reference
                foreach (PropertyItem item in tag.PropertyItems)
                {
                    TagDataRemovePropertyItem(item.TagID, item);
                }
                string command =
                    $"DELETE FROM {nameof(TagPool)} " +
                    $"WHERE {new TagPool().ID} = @{new TagPool().ID}";
                SqliteCommand SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new TagPool().ID}", ID);
                SqliteCommand.ExecuteNonQuery();

                // Remove all other TagData
                command =
                    $"DELETE FROM {nameof(TagData)} " +
                    $"WHERE {new TagData().TagID} = @{new TagData().TagID}";
                SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new TagData().TagID}", ID);
                SqliteCommand.ExecuteNonQuery();

                // Remove all references from DataItem ItemTags
                command =
                    $"DELETE FROM {nameof(ItemTags)} " +
                    $"WHERE {nameof(ItemTags.TagID)} = @{nameof(ItemTags.TagID)}";
                SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{nameof(ItemTags.TagID)}", ID);
                SqliteCommand.ExecuteNonQuery();

                // Set child items where ParentTagID is ID of removed tag to top level tags.
                command =
                    $"UPDATE {nameof(ItemTags)} " +
                    $"SET {nameof(ItemTags.ParentTagID)} = @{nameof(ItemTags.ParentTagID)}new, " +
                    $"{nameof(ItemTags.PropertyID)} = @{nameof(ItemTags.PropertyID)} " +
                    $"WHERE {nameof(ItemTags.ParentTagID)} = @{nameof(ItemTags.ParentTagID)}";
                SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{nameof(ItemTags.ParentTagID)}new", -1);
                SqliteCommand.Parameters.AddWithValue($"@{nameof(ItemTags.PropertyID)}", -1);
                SqliteCommand.Parameters.AddWithValue($"@{nameof(ItemTags.ParentTagID)}", ID);
                SqliteCommand.ExecuteNonQuery();
            }
#if !DEBUG
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
            }
            finally
#endif
            {
                _lock.Release();
            }
        }



        // Helper functions

        // Surnames
        private void TagDataAddSurnames(long TagID, List<string> Surnames)
        {
            int i = 0;
            foreach (string surname in Surnames)
            {
                string command =
                    $"INSERT INTO {nameof(TagData)}(" +
                    $"{new TagData().TagID}," +
                    $"{new TagData().Seq}," +
                    $"{new TagData().Type}," +
                    $"{new TagData().Value}" +
                    ") " +
                    $"VALUES (" +
                    $"@{new TagData().TagID}," +
                    $"@{new TagData().Seq}," +
                    $"@{new TagData().Type}," +
                    $"@{new TagData().Value}" +
                    ")";
                SqliteCommand SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new TagData().TagID}", TagID);
                SqliteCommand.Parameters.AddWithValue($"@{new TagData().Seq}", i.ToString());
                SqliteCommand.Parameters.AddWithValue($"@{new TagData().Type}", new TagData().Surname);
                SqliteCommand.Parameters.AddWithValue($"@{new TagData().Value}", surname);
                SqliteCommand.ExecuteNonQuery();
                i++;
            }
        }
        private void TagDataRemoveSurnames(long TagID)
        {
            string command =
                $"DELETE FROM {nameof(TagData)} " +
                $"WHERE {new TagData().TagID} = @{new TagData().TagID} " +
                $"AND {new TagData().Type} = @{new TagData().Type}";
            SqliteCommand SqliteCommand = new(command, dbConnection);
            SqliteCommand.Parameters.AddWithValue($"@{new TagData().TagID}", TagID);
            SqliteCommand.Parameters.AddWithValue($"@{new TagData().Type}", new TagData().Surname);
            SqliteCommand.ExecuteNonQuery();
        }

        // LogicChains

        private void TagDataAddLogicChains(long TagID, List<LogicChain> LogicChains)
        {
            long chainID = 0;
            foreach (LogicChain chain in LogicChains)
            {
                chain.ChainID = chainID;
                if (chain.LogicChainData.Count > 0)
                {
                    chain.LogicChainData[^1].OnChainTagID = TagID;
                }
                else
                {
                    continue;
                }
                long parentDataItemID = -1;
                foreach (LogicChainItem item in chain.LogicChainData)
                {
                    item.ChainID = chain.ChainID;
                    item.ParentDataItemID = parentDataItemID;
                    parentDataItemID = TagDataAddLogicChainItem(TagID, item);
                }
                chainID++;
            }
        }
        private long TagDataAddLogicChainItem(long TagID, LogicChainItem Item)
        {
#if !DEBUG
            try
#endif
            {
                string seq = $"{Item.ChainID} {Item.ParentDataItemID} {Item.ParentPropertyItemID}";

                string command =
                    $"INSERT INTO {nameof(TagData)}(" +
                    $"{new TagData().TagID}," +
                    $"{new TagData().Seq}," +
                    $"{new TagData().Type}," +
                    $"{new TagData().Value}" +
                    ") " +
                    $"VALUES (" +
                    $"@{new TagData().TagID}," +
                    $"@{new TagData().Seq}," +
                    $"@{new TagData().Type}," +
                    $"@{new TagData().Value}" +
                    ")";
                SqliteCommand SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new TagData().TagID}", TagID);
                SqliteCommand.Parameters.AddWithValue($"@{new TagData().Seq}", seq);
                SqliteCommand.Parameters.AddWithValue($"@{new TagData().Type}", new TagData().LogicChainItem);
                SqliteCommand.Parameters.AddWithValue($"@{new TagData().Value}", Item.OnChainTagID);
                SqliteCommand.ExecuteNonQuery();
                // Get inserted ID
                command = "SELECT last_insert_rowid()";
                SqliteCommand = new(command, dbConnection);
                Item.ID = (long)(SqliteCommand.ExecuteScalar() ?? -1);
                SqliteCommand.ExecuteNonQuery();
            }
#if !DEBUG
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
            }
#endif
            return Item.ID;
        }
        private void TagDataRemoveLogicChainByID(long TagID, long ChainID)
        {
            List<long> tagDataIDs = [];
            string subCommand =
                $"SELECT * FROM {nameof(TagData)} " +
                $"WHERE {new TagData().TagID} = @{new TagData().TagID} " +
                $"AND {new TagData().Type} = @{new TagData().Type}";
            SqliteCommand SqliteCommand = new(subCommand, dbConnection);
            SqliteCommand.Parameters.AddWithValue($"@{new TagData().TagID}", TagID);
            SqliteCommand.Parameters.AddWithValue($"@{new TagData().Type}", new TagData().LogicChainItem);
            SqliteDataReader reader = SqliteCommand.ExecuteReader();
            while (reader.Read())
            {
                string seq = reader.GetString(2);
                string[] seqval = seq.Split(' ');
                if (seqval.Length == 0) continue;
                long chainID = -1;
                long.TryParse(seqval[0], out chainID);
                if (chainID != -1)
                {
                    tagDataIDs.Add(reader.GetInt64(0));
                }
            }

            var parameters = tagDataIDs.Select((_, index) => $"@id{index}").ToArray();

            subCommand =
                $"DELETE FROM {nameof(TagData)} " +
                $"WHERE {new TagData().ID} IN ({string.Join(",", parameters)})";
            SqliteCommand = new(subCommand, dbConnection);
            for (int i = 0; i < tagDataIDs.Count; i++)
            {
                SqliteCommand.Parameters.AddWithValue($"@id{i}", tagDataIDs[i]);
            }
            SqliteCommand.ExecuteNonQuery();
        }
        private void TagDataRemoveLogicChainsOfTag(long TagID)
        {
            string command =
                $"DELETE FROM {nameof(TagData)} " +
                $"WHERE {new TagData().TagID} = @{new TagData().TagID} " +
                $"AND {new TagData().Type} = @{new TagData().Type}";
            SqliteCommand SqliteCommand = new(command, dbConnection);
            SqliteCommand.Parameters.AddWithValue($"@{new TagData().TagID}", TagID);
            SqliteCommand.Parameters.AddWithValue($"@{new TagData().Type}", new TagData().LogicChainItem);
            SqliteCommand.ExecuteNonQuery();
        }

        // RestrictionLogicChains

        private void TagDataAddRestrictionLogicChains(long TagID, long PropertyID, List<LogicChain> RestrictionLogicChains)
        {
            int chainID = 0;
            foreach (LogicChain chain in RestrictionLogicChains)
            {
                if (chain.LogicChainData.Count > 1)
                {
                    chain.LogicChainData[^1].OnChainTagID = -1;
                }
                long parentDataItemID = -1;
                foreach (LogicChainItem chainitem in chain.LogicChainData)
                {
                    chainitem.ParentDataItemID = parentDataItemID;
                    parentDataItemID = TagDataAddRestrictionLogicChainItem(TagID, chainitem, PropertyID, chainID);
                }
                chainID++;
            }
        }
        private long TagDataAddRestrictionLogicChainItem(long tagID, LogicChainItem item, long PropertyID, int chainID)
        {
#if !DEBUG
            try
#endif
            {
                string seq = $"{chainID} {item.ParentDataItemID} {item.ParentPropertyItemID} {PropertyID}";

                string command =
                    $"INSERT INTO {nameof(TagData)}(" +
                    $"{new TagData().TagID}," +
                    $"{new TagData().Seq}," +
                    $"{new TagData().Type}," +
                    $"{new TagData().Value}" +
                    ") " +
                    $"VALUES (" +
                    $"@{new TagData().TagID}," +
                    $"@{new TagData().Seq}," +
                    $"@{new TagData().Type}," +
                    $"@{new TagData().Value}" +
                    ")";
                SqliteCommand SqliteCommand = new(command, dbConnection);
                SqliteCommand.Parameters.AddWithValue($"@{new TagData().TagID}", tagID);
                SqliteCommand.Parameters.AddWithValue($"@{new TagData().Seq}", seq);
                SqliteCommand.Parameters.AddWithValue($"@{new TagData().Type}", new TagData().RestrictionLogicChainItem);
                SqliteCommand.Parameters.AddWithValue($"@{new TagData().Value}", item.OnChainTagID);
                SqliteCommand.ExecuteNonQuery();
                // Get inserted ID
                command = "SELECT last_insert_rowid()";
                SqliteCommand = new(command, dbConnection);
                item.ID = (long)(SqliteCommand.ExecuteScalar() ?? -1);
                if (item.ID == -1) return item.ID;
            }
#if !DEBUG
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
            }
#endif
            return item.ID;
        }
        private void TagDataRemoveRestrictionLogicChainsOfPropertyItem(long TagID, long PropertyID)
        {
            string seq = $"% % % {PropertyID}";
            string command =
                $"DELETE FROM {nameof(TagData)} " +
                $"WHERE {new TagData().TagID} = @{new TagData().TagID} " +
                $"AND {new TagData().Type} = @{new TagData().Type} " +
                $"AND {new TagData().Seq} LIKE @{new TagData().Seq}";
            SqliteCommand SqliteCommand = new(command, dbConnection);
            SqliteCommand.Parameters.AddWithValue($"@{new TagData().TagID}", TagID);
            SqliteCommand.Parameters.AddWithValue($"@{new TagData().Type}", new TagData().RestrictionLogicChainItem);
            SqliteCommand.Parameters.AddWithValue($"@{new TagData().Seq}", seq);
            SqliteCommand.ExecuteNonQuery();
        }
        private void TagDataRemoveRestrictionLogicChainByID(long TagID, long chainID)
        {
            List<long> tagDataIDs = [];
            string subCommand =
                $"SELECT * FROM {nameof(TagData)} " +
                $"WHERE {new TagData().TagID} = @{new TagData().TagID} " +
                $"AND {new TagData().Type} = @{new TagData().Type}";
            SqliteCommand SqliteCommand = new(subCommand, dbConnection);
            SqliteCommand.Parameters.AddWithValue($"@{new TagData().TagID}", TagID);
            SqliteCommand.Parameters.AddWithValue($"@{new TagData().Type}", new TagData().RestrictionLogicChainItem);
            SqliteDataReader reader = SqliteCommand.ExecuteReader();
            while (reader.Read())
            {
                string seq = reader.GetString(2);
                string[] seqval = seq.Split(' ');
                if (seqval.Length == 0) continue;
                long id = -1;
                if (long.TryParse(seqval[0], out id))
                {
                    tagDataIDs.Add(id);
                }
            }

            subCommand =
                $"DELETE FROM {nameof(TagData)} " +
                $"WHERE {new TagData().ID} IN @{new TagData().ID}";
            SqliteCommand = new(subCommand, dbConnection);
            SqliteCommand.Parameters.AddWithValue($"@{new TagData().ID}", string.Join(",", tagDataIDs));
            SqliteCommand.ExecuteNonQuery();
        }
        public async Task<List<LogicChain>> TagDataGetRestrictionLogicChainsOfPropertyItem(long PropertyID, long tagID = -1)
        {
            await _lock.WaitAsync();

            try
            {
                Dictionary<long, string> ParentPropertyNameTempDict = [];
                Dictionary<long, string> OnChainTagNameTempDict = [];

                if (tagID == -1)
                {
                    // Get tag ID to fasten search
                    string command =
                        $"SELECT * FROM {nameof(TagData)} " +
                        $"WHERE {new TagData().Type} = @{new TagData().Type} " +
                        $"AND  {new TagData().ID} = @{new TagData().ID} " +
                        $"LIMIT 1";
                    SqliteCommand SqliteCommand = new(command, dbConnection);
                    SqliteCommand.Parameters.AddWithValue($"@{new TagData().Type}", new TagData().PropertyItem);
                    SqliteCommand.Parameters.AddWithValue($"@{new TagData().ID}", PropertyID);
                    SqliteDataReader SqliteDataReader = SqliteCommand.ExecuteReader();
                    while (SqliteDataReader.Read())
                    {
                        tagID = SqliteDataReader.GetInt64(1);
                    }
                    if (tagID == -1) return [];
                }

                // Get RestrictedTagLogicChains
                string searchSeq = $"% % % {PropertyID}";
                string cmd =
                    $"SELECT * FROM {nameof(TagData)} " +
                    $"WHERE {new TagData().TagID} = @{new TagData().TagID} " +
                    $"AND {new TagData().Type} = @{new TagData().Type} " +
                    $"AND {new TagData().Seq} LIKE @{new TagData().Seq}";
                SqliteCommand SqlCmd = new(cmd, dbConnection);
                SqlCmd.Parameters.AddWithValue($"@{new TagData().TagID}", tagID);
                SqlCmd.Parameters.AddWithValue($"@{new TagData().Type}", new TagData().RestrictionLogicChainItem);
                SqlCmd.Parameters.AddWithValue($"@{new TagData().Seq}", searchSeq);
                SqliteDataReader reader = SqlCmd.ExecuteReader();
                List<LogicChainItem> allRestrictionChainItems = [];
                reader.GetRestrictedTagLogicChainsFromReader(ref allRestrictionChainItems, dbConnection, MessageManager, ref ParentPropertyNameTempDict, ref OnChainTagNameTempDict);

                // Sort restriction chain items
                // Use a fake tag to use function to sort the restriction logic chains.
                Tag tag = new();
                tag.PropertyItems.Add(new()
                {
                    ID = PropertyID
                });
                tag.SortRestrictedLogicChainsOfTag(ref allRestrictionChainItems);
                return tag.PropertyItems[0].RestrictedTagLogicChains;
            }
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
                return [];
            }
            finally
            {
                _lock.Release();
            }
        }

        // PropertyItems

        private void TagDataAddPropertyItems(long TagID, List<PropertyItem> PropertyItems)
        {
            foreach (PropertyItem PropertyItem in PropertyItems)
            {
                TagDataAddPropertyItem(TagID, PropertyItem);
            }
        }
        private void TagDataAddPropertyItem(long TagID, PropertyItem PropertyItem)
        {
            string command =
                $"INSERT INTO {nameof(TagData)}(" +
                $"{new TagData().TagID}," +
                $"{new TagData().Seq}," +
                $"{new TagData().Type}," +
                $"{new TagData().Value}" +
                $") " +
                $"VALUES (" +
                $"@{new TagData().TagID}," +
                $"@{new TagData().Seq}," +
                $"@{new TagData().Type}," +
                $"@{new TagData().Value}" +
                ")";
            SqliteCommand SqliteCommand = new(command, dbConnection);
            SqliteCommand.Parameters.AddWithValue($"@{new TagData().TagID}", TagID);
            SqliteCommand.Parameters.AddWithValue($"@{new TagData().Seq}", $"{PropertyItem.Seq.ToString()} {(PropertyItem.IsContainsRelation ? 1 : 0)}");
            SqliteCommand.Parameters.AddWithValue($"@{new TagData().Type}", new TagData().PropertyItem);
            SqliteCommand.Parameters.AddWithValue($"@{new TagData().Value}", PropertyItem.PropertyName);
            SqliteCommand.ExecuteNonQuery();

            command = "SELECT last_insert_rowid()";
            SqliteCommand = new(command, dbConnection);
            PropertyItem.ID = (long)(SqliteCommand.ExecuteScalar() ?? -1);
            if (PropertyItem.ID == -1) return;
            // Log restriction logic chains
            int chainID = 0;
            foreach (LogicChain chain in PropertyItem.RestrictedTagLogicChains)
            {
                if (chain.LogicChainData.Count > 0)
                {
                    chain.LogicChainData[^1].OnChainTagID = TagID;
                }
                else
                {
                    continue;
                }
                long parentDataItemID = -1;
                foreach (LogicChainItem chainitem in chain.LogicChainData)
                {
                    chainitem.ParentDataItemID = parentDataItemID;
                    parentDataItemID = TagDataAddRestrictionLogicChainItem(TagID, chainitem, PropertyItem.ID, chainID);
                }
                chainID++;
            }
        }
        private void TagDataUpdatePropertyItemSeq(long PropertyID, long newSeq, string newName, bool isContainRelation)
        {
            string command =
                $"UPDATE {nameof(TagData)} " +
                $"SET {nameof(TagData.Seq)} = @{nameof(TagData.Seq)}," +
                $"{nameof(TagData.Value)} = @{nameof(TagData.Value)} " +
                $"WHERE {nameof(TagData.ID)} = @{nameof(TagData.ID)}";
            SqliteCommand SqliteCommand = new(command, dbConnection);
            SqliteCommand.Parameters.AddWithValue($"@{nameof(TagData.Seq)}", $"{newSeq.ToString()} {(isContainRelation ? 1 : 0)}");
            SqliteCommand.Parameters.AddWithValue($"@{nameof(TagData.Value)}", newName);
            SqliteCommand.Parameters.AddWithValue($"@{nameof(TagData.ID)}", PropertyID);
            SqliteCommand.ExecuteNonQuery();
        }
        private void TagDataRemovePropertyItem(long PropertyTagID, PropertyItem Property)
        {
            // Remove all references

            long removedPropertyID = Property.ID;
            string searchSeq = $"% % % {removedPropertyID}";
            // Remove restriction logic chains
            string command =
                $"DELETE FROM {nameof(TagData)} " +
                $"WHERE {new TagData().TagID} = @{new TagData().TagID} " +
                $"AND {new TagData().Type} = @{new TagData().Type} " +
                $"AND {new TagData().Seq} LIKE @{new TagData().Seq}";
            SqliteCommand SqliteCommand = new(command, dbConnection);
            SqliteCommand.Parameters.AddWithValue($"@{new TagData().TagID}", PropertyTagID);
            SqliteCommand.Parameters.AddWithValue($"@{new TagData().Type}", new TagData().RestrictionLogicChainItem);
            SqliteCommand.Parameters.AddWithValue($"@{new TagData().Seq}", searchSeq);
            SqliteCommand.ExecuteNonQuery();

            // Remove outer references
            searchSeq = $"% % {removedPropertyID}";
            // Find logic chains, get their tag ID and chain ID
            // (tagID, chainID)
            List<(long, long)> chainsBeingDeleted = [];
            List<(long, long)> restrictionChainsBeingDeleted = [];
            command =
                $"SELECT * FROM {nameof(TagData)} " +
                $"WHERE {new TagData().Type} = @{new TagData().Type} " +
                $"AND {new TagData().Seq} LIKE @{new TagData().Seq}";
            SqliteCommand = new(command, dbConnection);
            SqliteCommand.Parameters.AddWithValue($"@{new TagData().Type}", new TagData().LogicChainItem);
            SqliteCommand.Parameters.AddWithValue($"@{new TagData().Seq}", searchSeq);
            SqliteDataReader reader = SqliteCommand.ExecuteReader();
            while (reader.Read())
            {
                string seq;
                long tagID = reader.GetInt64(1);
                seq = reader.GetString(2);
                string[] seqval = seq.Split(' ');
                long chainID = -1;
                long.TryParse(seqval[0], out chainID);
                if (chainID == -1) continue;
                if (chainsBeingDeleted.Contains((tagID, chainID))) continue;
                chainsBeingDeleted.Add((tagID, chainID));
            }
            // Remove whole chain
            foreach ((long, long) ids in chainsBeingDeleted)
            {
                TagDataRemoveLogicChainByID(ids.Item1, ids.Item2);
            }
            // Find restriction chains
            command =
                $"SELECT *  FROM {nameof(TagData)} " +
                $"WHERE {new TagData().TagID} = @{new TagData().TagID} " +
                $"AND {new TagData().Type} = @{new TagData().Type} " +
                $"AND {new TagData().Seq} LIKE @{new TagData().Seq}";
            SqliteCommand = new(command, dbConnection);
            SqliteCommand.Parameters.AddWithValue($"@{new TagData().TagID}", PropertyTagID);
            SqliteCommand.Parameters.AddWithValue($"@{new TagData().Type}", new TagData().RestrictionLogicChainItem);
            SqliteCommand.Parameters.AddWithValue($"@{new TagData().Seq}", searchSeq);
            reader = SqliteCommand.ExecuteReader();
            while (reader.Read())
            {
                string seq;
                long tagID = reader.GetInt64(1);
                seq = reader.GetString(2);
                string[] seqval = seq.Split(' ');
                long chainID = -1;
                long.TryParse(seqval[0], out chainID);
                if (chainID == -1) continue;
                if (restrictionChainsBeingDeleted.Contains((tagID, chainID))) continue;
                restrictionChainsBeingDeleted.Add((tagID, chainID));
            }
            // Remove whole chain
            foreach ((long, long) ids in restrictionChainsBeingDeleted)
            {
                TagDataRemoveRestrictionLogicChainByID(ids.Item1, ids.Item2);
            }

            // Remove PropertyItem itself

            command =
                $"DELETE FROM {nameof(TagData)} " +
                $"WHERE {new TagData().ID} = @{new TagData().ID}";
            SqliteCommand = new(command, dbConnection);
            SqliteCommand.Parameters.AddWithValue($"@{new TagData().ID}", Property.ID);
            SqliteCommand.ExecuteNonQuery();
        }

        private bool PropertyItemGetIsContainRelationByID(long ID)
        {
            string command =
                $"SELECT * FROM {nameof(TagData)} " +
                $"WHERE {nameof(TagData.Type)} = @{nameof(TagData.Type)} " +
                $"AND {nameof(TagData.ID)} = {nameof(TagData.ID)} " +
                $"LIMIT 1";
            SqliteCommand sqliteCommand = new(command, dbConnection);
            sqliteCommand.Parameters.AddWithValue($"@{nameof(TagData.Type)}", nameof(TagData.PropertyItem));
            sqliteCommand.Parameters.AddWithValue($"@{nameof(TagData.ID)}", ID);
            SqliteDataReader reader = sqliteCommand.ExecuteReader();
            while (reader.Read())
            {
                string rawSeq = reader.GetString(2);
                int[] intArray = rawSeq.Split(' ')
                                       .Select(int.Parse)
                                       .ToArray();
                if (intArray.Length >= 2) // TDB version 0.0.1, for compatibility
                {
                    return intArray[1] == 1;
                }
            }
            return false;
        }
    }

    public static class DBContentManagerExtension
    {
        public static void TagPoolAddTagsFromReader(this SqliteDataReader reader,
            ref HashSet<Tag> tags,
            SqliteConnection? dbConnection,
            MessageManager MessageManager,
            ref Dictionary<long, string> ParentPropertyNameTempDict,
            ref Dictionary<long, string> OnChainTagNameTempDict)
        {
            if (dbConnection == null) return;

#if !DEBUG
            try
#endif
            {
                while (reader.Read())
                {
                    Tag tag = new();
                    tag.ID = reader.GetInt64(0);
                    tag.MainName = reader.GetString(1);
                    tag.Description = reader.GetString(2);
                    tag.CreatedTime = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(3)).DateTime.ToLocalTime();
                    tag.ModifiedTime = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(4)).DateTime.ToLocalTime();

                    // Get surnames
                    string subcommand =
                        $"SELECT * FROM {nameof(TagData)} " +
                        $"WHERE {new TagData().TagID} = {tag.ID} " +
                        $"AND {new TagData().Type} = @{new TagData().Type}";
                    SqliteCommand subSqlCmd = new(subcommand, dbConnection);
                    subSqlCmd.Parameters.AddWithValue($"@{new TagData().Type}", new TagData().Surname);
                    SqliteDataReader subReader = subSqlCmd.ExecuteReader();
                    while (subReader.Read())
                    {
                        string surname = subReader.GetString(4);
                        if (surname.Trim() != string.Empty)
                        {
                            tag.Surnames.Add(surname);
                        }
                    }

                    // Get property items
                    subcommand =
                        $"SELECT * FROM {nameof(TagData)} " +
                        $"WHERE {new TagData().TagID} = @{new TagData().TagID} " +
                        $"AND {new TagData().Type} = @{new TagData().Type}";
                    subSqlCmd = new(subcommand, dbConnection);
                    subSqlCmd.Parameters.AddWithValue($"@{new TagData().TagID}", tag.ID);
                    subSqlCmd.Parameters.AddWithValue($"@{new TagData().Type}", new TagData().PropertyItem);
                    subReader = subSqlCmd.ExecuteReader();
                    List<PropertyItem> properties = [];

                    while (subReader.Read())
                    {
                        PropertyItem propertyItem = new();
                        propertyItem.ID = subReader.GetInt64(0);
                        propertyItem.TagID = subReader.GetInt64(1);
                        string rawSeq = subReader.GetString(2);
                        int[] intArray = rawSeq.Split(' ')
                                               .Select(int.Parse)
                                               .ToArray();
                        if (intArray.Length >= 2) // TDB version 0.0.1, for compatibility
                        {
                            propertyItem.IsContainsRelation = intArray[1] == 1;
                        }
                        if (intArray.Length >= 1)// TDB version 0.0.0
                        {
                            propertyItem.Seq = intArray[0];
                        }
                        propertyItem.PropertyName = subReader.GetString(4);
                        tag.PropertyItems.Add(propertyItem);
                    }

                    // Get RestrictedTagLogicChains
                    subcommand =
                        $"SELECT * FROM {nameof(TagData)} " +
                        $"WHERE {new TagData().TagID} = @{new TagData().TagID} " +
                        $"AND {new TagData().Type} = @{new TagData().Type}";
                    subSqlCmd = new(subcommand, dbConnection);
                    subSqlCmd.Parameters.AddWithValue($"@{new TagData().TagID}", tag.ID);
                    subSqlCmd.Parameters.AddWithValue($"@{new TagData().Type}", new TagData().RestrictionLogicChainItem);
                    subReader = subSqlCmd.ExecuteReader();
                    List<LogicChainItem> allRestrictionChainItems = [];
                    subReader.GetRestrictedTagLogicChainsFromReader(ref allRestrictionChainItems, dbConnection, MessageManager, ref ParentPropertyNameTempDict, ref OnChainTagNameTempDict);

                    // Sort restriction chain items
                    tag.SortRestrictedLogicChainsOfTag(ref allRestrictionChainItems);

                    // Get LogicChains
                    subcommand =
                        $"SELECT * FROM {nameof(TagData)} " +
                        $"WHERE {new TagData().TagID} = @{new TagData().TagID} " +
                        $"AND {new TagData().Type} = @{new TagData().Type}";
                    subSqlCmd = new(subcommand, dbConnection);
                    subSqlCmd.Parameters.AddWithValue($"@{new TagData().TagID}", tag.ID);
                    subSqlCmd.Parameters.AddWithValue($"@{new TagData().Type}", new TagData().LogicChainItem);
                    subReader = subSqlCmd.ExecuteReader();
                    List<LogicChainItem> allChainItems = [];

                    // Get chain items
                    while (subReader.Read())
                    {
                        // Get chain item
                        LogicChainItem chainItem = new();
                        chainItem.ID = subReader.GetInt64(0);
                        chainItem.TagID = subReader.GetInt64(1);
                        string seq = subReader.GetString(2);
                        chainItem.OnChainTagID = long.Parse(subReader.GetString(4));
                        string[] seqval = seq.Split(' ');
                        if (seqval.Length != 3)
                        {
                            MessageManager.PushMessage(MessageType.Warning, $"TagData which ID = {chainItem.ID} has an invalid value.");
                            continue;
                        }

                        List<long> seqdat = [];
                        try
                        {
                            foreach (string i in seqval)
                            {
                                seqdat.Add(long.Parse(i));
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageManager.PushMessage(MessageType.Error, $"TagData which ID = {chainItem.ID} has an invalid value. Non-integer value was found");
                            MessageManager.PushMessage(MessageType.Error, ex.Message);
                            continue;
                        }

                        chainItem.ChainID = seqdat[0];
                        chainItem.ParentDataItemID = seqdat[1];
                        chainItem.ParentPropertyItemID = seqdat[2];

                        if (!ParentPropertyNameTempDict.ContainsKey(chainItem.ParentPropertyItemID))
                        {
                            string cmd =
                                $"SELECT * FROM {nameof(TagData)} " +
                                $"WHERE {new TagData().ID} = @{new TagData().ID}";
                            SqliteCommand sqlcmd = new(cmd, dbConnection);
                            sqlcmd.Parameters.AddWithValue($"@{new TagData().ID}", chainItem.ParentPropertyItemID);
                            SqliteDataReader sqlreader = sqlcmd.ExecuteReader();
                            while (sqlreader.Read())
                            {
                                ParentPropertyNameTempDict.Add(chainItem.ParentPropertyItemID, sqlreader.GetString(4));
                                break;
                            }
                        }
                        if (!ParentPropertyNameTempDict.ContainsKey(chainItem.ParentPropertyItemID))
                        {
                            ParentPropertyNameTempDict.Add(chainItem.ParentPropertyItemID, string.Empty);
                        }
                        chainItem.ParentPropertyItemName = ParentPropertyNameTempDict[chainItem.ParentPropertyItemID];

                        if (!OnChainTagNameTempDict.ContainsKey(chainItem.OnChainTagID))
                        {
                            string cmd =
                                $"SELECT * FROM {nameof(TagPool)} " +
                                $"WHERE {new TagPool().ID} = @{new TagPool().ID}";
                            SqliteCommand sqlcmd = new(cmd, dbConnection);
                            sqlcmd.Parameters.AddWithValue($"@{new TagPool().ID}", chainItem.OnChainTagID);
                            SqliteDataReader sqlreader = sqlcmd.ExecuteReader();
                            while (sqlreader.Read())
                            {
                                OnChainTagNameTempDict.Add(chainItem.OnChainTagID, sqlreader.GetString(1));
                                break;
                            }
                        }
                        if (!OnChainTagNameTempDict.ContainsKey(chainItem.OnChainTagID))
                        {
                            OnChainTagNameTempDict.Add(chainItem.OnChainTagID, string.Empty);
                        }
                        chainItem.OnChainTagName = OnChainTagNameTempDict[chainItem.OnChainTagID];

                        allChainItems.Add(chainItem);
                    }

                    // Sort items
                    Dictionary<long, List<LogicChainItem>> chainSortDict = [];
                    foreach (LogicChainItem item in allChainItems)
                    {
                        if (!chainSortDict.ContainsKey(item.ChainID))
                        {
                            chainSortDict.Add(item.ChainID, []);
                        }
                        chainSortDict[item.ChainID].Add(item);
                    }

                    foreach (KeyValuePair<long, List<LogicChainItem>> rawValue in chainSortDict)
                    {
                        List<LogicChainItem> chainSource = rawValue.Value;
                        LogicChain chain = new();
                        chain.ChainID = chainSource[0].ChainID;
                        LogicChainItem item = new();
                        if (!chainSource.Any(x => x.ParentDataItemID == -1))
                        {
                            continue;
                        }
                        item = chainSource.First(x => x.ParentDataItemID == -1);
                        chainSource.Remove(item);
                        chain.LogicChainData.Add(item);

                        long thisChainItemID = item.ID;
                        while (true)
                        {
                            if (chainSource.Count == 0)
                            {
                                break;
                            }
                            if (!chainSource.Any(x => x.ParentDataItemID == thisChainItemID))
                            {
                                break;
                            }
                            item = chainSource.First(x => x.ParentDataItemID == thisChainItemID);
                            chain.LogicChainData.Add(item);
                            chainSource.Remove(item);
                            thisChainItemID = item.ID;
                        }
                        tag.LogicChains.Add(chain);
                    }

                    tags.Add(tag);
                }
            }
#if !DEBUG
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
            }
#endif
        }

        public static void DataItemsAddDataItemsFromReader(this SqliteDataReader reader,
            ref HashSet<DataItem> dataItems,
            SqliteConnection? dbConnection,
            MessageManager MessageManager)
        {
            Dictionary<long, string> TagNameTempDict = [];
            Dictionary<long, string> PropertyNameTempDict = [];

#if !DEBUG
            try
#endif
            {
                if (dbConnection == null) return;
                while (reader.Read())
                {
                    DataItem item = new();
                    item.ID = reader.GetInt64(0);
                    item.ParentID = reader.GetInt64(1);
                    item.ItemType = reader.GetString(2);
                    item.CreatedTime = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(3)).UtcDateTime;
                    item.ModifiedTime = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(4)).UtcDateTime;

                    // Get DataItem in table DataItems
                    string command =
                        $"SELECT * FROM {nameof(DataItemFastSearch)} " +
                        $"WHERE {new DataItemFastSearch().DataItemID} = @{new DataItemFastSearch().DataItemID} " +
                        $"LIMIT 1";
                    SqliteCommand SqliteCommand = new(command, dbConnection);
                    SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().DataItemID}", item.ID);
                    SqliteDataReader SqliteDataReader = SqliteCommand.ExecuteReader();
                    while (SqliteDataReader.Read())
                    {
                        item.Title = SqliteDataReader.GetString(1);
                        item.Description = SqliteDataReader.GetString(2);
                        item.RefPath = SqliteDataReader.GetString(3);
                        item.SearchText = SqliteDataReader.GetString(4);
                    }

                    // Get tag data sources
                    List<ItemTagDataSource> itemTagDataSources = [];
                    command =
                        $"SELECT * FROM {nameof(ItemTags)} " +
                        $"WHERE {new ItemTags().ItemID} = @{new ItemTags().ItemID} ";
                    SqliteCommand = new(command, dbConnection);
                    SqliteCommand.Parameters.AddWithValue($"@{new ItemTags().ItemID}", item.ID);
                    SqliteDataReader = SqliteCommand.ExecuteReader();
                    while (SqliteDataReader.Read())
                    {
                        ItemTagDataSource itemTag = new()
                        {
                            ID = SqliteDataReader.GetInt64(0),
                            ParentPropertyID = SqliteDataReader.GetInt64(1),
                            TagID = SqliteDataReader.GetInt64(2),
                            ParentTagID = SqliteDataReader.GetInt64(3),
                        };
                        if (!TagNameTempDict.ContainsKey(itemTag.TagID))
                        {
                            string tagName = string.Empty;
                            tagName = dbConnection.TagPoolGetTagName(itemTag.TagID);
                            TagNameTempDict.Add(itemTag.TagID, tagName);
                            itemTag.TagName = tagName;
                        }
                        else
                        {
                            itemTag.TagName = TagNameTempDict[itemTag.TagID];
                        }
                        if (!PropertyNameTempDict.ContainsKey(itemTag.ParentPropertyID))
                        {
                            string propertyName = string.Empty;
                            propertyName = dbConnection.TagDataGetPropertyName(itemTag.ParentPropertyID);
                            PropertyNameTempDict.Add(itemTag.ParentPropertyID, propertyName);
                            itemTag.ParentPropertyName = propertyName;
                        }
                        else
                        {
                            itemTag.ParentPropertyName = PropertyNameTempDict[itemTag.ParentPropertyID];
                        }


                        itemTagDataSources.Add(itemTag);
                    }

                    item.ItemTags = itemTagDataSources.ConvertIntoItemTagTree();

                    dataItems.Add(item);
                }

            }
#if !DEBUG
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
            }

#endif
        }

        public static async IAsyncEnumerable<DataItem> DataItemsAddDataItemsIterativeFromReader(this SqliteDataReader reader,
            SqliteConnection? dbConnection,
            MessageManager MessageManager)
        {
            Dictionary<long, string> TagNameTempDict = [];
            Dictionary<long, string> PropertyNameTempDict = [];

#if !DEBUG
            try
#endif
            {
                if (dbConnection == null) yield break;
                while (reader.Read())
                {
                    DataItem item = new();
                    item.ID = reader.GetInt64(0);
                    item.ParentID = reader.GetInt64(1);
                    item.ItemType = reader.GetString(2);
                    item.CreatedTime = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(3)).UtcDateTime;
                    item.ModifiedTime = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(4)).UtcDateTime;

                    // Get DataItem in table DataItems
                    string command =
                        $"SELECT * FROM {nameof(DataItemFastSearch)} " +
                        $"WHERE {new DataItemFastSearch().DataItemID} = @{new DataItemFastSearch().DataItemID} " +
                        $"LIMIT 1";
                    SqliteCommand SqliteCommand = new(command, dbConnection);
                    SqliteCommand.Parameters.AddWithValue($"@{new DataItemFastSearch().DataItemID}", item.ID);
                    SqliteDataReader SqliteDataReader = SqliteCommand.ExecuteReader();
                    while (SqliteDataReader.Read())
                    {
                        item.Title = SqliteDataReader.GetString(1);
                        item.Description = SqliteDataReader.GetString(2);
                        item.RefPath = SqliteDataReader.GetString(3);
                        item.SearchText = SqliteDataReader.GetString(4);
                    }

                    // Get tag data sources
                    List<ItemTagDataSource> itemTagDataSources = [];
                    command =
                        $"SELECT * FROM {nameof(ItemTags)} " +
                        $"WHERE {new ItemTags().ItemID} = @{new ItemTags().ItemID} ";
                    SqliteCommand = new(command, dbConnection);
                    SqliteCommand.Parameters.AddWithValue($"@{new ItemTags().ItemID}", item.ID);
                    SqliteDataReader = SqliteCommand.ExecuteReader();
                    while (SqliteDataReader.Read())
                    {
                        ItemTagDataSource itemTag = new()
                        {
                            ID = SqliteDataReader.GetInt64(0),
                            ParentPropertyID = SqliteDataReader.GetInt64(1),
                            TagID = SqliteDataReader.GetInt64(2),
                            ParentTagID = SqliteDataReader.GetInt64(3),
                        };
                        if (!TagNameTempDict.ContainsKey(itemTag.TagID))
                        {
                            string tagName = string.Empty;
                            tagName = dbConnection.TagPoolGetTagName(itemTag.TagID);
                            TagNameTempDict.Add(itemTag.TagID, tagName);
                            itemTag.TagName = tagName;
                        }
                        else
                        {
                            itemTag.TagName = TagNameTempDict[itemTag.TagID];
                        }
                        if (!PropertyNameTempDict.ContainsKey(itemTag.ParentPropertyID))
                        {
                            string propertyName = string.Empty;
                            propertyName = dbConnection.TagDataGetPropertyName(itemTag.ParentPropertyID);
                            PropertyNameTempDict.Add(itemTag.ParentPropertyID, propertyName);
                            itemTag.ParentPropertyName = propertyName;
                        }
                        else
                        {
                            itemTag.ParentPropertyName = PropertyNameTempDict[itemTag.ParentPropertyID];
                        }


                        itemTagDataSources.Add(itemTag);
                    }

                    item.ItemTags = itemTagDataSources.ConvertIntoItemTagTree();

                    yield return item;
                }

            }
#if !DEBUG
            finally
            {

            }
#endif
        }

        public static string TagPoolGetTagName(this SqliteConnection dbConnection, long tagID)
        {
            string command =
                $"SELECT * FROM {nameof(TagPool)} " +
                $"WHERE {new TagPool().ID} = @{new TagPool().ID} " +
                $"LIMIT 1";
            SqliteCommand SqliteCommand = new(command, dbConnection);
            SqliteCommand.Parameters.AddWithValue($"@{new TagPool().ID}", tagID);
            SqliteDataReader reader = SqliteCommand.ExecuteReader();
            while (reader.Read())
            {
                return reader.GetString(1);
            }
            return string.Empty;
        }

        public static string TagDataGetPropertyName(this SqliteConnection dbConnection, long PropertyID)
        {
            string command =
                $"SELECT * FROM {nameof(TagData)} " +
                $"WHERE {new TagData().ID} = @{new TagData().ID} " +
                $"LIMIT 1";
            SqliteCommand SqliteCommand = new(command, dbConnection);
            SqliteCommand.Parameters.AddWithValue($"@{new TagPool().ID}", PropertyID);
            SqliteDataReader reader = SqliteCommand.ExecuteReader();
            while (reader.Read())
            {
                return reader.GetString(4);
            }
            return string.Empty;
        }

        public static void GetRestrictedTagLogicChainsFromReader(this SqliteDataReader reader,
            ref List<LogicChainItem> allRestrictionChainItems,
            SqliteConnection? dbConnection,
            MessageManager MessageManager,
            ref Dictionary<long, string> ParentPropertyNameTempDict,
            ref Dictionary<long, string> OnChainTagNameTempDict)
        {
            try
            {
                if (dbConnection == null) return;
                while (reader.Read())
                {
                    // Get restriction chain item
                    LogicChainItem chainItem = new();
                    chainItem.ID = reader.GetInt64(0);
                    chainItem.TagID = reader.GetInt64(1);
                    string seq = reader.GetString(2);
                    chainItem.OnChainTagID = long.Parse(reader.GetString(4));
                    string[] seqval = seq.Split(' ');
                    if (seqval.Length != 4)
                    {
                        MessageManager.PushMessage(MessageType.Warning, $"TagData which ID = {chainItem.ID} has an invalid value.");
                        continue;
                    }

                    List<int> seqdat = [];
                    try
                    {
                        foreach (string i in seqval)
                        {
                            seqdat.Add(int.Parse(i));
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageManager.PushMessage(MessageType.Error, $"TagData which ID = {chainItem.ID} has an invalid value. Non-integer value was found");
                        MessageManager.PushMessage(MessageType.Error, ex.Message);
                        continue;
                    }

                    chainItem.ChainID = seqdat[0];
                    chainItem.ParentDataItemID = seqdat[1];
                    chainItem.ParentPropertyItemID = seqdat[2];
                    chainItem.RestrictionPropertyID = seqdat[3];

                    if (!ParentPropertyNameTempDict.ContainsKey(chainItem.ParentPropertyItemID))
                    {
                        string cmd =
                            $"SELECT * FROM {nameof(TagData)} " +
                            $"WHERE {new TagData().ID} = @{new TagData().ID}";
                        SqliteCommand sqlcmd = new(cmd, dbConnection);
                        sqlcmd.Parameters.AddWithValue($"@{new TagData().ID}", chainItem.ParentPropertyItemID);
                        SqliteDataReader sqlreader = sqlcmd.ExecuteReader();
                        while (sqlreader.Read())
                        {
                            ParentPropertyNameTempDict.Add(chainItem.ParentPropertyItemID, sqlreader.GetString(4));
                        }
                    }
                    if (!ParentPropertyNameTempDict.ContainsKey(chainItem.ParentPropertyItemID))
                    {
                        ParentPropertyNameTempDict.Add(chainItem.ParentPropertyItemID, string.Empty);
                    }
                    chainItem.ParentPropertyItemName = ParentPropertyNameTempDict[chainItem.ParentPropertyItemID];

                    if (!OnChainTagNameTempDict.ContainsKey(chainItem.OnChainTagID))
                    {
                        string cmd =
                            $"SELECT * FROM {nameof(TagPool)} " +
                            $"WHERE {new TagPool().ID} = @{new TagPool().ID}";
                        SqliteCommand sqlcmd = new(cmd, dbConnection);
                        sqlcmd.Parameters.AddWithValue($"@{new TagPool().ID}", chainItem.OnChainTagID);
                        SqliteDataReader sqlreader = sqlcmd.ExecuteReader();
                        while (sqlreader.Read())
                        {
                            OnChainTagNameTempDict.Add(chainItem.OnChainTagID, sqlreader.GetString(1));
                        }
                    }
                    if (!OnChainTagNameTempDict.ContainsKey(chainItem.OnChainTagID))
                    {
                        OnChainTagNameTempDict.Add(chainItem.OnChainTagID, string.Empty);
                    }
                    chainItem.OnChainTagName = OnChainTagNameTempDict[chainItem.OnChainTagID];

                    allRestrictionChainItems.Add(chainItem);
                }
            }
            catch (Exception ex)
            {
                MessageManager.PushMessage(MessageType.Error, ex.Message);
            }
        }

        public static void SortRestrictedLogicChainsOfTag(this Tag tag, ref List<LogicChainItem> allRestrictionChainItems)
        {
            // RestrictionPropertyID
            Dictionary<long, List<LogicChainItem>> restrictionChainSortDictSortByPropertyID = [];
            // RestrictionPropertyID, ChainID
            Dictionary<(long, long), List<LogicChainItem>> restrictionChainSortDictSortByChainID = [];

            // Sort by RestrictionPropertyID
            foreach (LogicChainItem item in allRestrictionChainItems)
            {
                if (!restrictionChainSortDictSortByPropertyID.ContainsKey(item.RestrictionPropertyID))
                {
                    restrictionChainSortDictSortByPropertyID.Add(item.RestrictionPropertyID, []);
                }
                restrictionChainSortDictSortByPropertyID[item.RestrictionPropertyID].Add(item);
            }

            // Sort by ChainID
            foreach (KeyValuePair<long, List<LogicChainItem>> rawValue in restrictionChainSortDictSortByPropertyID)
            {
                foreach (LogicChainItem item in rawValue.Value)
                {
                    if (!restrictionChainSortDictSortByChainID.ContainsKey((rawValue.Key, item.ChainID)))
                    {
                        restrictionChainSortDictSortByChainID.Add((rawValue.Key, item.ChainID), []);
                    }
                    restrictionChainSortDictSortByChainID[(rawValue.Key, item.ChainID)].Add(item);
                }
            }

            // Fill tag properties with chains
            foreach (KeyValuePair<(long, long), List<LogicChainItem>> rawValue in restrictionChainSortDictSortByChainID)
            {
                List<LogicChainItem> chainSource = rawValue.Value;
                LogicChain chain = new()
                {
                    ChainID = chainSource[0].ChainID
                };
                LogicChainItem item = new();
                if (!chainSource.Any(x => x.ParentDataItemID == -1))
                {
                    continue;
                }
                item = chainSource.First(x => x.ParentDataItemID == -1);
                chainSource.Remove(item);
                chain.LogicChainData.Add(item);

                long thisChainItemID = item.ID;
                while (true)
                {
                    if (chainSource.Count == 0)
                    {
                        break;
                    }
                    if (!chainSource.Any(x => x.ParentDataItemID == thisChainItemID))
                    {
                        break;
                    }
                    item = chainSource.First(x => x.ParentDataItemID == thisChainItemID);
                    chain.LogicChainData.Add(item);
                    chainSource.Remove(item);
                    thisChainItemID = item.ID;
                }
                if (chain.LogicChainData.Count > 0)
                {
                    if (!tag.PropertyItems.Any(x => x.ID == chain.LogicChainData[0].RestrictionPropertyID)) continue;
                    PropertyItem propertyItem = tag.PropertyItems.First(x => x.ID == chain.LogicChainData[0].RestrictionPropertyID);
                    propertyItem.RestrictedTagLogicChains.Add(chain);
                }
            }
        }

        public static void SaveDataItemTags(this SqliteConnection? dbConnection, List<ItemTagTreeItem> ItemTagSource, long ItemID)
        {
            if (dbConnection == null) return;
            foreach (ItemTagTreeItem itemTag in ItemTagSource)
            {
                dbConnection.SaveDataItemTagTreeRecursive(ItemID, -1, -1, itemTag);
            }
        }

        public static void SaveDataItemTagTreeRecursive(this SqliteConnection? dbConnection, long ItemID, long PropertyID, long ParentTagID, ItemTagTreeItem itemTagTreeItem)
        {
            if (itemTagTreeItem.MarkedToDelete) return; // Remove branch marked
            if (itemTagTreeItem.TagID == -1) return; // Avoid add empty tag
            if (dbConnection == null) return;
            string command =
                $"INSERT INTO {nameof(ItemTags)} (" +
                $"{new ItemTags().ItemID}, " +
                $"{new ItemTags().PropertyID}, " +
                $"{new ItemTags().TagID}, " +
                $"{new ItemTags().ParentTagID}" +
                $") " +
                $"VALUES (" +
                $"@{new ItemTags().ItemID}, " +
                $"@{new ItemTags().PropertyID}, " +
                $"@{new ItemTags().TagID}, " +
                $"@{new ItemTags().ParentTagID}" +
                $")";
            SqliteCommand SqliteCommand = new(command, dbConnection);
            SqliteCommand.Parameters.AddWithValue($"@{new ItemTags().ItemID}", ItemID);
            SqliteCommand.Parameters.AddWithValue($"@{new ItemTags().PropertyID}", PropertyID);
            SqliteCommand.Parameters.AddWithValue($"@{new ItemTags().TagID}", itemTagTreeItem.TagID);
            SqliteCommand.Parameters.AddWithValue($"@{new ItemTags().ParentTagID}", ParentTagID);
            SqliteCommand.ExecuteNonQuery();
            foreach (ItemTagTreePropertyItem propertyItem in itemTagTreeItem.PropertyItems)
            {
                foreach (ItemTagTreeItem treeItem in propertyItem.Children)
                {
                    SaveDataItemTagTreeRecursive(dbConnection, ItemID, propertyItem.PropertyID, itemTagTreeItem.TagID, treeItem);
                }
            }
        }
    }
}
