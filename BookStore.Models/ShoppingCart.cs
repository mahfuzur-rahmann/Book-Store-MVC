using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BookStore.Models;

public class ShoppingCart
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    [ForeignKey("ProductId")]
    [ValidateNever]
    public Product Product { get; set; }



    
    public string ApplicationUserId { get; set; }

    
    [ValidateNever]
    [ForeignKey("ApplicationUserId")]
    public ApplicationUser ApplicationUser { get; set; }

    [Range(1, 1000, ErrorMessage = "Please enter a value between 1 to 1000...")]
    public int Count { get; set; }

    [NotMapped]
    public double Price { get; set; }
}