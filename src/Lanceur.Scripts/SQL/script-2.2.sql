/* Usage per year
 */
create view stat_usage_per_year_v as
select
    count(*)                         as exec_count,
    strftime('%Y-01-01', time_stamp) as year
from alias_usage
group by strftime('%Y-01-01', time_stamp)
order by time_stamp