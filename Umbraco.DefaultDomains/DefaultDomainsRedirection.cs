using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Models;
using UrlRewritingNet.Web;

namespace Umbraco.DefaultDomains
{
    public class DefaultDomainRedirection
    {
        public static void Register()
        {
            UmbracoApplicationBase.ApplicationInit += AddRedirects;
        }

        private static void AddRedirects(object sender, EventArgs e)
        {
            if (HttpContext.Current.IsDebuggingEnabled)
                return;

            try
            { 
                if (HttpContext.Current.Request.Url.Host.Equals("localhost", StringComparison.CurrentCultureIgnoreCase))
                    return;
            }
            catch { }

            var firstName = FindExistingRedirect();
            var contentDomains = ContentDomains.GetContentDomains();

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

        private static void AddRules(IGrouping<IContent, ContentDomains.ContentDomain> group, string firstName)
        {
            var defaultDomain = ContentDomains.GetDefaultDomain(group.Key);

            if (String.IsNullOrWhiteSpace(defaultDomain))
                return;

            foreach (var contentDomain in group)
                AddRule(contentDomain, defaultDomain, firstName);
        }

        private static void AddRule(ContentDomains.ContentDomain contentDomain, string defaultDomain, string firstName)
        {
            var contentDomainName = ContentDomains.SchemeExpr.Replace(contentDomain.Domain.Name, "").TrimEnd('/');
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
    }
}