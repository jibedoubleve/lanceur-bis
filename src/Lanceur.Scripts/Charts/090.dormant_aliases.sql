with 
    active_aliases as (
        select
            a.id              as id_alias,
            max(b.time_stamp) as last_usage
        from
            alias a
            left join alias_usage b on a.id = b.id_alias
        where deleted_at is null
        group by a.id
    )

select
    '< 7 days'   as x,
    count(*)     as y
from active_aliases
where last_usage >= date('now', '-7 days')

union all

select
    '7 days - 1 month' as x,
    count(*)           as y
from active_aliases
where
    last_usage < date('now', '-7 days')
    and last_usage >= date('now', '-1 month')

union all

select
    '1 - 6 months' as x,
    count(*)       as y
from active_aliases
where
    last_usage < date('now', '-1 month')
    and last_usage >= date('now', '-6 months')

union all

select
    '6 - 12 months' as x,
    count(*)     as y
from active_aliases
where 
    last_usage < date('now', '-6 months')
    and last_usage >= date('now', '-12 months')
union all

select
    '> 1 year' as x,
    count(*)     as y
from active_aliases
where last_usage < date('now', '-12 months')

union all

select
    'Never used' as x,
    count(*)     as y
from active_aliases
where last_usage is null

