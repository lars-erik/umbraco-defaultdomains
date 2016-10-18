using System.Collections.Generic;
using System.Linq;
using umbraco.cms.businesslogic.web;
using Umbraco.Web.WebApi;

namespace Umbraco.DefaultDomains.Controllers
{
    public class DefaultDomainsController : UmbracoAuthorizedApiController
    {
        public IEnumerable<string> GetDomains(int id)
        {
            return Domain.GetDomainsById(id).Select(d => d.Name);
        }
    }
}
