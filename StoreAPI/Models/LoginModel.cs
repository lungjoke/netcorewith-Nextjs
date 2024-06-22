using System.ComponentModel.DataAnnotations;

namespace StoreAPI.Models;

public class LoginModel{

    [Required(ErrorMessage ="Username is Required")]
    public string? Username {get; set;} 
    
    [Required(ErrorMessage ="Password is Required")]
    public string? Password {get; set;} 
}