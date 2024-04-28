using Mango.Web.Models;
using Mango.Web.Services.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Mango.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;

        public AuthController(IAuthService authService, ITokenProvider tokenProvider)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
        }

        [HttpGet]
        public IActionResult Login()
        {
            var loginRequestDto = new LoginRequestDto();

            return View(loginRequestDto);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDto obj)
        {
            var responseDto = await _authService.LoginAsync(obj);
            
            if (responseDto != null && responseDto.IsSuccess)
            {
                var loginResponseDto = JsonConvert
                    .DeserializeObject<LoginResponseDto>(responseDto.Result.ToString());

                await SignInUser(loginResponseDto);

                _tokenProvider.SetToken(loginResponseDto.Token);

                return RedirectToAction("Index", "Home");
            }            
                
            TempData["error"] = responseDto.Message;
            //ModelState.AddModelError("CustomError", responseDto.Message);            
            return View(obj);            
        }

        [HttpGet]
        public IActionResult Register()
        {
            var roleList = new List<SelectListItem>
            {
                new SelectListItem{ Text = SD.RoleAdmin, Value = SD.RoleAdmin },
                new SelectListItem{ Text = SD.RoleCustomer, Value = SD.RoleCustomer }
            };

            ViewBag.RoleList = roleList;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegistrationRequestDto obj)
        {
            var result = await _authService.RegisterAsync(obj);
            ResponseDto assignedRole;

            if (result != null && result.IsSuccess)
            {
                if (string.IsNullOrEmpty(obj.RoleName))
                    obj.RoleName = SD.RoleCustomer;

                assignedRole = await _authService.AssignRoleAsync(obj);

                if (assignedRole != null && assignedRole.IsSuccess)
                {
                    TempData["success"] = "Registration Successful";
                    return RedirectToAction(nameof(Login));
                }
            }
            else            
                TempData["error"] = result.Message;            

            var roleList = new List<SelectListItem>
            {
                new SelectListItem{ Text = SD.RoleAdmin, Value = SD.RoleAdmin },
                new SelectListItem{ Text = SD.RoleCustomer, Value = SD.RoleCustomer }
            };

            ViewBag.RoleList = roleList;

            return View(obj);
        }

        [HttpGet]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync();
            _tokenProvider.ClearToken();

            return RedirectToAction("Index", "Home");
        }

        private async Task SignInUser(LoginResponseDto model)
        {
            var handler = new JwtSecurityTokenHandler();

            var jwt = handler.ReadJwtToken(model.Token);

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

            var emailClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
            var subClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            var nameClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name);
            var roleClaim = jwt.Claims.FirstOrDefault(c => c.Type == "role");

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Email, emailClaim != null 
                    ? emailClaim.Value 
                    : string.Empty),
                new(JwtRegisteredClaimNames.Sub, subClaim != null 
                    ? subClaim.Value 
                    : string.Empty),
                new(JwtRegisteredClaimNames.Name, nameClaim != null 
                    ? nameClaim.Value 
                    : string.Empty),
                new(ClaimTypes.Name, emailClaim != null 
                    ? emailClaim.Value 
                    : string.Empty),
                new(ClaimTypes.Role, roleClaim != null 
                    ? roleClaim.Value 
                    : string.Empty)
            };

            identity.AddClaims(claims);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }
    }
}
