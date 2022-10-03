using Authentication.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Student,Manager")]
public class StudentController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<StudentController> _logger;

    public StudentController(ILogger<StudentController> logger)
    {
        _logger = logger;
    }
        [Authorize(Roles = "Student")]
        [HttpGet]
        public IActionResult GetStudent()
        {

            return Ok("From student controller");
        }

        [Authorize(Roles = "Manager")]
        [HttpGet("manager")]
        public IActionResult GetStudentmanager()
        {

            return Ok("From student controller and Manager API");
        }
}
