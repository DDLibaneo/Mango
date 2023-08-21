namespace Mango.Services.ShoppingCartAPI.Models.Dto.In
{
    public class CartCouponDto
    {
        public string? UserId { get; set; }

        public string? CouponCode { get; set; }

        public double Discount { get; set; }
    }
}