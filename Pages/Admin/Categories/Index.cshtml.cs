using System.ComponentModel.DataAnnotations;
using CityEventsHub.Data;
using CityEventsHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CityEventsHub.Pages.Admin.Categories;

[Authorize(Policy = "AdministratorOnly")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public NewCategoryInput Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public List<Category> Categories { get; set; } = new();

    public class NewCategoryInput
    {
        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string IconClass { get; set; } = "fa-solid fa-calendar-days";
    }

    public async Task OnGetAsync()
    {
        Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            return Page();
        }

        if (await _context.Categories.AnyAsync(c => c.Name == Input.Name))
        {
            ErrorMessage = "A category with that name already exists.";
            return RedirectToPage();
        }

        _context.Categories.Add(new Category
        {
            Name = Input.Name,
            Description = Input.Description,
            IconClass = string.IsNullOrWhiteSpace(Input.IconClass) ? "fa-solid fa-calendar-days" : Input.IconClass
        });
        await _context.SaveChangesAsync();

        StatusMessage = "Category created.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category is null)
        {
            return RedirectToPage();
        }

        var inUse = await _context.Events.AnyAsync(e => e.CategoryId == id);
        if (inUse)
        {
            ErrorMessage = "This category is in use by one or more events and cannot be deleted.";
            return RedirectToPage();
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        StatusMessage = "Category deleted.";
        return RedirectToPage();
    }
}
