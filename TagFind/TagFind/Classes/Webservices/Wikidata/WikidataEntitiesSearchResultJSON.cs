using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TagFind.Classes.Webservices.Wikidata
{
    public class WikidataEntitiesSearchResultJSON
    {
        [JsonPropertyName("searchinfo")]
        public SearchInfo SearchInfo { get; set; } = new();

        [JsonPropertyName("search")]
        public List<SearchResult> SearchResults { get; set; } = [];

        [JsonPropertyName("search-continue")]
        public int? SearchContinue { get; set; }

        [JsonPropertyName("success")]
        public int Success { get; set; }
    }

    public class SearchInfo
    {
        [JsonPropertyName("search")]
        public string Search { get; set; } = string.Empty;
    }

    public class SearchResult
    {
        [JsonPropertyName("id")]
        public string ID { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("pageid")]
        public int PageId { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("repository")]
        public string Repository { get; set; } = string.Empty;

        [JsonPropertyName("display")]
        public DisplayInfo Display { get; set; } = new();

        [JsonPropertyName("match")]
        public MatchInfo Match { get; set; } = new();

        [JsonPropertyName("aliases")]
        public List<string> Aliases { get; set; } = [];
    }

    public class DisplayInfo
    {
        [JsonPropertyName("label")]
        public LanguageValue Label { get; set; } = new();

        [JsonPropertyName("description")]
        public LanguageValue Description { get; set; } = new();
    }

    public class MatchInfo
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("language")]
        public string Language { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class LanguageValue
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;

        [JsonPropertyName("language")]
        public string Language { get; set; } = string.Empty;
    }
}
