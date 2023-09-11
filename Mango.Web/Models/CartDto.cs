namespace Mango.Web.Models
{
    public class CartDto
    {
        public CartHeadersDto CartHeader { get; set; }

        public IEnumerable<CartDetailsDto>? CartDetails { get; set; }
    }
}
