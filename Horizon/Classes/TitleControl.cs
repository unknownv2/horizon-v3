using System.Diagnostics.Contracts;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NoDev.XContent;
using Newtonsoft.Json;

namespace NoDev.Horizon
{
    internal static class TitleControl
    {
        [Pure]
        internal static uint GetProperTitleID(XContentPackage package)
        {
            uint titleId = package.Header.Metadata.ExecutionId.TitleID;
            if (titleId != 0)
                return titleId;

            switch (package.Header.Metadata.TitleName)
            {
                case "FIFA 11":
                    return 0x12345678;
            }

            return 0;
        }

        private const string MarketplaceURL = "http://marketplace.xbox.com/{0}/SiteSearch/xbox/";
        internal static async Task<dynamic> MarketplaceQuery(string region, string titleName, int itemsPerPage)
        {
            var searchClient = new WebClient();
            searchClient.Encoding = Encoding.UTF8;
            searchClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.1 (KHTML, like Gecko) Chrome/21.0.1281.55 Safari/537.1");
            searchClient.QueryString.Add("PageSize", itemsPerPage.ToString(CultureInfo.InvariantCulture));
            searchClient.QueryString.Add("query", titleName);

            string jsonResult = await searchClient.DownloadStringTaskAsync(string.Format(MarketplaceURL, region));

            return JsonConvert.DeserializeObject(jsonResult);
        }
    }
}
