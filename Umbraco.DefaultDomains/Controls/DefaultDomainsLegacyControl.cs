using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.datatype;

namespace Umbraco.DefaultDomains.Controls
{
    public class DefaultDomainsLegacyControl : AbstractDataEditor
    {
        public static readonly Guid DataTypeId = new Guid("7C409A3E-DBD9-4E8B-AE13-FCD943CED7BF");
        public const string Name = "Default Domains Legacy Control";

        private DropDownList control = new DropDownList();

        public override Guid Id
        {
            get { return DataTypeId; }
        }

        public override string DataTypeName
        {
            get { return Name; }
        }

        public DefaultDomainsLegacyControl()
        {
            RenderControl = control;
            control.Init += OnInit;
            DataEditorControl.OnSave += OnSave;
        }

        private void OnInit(object sender, EventArgs e)
        {
            var data = Data as DefaultData;
            if (data == null)
                return;

            var value = data.Value as string;
            var domains = GetDomains(data.NodeId).ToList();
            domains.Insert(0, "");
            control.DataSource = domains;
            control.DataBind();

            if (!String.IsNullOrWhiteSpace(value) && domains.Contains(value))
                control.SelectedValue = value;
        }

        private void OnSave(EventArgs e)
        {
            Data.Value = control.SelectedValue;
        }

        private IEnumerable<string> GetDomains(int id)
        {
            return Domain.GetDomainsById(id).Select(d => d.Name);
        }
    }
}
