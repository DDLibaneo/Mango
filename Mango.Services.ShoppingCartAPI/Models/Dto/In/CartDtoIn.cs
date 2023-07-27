namespace Mango.Services.ShoppingCartAPI.Models.Dto.In
{
    public class CartDtoIn
    {
        public CartHeaderDtoIn CartHeader { get; set; }

        public IEnumerable<CartDetailsDtoIn>? CartDetails { get; set; }
    }
}
