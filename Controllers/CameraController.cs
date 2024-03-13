using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Camera_api.Models;
using Camera_api.Models.Returns;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Camera_api.Models.DTO;

namespace Camera_api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CameraController : ControllerBase
    {
        private readonly CameraContext _context;

        public CameraController(CameraContext context)
        {
            _context = context;
        }



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

        [HttpGet("GetImageAt")]
        public async Task<ActionResult<List<ImageReturn>>> GetImageAt()
        {
            try
            {
                var returnList = await _context.ImageReturns
                    .FromSqlRaw("EXEC GetImageAt")
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
    }
}
