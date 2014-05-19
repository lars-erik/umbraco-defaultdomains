using System.Collections.Generic;
using System.Linq;
using umbraco.cms.businesslogic.web;
using Umbraco.Web.WebApi;

namespace Umbraco.DefaultDomains.Controllers
{
    [UmbracoAuthorize]
    public class DefaultDomainsController : UmbracoApiController
    {
        public IEnumerable<string> GetDomains(int id)
        {
            return Domain.GetDomainsById(id).Select(d => d.Name);
        }
    }
}
