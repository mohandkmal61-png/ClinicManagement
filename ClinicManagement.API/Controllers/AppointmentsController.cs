using ClinicManagement.Application.DTOs.Appointments;
using ClinicManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin,Doctor,Receptionist")]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AppointmentDto>>> GetAll(CancellationToken cancellationToken)
        {
            var appointments = await _appointmentService.GetAllAsync(cancellationToken);
            return Ok(appointments);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AppointmentDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var appointment = await _appointmentService.GetByIdAsync(id, cancellationToken);
            return appointment is null ? NotFound() : Ok(appointment);
        }

        [HttpPost]
        public async Task<ActionResult<AppointmentDto>> Create(
            [FromBody] CreateAppointmentDto dto,
            CancellationToken cancellationToken)
        {
            var appointment = await _appointmentService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateAppointmentDto dto,
            CancellationToken cancellationToken)
        {
            await _appointmentService.UpdateAsync(id, dto, cancellationToken);
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            await _appointmentService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
