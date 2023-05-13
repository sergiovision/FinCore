using System.Collections.Specialized;
using Autofac;
using BusinessLogic.BusinessObjects;
using BusinessObjects.BusinessObjects;
using Microsoft.Extensions.Configuration;

namespace BusinessLogic;

public class XTradeConfig
{
    private readonly IConfiguration Configuration;

    public XTradeConfig(IConfiguration config)
    {
        Configuration = config;
    }

    public static XTradeConfig Self()
    {
        var config = MainService.thisGlobal.Container.Resolve<XTradeConfig>();
        return config;
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

    private string Value(string name) 
    {
        return Configuration[name];
    }

    #region Crypto
    public string KuCoinAPIKey => Value("KuCoinAPIKey");

    public string KuCoinAPISecret => Value("KuCoinAPISecret");
    
    public string KuCoinPassPhrase => Value("KuCoinPassPhrase");
    public string KuCoinFutureAPIKey => Value("KuCoinFutureAPIKey");

    public string KuCoinFutureAPISecret => Value("KuCoinFutureAPISecret");
    
    public string KuCoinFuturePassPhrase => Value("KuCoinFuturePassPhrase");
    
    #endregion
    
}
