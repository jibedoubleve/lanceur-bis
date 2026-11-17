with recursive
    -- All the days of the week
    day(value) as (select 1 union all select value + 1 from day where value < 7),
    -- All the hours of the day
    hour(value) as (select 0 union all select (value + 1) from hour where value < 23),
    -- Usage count per day and hour
    heatmap(day,hour,usage) as (
        select
            cast(strftime('%u', time_stamp) as integer) as day,
            cast(strftime('%H', time_stamp) as integer) as hour,
            count(*)                                    as usage
        from alias_usage
        where (@year is null or (
                -- Use the interval allow to use the index for time_stamp
                time_stamp >= printf('%04d-01-01', @year)
                and time_stamp <  printf('%04d-01-01', @year + 1)
            )) and (
                @idalias is null
                or id_alias = @idalias
            )
        group by day, hour
    )
/* Cartesian product between days and hours to get every hour of every day.
   Then, left join the usage and fill the gaps with 0.
 */
select
    printf('%02d:00', h.value)  as x,
    case d.value
        when 1 then 'Monday'
        when 2 then 'Tuesday'
        when 3 then 'Wednesday'
        when 4 then 'Thursday'
        when 5 then 'Friday'
        when 6 then 'Saturday'
        when 7 then 'Sunday'
        end as serie,
    coalesce(hm.usage,0) as y
from day d
         cross join hour h
         left join heatmap hm on hm.day = d.value and hm.hour = h.value
order by d.value, h.value