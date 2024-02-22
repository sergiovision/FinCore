using System;
using System.ComponentModel.Design.Serialization;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BusinessObjects.BusinessObjects;

public class SignalInfo
{
    public int Id { get; set; }

    [JsonIgnore]
    public EnumSignals SignalName => (EnumSignals) Id;

    [JsonIgnore]
    public SignalFlags Flag => (SignalFlags) Flags;
    
    public long Flags { get; set; }

    public long ObjectId { get; set; }

    public long ChartId { get; set; }

    public string Sym { get; set; }

    public int Value { get; set; }
    
    public string Key  { get; set; }
    
    public bool Handled  { get; set; }
    
    public string Data { get; private set; }

    public void SetData(string strData)
    {
        Data = strData;
    }

    public static SignalInfo Create(string jsonString)
    {
        SignalInfo sig = new SignalInfo();
        if (string.IsNullOrEmpty(jsonString))
            return sig;
        JToken mainToken = JToken.Parse(jsonString);
        foreach (JProperty prop in mainToken.Children<JProperty>())
        {
            try
            {
                //Class1.LogMessage(prop.Name + "=" + prop.Value);
                switch (prop.Name)
                {
                    case "Id":
                    {
                        int.TryParse(prop.Value.ToString(), out int  val);
                        sig.Id = val;
                    }
                        break;
                    case "Flags":
                    {
                        long.TryParse(prop.Value.ToString(), out long val);
                        sig.Flags = val;
                    }
                        break;
                    case "ObjectId":
                    {
                        long.TryParse(prop.Value.ToString(), out long val);
                        sig.ObjectId = val;
                    }
                        break;
                    case "ChartId":
                    {
                        long.TryParse(prop.Value.ToString(), out long val);
                        sig.ChartId = val;
                    }
                        break;
                    case "Sym":
                    {
                        sig.Sym = prop.Value.ToString();
                    }
                        break;
                    case "Key":
                    {
                        sig.Key = prop.Value.ToString();
                    }
                        break;
                    case "Value":
                    {
                        int.TryParse(prop.Value.ToString(), out int val);
                        sig.Value = val;
                    }
                        break;
                    case "Data":
                        sig.Data = prop.Value.ToString();
                        break;
                }
                
            }
            catch (Exception e)
            {
                System.Console.WriteLine($"Failed parse SignalInfo: {prop} {e}");
            }
        }
        return sig;
    }
}
