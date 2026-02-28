using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using NLog;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.ImportLists.Hardcover
{
    public class HardcoverImportRequestGenerator : IImportListRequestGenerator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public HardcoverImportSettings Settings { get; set; }

        public int MaxPages { get; set; } = 1;
        public int PageSize { get; set; } = 200;

        public ImportListPageableRequestChain GetListItems()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetPagedRequests());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetPagedRequests()
        {
            var apiKey = NormalizeApiKey(Settings.ApiKey);

            if (Settings.ListIds != null && Settings.ListIds.Any())
            {
                Logger.Info("Hardcover: Fetching books for lists '{0}'", string.Join(",", Settings.ListIds));

                // Query to fetch selected lists with their books and author info
                var listGraphQlBody = JsonSerializer.Serialize(new
                {
                    query = @"
                        query ListBooks($slugs: [String!]!) { me { lists(where: { slug: { _in: $slugs } } ) { slug name list_books { book { id title contributions { author { id name } } } } } } }
                    ",
                    variables = new
                    {
                        slugs = Settings.ListIds
                    }
                });

                var listRequest = new HttpRequestBuilder($"{Settings.BaseUrl.TrimEnd('/')}/v1/graphql")
                    .Post()
                    .Accept(HttpAccept.Json)
                    .SetHeader("Authorization", $"Bearer {apiKey}")
                    .SetHeader("X-Api-Key", apiKey)
                    .SetHeader("User-Agent", "Readarr (Hardcover Import)")
                    .SetHeader("Content-Type", "application/json")
                    .KeepAlive()
                    .Build();

                listRequest.SetContent(listGraphQlBody);

                yield return new ImportListRequest(listRequest);
            }

            if (Settings.Statuses != null && Settings.Statuses.Any())
            {
                Logger.Info("Hardcover: Fetching books for statuses '{0}'", string.Join(",", Settings.Statuses));

                // Query to fetch user books by status
                var statusGraphQlBody = JsonSerializer.Serialize(new
                {
                    query = @"
                        query StatusBooks($statusIds: [Int!]!) { me { user_books(where: { status_id: { _in: $statusIds } } ) { book { id title contributions { author { id name } } } } } }
                    ",
                    variables = new
                    {
                        statusIds = Settings.Statuses
                    }
                });

                var statusRequest = new HttpRequestBuilder($"{Settings.BaseUrl.TrimEnd('/')}/v1/graphql")
                    .Post()
                    .Accept(HttpAccept.Json)
                    .SetHeader("Authorization", $"Bearer {apiKey}")
                    .SetHeader("X-Api-Key", apiKey)
                    .SetHeader("User-Agent", "Readarr (Hardcover Import)")
                    .SetHeader("Content-Type", "application/json")
                    .KeepAlive()
                    .Build();

                statusRequest.SetContent(statusGraphQlBody);

                yield return new ImportListRequest(statusRequest);
            }
        }

        private string NormalizeApiKey(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return string.Empty;
            }

            var trimmed = apiKey.Trim();
            const string bearerPrefix = "bearer ";

            if (trimmed.StartsWith(bearerPrefix, System.StringComparison.OrdinalIgnoreCase))
            {
                return trimmed.Substring(bearerPrefix.Length).Trim();
            }

            return trimmed;
        }
    }
}
