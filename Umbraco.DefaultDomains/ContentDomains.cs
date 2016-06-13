using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.DefaultDomains.Controls;

namespace Umbraco.DefaultDomains
{
    static internal class ContentDomains
    {
        private const string PropertyEditorAlias = "defaultdomains.editor";
        public static readonly Regex SchemeExpr = new Regex("(?:https?(?:://))+");
        public static readonly bool IsUmbraco7 = typeof(PropertyType).GetProperty("PropertyEditorAlias") != null;

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

        private static IContentService ContentService
        {
            get { return ApplicationContext.Current.Services.ContentService; }
        }

        private static IEnumerable<IContent> GetRootContent(IEnumerable<Domain> domains)
        {
            var rootIds = domains.Select(d => d.RootNodeId).Distinct();
            return rootIds.Select(id => ContentService.GetById(id));
        }

        public static string GetDefaultDomain(IContentBase root)
        {
            var propType = root.PropertyTypes.FirstOrDefault(pt => pt.DataTypeId == DefaultDomainsLegacyControl.DataTypeId);
            if (propType == null && IsUmbraco7)
                propType = root.PropertyTypes.FirstOrDefault(pt => pt.PropertyEditorAlias.Equals(PropertyEditorAlias, System.StringComparison.InvariantCultureIgnoreCase));
            if (propType == null)
                return null;
            var prop = root.Properties.FirstOrDefault(p => p.Alias == propType.Alias);
            if (prop == null)
                return null;
            if (prop.Value == null)
                return null;

            return (string)prop.Value;
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

    }
}