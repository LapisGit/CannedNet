using System.Text.Json.Serialization;

namespace CannedNet.Models;

public class BuyItemRequest
{
    [JsonPropertyName("StorefrontType")]
    public int StorefrontType { get; set; }
    
    [JsonPropertyName("PurchasableItemId")]
    public int PurchasableItemId { get; set; }
    
    [JsonPropertyName("CurrencyType")]
    public int CurrencyType { get; set; }
    
    [JsonPropertyName("CouponConsumablePlayerMappingId")]
    public int? CouponConsumablePlayerMappingId { get; set; }
    
    [JsonPropertyName("Gift")]
    public object? Gift { get; set; }
}

