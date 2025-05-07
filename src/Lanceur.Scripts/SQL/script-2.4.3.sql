/* Create a view that lists the most recent usage for each alias.
 * If the last usage is NULL, the alias has never been used.
 */

drop view if exists data_last_usage_v;
create view data_last_usage_v as
select
    a.id              as id_alias,
    max(b.time_stamp) as last_usage
from
    alias a
    left join alias_usage b on a.id = b.id_alias
group by a.id;