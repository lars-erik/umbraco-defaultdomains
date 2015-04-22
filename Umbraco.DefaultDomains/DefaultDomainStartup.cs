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
            DefaultDomainMode mode;
            Enum.TryParse(modeString, out mode);

            if (mode == DefaultDomainMode.Redirect)
                throw new Exception("Vi bruker ikke Redirect lenger");
                //DefaultDomainRedirection.Register();

            DefaultDomainCanonicalHeaders.Register(umbracoApplication, applicationContext);
        }
    }

    public enum DefaultDomainMode
    {
        CanonicalHeader,
        Redirect
    }
}
