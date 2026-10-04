-- CityEventsHub: move past events into the future so they show on the public pages.
-- Every event whose date has passed is rescheduled 3 days apart, starting 2 days from today
-- (2, 5, 8 ... days ahead), keeping the events in their original order.
-- Events that are already in the future are left alone, so the script is safe to run again.

SET NOCOUNT ON;

DECLARE @today date = CAST(GETUTCDATE() AS date);

WITH PastEvents AS (
    SELECT Id, ROW_NUMBER() OVER (ORDER BY [Date], Id) AS Position
    FROM Events
    WHERE [Date] < @today
)
UPDATE e
SET e.[Date] = DATEADD(day, p.Position * 3 - 1, @today)
FROM Events e
JOIN PastEvents p ON p.Id = e.Id;

SELECT Id, [Date], Title FROM Events ORDER BY [Date];
