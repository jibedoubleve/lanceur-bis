/* Add lua_script in the query (#1010)
 */
drop view if exists data_doubloons_v;
create view data_doubloons_v as
with normalised_alias as (
    select
        id,
        coalesce(arguments, '')   as n_arguments,
        coalesce(file_name, '')   as n_file_name,
        coalesce(run_as, '')      as n_run_as,
        coalesce(working_dir, '') as n_working_dir,
        coalesce(notes, '')       as n_notes,
        coalesce(icon, '')        as n_icon,
        coalesce(exec_count, 0)   as n_exec_count,
        coalesce(lua_script, '')  as n_lua_script
    from alias
)
select
    a.id            as id,
    synonyms        as name,
    a.n_arguments   as arguments,
    a.n_file_name   as file_name,
    a.n_run_as      as run_as,
    a.n_working_dir as working_dir,
    a.n_notes       as notes,
    a.n_icon        as icon,
    a.n_exec_count  as exec_count,
    a.n_lua_script  as lua_script
from
    normalised_alias a
    left join data_alias_synonyms_v b on a.id = b.id_alias
where (n_arguments, n_file_name, n_run_as, n_notes, n_icon, n_lua_script) in (
    select
        n_arguments,
        n_file_name,
        n_run_as,
        n_notes,
        n_icon,
        n_lua_script
    from normalised_alias
    group by
        n_arguments,
        n_file_name,
        n_run_as,
        n_notes,
        n_icon,
        n_lua_script
    having count(*) > 1
)
order by id    