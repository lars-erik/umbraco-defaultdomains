using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using UrlRewritingNet.Web;

namespace Umbraco.DefaultDomains
{
    public class DefaultDomainRedirection
    {
        private const string PropertyEditorAlias = "defaultdomains.editor";
        private static readonly Regex SchemeExpr = new Regex("(?:https?(?:://))+");

        private static IContentService ContentService
        {
            get { return ApplicationContext.Current.Services.ContentService; }
        }

        public static void Register()
        {
            UmbracoApplicationBase.ApplicationInit += AddRedirects;
        }

        private static void AddRedirects(object sender, EventArgs e)
        {
            var firstName = FindExistingRedirect();
            var contentDomains = GetContentDomains();

            foreach (var group in contentDomains)
                AddRules(group, firstName);

#if DEBUG
            var debugString = GetDebugString();
#endif
        }

        private static string GetDebugString()
        {
            var rewrites = GetRewrites();
            return String.Join(Environment.NewLine,
                rewrites.Cast<RegExRewriteRule>().Select(r => String.Format("{0} => {1}", r.VirtualUrl, r.DestinationUrl)));
        }

        private static void AddRules(IGrouping<IContent, ContentDomain> group, string firstName)
        {
            var defaultDomain = GetDefaultDomain(group.Key);

            if (String.IsNullOrWhiteSpace(defaultDomain))
                return;

            foreach (var contentDomain in group)
                AddRule(contentDomain, defaultDomain, firstName);
        }

        private static void AddRule(ContentDomain contentDomain, string defaultDomain, string firstName)
        {
            var contentDomainName = SchemeExpr.Replace(contentDomain.Domain.Name, "");
            if (contentDomainName.Equals(defaultDomain, StringComparison.InvariantCultureIgnoreCase))
                return;

            var key = String.Format("{0}.PreferredDomain.{1}", contentDomain.Content.Id, contentDomain.Domain.Name);
            var rule = CreateRule(contentDomainName, defaultDomain);

            if (firstName == null)
                UrlRewriting.AddRewriteRule(key, rule);
            else
                UrlRewriting.InsertRewriteRule(firstName, key, rule);
        }

        private static RegExRewriteRule CreateRule(string contentDomainName, string defaultDomain)
        {
            var virtualUrl = String.Format("//{0}/(.*)", contentDomainName);
            var destinationUrl = String.Format("//{0}/$1", defaultDomain);
            var rule = new RegExRewriteRule
            {
                Redirect = RedirectOption.Domain,
                RedirectMode = RedirectModeOption.Permanent,
                IgnoreCase = true,
                RewriteUrlParameter = RewriteUrlParameterOption.IncludeQueryStringForRewrite,
                VirtualUrl = virtualUrl,
                DestinationUrl = destinationUrl
            };
            return rule;
        }

        private static string FindExistingRedirect()
        {
            var rewrites = GetRewrites();

            if (rewrites.Any())
                return rewrites.First().Name;

            return null;
        }

        private static IList<RewriteRule> GetRewrites()
        {
            var module = (UrlRewriteModule)HttpContext.Current.ApplicationInstance.Modules["UrlRewriteModule"];
            var rewrites = (List<RewriteRule>)module.GetType().GetProperty("Redirects", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(module);
            return rewrites;
        }

        private static IEnumerable<IGrouping<IContent, ContentDomain>> GetContentDomains()
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

        private static IEnumerable<IContent> GetRootContent(IEnumerable<Domain> domains)
        {
            var rootIds = domains.Select(d => d.RootNodeId).Distinct();
            return ContentService.GetByIds(rootIds);
        }

        private static string GetDefaultDomain(IContentBase root)
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

        private class ContentDomain
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