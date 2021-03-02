namespace Leads.WebApi.Framework.Models
{
    public class LeadPostModel
    {
        public string Name { get; set; }
        public string PinCode { get; set; }
        public int SubAreaId { get; set; }
        public string Address { get; set; }
        public string MobileNumber { get; set; }
        public string Email { get; set; }
    }
}
