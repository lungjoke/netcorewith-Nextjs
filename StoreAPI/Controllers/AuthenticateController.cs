using System;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StoreAPI.Migrations;
using StoreAPI.Models;

namespace StoreAPI.Controllerrs;

[ApiController]
[Route("api/[controller]")]
public class AuthenticateController : ControllerBase
{
    //
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    //constructor
    public AuthenticateController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

    //register for normal user
[HttpPost]
[Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)

    {
        //เช็ค user ว่าซ้ำไหม
        var userExist = await _userManager.FindByNameAsync(model.Username);
        if(userExist !=null)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new Response{
                    Status ="Error",
                    Message = "User already exist!"
                }
            );
        }
        //เช็คว่ามี email นี้ในระบบไหม
    IdentityUser user = new()
    {
      Email = model.Email,
      SecurityStamp = Guid.NewGuid().ToString(),
      UserName = model.Username  
    };
    //สร้าง User ใหม่
    var result = await _userManager.CreateAsync(user , model.Password);
    //สร้างไม่สำเร็จ
    if(!result.Succeeded)
    {
    return StatusCode(
        StatusCodes.Status500InternalServerError,
                new Response {
                    Status ="Error",
                    Message = "User creation fail! Please check user details and try again."
                    }
    );
    }
    //สร้างสำเร็จ
    return Ok(new Response {
        Status = "Success",
        Message = "User created successfully!"
    });
    }
   
    //register for Admin
[HttpPost]
[Route("register-admin")]
 public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
    {
        //เช็ค user ว่าซ้ำไหม
        var userExist = await _userManager.FindByNameAsync(model.Username);
        if(userExist !=null)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new Response{
                    Status ="Error",
                    Message = "User already exist!"
                }
            );
        }
        //เช็คว่ามี email นี้ในระบบไหม
    IdentityUser user = new()
    {
      Email = model.Email,
      SecurityStamp = Guid.NewGuid().ToString(),
      UserName = model.Username  
    };
    //สร้าง User ใหม่
    var result = await _userManager.CreateAsync(user , model.Password);
    
    //สร้างไม่สำเร็จ
    if(!result.Succeeded)
    
    return StatusCode(
        StatusCodes.Status500InternalServerError,
                new Response {
                    Status ="Error",
                    Message = "User creation fail! Please check user details and try again."
                    }
                    );
    if (!await _roleManager.RoleExistsAsync(UserRoles.Admin)){
        await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
        await _userManager.AddToRoleAsync(user, UserRoles.Admin);
    }else if(!await _roleManager.RoleExistsAsync(UserRoles.Manager)){
        await _roleManager.CreateAsync(new IdentityRole(UserRoles.Manager));
        await _userManager.AddToRoleAsync(user, UserRoles.Manager);
    }else if(!await _roleManager.RoleExistsAsync(UserRoles.User)){
        await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));
        await _userManager.AddToRoleAsync(user, UserRoles.User);
     }   
         
    return Ok(new Response { Status = "Success", Message = "User created successfully!"});
    
    }

//login
[HttpPost]
[Route("login")]
public async Task<IActionResult> Login([FromBody] LoginModel model)
{
    var user =await _userManager.FindByNameAsync(model.Username!);
    if(user != null && await _userManager.CheckPasswordAsync(user, model.Password!))
    {
        var userRoles = await _userManager.GetRolesAsync(user);

        var authClims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        foreach (var userRole in userRoles)
        {
            authClims.Add(new Claim(ClaimTypes.Role, userRole));
        }
        var token = GetToken(authClims);
        
        return Ok(new{
            token = new JwtSecurityTokenHandler().WriteToken(token),
            expiration = token.ValidTo
        });
    }
    return Unauthorized();
}


//Refresh token
[HttpPost]
[Route("refresh-token")]
public IActionResult RefreshToken([FromBody] RefreshTokenModel model)
{
    var authHeader = Request.Headers["Authorization"];
    if (authHeader.ToString().StartsWith("Bearer"))
    {
        var token = authHeader.ToString().Substring(7);
        var tokenHandler = new JwtSecurityTokenHandler();
        var key= Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]!);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            },out SecurityToken validatedToken);
            var jwtToken = (JwtSecurityToken)validatedToken;
            var user = new{
                name = jwtToken.Claims.First(x=>x.Type == "unique_name").Value,
                Role = jwtToken.Claims.First(x=>x.Type == ClaimTypes.Role).Value
            };
            var authClims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var newToken = GetToken(authClims);
            return Ok(new{
                token = new JwtSecurityTokenHandler().WriteToken(newToken),
                expiration = newToken.ValidTo
            });
        }
        catch
        {
            return Unauthorized();

        }
    }
    return Unauthorized();
}

//logout
[HttpPost]
[Route("logout")]
public async Task<IActionResult>Logout()
{
    var username = User.Identity?.Name;
    if(username !=null )
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user != null)
        {
        await _userManager.UpdateSecurityStampAsync(user);
        return Ok(new Response {Status = "Success", Message = "User logged out!"});
        }
    }
    return Ok();
}
//Method for generating JWT token
private JwtSecurityToken GetToken(List<Claim> authClims)
{
    var authSingingkey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));

    var token = new JwtSecurityToken(
        issuer:_configuration["JWT:ValidIssuer"],
        audience:_configuration["JWT:ValidAudience"],
        expires: DateTime.Now.AddHours(24),
        claims: authClims,
        signingCredentials: new SigningCredentials(authSingingkey, SecurityAlgorithms.HmacSha256)
    );
    return token;
}

public class RefreshTokenModel
{
    public required string Token {get; set;}
}
}