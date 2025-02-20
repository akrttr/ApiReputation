using ApiReputation.Application.Services;
using ApiReputation.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiReputation.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ApiInfoController : ControllerBase
    {
        private readonly ApiInfoService _apiService;

        public ApiInfoController(ApiInfoService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet("List")]
        public async Task<ActionResult<IEnumerable<ApiInfo>>> GetAll()
        {
            var apis = await _apiService.GetAllApiAsync();
            return Ok(apis);
        }

        [HttpGet("Get/{id}")]
        public async Task<ActionResult<ApiInfo>> GetById(int id)
        {
            var api = await _apiService.GetApiByIdAsync(id);
            if (api == null)  return NotFound();
            return Ok(api);
        }
        [Authorize]
        [HttpPost("Add")]
        public async Task<ActionResult> Add([FromBody] ApiInfo api)
        {
            await _apiService.AddApiAsync(api);
            return CreatedAtAction(nameof(GetById), new { id = api.Id }, api);
        }

        [Authorize]
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ApiInfo api)
        {
            if (id != api.Id)
                return BadRequest("ID uyumsuzluğu!");

            await _apiService.UpdateApiAsync(api);
            return NoContent();
        }

        [Authorize]
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _apiService.DeleteApiAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Bir hata oluştu.", error = ex.Message });
            }
        }

    }
}
