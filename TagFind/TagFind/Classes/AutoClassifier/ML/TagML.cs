using Microsoft.Data.Sqlite;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Windows.UI;

namespace TagFind.Classes.ML
{
    using DataItemFastSearch = Consts.DB.UserDB.DataItemFastSearch;
    using DataItems = Consts.DB.UserDB.DataItems;
    using ItemTags = Consts.DB.UserDB.ItemTags;

    public class TagML
    {
        private MLContext MLContext = new();
        private ITransformer Model;

        // Raw document with associated tag IDs (multi-label)
        public class DocumentData
        {
            public string Content { get; set; } = string.Empty;
            public HashSet<long> TagID { get; set; } = new HashSet<long>();
        }

        // Helper type used for binary training / prediction per tag
        private class BinaryData
        {
            public string Content { get; set; } = string.Empty;
            public bool Label { get; set; }
        }

        private class BinaryPrediction
        {
            [ColumnName("PredictedLabel")]
            public bool PredictedLabel { get; set; }
            public float Score { get; set; }
            public float Probability { get; set; }
        }

        // Train one binary classifier per tag (simple, reliable multi-label strategy)
        public void TrainFromData(string dbPath)
        {
            var trainingData = LoadDataFromSQLite(dbPath);

            if (trainingData.Count == 0)
                throw new InvalidOperationException("No training documents found.");

            // Build tag -> index map and persist it so prediction can map outputs back to TagIDs.
            var tagList = trainingData
                .SelectMany(d => d.TagID)
                .Distinct()
                .OrderBy(id => id)
                .ToList();

            var tagMap = tagList.Select((tagId, idx) => new { tagId, idx })
                                .ToDictionary(x => x.tagId, x => x.idx);

            Directory.CreateDirectory("ml_models");
            File.WriteAllText(Path.Combine("ml_models", "tag_map.json"),
                JsonSerializer.Serialize(tagMap.Keys.ToList()));

            // Train one binary model per tag.
            MLContext = new MLContext(seed: 0);

            var textPipeline = MLContext.Transforms.Text.FeaturizeText(
                outputColumnName: "Features",
                inputColumnName: nameof(BinaryData.Content));

            foreach (var tagId in tagMap.Keys)
            {
                // Prepare binary-labeled view for this tag
                var binaryExamples = trainingData.Select(d => new BinaryData
                {
                    Content = d.Content,
                    Label = d.TagID.Contains(tagId)
                });

                var dataView = MLContext.Data.LoadFromEnumerable(binaryExamples);

                // Choose a robust binary trainer (logistic SDCA).
                var trainer = MLContext.BinaryClassification.Trainers
                    .SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features");

                var pipeline = textPipeline.Append(trainer);

                var model = pipeline.Fit(dataView);

                // Save model per tag
                string modelPath = Path.Combine("ml_models", $"tag_{tagId}.zip");
                MLContext.Model.Save(model, dataView.Schema, modelPath);
            }
        }

        // Predict tags for a single text. Loads per-tag models and returns tags with probability.
        // Returns list ordered by descending probability.
        public List<(long TagId, float Probability)> PredictTags(string text, float threshold = 0.5f)
        {
            var modelsDir = "ml_models";
            var mapPath = Path.Combine(modelsDir, "tag_map.json");
            if (!File.Exists(mapPath)) return new();

            var tagIds = JsonSerializer.Deserialize<List<long>>(File.ReadAllText(mapPath)) ?? new();
            var results = new List<(long TagId, float Probability)>();

            MLContext ??= new MLContext();

            foreach (var tagId in tagIds)
            {
                string modelPath = Path.Combine(modelsDir, $"tag_{tagId}.zip");
                if (!File.Exists(modelPath)) continue;

                ITransformer loadedModel = MLContext.Model.Load(modelPath, out var schema);
                var predEngine = MLContext.Model.CreatePredictionEngine<BinaryData, BinaryPrediction>(loadedModel);

                var pred = predEngine.Predict(new BinaryData { Content = text });

                // Use Probability if available; else use Score (Score is raw score)
                float prob = pred.Probability > 0 ? pred.Probability : Sigmoid(pred.Score);

                if (prob >= threshold)
                {
                    results.Add((tagId, prob));
                }
            }

            return results.OrderByDescending(r => r.Probability).ToList();
        }

        // Utility sigmoid for raw scores when probability is not provided by trainer.
        private static float Sigmoid(float score)
        {
            return 1f / (1f + (float)Math.Exp(-score));
        }

        // Load documents + multi-label tag sets from SQLite
        private List<DocumentData> LoadDataFromSQLite(string dbPath)
        {
            var trainSources = new Dictionary<long, DocumentData>();

            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            // Query rows that have a valid RefPath and non-empty SearchText
            string commandText =
                $"SELECT DISTINCT " +
                $"{nameof(DataItemFastSearch.DataItemID)}, " +
                $"{nameof(DataItemFastSearch.RefPath)}, " +
                $"{nameof(DataItemFastSearch.SearchText)} " +
                $"FROM {nameof(DataItemFastSearch)} " +
                $"WHERE {nameof(DataItemFastSearch.RefPath)} IS NOT NULL " +
                $"AND {nameof(DataItemFastSearch.RefPath)} != '' ";

            using var cmd = new SqliteCommand(commandText, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                long dataItemID = reader.GetInt64(0);
                string content = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);

                // Get full search text.
                string getSearchTextCommandText =
                    $"SELECT {nameof(DataItemFastSearch.SearchText)} " +
                    $"FROM {nameof(DataItemFastSearch)} " +
                    $"WHERE {nameof(DataItemFastSearch.DataItemID)} = @{nameof(DataItemFastSearch.DataItemID)}";
                using SqliteCommand getSearchTextCmd = new SqliteCommand(getSearchTextCommandText, connection);
                getSearchTextCmd.Parameters.AddWithValue($"@{nameof(DataItemFastSearch.DataItemID)}", dataItemID);
                using SqliteDataReader SearchTextReader = getSearchTextCmd.ExecuteReader();
                StringBuilder fullSearchTextBuilder = new();
                while (SearchTextReader.Read())
                {
                    fullSearchTextBuilder.AppendLine(SearchTextReader.GetString(0));
                }

                var documentData = new DocumentData
                {
                    Content = fullSearchTextBuilder.ToString().Trim()
                };

                string subCommandText =
                    $"SELECT {nameof(ItemTags.TagID)} " +
                    $"FROM {nameof(ItemTags)} " +
                    $"WHERE {nameof(ItemTags.ItemID)} = @{nameof(ItemTags.ItemID)};";

                using var subCmd = new SqliteCommand(subCommandText, connection);
                subCmd.Parameters.AddWithValue($"@{nameof(ItemTags.ItemID)}", dataItemID);
                using var subReader = subCmd.ExecuteReader();
                while (subReader.Read())
                {
                    if (!subReader.IsDBNull(0))
                        documentData.TagID.Add(subReader.GetInt64(0));
                }

                // Optionally skip untagged documents during training
                if (documentData.TagID.Count == 0)
                    continue;

                if (trainSources.TryGetValue(dataItemID, out var existing))
                {
                    foreach (var tag in documentData.TagID)
                        existing.TagID.Add(tag);
                }
                else
                {
                    trainSources[dataItemID] = documentData;
                }
            }

            return trainSources.Values.ToList();
        }
    }
}
