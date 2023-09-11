namespace Mango.Web.Models
{
    public class CartDtoIn
    {
        public CartHeadersDtoIn CartHeader { get; set; }

        public IEnumerable<CartDetailsDtoIn>? CartDetails { get; set; }
    }
}
