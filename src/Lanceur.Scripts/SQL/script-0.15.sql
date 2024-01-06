/*
 * Execution count is now a column into alias table.
 * It is an optimisation as the queries are a bit slow
 */

alter table alias add exec_count number;

-- Calculate the count of every alias and uptate
-- the field 'count'.
-- !!! This query can take several seconds to be executed !!!
update alias
set
    exec_count = (
        select exec_count
        from stat_execution_count_v
        where id_keyword = alias.id
    )	
