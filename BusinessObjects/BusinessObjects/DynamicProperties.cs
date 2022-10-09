using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

namespace BusinessObjects.BusinessObjects;

public enum EntitiesEnum
{
    Undefined = 0,
    Account = 1,
    Settings = 2,
    Adviser = 3,
    MetaSymbol = 4,
    Symbol = 5,
    Terminal = 6,
    Deals = 7,
    Jobs = 8,
    ExpertCluster = 9,
    Wallet = 10,
    AccountState = 11,
    Rates = 12,
    Country = 13, //
    Currency = 14, //
    NewsEvent = 15, //
    Person = 16, //
    Site = 17, //
    Position = 18,
    Indicators = 19
}

public class DynamicProperties
{
    public int ID { get; set; }
    public int objId { get; set; }
    public short entityType { get; set; }
    public string Vals { get; set; }
    public DateTime? updated { get; set; }
}

public class DynamicProperty
{
    public string type;
    public string value;
}

public class TDynamicProperty<T>
{
    public string type;
    public T value;
}

public class DefaultProperties
{
    public static Dictionary<string, DynamicProperty> fillProperties(ref Dictionary<string, DynamicProperty> result,
        EntitiesEnum etype, int id, int objid, string value)
    {
        var p1 = new DynamicProperty
        {
            type = "integer",
            value = id.ToString()
        };
        if (!result.ContainsKey("ID"))
            result.Add("ID", p1);
        else
            result["ID"] = p1;
        var p2 = new DynamicProperty
        {
            type = "integer",
            value = objid.ToString()
        };
        if (!result.ContainsKey("ObjectID"))
            result.Add("ObjectID", p2);
        else
            result["ObjectID"] = p2;
        var p3 = new DynamicProperty
        {
            type = "integer"
        };
        return result;
    }

    public static int RGBtoInt(int r, int g, int b)
    {
        return (r << 0) | (g << 8) | (b << 16);
    }

    public static Dictionary<string, object> transformProperties(Dictionary<string, DynamicProperty> dbProps)
    {
        var result = new Dictionary<string, object>();
        if (dbProps == null || dbProps.Count == 0)
            return result;
        foreach (var prop in dbProps)
            switch (prop.Value.type)
            {
                case "integer":
                    result.Add(prop.Key, int.Parse(prop.Value.value));
                    break;
                case "hexinteger":
                {
                    var hexValue = prop.Value.value;
                    if (hexValue.StartsWith("#"))
                        hexValue = hexValue.Substring(1);
                    var value = int.Parse(hexValue, NumberStyles.HexNumber);
                    var c = Color.FromArgb(value);
                    value = RGBtoInt(c.R, c.G, c.B);
                    result.Add(prop.Key, value);
                }
                    break;
                case "double":
                    result.Add(prop.Key, double.Parse(prop.Value.value));
                    break;
                case "boolean":
                    result.Add(prop.Key, bool.Parse(prop.Value.value));
                    break;
                default:
                    result.Add(prop.Key, prop.Value.value);
                    break;
            }

        return result;
    }
}
