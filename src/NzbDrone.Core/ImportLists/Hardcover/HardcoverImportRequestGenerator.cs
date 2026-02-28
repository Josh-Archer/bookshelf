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
            var slugs = Settings.ListIds?.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray() ?? System.Array.Empty<string>();
            var statuses = Settings.Statuses?.ToArray() ?? System.Array.Empty<int>();

            if (slugs.Length == 0 && statuses.Length == 0)
            {
                yield break;
            }

            if (slugs.Length > 0)
            {
                Logger.Info("Hardcover: Fetching books for lists '{0}'", string.Join(",", slugs));
            }

            if (statuses.Length > 0)
            {
                Logger.Info("Hardcover: Fetching books for statuses '{0}'", string.Join(",", statuses));
            }

            // Single request for both list and status books. This avoids losing status results when
            // list results are empty due to import list paging semantics that stop after the first short page.
            var graphQlBody = JsonSerializer.Serialize(new
            {
                query = @"
                    query ListAndStatusBooks($slugs: [String!]!, $statusIds: [Int!]!) {
                        me {
                            lists(where: { slug: { _in: $slugs } }) {
                                slug
                                name
                                list_books {
                                    book {
                                        id
                                        title
                                        contributions {
                                            author { id name }
                                        }
                                    }
                                }
                            }
                            user_books(where: { status_id: { _in: $statusIds } }) {
                                book {
                                    id
                                    title
                                    contributions {
                                        author { id name }
                                    }
                                }
                            }
                        }
                    }
                ",
                variables = new
                {
                    slugs,
                    statusIds = statuses
                }
            });

            var request = new HttpRequestBuilder($"{Settings.BaseUrl.TrimEnd('/')}/v1/graphql")
                .Post()
                .Accept(HttpAccept.Json)
                .SetHeader("Authorization", $"Bearer {apiKey}")
                .SetHeader("X-Api-Key", apiKey)
                .SetHeader("User-Agent", "Readarr (Hardcover Import)")
                .SetHeader("Content-Type", "application/json")
                .KeepAlive()
                .Build();

            request.SetContent(graphQlBody);

            yield return new ImportListRequest(request);
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
