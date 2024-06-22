using System.ComponentModel.DataAnnotations;

namespace StoreAPI.Models;

public class RegisterModel
{
    [Required(ErrorMessage ="Username is Required")]
    public required string Username {get; set;} 

    [EmailAddress]
    [Required(ErrorMessage ="Email is Required")]
    public required string Email {get; set;} 

    [Required(ErrorMessage ="Password is Required")]
    public required string Password {get; set;} 

}