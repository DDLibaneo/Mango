namespace Mango.Services.ShoppingCartAPI.Models.Dto.In
{
    public class CartDetailsDtoIn
    {
        public int CartDetailsId { get; set; }

        public int CartHeaderId { get; set; }

        public int ProductId { get; set; }

        public int Count { get; set; }
    }
}
