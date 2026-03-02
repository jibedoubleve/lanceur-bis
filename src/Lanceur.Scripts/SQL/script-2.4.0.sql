/* 
 * Fix the view to prevent grouping of alias groups that should not be grouped 
 * under certain specific circumstances.
 */
drop view if exists stat_history_v;
create view stat_history_v as
select a.id          as id_keyword,
       s.synonyms    as keywords,
       su.time_stamp as time_stamp,
       cast(strftime('%Y', time_stamp) as integer) as year
from
    alias_usage su
    inner join alias a on a.id = su.id_alias
    inner join alias_name an on an.id_alias = a.id
    inner join data_alias_synonyms_v s on a.id = s.id_alias;