using ApiReputation.Application.Interfaces;
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
        private readonly IScannerService _scannerService;


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
        [Authorize(Roles = "Admin")]
        [HttpPost("Add")]
        public async Task<ActionResult> Add([FromBody] ApiInfo api)
        {
            await _apiService.AddApiAsync(api);
            return CreatedAtAction(nameof(GetById), new { id = api.Id }, api);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ApiInfo api)
        {
            if (id != api.Id)
                return BadRequest("ID uyumsuzluğu!");

            await _apiService.UpdateApiAsync(api);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/scan")]
        public async Task<IActionResult> ScanApiInfo(int id)
        {
            // Not: UnitOfWork ve Repository desenini tam olarak kurduysak,
            // kullanıcı kontrolünü burada yapmak daha doğru olur.
            // Şimdilik servisin bu kontrolü yaptığını varsayıyoruz.

            try
            {
                // Servisimizi çağırarak tüm işi ona yaptırıyoruz.
                var scanResult = await _scannerService.PerformScanAsync(id);

                // Başarılı olursa, tarama sonucunu direkt olarak kullanıcıya dönüyoruz.
                return Ok(scanResult);
            }
            catch (KeyNotFoundException ex)
            {
                // Servis "ApiInfo not found" hatası fırlatırsa, 404 Not Found dönüyoruz.
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Beklenmedik başka bir hata olursa 400 Bad Request dönüyoruz.
                return BadRequest(new { message = $"An error occurred during the scan: {ex.Message}" });
            }
        }
    }
}
