/* (#1322) Add thumbnail support to the most-used alias views
 *
 * Updated stat_history_v, stat_execution_count_by_year_v and stat_execution_count_v
 * to include the 'thumbnail' column. This allows displaying each alias's own thumbnail
 * instead of a default icon in the most-used aliases view.
 */
drop view if exists stat_history_v;
create view stat_history_v as
select
    a.id                             as id_keyword,
    group_concat (an.name, ', ')     as keywords,
    su.time_stamp                    as time_stamp,
    a.thumbnail                      as thumbnail,
    a.Icon                           as icon,
    cast(strftime ('%Y', time_stamp) as integer) as year
from
    alias_usage su
    inner join alias a on su.id_alias = a.id
    inner join alias_name an on a.id = an.id_alias
group by
    su.time_stamp, a.id; -- Include a.id to disambiguate rows when two different 
                         -- aliases share the same timestamp (highly unlikely 
                         -- in practice)
-------------------------------------------------------------------------------
drop view if exists stat_execution_count_by_year_v;
create view stat_execution_count_by_year_v as
select
    id_keyword        as id_keyword,
    keywords          as keywords,
    count(id_keyword) as exec_count,
    thumbnail         as thumbnail,
    Icon              as icon,
    year              as year
from stat_history_v sh 
group by year, id_keyword 
order by id_keyword, year;
-------------------------------------------------------------------------------
drop view if exists stat_execution_count_v;
create view stat_execution_count_v as
select
    id_keyword        as id_keyword,
    count(id_keyword) as exec_count,
    keywords          as keywords,
    thumbnail         as thumbnail,
    Icon              as icon
from stat_history_v sh
group by id_keyword
order by exec_count desc;
