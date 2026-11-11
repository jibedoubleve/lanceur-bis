select 
    'All years' as Key,
    null        as Value,
    0           as ord
union all
select 
    distinct strftime('%Y', time_stamp)         as Key,
    cast(strftime('%Y', time_stamp) as integer) as Value,
    1                                           as ord
from alias_usage
order by 
    ord asc,
    Key asc