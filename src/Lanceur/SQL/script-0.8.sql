-------------------------------------------------------------------------------
/*
 * Add a field 'hidden' to allow to hide some results from search queries
 */

alter table alias 
add 
	hidden integer not null default 0 check (hidden in (0,1)); -- 0 = FALSE 1 == TRUE

--
-- Add default keywords into alias table
--
insert into alias (id, file_name, hidden) values ((select max(rowid) + 1 from alias), 'add', 1); 
insert into alias_name (id_alias, name) values ((select max(rowid) from alias), 'add');

insert into alias (id, file_name, hidden) values ((select max(rowid) + 1 from alias),'=', 1); 
insert into alias_name (id_alias, name) values ((select max(rowid) from alias), '=');

insert into alias (id, file_name, hidden) values ((select max(rowid) + 1 from alias),'import' , 1); 
insert into alias_name (id_alias, name) values ((select max(rowid) from alias), 'import');

insert into alias (id, file_name, hidden) values ((select max(rowid) + 1 from alias),'quit', 1); 
insert into alias_name (id_alias, name) values ((select max(rowid) from alias), 'quit');

insert into alias (id, file_name, hidden) values ((select max(rowid) + 1 from alias),'sessions', 1); 
insert into alias_name (id_alias, name) values ((select max(rowid) from alias), 'sessions');

insert into alias (id, file_name, hidden) values ((select max(rowid) + 1 from alias),'setup', 1); 
insert into alias_name (id_alias, name) values ((select max(rowid) from alias), 'setup');

insert into alias (id, file_name, hidden) values ((select max(rowid) + 1 from alias),'version', 1); 
insert into alias_name (id_alias, name) values ((select max(rowid) from alias), 'version');

--
-- Create view to ease usage management
--
drop view if exists stat_usage_per_app_v;
create view stat_usage_per_app_v as
  select 
      id_alias        as id_alias, 
      id_session      as id_session,
      count(id_alias) as count
  from 
      alias_usage
  group by 
      id_alias;