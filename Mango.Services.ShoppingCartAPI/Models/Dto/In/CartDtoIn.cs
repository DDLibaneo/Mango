namespace Mango.Services.ShoppingCartAPI.Models.Dto.In
{
    public class CartDtoIn
    {
        public CartHeadersDtoIn CartHeader { get; set; }

        public IEnumerable<CartDetailsDtoIn>? CartDetails { get; set; }
    }
}
