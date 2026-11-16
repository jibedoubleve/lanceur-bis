-- #1525: some usages were saved with id_alias = 0 (no matching alias).
-- Attach them to a hidden placeholder alias so the history is kept.
-- Does nothing on a database without orphan usages.
insert into alias (notes, hidden)
select 'Holds usage history whose original alias was lost (see #1525). Kept for statistics only.', true
where exists (select 1 from alias_usage where id_alias = 0);

-- Keep the new id: the next insert would overwrite last_insert_rowid().
-- Stays empty when no alias was inserted above (last_insert_rowid() would then be stale).
create temp table new_alias as select last_insert_rowid() as id where changes() = 1;

update alias_usage
set id_alias = (select id from new_alias)
where id_alias = 0;

insert into alias_name (id_alias, name)
select id, 'orphan_usage' from new_alias;

drop table new_alias;