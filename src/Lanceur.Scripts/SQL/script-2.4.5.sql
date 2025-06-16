/* Fixing statistics issues (#
 */

-- Show execution count per alias
drop view if exists stat_execution_count_v;
create view stat_execution_count_v as
select
    id_keyword as id_keyword,
    count(*)   as exec_count,
    keywords   as keywords    
from
    stat_history_v sh
group by
    id_keyword
order by
    exec_count desc;

-- show execution count per alias and per year
drop view if exists stat_execution_count_by_year_v;
create view stat_execution_count_by_year_v as
select
    id_keyword as id_keyword,
    keywords   as keywords,
    count(id_keyword) as exec_count,
    strftime('%Y', time_stamp) as year
from
        stat_history_v sh
group by year, id_keyword
order by id_keyword, year;