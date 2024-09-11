update alias
set 
    icon = case
        when icon = 'RocketLaunchOutline' then 'Rocket24'
        when icon = 'Web'                 then 'link24'
        when icon = 'PageHidden'          then null
    end;