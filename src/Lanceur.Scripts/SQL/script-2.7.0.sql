/* (#1337) Create a diagnostic view for Steam alias doubloons.                                                                                                                                                                                                                                                   
 * A bug caused duplicate aliases to be created when updating the usage of Steam games.                                                                                                                                                                                                                          
 * This view exposes those doubloons so they can be identified and cleaned up if the issue reoccurs.                                                                                                                                                                                                             
 */
drop view if exists "main"."data_doubloons_steam_v ";
create view data_doubloons_steam_v as
with normalised_alias as (select id,
                                 hidden as n_hidden,
                                 coalesce(arguments, '')   as n_arguments,
                                 coalesce(file_name, '')   as n_file_name,
                                 coalesce(run_as, '')      as n_run_as,
                                 coalesce(working_dir, '') as n_working_dir,
                                 coalesce(notes, '')       as n_notes,
                                 coalesce(icon, '')        as n_icon,
                                 coalesce(thumbnail, '')   as n_thumbnail,
                                 coalesce(exec_count, 0)   as n_exec_count,
                                 coalesce(lua_script, '')  as n_lua_script
                          from alias
                          where
                              file_name like 'steam://run/%'
                             or file_name like 'steam://rungameid/%')
select a.id            as id,
       synonyms        as name,
       a.n_arguments   as arguments,
       a.n_file_name   as file_name,
       a.n_run_as      as run_as,
       a.n_working_dir as working_dir,
       a.n_notes       as notes,
       a.n_icon        as icon,
       a.n_thumbnail   as thumbnail,
       a.n_exec_count  as exec_count,
       a.n_lua_script  as lua_script
from normalised_alias a
         left join data_alias_synonyms_v b on a.id = b.id_alias
where (n_arguments, n_file_name, n_run_as, n_lua_script)
          in (select n_arguments,
                     n_file_name,
                     n_run_as,
                     n_lua_script
              from normalised_alias
              group by n_file_name
              having count(*) > 1
                 and n_hidden is true)
order by id;

/* The view
 */
drop view if exists "main"."data_doubloons_aliases_v";
create view data_doubloons_aliases_v as
with normalised_alias as (select id,
                                 hidden as n_hidden,
                                 coalesce(arguments, '')   as n_arguments,
                                 coalesce(file_name, '')   as n_file_name,
                                 coalesce(run_as, '')      as n_run_as,
                                 coalesce(working_dir, '') as n_working_dir,
                                 coalesce(notes, '')       as n_notes,
                                 coalesce(icon, '')        as n_icon,
                                 coalesce(thumbnail, '')   as n_thumbnail,
                                 coalesce(exec_count, 0)   as n_exec_count,
                                 coalesce(lua_script, '')  as n_lua_script
                          from alias)
select a.id            as id,
       synonyms        as name,
       a.n_arguments   as arguments,
       a.n_file_name   as file_name,
       a.n_run_as      as run_as,
       a.n_working_dir as working_dir,
       a.n_notes       as notes,
       a.n_icon        as icon,
       a.n_thumbnail   as thumbnail,
       a.n_exec_count  as exec_count,
       a.n_lua_script  as lua_script
from normalised_alias a
         left join data_alias_synonyms_v b on a.id = b.id_alias
where (n_arguments, n_file_name, n_run_as, n_lua_script)
          in (select n_arguments,
                     n_file_name,
                     n_run_as,
                     n_lua_script
              from normalised_alias
              group by n_arguments,
                       n_file_name,
                       n_run_as,
                       n_lua_script
              having count(*) > 1
                 and n_hidden is false)
order by id;

/* Union of steam and aliases views to have a unique view with all doubloons
 */
drop view if exists "main"."data_doubloons_v";
create view data_doubloons_v as
    select *  from data_doubloons_aliases_v
    union all
    select * from data_doubloons_steam_v;