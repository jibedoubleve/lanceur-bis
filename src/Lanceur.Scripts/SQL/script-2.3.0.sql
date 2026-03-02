begin
transaction;

update alias
set icon = 'Link24'
where file_name like 'http%';

commit transaction;

PRAGMA
foreign_keys=off;
begin
transaction;

/* The 'thumbnail' field in the 'alias' table is no longer in use.  
 * Therefore, we are removing it from the database. 
 */
--------------------------------------------------------------
---- Create a temporary table                             ----
--------------------------------------------------------------
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
    deleted_at            datetime         default null,
    lua_script            text,
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
       deleted_at,
       lua_script,
       exec_count,
       confirmation_required
from alias;

--------------------------------------------------------------
---- Recreate and fill alias table                        ----
--------------------------------------------------------------
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
       deleted_at,
       lua_script,
       exec_count,
       confirmation_required
from temp_alias;

commit transaction;
PRAGMA
foreign_keys=on;