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
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

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

        [AllowAnonymous]
        [HttpGet("Notification")]
        public async void TestNotification()
        {
            try
            {
                // The topic name can be optionally prefixed with "/topics/".
                var topic = "TestNotify";

                // See documentation on defining a message payload.
                var message = new Message()
                {
                    Notification = new Notification()
                    {
                        Title = "This is a Message from",
                        Body = "My AspNetCore Web Api. It Works Now",
                        ImageUrl = "https://cdn.discordapp.com/attachments/1068121192059371652/1204098416624541707/RDT_20240205_1442011513224325506559786.jpg?ex=660ade09&is=65f86909&hm=24e17f0e779ec14032a2ed15dfcb8304a5a4f25c68a691b7afcaa55e5310d348&"
                    },
                    Topic = topic,
                    Android = {
                        Priority = Priority.High,
                    },
                };

                // Send a message to the devices subscribed to the provided topic.
                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                // Response is a message ID string.
                Console.WriteLine("Successfully sent message: " + response);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
