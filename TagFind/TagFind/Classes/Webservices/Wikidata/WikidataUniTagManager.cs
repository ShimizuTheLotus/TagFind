using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Foundation;

namespace TagFind.Classes.Webservices.Wikidata
{
    /// <summary>
    /// This manager uses Wikidata as a source for UniTags. Wikidata has a large collection of unique entities with multilingual support.
    /// </summary>
    public class WikidataUniTagManager
    {
        // Reuse a single HttpClient instance for the lifetime of the application
        private HttpClient httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        /// <summary>
        /// Searches Wikidata for entities matching the provided search string.
        /// Returns a list of SearchResult or null if an exception occurred.
        /// </summary>
        public async Task<List<SearchResult>?> GetSearchResult(string searchString)
        {
            try
            {
                bool failed = false;
                WebView2 webView = new();
                await webView.EnsureCoreWebView2Async();

                // Allow JavaScript execution
                webView.CoreWebView2.Settings.IsWebMessageEnabled = true;

                TaskCompletionSource<string> tcs = new();

                TypedEventHandler<CoreWebView2, CoreWebView2WebMessageReceivedEventArgs> webMessageReceivedHandler = (sender, args) =>
                {
                    try
                    {
                        var data = args.TryGetWebMessageAsString();
                        tcs.TrySetResult(data);
                    }
                    catch (Exception ex)
                    {
                        failed = true;
                    }
                };
                webView.CoreWebView2.WebMessageReceived += webMessageReceivedHandler;

                webView.Source = new Uri($"https://www.wikidata.org/w/api.php?action=wbsearchentities&search={searchString}&language=en&limit=20&format=json");

                TypedEventHandler<WebView2, CoreWebView2NavigationCompletedEventArgs> navigationCompletedHandler = async (sender, args) =>
                {
                    if (args.IsSuccess)
                    {
                        try
                        {
                            // 方法1：直接读取页面文本（JSON页面就是纯文本）
                            var json = await webView.ExecuteScriptAsync(
                                "document.body.innerText");

                            // 移除JSON字符串的引号
                            tcs.TrySetResult(json);
                        }
                        catch (Exception ex)
                        {
                            failed = true;
                        }
                    }
                    else
                    {
                        failed = true;
                    }
                };
                webView.NavigationCompleted += navigationCompletedHandler;
                if (failed)
                {
                    webView.CoreWebView2.WebMessageReceived -= webMessageReceivedHandler;
                    webView.NavigationCompleted -= navigationCompletedHandler;
                    return null;
                }
                try
                {
                    string content = await tcs.Task;
                    content = UnescapeJsonString(content);

                    using var doc = JsonDocument.Parse(content);

                    if (!doc.RootElement.TryGetProperty("search", out JsonElement searchElement))
                    {
                        return new List<SearchResult>();
                    }

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    };

                    var results = JsonSerializer.Deserialize<List<SearchResult>>(searchElement.GetRawText(), options);

                    return results ?? new List<SearchResult>();
                }
                catch (Exception ex)
                {
                    return null;
                }
                finally
                {
                    webView.CoreWebView2.WebMessageReceived -= webMessageReceivedHandler;
                    webView.NavigationCompleted -= navigationCompletedHandler;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private string UnescapeJsonString(string json)
        {
            if (string.IsNullOrEmpty(json))
                return json;

            json = json.Trim('"');

            json = json.Replace("\\\"", "\"")
                       .Replace("\\n", "\n")
                       .Replace("\\t", "\t")
                       .Replace("\\r", "\r")
                       .Replace("\\\\", "\\");

            return json;
        }
    }
}
