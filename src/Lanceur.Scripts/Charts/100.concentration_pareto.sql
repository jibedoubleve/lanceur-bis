with
    usage_per_alias as (
        select
            u.id_alias,
            count(*) as usage
        from
            alias_usage u
            inner join alias a on a.id = u.id_alias
        where
            a.deleted_at is null
            and (@year is null or (
                u.time_stamp >= printf('%04d-01-01', @year)
                and u.time_stamp <  printf('%04d-01-01', @year + 1)
            ))
        group by u.id_alias
    ),
    ranked as (
        select
            row_number() over w as rank,
            sum(usage)   over w as cumulative,
            sum(usage)   over () as total
        from usage_per_alias
        window w as (order by usage desc, id_alias)
    )
select
    rank                                   as x,
    round(100.0 * cumulative / total, 1)   as y
from ranked
order by rank