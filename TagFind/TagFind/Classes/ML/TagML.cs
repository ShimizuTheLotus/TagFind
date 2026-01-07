using Microsoft.Data.Sqlite;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFind.Classes.ML
{
    using DataItems = Consts.DB.UserDB.DataItems;
    using DataItemFastSearch = Consts.DB.UserDB.DataItemFastSearch;
    using ItemTags = Consts.DB.UserDB.ItemTags;

    public class TagML
    {
        private MLContext MLContext = new();
        private ITransformer Model;

        public class DocumentData
        {
            public string Content { get; set; } = string.Empty;
            public HashSet<long> TagID { get; set; } = [];
        }

        public void TrainFromData(string dbPath)
        {
            var trainingData = LoadDataFromSQLite(dbPath);

            MLContext = new();
            var dataView = MLContext.Data.LoadFromEnumerable(trainingData);
            var pipeline = MLContext.Transforms.Text
                .FeaturizeText(outputColumnName: "Features", inputColumnName: nameof(DocumentData.Content))
                .Append(MLContext.Transforms.Conversion.MapValueToKey(outputColumnName: "Label", inputColumnName: nameof(DocumentData.TagID)))
                .Append(MLContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy())
                .Append(MLContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            var model = pipeline.Fit(dataView);
            MLContext.Model.Save(model, dataView.Schema, "flat_model.zip");
        }

        private List<DocumentData> LoadDataFromSQLite(string dbPath)
        {
            var documents = new List<DocumentData>();

            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            // Get data items where SearchText is not null or empty

            Dictionary<long, DocumentData> trainSources = [];
            string command =
                $"SELECT " +
                $"{nameof(DataItemFastSearch.DataItemID)}, " +
                $"{nameof(DataItemFastSearch.RefPath)}, " +
                $"{nameof(DataItemFastSearch.SearchText)} " +
                $"FROM {nameof(DataItemFastSearch)} " +
                $"WHERE {nameof(DataItemFastSearch.RefPath)} IS NOT NULL " +
                $"AND {nameof(DataItemFastSearch.RefPath)} != ''" +
                $"AND {nameof(DataItemFastSearch.SearchText)} IS NOT NULL " +
                $"AND TRIM{nameof(DataItemFastSearch.SearchText)}) != '' ";


            using SqliteCommand cmd = new(command, connection);
            using SqliteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                DocumentData documentData = new()
                {
                    Content = reader.GetString(2)
                };
                long dataItemID = reader.GetInt64(0);
                string subCommand =
                    $"SELECT " +
                    $"{nameof(ItemTags.TagID)} " +
                    $"FROM {nameof(ItemTags)} " +
                    $"WHERE {nameof(ItemTags.ItemID)} = @{nameof(ItemTags.ItemID)}";
                using SqliteCommand subCmd = new(subCommand, connection);
                subCmd.Parameters.AddWithValue($"@{nameof(ItemTags.ItemID)}", dataItemID);
                using SqliteDataReader subReader = subCmd.ExecuteReader();
                while (subReader.Read())
                {
                    documentData.TagID.Add(subReader.GetInt64(0));
                }
                
                trainSources.TryAdd(dataItemID, documentData);
            }

            return documents;
        }
    }
}
