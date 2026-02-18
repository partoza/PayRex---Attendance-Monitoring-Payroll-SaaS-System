using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace PayRex.Web.Pages.Admin
{
 [Authorize(Roles = "SuperAdmin,Admin")]
 public class AddUserModel : PageModel
 {
 [BindProperty]
 public InputModel Input { get; set; } = new();

 public string? ErrorMessage { get; set; }
 public string? SuccessMessage { get; set; }

 public class InputModel
 {
 [Required]
 public string FirstName { get; set; } = string.Empty;
 [Required]
 public string LastName { get; set; } = string.Empty;
 [Required]
 [EmailAddress]
 public string Email { get; set; } = string.Empty;
 [Required]
 public string Role { get; set; } = "Hr";
 [Required]
 public int CompanyId { get; set; }
 }

 public void OnGet()
 {
 }

 public async Task<IActionResult> OnPostAsync()
 {
 if (!ModelState.IsValid)
 {
 ErrorMessage = "Invalid input";
 return Page();
 }

 // For now create via API would be ideal. We'll just show success in UI stub.
 SuccessMessage = "User creation is stubbed in demo. Implement API call to create users.";
 return Page();
 }
 }
}
