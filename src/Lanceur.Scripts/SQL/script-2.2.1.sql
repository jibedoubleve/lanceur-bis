-- When thumbnails are empty, they are treated as paths.  
-- Since they cannot be resolved, they are displayed as empty images,  
-- which override the fallback icons.  
update alias
set
    thumbnail = NULL
where thumbnail = '';

-- Favicons use the 'ico' format, so replace all old '.png' entries with '.ico'.  
-- This update applies only to favicons.  
update alias
set thumbnail = replace(thumbnail, '.png', '.ico')
where
    thumbnail like '%favicon_%';