using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayRexApplication.Data;

namespace PayRexApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CompaniesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<CompaniesController> _logger;

        public CompaniesController(AppDbContext db, ILogger<CompaniesController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET api/Companies/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyCompany()
        {
            var companyIdStr = User.FindFirst("companyId")?.Value;
            if (string.IsNullOrEmpty(companyIdStr) || !int.TryParse(companyIdStr, out var companyId)) return Forbid();

            var company = await _db.Companies.AsNoTracking().SingleOrDefaultAsync(c => c.CompanyId == companyId);
            if (company == null) return NotFound();

            return Ok(company);
        }

        // GET api/Companies/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var companyIdStr = User.FindFirst("companyId")?.Value;
            if (string.IsNullOrEmpty(companyIdStr) || !int.TryParse(companyIdStr, out var userCompanyId)) return Forbid();

            // Only allow access to the same company for security
            if (userCompanyId != id) return Forbid();

            var company = await _db.Companies.AsNoTracking().SingleOrDefaultAsync(c => c.CompanyId == id);
            if (company == null) return NotFound();

            return Ok(company);
        }
    }
}
