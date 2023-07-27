namespace Mango.Services.ShoppingCartAPI.Models.Dto.In
{
    public class CartHeaderDtoIn
    {
        public int CartHeaderId { get; set; }

        public string? UserId { get; set; }

        public string? CouponCode { get; set; }

        public double Discount { get; set; }

        public double CartTotal { get; set; }
    }
}
