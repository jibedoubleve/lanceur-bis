/* 
 * Optimise the view and remove useless joins
 */
drop view if exists stat_history_v;
create view stat_history_v as
select
    su.id_alias                                 as id_keyword,
    group_concat(sn.name, ', ')                 as keywords,
    su.time_stamp                               as time_stamp,
    cast(strftime('%Y', time_stamp) as integer) as year
from
    alias_usage su    
    inner join alias_name sn on su.id_alias = sn.id_alias
group by
    su.time_stamp;