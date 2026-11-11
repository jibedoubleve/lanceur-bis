select
    strftime('%H:00', time_stamp) as X,
    count(*)                      as Y
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
group by strftime('%H:00', time_stamp)
order by strftime('%H:00', time_stamp)