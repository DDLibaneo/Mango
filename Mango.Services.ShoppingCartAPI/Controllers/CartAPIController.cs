using AutoMapper;
using Mango.MessageBus;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Models.Dto.In;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;
using System.Security.AccessControl;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartAPIController : ControllerBase
    {
        private readonly IMapper _mapper;        
        private readonly IProductService _productService;
        private readonly ICouponService _couponService;
        private readonly IMessageBus _messageBus;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _db;

        private ResponseDto _response;

        public CartAPIController(IMapper mapper, AppDbContext db, 
            IProductService productService, ICouponService couponService, 
            IMessageBus messageBus, IConfiguration configuration)
        {
            _mapper = mapper;
            _productService = productService;
            _couponService = couponService;
            _messageBus = messageBus;
            _configuration = configuration;
            _db = db;
            _response = new ResponseDto();
        }

        [HttpGet("GetCart/{userId}")]
        public async Task<ResponseDto> GetCart([FromRoute] string userId)
        {
            try
            {
                var cartHeaderFromDb = _db.CartHeaders.FirstOrDefault(c => c.UserId == userId);

                var cartDetailsFromDb = _db.CartDetails.Where(c => c.CartHeaderId == cartHeaderFromDb.CartHeaderId);

                var cartDto = new CartDto
                {
                    CartHeader = _mapper.Map<CartHeadersDto>(cartHeaderFromDb),
                    CartDetails = _mapper.Map<IEnumerable<CartDetailsDto>>(cartDetailsFromDb)
                };

                var productDtos = await _productService.GetProducts();

                foreach (var item in cartDto.CartDetails)
                {
                    item.Product = productDtos.FirstOrDefault(p => p.ProductId == item.ProductId);
                    cartDto.CartHeader.CartTotal += (item.Count * item.Product.Price);
                }

                //apply coupon if any
                if (!string.IsNullOrEmpty(cartDto.CartHeader.CouponCode))
                {
                    var couponDto = await _couponService.GetCouponAsync(cartDto.CartHeader.CouponCode);

                    if (couponDto != null && cartDto.CartHeader.CartTotal > couponDto.MinAmount) 
                    { 
                        cartDto.CartHeader.CartTotal -= couponDto.MinAmount;
                        cartDto.CartHeader.Discount = couponDto.DiscountAmount;
                    }
                }

                _response.Result = cartDto;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }

            return _response;
        }

        [HttpPost("ApplyCoupon")]
        public async Task<object> ApplyCoupon([FromBody] CartCouponDto cartCouponDto)
        {
            try
            {
                var cartFromDb = await _db.CartHeaders
                    .FirstAsync(c => c.UserId == cartCouponDto.UserId);
                cartFromDb.CouponCode = cartCouponDto.CouponCode;
                
                _db.CartHeaders.Update(cartFromDb);
                await _db.SaveChangesAsync();
                
                _response.Result = true;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }

            return _response;
        }

        [HttpPost("RemoveCoupon")]
        public async Task<object> RemoveCoupon([FromBody] CartCouponDto cartCouponDto)
        {
            try
            {
                var cartFromDb = await _db.CartHeaders.FirstAsync(c => c.UserId == cartCouponDto.UserId);
                cartFromDb.CouponCode = "";

                _db.CartHeaders.Update(cartFromDb);
                await _db.SaveChangesAsync();

                _response.Result = true;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }

            return _response;
        }

        [HttpPost("CartUpsert")]
        public async Task<ResponseDto> CartUpsert([FromBody] CartDtoIn cartDtoIn)
        {
            try
            {
                if (cartDtoIn == null || cartDtoIn.CartDetails == null)
                    throw new ArgumentNullException(nameof(cartDtoIn));

                var cartHeaderFromDb = await _db.CartHeaders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.UserId == cartDtoIn.CartHeader.UserId);

                if (cartHeaderFromDb == null)
                    await CreateHeaderAndDetails(cartDtoIn);
                else
                    await CreateOrUpdateCartDetailsAsync(cartDtoIn, cartHeaderFromDb);

                _response.Result = cartDtoIn;
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }

            return _response;
        }

        [HttpPost("RemoveCart/{cartDetailsId}")]
        public async Task<ResponseDto> RemoveCart([FromRoute] int cartDetailsId)
        {
            try
            {
                #region verifications
                if (cartDetailsId == 0)
                    throw new ArgumentException("parameter value was 0", nameof(cartDetailsId));

                var cartDetails = await _db.CartDetails
                    .FirstOrDefaultAsync(c => c.CartDetailsId == cartDetailsId) 
                        ?? throw new Exception($"CartDetails of Id {cartDetailsId} was not found.");
                #endregion

                var totalCountOfCartItem = await _db.CartDetails
                    .CountAsync(c => c.CartHeaderId == cartDetails.CartHeaderId);

                _db.CartDetails.Remove(cartDetails);

                if (totalCountOfCartItem == 1) 
                { 
                    var cartHeaderToRemove = await _db.CartHeaders
                        .FirstOrDefaultAsync(c => c.CartHeaderId == cartDetails.CartHeaderId) 
                            ?? throw new Exception($"CartHeader of Id {cartDetails.CartHeaderId} was not found.");
                    
                    _db.CartHeaders.Remove(cartHeaderToRemove);
                }

                await _db.SaveChangesAsync();

                _response.Result = true;
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }

            return _response;
        }

        [HttpPost("EmailCartRequest")]
        public async Task<object> EmailCartRequest([FromBody] CartDto cartDto)
        {
            try
            {
                await _messageBus.PublishMessage(cartDto, _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCart"));
                _response.Result = true;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }

            return _response;
        }
        
        private async Task CreateOrUpdateCartDetailsAsync(CartDtoIn cartDtoIn, CartHeaders? cartHeaderFromDb)
        {
            #region verifications
            if (cartDtoIn.CartDetails == null)
            {
                var message = @$"Property {nameof(cartDtoIn.CartDetails)} of object {nameof(cartDtoIn)} was null.";
                throw new Exception(message);
            }

            if (cartHeaderFromDb == null)
            {
                var message = $"Parameter null on method {CreateOrUpdateCartDetailsAsync}";
                throw new ArgumentNullException(nameof(cartHeaderFromDb), message);
            }
            #endregion

            // check if details has same product
            var productIdDto = cartDtoIn.CartDetails.First().ProductId;
            var cartHeaderIdDto = cartHeaderFromDb.CartHeaderId;

            var cartDetailsFromDb = await _db.CartDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ProductId == productIdDto
                    && c.CartHeaderId == cartHeaderIdDto);

            if (cartDetailsFromDb == null)
                await CreateCartDetailsAsync(cartDtoIn, cartHeaderFromDb);
            else
                await UpdateCountInCartDetailsAsync(cartDtoIn, cartDetailsFromDb);
        }

        private async Task UpdateCountInCartDetailsAsync(CartDtoIn cartDtoIn, CartDetails? cartDetailsFromDb)
        {
            #region verifications
            if (cartDtoIn.CartDetails == null)
            {
                var message = @$"Property {nameof(cartDtoIn.CartDetails)} of object {nameof(cartDtoIn)} was null.";
                throw new Exception(message);
            }

            if (cartDetailsFromDb == null)
            {
                var message = $"Parameter null on method {UpdateCountInCartDetailsAsync}";
                throw new ArgumentNullException(nameof(cartDetailsFromDb), message);
            }
            #endregion

            cartDtoIn.CartDetails.First().Count += cartDetailsFromDb.Count;
            cartDtoIn.CartDetails.First().CartHeaderId = cartDetailsFromDb.CartHeaderId;
            cartDtoIn.CartDetails.First().CartDetailsId = cartDetailsFromDb.CartDetailsId;

            var cartDetails = _mapper.Map<CartDetails>(cartDtoIn.CartDetails.First());
            _db.CartDetails.Update(cartDetails);

            await _db.SaveChangesAsync();
        }

        private async Task CreateCartDetailsAsync(CartDtoIn cartDtoIn, CartHeaders? cartHeaderFromDb)
        {
            #region verifications
            if (cartDtoIn.CartDetails == null)
            {
                var message = @$"Property {nameof(cartDtoIn.CartDetails)} of object {nameof(cartDtoIn)} was null.";
                throw new Exception(message);
            }

            if (cartHeaderFromDb == null)
            {
                var message = $"Parameter null on method {CreateCartDetailsAsync}";
                throw new ArgumentNullException(nameof(cartHeaderFromDb), message);
            }
            #endregion

            cartDtoIn.CartDetails.First().CartHeaderId = cartHeaderFromDb.CartHeaderId;

            var cartDetails = _mapper.Map<CartDetails>(cartDtoIn.CartDetails.First());
            _db.CartDetails.Add(cartDetails);

            await _db.SaveChangesAsync();
        }

        private async Task CreateHeaderAndDetails(CartDtoIn cartDtoIn)
        {
            if (cartDtoIn.CartDetails == null)
            {
                var message = @$"Property {nameof(cartDtoIn.CartDetails)} of object {nameof(cartDtoIn)} was null.";
                throw new Exception(message);
            }

            var cartHeader = _mapper.Map<CartHeaders>(cartDtoIn.CartHeader);
            _db.CartHeaders.Add(cartHeader);

            await _db.SaveChangesAsync();

            cartDtoIn.CartDetails.First().CartHeaderId = cartHeader.CartHeaderId;

            var cartDetails = _mapper.Map<CartDetails>(cartDtoIn.CartDetails.First());
            _db.CartDetails.Add(cartDetails);

            await _db.SaveChangesAsync();
        }
    }
}
