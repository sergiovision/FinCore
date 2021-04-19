using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Specialized;

namespace BusinessObjects
{
    public class XTradeConfig
    {
        private readonly IConfiguration Configuration;
        public XTradeConfig(IConfiguration config)
        {
            Configuration = config;
        }

        public string ConnectionStringName()
        {
            string result = Configuration["ConnectionStringName"];
            return result;
        }

        public string ConnectionString()
        {
            string result = Configuration[ConnectionStringName()];
            return result;
        }

        public NameValueCollection Quartz()
        {
            var properties = new NameValueCollection();
            foreach (var pair in Configuration.GetSection("quartz").GetChildren())
            {
                properties.Add(pair.Key, pair.Value);
            }
            return properties;
        }

        public string AngularDir()
        {
            try
            {
                string result = Configuration["AngularDir"];
                return result;
            }
            catch
            {
                return xtradeConstants.ANGULAR_DIR;
            }
        }

        public short WebPort()
        {
            try
            {
                string result = Configuration["WebPort"];
                return Int16.Parse(result);
            }
            catch
            {
                return xtradeConstants.WebBackend_PORT;
            }
        }

        public string DebugClientURL()
        {
            try
            {
                string result = Configuration["DebugClientURL"];
                return result;
            }
            catch
            {
                return "http://localhost:4200";
            }
        }


    }
}
