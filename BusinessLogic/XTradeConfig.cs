using System.Collections.Specialized;
using BusinessObjects.BusinessObjects;
using Microsoft.Extensions.Configuration;

namespace BusinessLogic
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
            var result = Configuration["ConnectionStringName"];
            return result;
        }

        public string ConnectionString()
        {
            var result = Configuration[ConnectionStringName()];
            return result;
        }

        public NameValueCollection Quartz()
        {
            var properties = new NameValueCollection();
            foreach (var pair in Configuration.GetSection("quartz").GetChildren()) properties.Add(pair.Key, pair.Value);
            return properties;
        }

        public string AngularDir()
        {
            try
            {
                var result = Configuration["AngularDir"];
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
                var result = Configuration["WebPort"];
                return short.Parse(result);
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
                var result = Configuration["DebugClientURL"];
                return result;
            }
            catch
            {
                return "http://localhost:4200";
            }
        }
    }
}