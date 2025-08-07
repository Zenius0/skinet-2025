using System;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class CreateProductDto
{
    [Required]
    public String Name { get; set; } = string.Empty;
    [Required]
    public String Description { get; set; } = string.Empty;
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public Decimal Price { get; set; }
    [Required]
    public String PictureUrl { get; set; } = string.Empty;
    [Required]
    public String Type { get; set; } = string.Empty;
    [Required]
    public String Brand { get; set; } = string.Empty;
    [Range(1, int.MaxValue, ErrorMessage = "Quantity in stock must be at least 1")]
    public int QuantityInStock { get; set; }
}
