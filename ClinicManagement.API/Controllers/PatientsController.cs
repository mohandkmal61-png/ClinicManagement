using System.ComponentModel.DataAnnotations;
using ClinicManagement.Application.DTOs.Common;
using ClinicManagement.Application.DTOs.Patients;
using ClinicManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin,Doctor,Receptionist")]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<PatientDto>>> GetAll(
            [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
            [FromQuery, Range(1, 100)] int pageSize = 10,
            [FromQuery] string? search = null,
            CancellationToken cancellationToken = default)
        {
            var patients = await _patientService.GetPageAsync(
                pageNumber,
                pageSize,
                search,
                cancellationToken);
            return Ok(patients);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PatientDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var patient = await _patientService.GetByIdAsync(id, cancellationToken);
            return patient is null ? NotFound() : Ok(patient);
        }

        [HttpPost]
        public async Task<ActionResult<PatientDto>> Create(
            [FromBody] CreatePatientDto dto,
            CancellationToken cancellationToken)
        {
            var patient = await _patientService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdatePatientDto dto,
            CancellationToken cancellationToken)
        {
            await _patientService.UpdateAsync(id, dto, cancellationToken);
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _patientService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
