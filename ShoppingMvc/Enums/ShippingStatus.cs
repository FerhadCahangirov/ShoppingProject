using System.ComponentModel;

namespace ShoppingMvc.Enums
{
    public enum ShippingStatus
    {
        [Description("Confirmed")]
        Confirmed,
        [Description("Processing")]
        Processing,
        [Description("Quality Check")]
        QualityCheck,
        [Description("Product Dispatched")]
        ProductDispatched,
        [Description("Product Delivered")]
        ProductDelivered
    }
}
