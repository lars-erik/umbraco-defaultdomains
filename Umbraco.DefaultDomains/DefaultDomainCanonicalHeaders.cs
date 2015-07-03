using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web;
using Umbraco.Web.Routing;

namespace Umbraco.DefaultDomains
{
    public class DefaultDomainCanonicalHeaders
    {
        private const StringComparison IgnoreCase = StringComparison.InvariantCultureIgnoreCase;

        public static void Register(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            PublishedContentRequest.Prepared += AddCanonicalHeader;
        }

        private static void AddCanonicalHeader(object sender, EventArgs e)
        {
            var contentRequest = ((PublishedContentRequest) sender);

            if (!contentRequest.HasPublishedContent)
            { 
                LogHelper.Debug<DefaultDomainCanonicalHeaders>("No published request");
                return;
            }

            var allDomains = Domain.GetDomains();
            var nodesWithDomain = contentRequest.PublishedContent.Ancestors()
                .Reverse()
                .Where(c => allDomains.Any(d => d.RootNodeId == c.Id));
            var domainNode = nodesWithDomain.FirstOrDefault();

            if (domainNode == null)
            {
                LogHelper.Debug<DefaultDomainCanonicalHeaders>("No domain node for " + contentRequest.PublishedContent.Name);
                return;
            }

            var domainContent = UmbracoContext.Current.Application.Services.ContentService.GetById(domainNode.Id);
            var defaultDomain = ContentDomains.GetDefaultDomain(domainContent);

            if (String.IsNullOrWhiteSpace(defaultDomain))
            {
                LogHelper.Debug<DefaultDomainCanonicalHeaders>("No default domain for " + contentRequest.PublishedContent.Name);
                return;
            }

            var application = HttpContext.Current;

            var canonicalUrl = new Uri(
                new Uri("http://" + defaultDomain),
                RequestUrl(application).GetComponents(UriComponents.PathAndQuery, UriFormat.UriEscaped)
                );
            LogHelper.Debug<DefaultDomainCanonicalHeaders>("Found canonical url for " + contentRequest.PublishedContent.Name);
            application.Response.AddHeader("Link", String.Format("<{0}>; rel=\"canonical\"", canonicalUrl));
        }

        private static bool IsCurrentDomain(HttpContext application, ContentDomains.ContentDomain c)
        {
            return RequestUrl(application).Host.Equals(c.Domain.Name, IgnoreCase);
        }

        private static Uri RequestUrl(HttpContext application)
        {
            return application.Request.Url;
        }
    }
}
