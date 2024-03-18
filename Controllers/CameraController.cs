using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Camera_api.Models;
using Camera_api.Models.Returns;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Camera_api.Models.DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace Camera_api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CameraController : ControllerBase
    {
        private readonly CameraContext _context;
        private readonly IConfiguration _configuration;
        public CameraController(CameraContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [Authorize]
        [HttpPost("PostImage")]
        public async Task<ActionResult> SaveImage(ImageDTO req)
        {
            try
            {
                var affectedRows = await _context.Database
                    .ExecuteSqlInterpolatedAsync($"EXEC SaveImage {req.ImageBase64}");

                if (affectedRows > 0)
                {
                    await _context.SaveChangesAsync(); // Save changes to the database
                    string confirmed = "New Image Confirmed";
                    return Ok(new { confirmed });
                }
                else
                {
                    return NotFound("No changes applied.");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize]
        [HttpGet("GetImages")]
        public async Task<ActionResult<List<ImageReturn>>> GetImages()
        {
            try
            {
                var returnList = await _context.ImageReturns
                    .FromSqlRaw("EXEC GetImages")
                    .ToListAsync();

                //Return not found, if not found or if theres no data
                if (returnList == null || returnList.Count == 0)
                {
                    return NotFound(); // Return 404 Not Found if no data is found
                }

                //Returns list of corps
                return Ok(returnList);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login([FromBody] Login userLogin)
        {

            IActionResult response = Unauthorized();

            var user = AuthenticateNewUser(userLogin);

            if (user != null)
            {
                var tokenString = GenerateJSONWebToken(user);
                response = Ok(new { token = tokenString });
            }



            return response;
        }
        private string GenerateJSONWebToken(object user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                null,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);

        }


        private Login AuthenticateNewUser(Login userLogin)
        {
            Login user = null;

            if (userLogin.Username == "u" && userLogin.Password == "p")
            {
                user = new Login { Username = userLogin.Username, Password = userLogin.Password };
            }

            return user;
        }
    }
}
