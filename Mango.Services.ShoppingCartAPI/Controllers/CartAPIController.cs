using AutoMapper;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Models.Dto.In;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartAPIController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _db;
        private ResponseDto _response;

        public CartAPIController(IMapper mapper, AppDbContext db)
        {
            _mapper = mapper;
            _db = db;
            _response = new ResponseDto();
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
                {
                    // create header and details
                    var cartHeader = _mapper.Map<CartHeaders>(cartDtoIn.CartHeader);
                    _db.CartHeaders.Add(cartHeader);
                    
                    await _db.SaveChangesAsync();

                    cartDtoIn.CartDetails.First().CartHeaderId = cartHeader.CartHeaderId;

                    var cartDetails = _mapper.Map<CartDetails>(cartDtoIn.CartDetails.First());
                    _db.CartDetails.Add(cartDetails);

                    await _db.SaveChangesAsync();
                }
                else
                {
                    // if header is not null 
                    // check if details has same product
                    var productIdDto = cartDtoIn?.CartDetails?.First().ProductId;
                    var cartHeaderIdDto =  cartHeaderFromDb.CartHeaderId;

                    var cartDetailsFromDb = await _db.CartDetails
                        .AsNoTracking()
                        .FirstOrDefaultAsync(c => c.ProductId == productIdDto 
                            && c.CartHeaderId == cartHeaderIdDto);

                    if (cartDetailsFromDb == null)
                    {
                        // create cardDetails
                        cartDtoIn.CartDetails.First().CartHeaderId = cartHeaderFromDb.CartHeaderId;

                        var cartDetails = _mapper.Map<CartDetails>(cartDtoIn.CartDetails.First());
                        _db.CartDetails.Add(cartDetails);

                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        // update count in cart details
                        cartDtoIn.CartDetails.First().Count += cartDetailsFromDb.Count;
                        cartDtoIn.CartDetails.First().CartHeaderId = cartDetailsFromDb.CartHeaderId;
                        cartDtoIn.CartDetails.First().CartDetailsId = cartDetailsFromDb.CartDetailsId;

                        var cartDetails = _mapper.Map<CartDetails>(cartDtoIn.CartDetails.First());
                        _db.CartDetails.Update(cartDetails);

                        await _db.SaveChangesAsync();
                    }
                }

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
                if (cartDetailsId == 0)
                    throw new ArgumentException("parameter value was 0", nameof(cartDetailsId));

                var cartDetails = await _db.CartDetails.FirstOrDefaultAsync(c => c.CartDetailsId == cartDetailsId) 
                    ?? throw new Exception($"CartDetails of Id {cartDetailsId} was not found.");
                
                var totalCountOfCartItem = await _db.CartDetails.CountAsync(c => c.CartDetailsId == cartDetailsId);

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
    }
}
