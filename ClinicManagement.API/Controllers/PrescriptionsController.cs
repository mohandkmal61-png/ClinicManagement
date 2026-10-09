using ClinicManagement.Application.DTOs.Prescriptions;
using ClinicManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin,Doctor")]
    [Route("api/[controller]")]
    public class PrescriptionsController : ControllerBase
    {
        private readonly IPrescriptionService _prescriptionService;

        public PrescriptionsController(IPrescriptionService prescriptionService)
        {
            _prescriptionService = prescriptionService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<PrescriptionDto>>> GetAll(
            CancellationToken cancellationToken)
        {
            var prescriptions = await _prescriptionService.GetAllAsync(cancellationToken);
            return Ok(prescriptions);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PrescriptionDto>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var prescription = await _prescriptionService.GetByIdAsync(id, cancellationToken);
            return prescription is null ? NotFound() : Ok(prescription);
        }

        [HttpPost]
        public async Task<ActionResult<PrescriptionDto>> Create(
            [FromBody] CreatePrescriptionDto dto,
            CancellationToken cancellationToken)
        {
            var prescription = await _prescriptionService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = prescription.Id }, prescription);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdatePrescriptionDto dto,
            CancellationToken cancellationToken)
        {
            await _prescriptionService.UpdateAsync(id, dto, cancellationToken);
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _prescriptionService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
