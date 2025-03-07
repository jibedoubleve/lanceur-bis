/* 
 * Enhancements:
 * - Added a 'year' column to the usage views to allow faster data retrieval.
 */

/* 
 * View: stat_history_v
 */
drop view if exists stat_history_v;
create view stat_history_v as
select
    s.id                                        as id_keyword,
    group_concat(sn.name, ', ')                 as keywords,
    su.time_stamp                               as time_stamp,
    cast(strftime('%Y', time_stamp) as integer) as year
from
    alias_usage su
    inner join alias s on su.id_alias = s.id
    inner join alias_name sn on s.id = sn.id_alias
group by
    su.time_stamp;

