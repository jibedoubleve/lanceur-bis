drop view if exists data_alias_synonyms_v;
create view data_alias_synonyms_v as
	select
    id_alias                 as id_alias, 
    group_concat(name, ', ') as synonyms
  from alias_name 
  group by id_alias;