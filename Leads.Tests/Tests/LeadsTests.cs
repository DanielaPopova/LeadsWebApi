using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Leads.WebApi.Framework.Common;
using Leads.WebApi.Framework.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RestSharp;

namespace Leads.Tests
{
    [TestClass]
    public class LeadsTests : TestBase
    {
        /// <summary>
        /// Verifies that Lead object is successfully created
        /// </summary>
        [TestMethod]
        public void TestCreateLeadWithValidInputData()
        {
            // Log Step 1 - Get all available Sub Areas
            var getSubAreasRequest = new RestRequest("/SubAreas", Method.GET);
            var getSubAreasResponse = RestClient.Execute<IList<SubArea>>(getSubAreasRequest);
            
            Assert.AreEqual(HttpStatusCode.OK, getSubAreasResponse.StatusCode, ErrorMessage.UnexpectedStatusCode);

            // Log Step 2 - Create Lead template
            var subArea = getSubAreasResponse.Data[0]; //Asuming we always have test data
            var lead = new LeadPostModel
            {
                Name = $"User {Utility.GenerateRandomString(8)}",
                PinCode = subArea.PinCode,
                SubAreaId = subArea.Id,
                Address = "user address",
                MobileNumber = "+359896566556",
                Email = "user_mail@abv.bg"
            };

            // Log Step 3 - Post request for Lead
            var postLeadRequest = new RestRequest("/Leads", Method.POST);
            postLeadRequest.AddJsonBody(lead);
            var postLeadResponse = RestClient.Execute(postLeadRequest);
            
            Assert.AreEqual(HttpStatusCode.OK, postLeadResponse.StatusCode, ErrorMessage.UnexpectedStatusCode);

            // Log Step 4 - Verify Lead is created
            var leadId = JsonConvert.DeserializeObject<Dictionary<string, string>>(postLeadResponse.Content);
            var getLeadRequest = new RestRequest("/Leads/{id}", Method.GET);
            getLeadRequest.AddUrlSegment("id", leadId["id"]);
            var getLeadResponse = RestClient.Execute<LeadGetModel>(getLeadRequest);
            
            Assert.AreEqual(HttpStatusCode.OK, getLeadResponse.StatusCode, ErrorMessage.UnexpectedStatusCode);

            // Log Step 5 - Verifications
            var createdLead = getLeadResponse.Data;
            
            Assert.AreEqual(lead.Name, createdLead.Name, "Names do not match.");
            Assert.AreEqual(lead.PinCode, createdLead.PinCode, "Pin Codes do not match.");
            Assert.AreEqual(lead.PinCode, createdLead.SubArea.PinCode, "PinCodes do not match.");
            Assert.AreEqual(lead.SubAreaId, createdLead.SubAreaId, "Sub Areas do not match.");
            Assert.AreEqual(lead.SubAreaId, createdLead.SubArea.Id, "Sub Areas do not match.");
            Assert.AreEqual(lead.Address, createdLead.Address, "Addresses do not match.");
            Assert.AreEqual(lead.MobileNumber, createdLead.MobileNumber, "Mobile Numbers do not match.");
            Assert.AreEqual(lead.Email, createdLead.Email, "Emails do not match.");

            // log Step 6 - Should be tear down, e.g. delete createdLead
        }

        /// <summary>
        /// Verifies that two Leads with same input data can exist in the system
        /// </summary>
        [TestMethod]
        public void TestTwoLeadsWithSameDataCanBeCreated()
        {
            // Log Step 1 - Create Lead template
            var leadTemplate = new LeadPostModel
            {
                Name = "User",
                PinCode = "567",
                SubAreaId = 4,
                Address = "user address",
                MobileNumber = "user mobile phone",
                Email = "user_mail@abv.bg"
            };

            // Log Step 2 - Create two Leads
            var ids = new string[2];
            for (int i = 0; i < ids.Length; i++)
            {
                var postLeadRequest = new RestRequest("/Leads", Method.POST);
                postLeadRequest.AddJsonBody(leadTemplate);
                var postLeadResponse = RestClient.Execute(postLeadRequest);
                var leadId = JsonConvert.DeserializeObject<Dictionary<string, string>>(postLeadResponse.Content);
                ids[i] = leadId["id"];
            }
            Assert.AreNotEqual(ids[0], ids[1], "IDs should not be the same.");

            // Log Step 3 - Get the two created Leads
            var leads = new LeadGetModel[2];
            for (int i = 0; i < leads.Length; i++)
            {
                var getLeadRequest = new RestRequest("/Leads/{id}", Method.GET);
                getLeadRequest.AddUrlSegment("id", ids[i]);
                var getLeadResponse = RestClient.Execute<LeadGetModel>(getLeadRequest);
                leads[i] = getLeadResponse.Data;
            }

            // Log Step 4 - Verify properties are equal
            // Cannot use AreSame because of the Id
            Assert.AreEqual(leads[0].Name, leads[1].Name, "Names do not match.");
            Assert.AreEqual(leads[0].PinCode, leads[1].PinCode, "Pin Codes do not match.");
            Assert.AreEqual(leads[0].SubAreaId, leads[1].SubAreaId, "Sub Areas do not match.");
            Assert.AreEqual(leads[0].SubArea.PinCode, leads[1].SubArea.PinCode, "PinCodes do not match.");
            Assert.AreEqual(leads[0].SubArea.Id, leads[1].SubArea.Id, "Sub Areas do not match.");
            Assert.AreEqual(leads[0].Address, leads[1].Address, "Addresses do not match.");
            Assert.AreEqual(leads[0].MobileNumber, leads[1].MobileNumber, "Mobile Numbers do not match.");
            Assert.AreEqual(leads[0].Email, leads[1].Email, "Emails do not match.");
        }

