using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JamesEsg.Common.Library.Auth.Models;
using JamesEsg.Identity.API.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace JamesEsg.Identity.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AccountsController : ControllerBase
{
    private readonly ApplicationDbContext _context;  
    private readonly ILogger<AccountsController> _logger;  
    private readonly IConfiguration _configuration;  
    private readonly UserManager<ApiUser> _userManager;  
    private readonly SignInManager<ApiUser> _signInManager;  
  
    public AccountsController(  
        ApplicationDbContext context,  
        ILogger<AccountsController> logger,  
        IConfiguration configuration,  
        UserManager<ApiUser> userManager,  
        SignInManager<ApiUser> signInManager)  
    {
        _context = context;  
        _logger = logger;  
        _configuration = configuration;  
        _userManager = userManager;  
        _signInManager = signInManager;  
    }  
    
    [HttpPost]  
    [ResponseCache(NoStore = true)]  
    public async Task<ActionResult> Register(RegisterDto input)  
    {
        var newUser = new ApiUser
        {
            UserName = input.UserName,
            Email = input.Email
        };
        
        var result = await _userManager.CreateAsync(newUser, input.Password!);

        if (!result.Succeeded)
            throw new Exception($"Error: {string.Join(" ", result.Errors.Select(e => e.Description))}"); 
        
        _logger.LogInformation("User ({UserName} {Email}) has been created", newUser.UserName, newUser.Email);  
        return Ok($"New user has been created ({newUser.UserName}) {newUser.Email})");
    }  
    
    [HttpPost]  
    [ResponseCache(NoStore = true)]  
    public async Task<ActionResult> Login(LoginDto input)  
    {
        try  
        {  
            var user = await _userManager.FindByNameAsync(input.Username!);  
  
            if (user is null || !await _userManager.CheckPasswordAsync(user, input.Password!))  
            {
                throw new Exception($"Invalid login attempt");  
            }  
        
            var signingCredentials = new SigningCredentials(  
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SigningKey"])),  
                SecurityAlgorithms.HmacSha256);  
  
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.UserName) };

            var jwtObject = new JwtSecurityToken(  
                issuer: _configuration["JWT:Issuer"],  
                audience: _configuration["JWT:Audience"],  
                claims: claims,  
                expires: DateTime.Now.AddSeconds(300),  
                signingCredentials: signingCredentials);  
  
            var jwtString = new JwtSecurityTokenHandler().WriteToken(jwtObject);  
            return Ok(jwtString);  
        }
        catch (Exception e)  
        {
            var exceptionDetails = new ProblemDetails
            {
                Detail = e.Message,
                Status = StatusCodes.Status401Unauthorized,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            };
            return Unauthorized(exceptionDetails);  
        }
    }
}