using BuildingBlock.Shared.Models;
using LeaveService.Entity;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace LeaveService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveController : ControllerBase
    {
        private static readonly List<Leave> _leaveRequests = new List<Leave>();
        private readonly IPublishEndpoint _publishEndpoint;

        public LeaveController(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost("apply")]
        public async Task<IActionResult> ApplyLeave([FromBody] LeaveRequest request)
        {
            int id = _leaveRequests.LastOrDefault()?.Id??1;
            _leaveRequests.Add(new Leave()
            {
                Id = id,
                EmployeeEmail = request.EmployeeEmail,
                StartDate = request.StartDate,
                EndDate = request.EndDate,  
                Status = "Pending"
            });

            try
            {
                await _publishEndpoint.Publish(new LeaveEvent
                {
                    UserEmail = request.EmployeeEmail,
                    Status = "Pending"
                });
            }
            catch(Exception ex)
            {

            }

            return Ok(new { Message = "Leave request submitted." });
        }

        [HttpPost("approve/{id}")]
        public async Task<IActionResult> ApproveLeave(int id)
        {
            var leave = _leaveRequests.FirstOrDefault(x=> x.Id == id);
            if (leave == null) return NotFound("Leave request not found.");

            leave.Status = "Approved";

            // Publish LeaveEvent to RabbitMQ
            await _publishEndpoint.Publish(new LeaveEvent
            {
                UserEmail = leave.EmployeeEmail,
                Status = "Approved"
            });

            return Ok(new { Message = "Leave Approved" });
        }

        [HttpPost("reject/{id}")]
        public async Task<IActionResult> RejectLeave(int id)
        {
            var leave = _leaveRequests.FirstOrDefault(x => x.Id == id);
            if (leave == null) return NotFound("Leave request not found.");

            leave.Status = "Rejected";

            // Publish LeaveEvent to RabbitMQ
            await _publishEndpoint.Publish(new LeaveEvent
            {
                UserEmail = leave.EmployeeEmail,
                Status = "Rejected"
            });

            return Ok(new { Message = "Leave Rejected" });
        }
    }
}
