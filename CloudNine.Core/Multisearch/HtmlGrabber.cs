using System.Threading.Tasks;

using CloudNine.Core.Http;

namespace CloudNine.Core.Multisearch
{
    public class HtmlGrabber
    {
        protected FanfictionClient Web { get; init; }

        public HtmlGrabber(FanfictionClient client)
        {
            this.Web = client;
        }

        protected async Task<string> GetHtmlAsync(string url)
        {
            string html = await Web.Client.GetStringAsync(url);

            return html;
        }
    }
}
