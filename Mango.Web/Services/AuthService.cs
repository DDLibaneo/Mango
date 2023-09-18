using Mango.Web.Models;
using Mango.Web.Services.IService;
using Mango.Web.Utility;

namespace Mango.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly IBaseService _baseService;
        private readonly string AUTH_ROUTE = "/api/auth/";

        public AuthService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto?> AssignRoleAsync(RegistrationRequestDto registrationRequestDto)
        {
            var request = new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                Url = SD.AuthAPIBase + AUTH_ROUTE + "AssignRole",
                Data = registrationRequestDto
            };

            return await _baseService.SendAsync(request);
        }

        public async Task<ResponseDto?> LoginAsync(LoginRequestDto loginRequestDto)
        {
            var request = new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                Url = SD.AuthAPIBase + AUTH_ROUTE + "login",
                Data = loginRequestDto
            };

            return await _baseService.SendAsync(request, withBearer: false);
        }

        public async Task<ResponseDto?> RegisterAsync(RegistrationRequestDto registrationRequestDto)
        {
            var request = new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                Url = SD.AuthAPIBase + AUTH_ROUTE + "register",
                Data = registrationRequestDto
            };

            return await _baseService.SendAsync(request, withBearer: false);
        }
    }
}
