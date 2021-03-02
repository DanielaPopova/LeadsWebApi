using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.Configuration;

namespace Leads.Tests
{
    public abstract class TestBase
    {
        public IRestClient RestClient { get; private set; }

        [TestInitialize]
        public void SetUp()
        {
            RestClient = new RestClient(ConstructUri());
            RestClient.UseNewtonsoftJson();
        }

        private Uri ConstructUri()
        {
            var protocol = ConfigurationManager.AppSettings["protocol"];
            if (string.IsNullOrEmpty(protocol))
                throw new ArgumentException(nameof(protocol));

            var host = ConfigurationManager.AppSettings["host"];
            if (string.IsNullOrEmpty(host))
                throw new ArgumentException(nameof(host));

            var port = ConfigurationManager.AppSettings["portIIS"];
            if (string.IsNullOrEmpty(port))
                throw new ArgumentException(nameof(port));

            var basePath = ConfigurationManager.AppSettings["basePath"];
            if (string.IsNullOrEmpty(basePath))
                throw new ArgumentException(nameof(basePath));

            return new UriBuilder(protocol, host, int.Parse(port), basePath).Uri;
        }
    }
}
