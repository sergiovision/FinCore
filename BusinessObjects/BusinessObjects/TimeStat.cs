using System;

namespace BusinessObjects.BusinessObjects;

public enum TimePeriod
{
    Daily,
    Weekly,
    Monthly
}

public class TimeStat
{
    public DateTime Date { get; set; }
    public int X { get; set; }
    public TimePeriod Period { get; set; }
    public decimal CheckingValue { get; set; }
    public decimal InvestingValue { get; set; }
    public decimal CheckingChange { get; set; }
    public decimal InvestingChange { get; set; }
    public decimal Gains { get; set; }
    public decimal Losses { get; set; }
}
