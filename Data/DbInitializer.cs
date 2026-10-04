using CityEventsHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CityEventsHub.Data;

/// <summary>
/// Applies pending migrations and seeds roles, a default administrator account,
/// default categories/cities, and a handful of sample approved events.
/// Safe to run on every startup: every step checks for existing data first.
/// </summary>
public static class DbInitializer
{
    public const string AdminEmail = "admin@cityeventshub.com";
    public const string AdminPassword = "Admin@123";

    public static readonly string[] Roles = { "Administrator", "Organizer", "RegisteredUser" };

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await SeedRolesAsync(roleManager);
        var admin = await SeedAdminAsync(userManager);
        var categories = await SeedCategoriesAsync(context);
        var cities = await SeedCitiesAsync(context);
        await SeedSampleEventsAsync(context, admin, categories, cities);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task<ApplicationUser> SeedAdminAsync(UserManager<ApplicationUser> userManager)
    {
        var admin = await userManager.FindByEmailAsync(AdminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true,
                FullName = "Site Administrator",
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, AdminPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to seed administrator account: " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(admin, "Administrator"))
        {
            await userManager.AddToRoleAsync(admin, "Administrator");
        }

        return admin;
    }

    private static readonly (string Name, string Description, string Icon)[] DefaultCategories =
    {
        ("Conferences", "Professional and industry conferences.", "fa-solid fa-users-rectangle"),
        ("Seminars", "Educational seminars and workshops.", "fa-solid fa-chalkboard-user"),
        ("Festivals", "Cultural and community festivals.", "fa-solid fa-champagne-glasses"),
        ("Concerts", "Live music and concerts.", "fa-solid fa-music"),
        ("Religious Events", "Religious gatherings and observances.", "fa-solid fa-place-of-worship"),
        ("Sports", "Sporting events and tournaments.", "fa-solid fa-futbol"),
        ("Government Events", "Public sector and government events.", "fa-solid fa-landmark"),
        ("Exhibitions", "Art, trade, and product exhibitions.", "fa-solid fa-image"),
        ("Job Fairs", "Career and recruitment fairs.", "fa-solid fa-briefcase"),
        ("University Events", "Campus and academic events.", "fa-solid fa-graduation-cap"),
        ("Charity Events", "Fundraisers and charity drives.", "fa-solid fa-hand-holding-heart"),
        ("Food Festivals", "Food and culinary festivals.", "fa-solid fa-utensils"),
        ("Weddings", "Public wedding invitations.", "fa-solid fa-rings-wedding"),
        ("Book Launches", "Author talks and book launches.", "fa-solid fa-book"),
    };

    private static async Task<List<Category>> SeedCategoriesAsync(ApplicationDbContext context)
    {
        if (!await context.Categories.AnyAsync())
        {
            context.Categories.AddRange(DefaultCategories.Select(c => new Category
            {
                Name = c.Name,
                Description = c.Description,
                IconClass = c.Icon
            }));
            await context.SaveChangesAsync();
        }

        return await context.Categories.ToListAsync();
    }

    private static readonly (string Name, string Region, string Country, double Lat, double Lng)[] DefaultCities =
    {        
        ("Garowe", "Nugaal", "Somalia", 8.4080, 48.4840),
        ("Bosaso", "Bari", "Somalia", 11.2760, 49.1880),
        ("Qardho", "Karkaar", "Somalia", 9.5120, 49.0870),
        ("Galkayo", "Mudug", "Somalia", 6.7870, 47.4390),
        ("Eyl", "Nugaal", "Somalia", 7.9800, 49.8150),
        ("Badhan", "Sanaag", "Somalia", 10.7090, 48.3370),
        ("Dhahar", "Haylaan", "Somalia", 9.9400, 48.8680),
        ("Burtinle", "Nugaal", "Somalia", 7.7980, 47.9940),
        ("Harfo", "Mudug", "Somalia", 7.3500, 47.6220),
    };

    private static async Task<List<City>> SeedCitiesAsync(ApplicationDbContext context)
    {
        if (!await context.Cities.AnyAsync())
        {
            context.Cities.AddRange(DefaultCities.Select(c => new City
            {
                Name = c.Name,
                Region = c.Region,
                Country = c.Country,
                Latitude = c.Lat,
                Longitude = c.Lng
            }));
            await context.SaveChangesAsync();
        }

        return await context.Cities.ToListAsync();
    }

    private static async Task SeedSampleEventsAsync(
        ApplicationDbContext context,
        ApplicationUser organizer,
        List<Category> categories,
        List<City> cities)
    {
        if (await context.Events.AnyAsync())
        {
            return;
        }

        Category? Cat(string name) => categories.FirstOrDefault(c => c.Name == name);
        City? Loc(string name) => cities.FirstOrDefault(c => c.Name == name);

        var today = DateTime.UtcNow.Date;

        var sampleEvents = new List<Event>
        {
            new()
            {
                Title = "Somali Tech Summit 2026",
                Description = "A gathering of technology leaders, startups, and developers from across the region.",
                Category = Cat("Conferences"),
                Date = today.AddDays(14),
                Time = new TimeSpan(9, 0, 0),
                Venue = "Garowe Convention Center",
                City = Loc("Garowe"),
                OrganizerId = organizer.Id,
                ContactEmail = "info@somalitechsummit.example",
                RegistrationRequired = true,
                MaxAttendees = 500,
                IsApproved = true
            },
            new()
            {
                Title = "Garowe Cultural Festival",
                Description = "Celebrating Somali music, food, and art in the heart of Garowe.",
                Category = Cat("Festivals"),
                Date = today.AddDays(21),
                Time = new TimeSpan(16, 0, 0),
                Venue = "Garowe Central Park",
                City = Loc("Garowe"),
                OrganizerId = organizer.Id,
                ContactEmail = "events@garowefestival.example",
                RegistrationRequired = false,
                IsApproved = true
            },
            new()
            {
                Title = "Qardho Live Music Night",
                Description = "An evening of live performances from East Africa's rising music stars.",
                Category = Cat("Concerts"),
                Date = today.AddDays(7),
                Time = new TimeSpan(19, 30, 0),
                Venue = "Qardho Cultural Center",
                City = Loc("Qardho"),
                OrganizerId = organizer.Id,
                ContactEmail = "tickets@qardholive.example",
                RegistrationRequired = true,
                MaxAttendees = 1200,
                IsApproved = true
            },
            new()
            {
                Title = "Bosaso Charity Gala",
                Description = "A black-tie fundraising gala supporting community health initiatives.",
                Category = Cat("Charity Events"),
                Date = today.AddDays(30),
                Time = new TimeSpan(18, 0, 0),
                Venue = "Grand Hall Bosaso",
                City = Loc("Bosaso"),
                OrganizerId = organizer.Id,
                ContactEmail = "gala@bosasocharity.example",
                RegistrationRequired = true,
                MaxAttendees = 300,
                IsApproved = true
            },
            new()
            {
                Title = "Galkayo Job Fair 2026",
                Description = "Connecting job seekers with leading employers across multiple industries.",
                Category = Cat("Job Fairs"),
                Date = today.AddDays(10),
                Time = new TimeSpan(10, 0, 0),
                Venue = "Galkayo World Trade Centre",
                City = Loc("Galkayo"),
                OrganizerId = organizer.Id,
                ContactEmail = "careers@galkayojobfair.example",
                RegistrationRequired = true,
                MaxAttendees = 2000,
                IsApproved = true
            },
            new()
            {
                Title = "Eyl Book Launch: 'Bridges of Two Continents'",
                Description = "A book launch and author talk celebrating a new historical novel.",
                Category = Cat("Book Launches"),
                Date = today.AddDays(5),
                Time = new TimeSpan(17, 0, 0),
                Venue = "Eyl City Library",
                City = Loc("Eyl"),
                OrganizerId = organizer.Id,
                ContactEmail = "press@eybooks.example",
                RegistrationRequired = false,
                IsApproved = true
            },
            new()
            {
                Title = "Badhan Food Festival",
                Description = "A celebration of international cuisine with over 50 food vendors.",
                Category = Cat("Food Festivals"),
                Date = today.AddDays(18),
                Time = new TimeSpan(12, 0, 0),
                Venue = "Badhan Central Market",
                City = Loc("Badhan"),
                OrganizerId = organizer.Id,
                ContactEmail = "hello@badhanfoodfest.example",
                RegistrationRequired = false,
                IsApproved = true
            },
            new()
            {
                Title = "Burtinle University Open Day",
                Description = "Prospective students explore campus life, programs, and scholarships.",
                Category = Cat("University Events"),
                Date = today.AddDays(25),
                Time = new TimeSpan(9, 30, 0),
                Venue = "Burtinle State University",
                City = Loc("Burtinle"),
                OrganizerId = organizer.Id,
                ContactEmail = "admissions@bsu.example",
                RegistrationRequired = true,
                MaxAttendees = 800,
                IsApproved = true
            },
            new()
            {
                Title = "East Africa Sports Championship",
                Description = "Regional athletics championship featuring track and field events.",
                Category = Cat("Sports"),
                Date = today.AddDays(40),
                Time = new TimeSpan(8, 0, 0),
                Venue = "Dhahar National Stadium",
                City = Loc("Dhahar"),
                OrganizerId = organizer.Id,
                ContactEmail = "info@eastafricachampionship.example",
                RegistrationRequired = true,
                MaxAttendees = 5000,
                IsApproved = true
            },
            new()
            {
                Title = "Harfo Art Exhibition",
                Description = "A showcase of contemporary Somali art and photography.",
                Category = Cat("Exhibitions"),
                Date = today.AddDays(12),
                Time = new TimeSpan(15, 0, 0),
                Venue = "Harfo National Museum",
                City = Loc("Harfo"),
                OrganizerId = organizer.Id,
                ContactEmail = "curator@harfoart.example",
                RegistrationRequired = false,
                IsApproved = true
            },
        };

        // A sample event whose category or city is not in the database is skipped rather than
        // failing startup (the tables may hold a different set than the defaults above).
        sampleEvents.RemoveAll(e => e.Category is null || e.City is null);

        context.Events.AddRange(sampleEvents);
        await context.SaveChangesAsync();
    }
}
