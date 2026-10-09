using ClinicManagement.Application.DTOs.MedicalRecords;
using ClinicManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin,Doctor")]
    [Route("api/[controller]")]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly IMedicalRecordService _medicalRecordService;

        public MedicalRecordsController(IMedicalRecordService medicalRecordService)
        {
            _medicalRecordService = medicalRecordService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<MedicalRecordDto>>> GetAll(
            CancellationToken cancellationToken)
        {
            var medicalRecords = await _medicalRecordService.GetAllAsync(cancellationToken);
            return Ok(medicalRecords);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MedicalRecordDto>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var medicalRecord = await _medicalRecordService.GetByIdAsync(id, cancellationToken);
            return medicalRecord is null ? NotFound() : Ok(medicalRecord);
        }

        [HttpPost]
        public async Task<ActionResult<MedicalRecordDto>> Create(
            [FromBody] CreateMedicalRecordDto dto,
            CancellationToken cancellationToken)
        {
            var medicalRecord = await _medicalRecordService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = medicalRecord.Id }, medicalRecord);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateMedicalRecordDto dto,
            CancellationToken cancellationToken)
        {
            await _medicalRecordService.UpdateAsync(id, dto, cancellationToken);
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _medicalRecordService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
