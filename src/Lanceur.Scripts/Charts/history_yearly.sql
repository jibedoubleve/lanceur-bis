select
    date(time_stamp, 'start of year') as x,
    count(*)                           as y
from alias_usage
where
    (
        @year is null
        or ( -- Use the interval allow to use the index for time_stamp
        time_stamp >= printf('%04d-01-01', @year)
        and time_stamp <  printf('%04d-12-31', @year + 1)
    )) and (
        @idalias is null
        or id_alias = @idalias
    )
group by date(time_stamp, 'start of year')
order by date(time_stamp, 'start of year')
