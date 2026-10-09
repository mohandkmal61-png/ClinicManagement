using ClinicManagement.Application.DTOs.Doctors;
using ClinicManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin,Doctor")]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<DoctorDto>>> GetAll(CancellationToken cancellationToken)
        {
            var doctors = await _doctorService.GetAllAsync(cancellationToken);
            return Ok(doctors);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<DoctorDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var doctor = await _doctorService.GetByIdAsync(id, cancellationToken);
            return doctor is null ? NotFound() : Ok(doctor);
        }

        [HttpPost]
        public async Task<ActionResult<DoctorDto>> Create(
            [FromBody] CreateDoctorDto dto,
            CancellationToken cancellationToken)
        {
            var doctor = await _doctorService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = doctor.Id }, doctor);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateDoctorDto dto,
            CancellationToken cancellationToken)
        {
            await _doctorService.UpdateAsync(id, dto, cancellationToken);
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _doctorService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
