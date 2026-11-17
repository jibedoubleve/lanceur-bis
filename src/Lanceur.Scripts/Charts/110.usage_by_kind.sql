with
    usage as (
        select
            trim(a.file_name) as target,
            count(*)          as launches
        from
            alias a
            inner join alias_usage u on a.id = u.id_alias
        where
            a.deleted_at is null
            and (@year is null or (
                u.time_stamp >= printf('%04d-01-01', @year)
                and u.time_stamp <  printf('%04d-01-01', @year + 1)
            ))
        group by a.id
    ),
    classified as (
        select
            case
                -- 1. Explicit protocols
                when target like 'steam://%'                              then 'Steam games'
                when target like 'http://%' or target like 'https://%'   then 'Websites'
                when target like 'package:%'                              then 'Store apps'
                -- 2. Paths (local or network): an internet shortcut opens a website
                when target like '_:\%' or target like '\%'
                    then iif(target like '%.url', 'Websites', 'Applications')
                -- 3. Macros
                when target like '@%@'                                    then 'Macros'
                -- 4. System programs found through the PATH
                when target like '%.exe' or target like '%.bat' or target like '%.cmd'
                    or target like '%.msc' or target like '%.cpl'         then 'Applications'
                -- 5. A domain without protocol (www.google.com, google.com/maps)
                when instr(target, '.') > 0 and instr(target, ' ') = 0   then 'Websites'
                -- 6. Built-in commands (setup, quit, help...)
                else 'Lanceur commands'
            end as x,
            launches
        from usage
    )
select
    x,
    sum(launches) as y
from classified
group by x
order by y desc