        /// <summary>
        /// Verifies that Lead object cannot be created if SubArea's pinCode or subAreaId are non-existent
        /// </summary>
        /// <param name="pinCode"></param>
        /// <param name="subAreaId"></param>
        [TestMethod]
        [DataRow("123", 20)]
        [DataRow("20", 1)]
        public void TestCreateLeadWithInvalidSubAreaData(string pinCode, int subAreaId)
        {
            // Log Step 1 - Create Lead template
            var lead = new LeadPostModel
            {
                Name = $"User {Utility.GenerateRandomString(8)}",
                PinCode = pinCode,
                SubAreaId = subAreaId,
                Address = "user address",
                MobileNumber = "+359896566556",
                Email = "user_mail@abv.bg"
            };

            // Log Step 2 - Send Post request for Lead with invalid SubArea data
            var postLeadRequest = new RestRequest("/Leads", Method.POST);
            postLeadRequest.AddJsonBody(lead);
            var postLeadResponse = RestClient.Execute<ResponseError>(postLeadRequest);
            var responseError = postLeadResponse.Data;
            
            Assert.AreEqual(HttpStatusCode.BadRequest, postLeadResponse.StatusCode, ErrorMessage.UnexpectedStatusCode);
            Assert.AreEqual("SubArea is invalid", responseError.Message, ErrorMessage.UnexpectedResponseMessage);
        }

        /// <summary>
        /// Verifies that Lead object cannot be created with empty string as name, pinCode or address
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pinCode"></param>
        /// <param name="address"></param>
        /// <param name="mobileNumber"></param>
        /// <param name="email"></param>
        /// <param name="errorMessage"></param>
        [TestMethod]
        [DataRow("", "123", "Sofia", "+359894566556", "user_test@abv.bg", "Name cannot be empty\r\nParameter name: Name")]
        [DataRow("user", "", "Sofia", "+359894566556", "user_test@abv.bg", "PinCode cannot be empty\r\nParameter name: PinCode")]
        [DataRow("user", "123", "", "+359894566556", "user_test@abv.bg", "Address cannot be empty\r\nParameter name: Address")]
        public void TestCreateLeadWithEmptyInputData(string name, string pinCode, string address,
            string mobileNumber, string email, string errorMessage)
        {
            // Log Step 1 - Create Lead template
            var lead = new LeadPostModel
            {
                Name = name,
                PinCode = pinCode,
                SubAreaId = 1,
                Address = address,
                MobileNumber = mobileNumber,
                Email = email
            };

            // Log Step 2 - Send Post request for Lead with empty name/pinCode/address
            var postLeadRequest = new RestRequest("/Leads", Method.POST);
            postLeadRequest.AddJsonBody(lead);
            var postLeadResponse = RestClient.Execute<ResponseError>(postLeadRequest);
            var responseError = postLeadResponse.Data;

            Assert.AreEqual(HttpStatusCode.BadRequest, postLeadResponse.StatusCode, ErrorMessage.UnexpectedStatusCode);
            Assert.AreEqual(errorMessage, responseError.Message, ErrorMessage.UnexpectedResponseMessage);
        }

        /// <summary>
        /// Verifies correct response status code when Lead object is searched by non-existent id
        /// </summary>
        [TestMethod]
        public void TestResponseStatusCodeOfNonExistentLead()
        {
            // Log Step 1 - Send Get request with non-existent id to Lead
            var nonExistentId = Guid.NewGuid();
            var getLeadRequest = new RestRequest("/Leads/{id}", Method.GET);
            getLeadRequest.AddUrlSegment("id", nonExistentId.ToString());
            var getLeadResponse = RestClient.Execute(getLeadRequest);

            Assert.AreEqual(HttpStatusCode.NotFound, getLeadResponse.StatusCode, ErrorMessage.UnexpectedStatusCode);
        }

        /// <summary>
        /// Verifies that no Sub Areas will be displayed by invalid SubAreaId
        /// </summary>
        [TestMethod]
        public void TestNoResultsAreShownWithInvalidSubAreaId()
        {
            // Log Step 1 - Send Get request with non-existent pinCode to all Sub Areas
            var invalidSubAreaId = Utility.GenerateRandomString(5);
            var getSubAreaRequest = new RestRequest("SubAreas/Filter/PinCode/{pinCode}", Method.GET);
            getSubAreaRequest.AddUrlSegment("pinCode", invalidSubAreaId);
            var getSubAreaResponse = RestClient.Execute<IList<SubArea>>(getSubAreaRequest);

            Assert.AreEqual(HttpStatusCode.OK, getSubAreaResponse.StatusCode, ErrorMessage.UnexpectedStatusCode);
            Assert.IsTrue(!getSubAreaResponse.Data.Any(), "SubArea collection is not empty.");
        }
    }
}
