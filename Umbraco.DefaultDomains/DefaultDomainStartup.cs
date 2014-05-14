using Umbraco.Core;

namespace Umbraco.DefaultDomains
{
    public class DefaultDomainStartup : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            base.ApplicationStarted(umbracoApplication, applicationContext);

            DefaultDomainRedirection.Register();
        }
    }
}
