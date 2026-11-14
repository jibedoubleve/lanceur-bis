select
    case strftime('%u', time_stamp)
        when '1' then 'Monday'
        when '2' then 'Tuesday'
        when '3' then 'Wednesday'
        when '4' then 'Thursday'
        when '5' then 'Friday'
        when '6' then 'Saturday'
        when '7' then 'Sunday'
        end as X,
    count(id) as Y
from alias_usage
where
    (@year is null or (
        -- Use the interval allow to use the index for time_stamp
        time_stamp >= printf('%04d-01-01', @year)
          and time_stamp <  printf('%04d-01-01', @year + 1)
    )) and (
        @idalias is null
        or id_alias = @idalias
    )
group by strftime('%u', time_stamp)
order by strftime('%u', time_stamp)