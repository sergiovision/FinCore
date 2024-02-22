using System;
using System.Collections.Concurrent;
using System.Linq;
using BusinessObjects.BusinessObjects;

namespace BusinessLogic;

public class SignalsCache
{
    private static SignalsCache instance = null;
    private static readonly object padlock = new object();
    private ConcurrentDictionary<string, SignalInfo> signals = new ConcurrentDictionary<string, SignalInfo>();

    // Private constructor to prevent instantiation from outside
    private SignalsCache() { }

    // Public static method to get the instance
    public static SignalsCache Instance
    {
        get
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new SignalsCache();
                }
                return instance;
            }
        }
    }

    public void AddSignal(string key, SignalInfo value)
    {
        signals.TryAdd(key, value);
    }

    public bool DeleteSignal(string key)
    {
        return signals.TryRemove(key, out var removedValue);
    }

    public SignalInfo Get(string key)
    {
        signals.TryGetValue(key, out var value);
        return value;
    }

    public bool Contains(string key)
    {
        return signals.ContainsKey(key);
    }
    
    public void Clear()
    {
        signals.Clear();
    }

    // Method to delete signals by ObjectId
    public void DeleteSignalByObjId(int objId)
    {
        var keysToDelete = signals.Where(pair => pair.Value.ObjectId == objId).Select(pair => pair.Key).ToList();
        foreach (var key in keysToDelete)
        {
            signals.TryRemove(key, out var removedValue);
        }
    }
    
    public void DeleteSignalBySymbol(string symbol)
    {
        var keysToDelete = signals.Where(pair => pair.Value.Sym.Equals(symbol, StringComparison.InvariantCultureIgnoreCase)).Select(pair => pair.Key).ToList();
        foreach (var key in keysToDelete)
        {
            signals.TryRemove(key, out var removedValue);
        }
    }


}
