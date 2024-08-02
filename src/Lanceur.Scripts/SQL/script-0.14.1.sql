/*
 * Split icons and thumbnails into separate columns
 */
alter table alias
    add thumbnail text;

/*
 * Split icons and thumbnails. This means that icon should represent 
 * a key that references  an icon, the application should know how
 * to handle this. Precedence is also handled by the application. 
 * The expected rule is thumbnail has the priority and icon is a
 * fallback if there's no image to display
 */
update alias
set icon      = null,
    thumbnail = icon
where icon like '%\%'
-- With a backslash, it means it is an URI to a file
-- The risk of an error is low as the icon is a key
-- (no space, no special char)