namespace Mango.Services.ShoppingCartAPI.Models.Dto
{
    public class CartDetailsDto
    {
        public int CartDetailsId { get; set; }

        public int CartHeaderId { get; set; }

        public CartHeadersDto CartHeader { get; set; }

        public int ProductId { get; set; }

        public ProductDto Product { get; set; }

        public int Count { get; set; }
    }
}
