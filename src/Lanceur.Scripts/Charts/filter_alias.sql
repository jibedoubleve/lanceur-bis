select
    'All aliases' as Key,
    null          as Value,
    0             as ord
union all
select 
    Key,
    Value,
    1      as ord
from (
    select
        group_concat(an.name, ', ') as Key,
        an.id_alias                 as Value
    from
        alias a
        inner join alias_name an on a.id = an.id_alias
    where hidden is false
    group by id_alias
)
order by
    ord asc,
    Key asc