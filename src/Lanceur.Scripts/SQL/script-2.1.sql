-------------------------------------------------------------------------------
/*
 * View with all the usage by alias and by year 
 */
drop view if exists stat_execution_count_by_year_v;
create view stat_execution_count_by_year_v as
select
    id_keyword                 as id_keyword,
    count(*)                   as exec_count,
    keywords                   as keywords,
    strftime('%Y', time_stamp) as year
from
    stat_history_v sh
group by
	year,
    id_keyword
order by exec_count desc;