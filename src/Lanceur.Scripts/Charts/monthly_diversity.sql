select
    date(time_stamp, 'start of month') as x,
    count(distinct id_alias)           as y
from alias_usage
where
    @year is null or (
        -- Use the interval allow to use the index for time_stamp
        time_stamp >= printf('%04d-01-01' , @year)
        and time_stamp  < printf('%04d-01-01', @year + 1)
    )
group by date(time_stamp, 'start of month')
order by date(time_stamp, 'start of month')
