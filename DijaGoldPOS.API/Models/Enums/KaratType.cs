using System.ComponentModel;

namespace DijaGoldPOS.API.Models.Enums;

/// <summary>
/// Represents the karat types supported in the Egyptian gold market
/// </summary>
public enum KaratType
{
    /// <summary>18 Karat Gold</summary>
    [Description("18K Gold")]
    K18 = 18,
    
    /// <summary>21 Karat Gold (Popular in Egypt)</summary>
    [Description("21K Gold")]
    K21 = 21,
    
    /// <summary>22 Karat Gold</summary>
    [Description("22K Gold")]
    K22 = 22,
    
    /// <summary>24 Karat Gold (Pure Gold)</summary>
    [Description("24K Gold")]
    K24 = 24
}