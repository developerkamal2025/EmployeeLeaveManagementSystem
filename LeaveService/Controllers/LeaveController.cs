using System.Security.Claims;
using BuildingBlock.Shared;
using BuildingBlock.Shared.Enums;
using BuildingBlock.Shared.Models;
using LeaveService.Models;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LeaveService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveController : ControllerBase
    {
        private static readonly List<Leave> _leaveRequests = new List<Leave>();
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IHttpClientFactory _httpClientFactory;

        public LeaveController(IPublishEndpoint publishEndpoint, IHttpClientFactory httpClientFactory)
        {
            _publishEndpoint = publishEndpoint;
            _httpClientFactory = httpClientFactory;
        }

        [Authorize]
        [HttpPost("apply")]
        public async Task<IActionResult> ApplyLeave(LeaveRequest request)
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Split(" ")[1];

            string role = await CommonService.GetJwtTokenClaim(token, System.Security.Claims.ClaimTypes.Role);

            int totalLeaveAllowed = 0;
            if (Enum.TryParse(typeof(EmployeeLeaveSettings), role, out var enumValue))
            {
                totalLeaveAllowed = (int)(EmployeeLeaveSettings)enumValue;
            }
            
            int id = _leaveRequests.LastOrDefault()?.Id ?? 1;
            int days = (request.StartDate - request.EndDate).Days;
            int leaveTaken = _leaveRequests.Where(x => x.EmployeeEmail == request.EmployeeEmail && x.Status == "Approved")?.Count() ?? 0;
            
            if ((leaveTaken+ days) <= totalLeaveAllowed)
            {
                _leaveRequests.Add(new Leave()
                {
                    Id = id,
                    EmployeeEmail = request.EmployeeEmail,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Status = "Pending"
                });

                await _publishEndpoint.Publish(new LeaveEvent
                {
                    FromUser = request.EmployeeEmail,
                    ToUser = "manager@yopmail.com",
                    Status = "Pending"
                });
            }
            else
            {
                return Ok(new { Message = "Leave limit exceeded." });
            }

            return Ok(new { Message = "Leave request submitted." });
        }

        [Authorize(Roles ="Manager")]
        [HttpPost("approve/{id}")]
        public async Task<IActionResult> ApproveLeave(int id)
        {
            var leave = _leaveRequests.FirstOrDefault(x => x.Id == id);
            if (leave == null) return NotFound("Leave request not found.");

            leave.Status = "Approved";

            // Publish LeaveEvent to RabbitMQ
            await _publishEndpoint.Publish(new LeaveEvent
            {
                FromUser = "manager@yopmail.com",
                ToUser = leave.EmployeeEmail,
                Status = "Approved"
            });

            return Ok(new { Message = "Leave Approved" });
        }

        [Authorize(Roles = "Manager")]
        [HttpPost("reject/{id}")]
        public async Task<IActionResult> RejectLeave(int id)
        {
            var leave = _leaveRequests.FirstOrDefault(x => x.Id == id);
            if (leave == null) return NotFound("Leave request not found.");

            leave.Status = "Rejected";

            // Publish LeaveEvent to RabbitMQ
            await _publishEndpoint.Publish(new LeaveEvent
            {
                FromUser = "manager@yopmail.com",
                ToUser = leave.EmployeeEmail,
                Status = "Rejected"
            });

            return Ok(new { Message = "Leave Rejected" });
        }

        [Authorize(Roles = "HR,Manager")]
        [HttpGet("GetAllLeaves")]
        public async Task<IActionResult> GetAllLeaves()
        {
            var leave = _leaveRequests.ToList();

            return Ok(leave);
        }

        private async Task<UserModel> GetUserDetail(string email)
        {
            var _httpClient = _httpClientFactory.CreateClient("UserService");
            var response = await _httpClient.GetAsync($"/api/User/GetUserByEmail{email}");
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<UserModel>(content);

            return user;
        }
    }
}
