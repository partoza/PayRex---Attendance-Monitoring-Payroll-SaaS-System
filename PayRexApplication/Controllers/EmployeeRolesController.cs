using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayRexApplication.Data;
using PayRexApplication.Models;
using PayRexApplication.DTOs;

namespace PayRexApplication.Controllers
{
 [ApiController]
 [Route("api/[controller]")]
 [Authorize(Roles = "SuperAdmin,Admin,HR")]
 public class EmployeeRolesController : ControllerBase
 {
 private readonly AppDbContext _db;
 public EmployeeRolesController(AppDbContext db) { _db = db; }

 [HttpGet]
 public async Task<IActionResult> GetRoles()
 {
	 var userCompanyIdStr = User.FindFirst("companyId")?.Value;
	 if (string.IsNullOrEmpty(userCompanyIdStr) || !int.TryParse(userCompanyIdStr, out var userCompanyId)) return Forbid();

	 var roles = await _db.EmployeeRoles.Where(r => r.CompanyId == userCompanyId).ToListAsync();
	 return Ok(roles);
 }

 [HttpPost("sync")]
 public async Task<IActionResult> SyncRoles([FromBody] List<EmployeeRoleDto> roles)
 {
 var userCompanyIdStr = User.FindFirst("companyId")?.Value;
 if (string.IsNullOrEmpty(userCompanyIdStr) || !int.TryParse(userCompanyIdStr, out var userCompanyId)) return Forbid();

 // Upsert by RoleId (0/new = insert), ensure CompanyId
 foreach (var r in roles)
 {
 if (r.RoleId <=0)
 {
 var newRole = new EmployeeRole
 {
	 CompanyId = userCompanyId,
	 RoleName = r.RoleName,
	 BasicRate = r.BasicRate,
	 RateType = r.RateType,
	 Description = r.Description,
	 IsActive = true,
	 CreatedAt = DateTime.UtcNow
 };
 _db.EmployeeRoles.Add(newRole);
 }
 else
 {
 var existing = await _db.EmployeeRoles.FindAsync(r.RoleId);
 if (existing != null && existing.CompanyId == userCompanyId)
 {
 existing.RoleName = r.RoleName;
 existing.BasicRate = r.BasicRate;
 existing.RateType = r.RateType;
 existing.Description = r.Description;
 existing.UpdatedAt = DateTime.UtcNow;
 }
 }
 }

 await _db.SaveChangesAsync();
 return Ok(new { message = "Roles synced" });
 }

 // Mark inactive instead of delete
 [HttpDelete("{id}")]
 public async Task<IActionResult> Delete(int id)
 {
 var userCompanyIdStr = User.FindFirst("companyId")?.Value;
 if (string.IsNullOrEmpty(userCompanyIdStr) || !int.TryParse(userCompanyIdStr, out var userCompanyId)) return Forbid();

 var role = await _db.EmployeeRoles.FindAsync(id);
 if (role == null || role.CompanyId != userCompanyId) return NotFound();

 // Prevent deletion of built-in roles (HR, Accountant)
 if (role.IsBuiltIn)
     return BadRequest(new { message = "Cannot delete built-in roles. You can edit their description and rate instead." });

 // mark inactive
 role.IsActive = false;
 role.UpdatedAt = DateTime.UtcNow;
 await _db.SaveChangesAsync();
 return Ok(new { message = "Role marked inactive" });
 }

 [HttpPatch("{id}/toggle-active")]
 public async Task<IActionResult> ToggleActive(int id)
 {
 var userCompanyIdStr = User.FindFirst("companyId")?.Value;
 if (string.IsNullOrEmpty(userCompanyIdStr) || !int.TryParse(userCompanyIdStr, out var userCompanyId)) return Forbid();
 var role = await _db.EmployeeRoles.FindAsync(id);
 if (role == null || role.CompanyId != userCompanyId) return NotFound();
 role.IsActive = !role.IsActive;
 role.UpdatedAt = DateTime.UtcNow;
 await _db.SaveChangesAsync();
 return Ok(new { message = "Role active state toggled", isActive = role.IsActive });
 }
 }
}
