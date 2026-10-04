-- CityEventsHub: replace the city list with Puntland cities.
--   1. Adds the Puntland cities that are not in the database yet.
--   2. Converts the original sample events to their Puntland versions (title, venue, city, contact).
--   3. Moves any other event that is still in a non-Puntland city to Garowe.
--   4. Removes the non-Puntland cities.
-- Safe to run more than once.

SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

DECLARE @Puntland TABLE (Name nvarchar(100), Region nvarchar(100), Latitude float, Longitude float);
INSERT INTO @Puntland (Name, Region, Latitude, Longitude) VALUES
    (N'Garowe',   N'Nugaal',  8.4080, 48.4840),
    (N'Bosaso',   N'Bari',    11.2760, 49.1880),
    (N'Qardho',   N'Karkaar', 9.5120, 49.0870),
    (N'Galkayo',  N'Mudug',   6.7870, 47.4390),
    (N'Eyl',      N'Nugaal',  7.9800, 49.8150),
    (N'Badhan',   N'Sanaag',  10.7090, 48.3370),
    (N'Dhahar',   N'Haylaan', 9.9400, 48.8680),
    (N'Burtinle', N'Nugaal',  7.7980, 47.9940),
    (N'Harfo',    N'Mudug',   7.3500, 47.6220);

-- 1. Add missing Puntland cities
INSERT INTO Cities (Name, Region, Country, Latitude, Longitude)
SELECT p.Name, p.Region, N'Somalia', p.Latitude, p.Longitude
FROM @Puntland p
WHERE NOT EXISTS (SELECT 1 FROM Cities c WHERE c.Name = p.Name);

-- 2. Convert the sample events
DECLARE @Samples TABLE (OldTitle nvarchar(200), NewTitle nvarchar(200), Description nvarchar(1000),
                        Venue nvarchar(200), City nvarchar(100), ContactEmail nvarchar(200));
INSERT INTO @Samples (OldTitle, NewTitle, Description, Venue, City, ContactEmail) VALUES
    (N'Somali Tech Summit 2026', N'Somali Tech Summit 2026',
     N'A gathering of technology leaders, startups, and developers from across the region.',
     N'Garowe Convention Center', N'Garowe', N'info@somalitechsummit.example'),
    (N'Hargeisa Cultural Festival', N'Garowe Cultural Festival',
     N'Celebrating Somali music, food, and art in the heart of Garowe.',
     N'Garowe Central Park', N'Garowe', N'events@garowefestival.example'),
    (N'Nairobi Live Music Night', N'Qardho Live Music Night',
     N'An evening of live performances from East Africa''s rising music stars.',
     N'Qardho Cultural Center', N'Qardho', N'tickets@qardholive.example'),
    (N'London Charity Gala', N'Bosaso Charity Gala',
     N'A black-tie fundraising gala supporting community health initiatives.',
     N'Grand Hall Bosaso', N'Bosaso', N'gala@bosasocharity.example'),
    (N'Dubai Job Fair 2026', N'Galkayo Job Fair 2026',
     N'Connecting job seekers with leading employers across multiple industries.',
     N'Galkayo World Trade Centre', N'Galkayo', N'careers@galkayojobfair.example'),
    (N'Istanbul Book Launch: ''Bridges of Two Continents''', N'Eyl Book Launch: ''Bridges of Two Continents''',
     N'A book launch and author talk celebrating a new historical novel.',
     N'Eyl City Library', N'Eyl', N'press@eybooks.example'),
    (N'Toronto Food Festival', N'Badhan Food Festival',
     N'A celebration of international cuisine with over 50 food vendors.',
     N'Badhan Central Market', N'Badhan', N'hello@badhanfoodfest.example'),
    (N'Minneapolis University Open Day', N'Burtinle University Open Day',
     N'Prospective students explore campus life, programs, and scholarships.',
     N'Burtinle State University', N'Burtinle', N'admissions@bsu.example'),
    (N'East Africa Sports Championship', N'East Africa Sports Championship',
     N'Regional athletics championship featuring track and field events.',
     N'Dhahar National Stadium', N'Dhahar', N'info@eastafricachampionship.example'),
    (N'Mogadishu Art Exhibition', N'Harfo Art Exhibition',
     N'A showcase of contemporary Somali art and photography.',
     N'Harfo National Museum', N'Harfo', N'curator@harfoart.example');

UPDATE e
SET e.Title = s.NewTitle,
    e.Description = s.Description,
    e.Venue = s.Venue,
    e.ContactEmail = s.ContactEmail,
    e.CityId = c.Id
FROM Events e
JOIN @Samples s ON s.OldTitle = e.Title
JOIN Cities c ON c.Name = s.City;

-- 3. Any other event still in a non-Puntland city moves to Garowe
UPDATE e
SET e.CityId = (SELECT Id FROM Cities WHERE Name = N'Garowe')
FROM Events e
JOIN Cities c ON c.Id = e.CityId
WHERE c.Name NOT IN (SELECT Name FROM @Puntland);

-- 4. Remove the non-Puntland cities
DELETE FROM Cities WHERE Name NOT IN (SELECT Name FROM @Puntland);

COMMIT TRANSACTION;

SELECT Id, Name, Region FROM Cities ORDER BY Name;
SELECT e.Id, e.Title, c.Name AS City, e.Venue FROM Events e JOIN Cities c ON c.Id = e.CityId ORDER BY e.Id;
