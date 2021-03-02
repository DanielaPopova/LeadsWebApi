using System;

namespace Leads.WebApi.Framework.Models
{
    public class LeadGetModel : LeadPostModel
    {
        public string Id { get; set; }
        public SubArea SubArea { get; set; }
    }
}
