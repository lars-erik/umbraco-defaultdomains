using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.DefaultDomains
{
    static internal class ContentDomains
    {
        public static IEnumerable<IGrouping<IContent, ContentDomain>> GetContentDomains()
        {
            var domains = Domain.GetDomains().ToList();
            var contentDomains = domains.
                Join(
                    GetRootContent(domains),
                    d => d.RootNodeId,
                    c => c.Id,
                    (d, c) => new ContentDomain(d, c)
                )
                .GroupBy(cd => cd.Content);
            return contentDomains;
        }

        public class ContentDomain
        {
            public Domain Domain { get; set; }
            public IContent Content { get; set; }

            public ContentDomain(Domain domain, IContent content)
            {
                Domain = domain;
                Content = content;
            }
        }

        private static IContentService ContentService
        {
            get { return ApplicationContext.Current.Services.ContentService; }
        }

        private static IEnumerable<IContent> GetRootContent(IEnumerable<Domain> domains)
        {
            var rootIds = domains.Select(d => d.RootNodeId).Distinct();
            return ContentService.GetByIds(rootIds);
        }

        private const string PropertyEditorAlias = "defaultdomains.editor";

        public static string GetDefaultDomain(IContentBase root)
        {
            var propType = root.PropertyTypes.FirstOrDefault(pt => pt.PropertyEditorAlias == PropertyEditorAlias);
            if (propType == null)
                return null;
            var prop = root.Properties.FirstOrDefault(p => p.Alias == propType.Alias);
            if (prop == null)
                return null;
            if (prop.Value == null)
                return null;

            return SchemeExpr.Replace((string)prop.Value, "");
        }

        public static readonly Regex SchemeExpr = new Regex("(?:https?(?:://))+");
    }
}