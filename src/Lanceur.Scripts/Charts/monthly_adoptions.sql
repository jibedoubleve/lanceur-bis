select
    first_month  as x,
    count(alias) as y
from (
     select
        date(min(time_stamp), 'start of month') as first_month,
        id_alias                                as alias
    from alias_usage
    group by id_alias
)
where
    @year is null or (
    -- Use the interval allow to use the index for time_stamp
    x >= printf('%04d-01-01' , @year)
    and x  < printf('%04d-01-01', @year + 1)
)
group by first_month
order by first_month