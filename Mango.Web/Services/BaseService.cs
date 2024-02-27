using Mango.Web.Models;
using Mango.Web.Services.IService;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using static Mango.Web.Utility.SD;

namespace Mango.Web.Services
{
    public class BaseService : IBaseService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenProvider _tokenProvider;
        private readonly string APPLICATION_JSON = "application/json";

        public BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider)
        {
            _httpClientFactory = httpClientFactory;
            _tokenProvider = tokenProvider;
        }

        public async Task<ResponseDto?> SendAsync(RequestDto requestDto, bool withBearer = true)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MangoAPI");
                var message = new HttpRequestMessage();

				if (requestDto.ContentType == ContentType.MultipartFormData)
					message.Headers.Add("Accept", "*/*");
				else
					message.Headers.Add("Accept", "application/json");
				
				if (withBearer)
                {
                    var token = _tokenProvider.GetToken();
                    message.Headers.Add("Authorization", $"Bearer {token}");
                }

                message.RequestUri = new Uri(requestDto.Url);

                if (requestDto.ContentType == ContentType.MultipartFormData) 
                {
                    var content = new MultipartFormDataContent();

                    foreach (var property in requestDto.Data.GetType().GetProperties())
                    {
                        var propertyValue = property.GetValue(requestDto.Data);

                        if (propertyValue is FormFile)
                        {
                            var file = (FormFile)propertyValue;
                            
                            if (file != null)
                            {
                                using (var streamContent = new StreamContent(file.OpenReadStream()))
                                {
                                    content.Add(streamContent, property.Name, file.FileName);
                                }
                            }
                        }
                        else
                        {
                            var propertyValueString = propertyValue == null 
                                ? string.Empty 
                                : propertyValue.ToString();

                            using (var stringContent = new StringContent(propertyValueString ?? string.Empty))
                            {
                                content.Add(stringContent, property.Name);
                            }
                        }
                    }

                    message.Content = content;
                }
                else
                {
					if (requestDto.Data != null)
					{
						var serializedDto = JsonConvert.SerializeObject(requestDto.Data);
						message.Content = new StringContent(serializedDto, Encoding.UTF8, APPLICATION_JSON);
					}
				}
                
                HttpResponseMessage? apiResponse = null;

                switch (requestDto.ApiType)
                {
                    case ApiType.POST:
                        message.Method = HttpMethod.Post;
                        break;
                    case ApiType.DELETE:
                        message.Method = HttpMethod.Delete;
                        break;
                    case ApiType.PUT:
                        message.Method = HttpMethod.Put;
                        break;
                    case ApiType.GET:
                        break;
                    default:
                        message.Method = HttpMethod.Get;
                        break;
                }

                apiResponse = await client.SendAsync(message);

                switch (apiResponse.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return new()
                        {
                            IsSuccess = false,
                            Message = "Not Found"
                        };
                    case HttpStatusCode.Unauthorized:
                        return new()
                        {
                            IsSuccess = false,
                            Message = "Unauthorized"
                        };
                    case HttpStatusCode.Forbidden:
                        return new()
                        {
                            IsSuccess = false,
                            Message = "Forbidden"
                        };
                    case HttpStatusCode.InternalServerError:
                        return new()
                        {
                            IsSuccess = false,
                            Message = "InternalServerError"
                        };
                    default:
                        var apiContent = await apiResponse.Content.ReadAsStringAsync();
                        var apiResponseDto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
                        return apiResponseDto;
                }
            }
            catch (Exception ex)
            {
                var dto = new ResponseDto
                {
                    IsSuccess = false,
                    Message = ex.Message
                };

                return dto;
            }
        }
    }
}
