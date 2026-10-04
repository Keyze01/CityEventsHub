using System.ComponentModel.DataAnnotations;
using CityEventsHub.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CityEventsHub.Pages.Admin.Categories;

[Authorize(Policy = "AdministratorOnly")]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string IconClass { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category is null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IconClass = category.IconClass
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var category = await _context.Categories.FindAsync(Input.Id);
        if (category is null)
        {
            return NotFound();
        }

        category.Name = Input.Name;
        category.Description = Input.Description;
        category.IconClass = Input.IconClass;
        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = "Category updated.";
        return RedirectToPage("/Admin/Categories/Index");
    }
}
