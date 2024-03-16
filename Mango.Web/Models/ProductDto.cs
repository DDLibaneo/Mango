using Mango.Web.Utility;
using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Models;

public class ProductDto
{
    public int ProductId { get; set; }

    public string Name { get; set; }

    public double Price { get; set; }

    public string Description { get; set; }

    public string CategoryName { get; set; }

	[Range(1, 100)]
	public int Count { get; set; } = 1;

	public string? ImageUrl { get; set; }

    public string? ImageLocalPath { get; set; }

    [MaxFileSize(1)]
    [AllowedExtensions(new string[] { ".png", ".jpg", ".jpeg" })]
    public IFormFile? Image { get; set; }
}
