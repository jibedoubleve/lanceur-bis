/*
 * Add a new column to the 'alias' table to support logical deletion 
 * functionality.
 */

--------------------------------------------------------
-- create and fill temp table
--------------------------------------------------------
drop table if exists temp_alias;

create table temp_alias
(
    id                    integer primary key,
    arguments             text,
    file_name             text,
    notes                 text,
    run_as                text,
    start_mode            text,
    working_dir           text,
    icon                  text,
    hidden                integer not null default 0 check (hidden in (0, 1)),
    lua_script            text,
    thumbnail             text,
    exec_count            number,
    confirmation_required integer          default 0
);

insert into temp_alias
select id,
       arguments,
       file_name,
       notes,
       run_as,
       start_mode,
       working_dir,
       icon,
       hidden,
       lua_script,
       thumbnail,
       exec_count,
       confirmation_required
from alias;

--------------------------------------------------------
-- drop and recreate and fill new table
--------------------------------------------------------
drop table if exists alias;

create table alias
(
    id                    integer primary key,
    arguments             text,
    file_name             text,
    notes                 text,
    run_as                text,
    start_mode            text,
    working_dir           text,
    icon                  text,
    hidden                integer not null default 0 check (hidden in (0, 1)),
    deleted_at            datetime         default null,
    lua_script            text,
    thumbnail             text,
    exec_count            number,
    confirmation_required integer          default 0
);

insert into alias
select id,
       arguments,
       file_name,
       notes,
       run_as,
       start_mode,
       working_dir,
       icon,
       hidden,
       null as deleted_at,
       lua_script,
       thumbnail,
       exec_count,
       confirmation_required
from temp_alias;

--------------------------------------------------------
--- cleanup temp tables
--------------------------------------------------------
drop table if exists temp_alias;
