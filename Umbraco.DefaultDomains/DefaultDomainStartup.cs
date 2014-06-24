using System;
using System.Configuration;
using Umbraco.Core;

namespace Umbraco.DefaultDomains
{
    public class DefaultDomainStartup : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            base.ApplicationStarted(umbracoApplication, applicationContext);

            var modeString = ConfigurationManager.AppSettings["Umbraco.DefaultDomains.Mode"];
            var mode = DefaultDomainMode.CanonicalHeader;
            Enum.TryParse(modeString, out mode);

            if (mode == DefaultDomainMode.Redirect)
                DefaultDomainRedirection.Register();
            else
                DefaultDomainCanonicalHeaders.Register(umbracoApplication, applicationContext);
        }
    }

    public enum DefaultDomainMode
    {
        Redirect,
        CanonicalHeader
    }
}
