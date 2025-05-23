/* Add lua_script in the query (#1010)
 */

drop view if exists data_doubloons_v;
create view data_doubloons_v as
select distinct
    a.id,
    group_concat(distinct an.name) as name,
    a.arguments,
    a.file_name,
    a.run_as,
    a.working_dir,
    a.notes,
    a.icon,
    a.exec_count,
    a.lua_script
from
    alias a
        inner join (
        select
            file_name,
            arguments,
            run_as,
            working_dir,
            lua_script
        from alias
        group by file_name,
                 arguments,
                 run_as,
                 working_dir
        having count(*) > 1
    ) t on (a.file_name = t.file_name      or (a.file_name = t.file_name) is null)
        and (a.arguments = t.arguments     or (a.arguments = t.arguments) is null)
        and (a.run_as = t.run_as           or (a.run_as = t.run_as) is null)
        and (a.working_dir = t.working_dir or (a.working_dir = t.working_dir) is null)
        and (a.lua_script = t.lua_script   or (a.lua_script = t.lua_script) is null)
        inner join alias_name an on a.id = an.id_alias
group by a.id