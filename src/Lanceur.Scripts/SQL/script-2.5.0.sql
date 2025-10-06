/*
 * Add new indexes to optimise query execution
 */
    
-- for prefix searches (like 'abc%') on the name
create index if not exists ix_alias_name_name       on alias_name(name collate nocase);
create index if not exists ix_alias_name_id_name    on alias_name(id_alias);


-- if your queries also perform prefix searches on arguments:
create index if not exists ix_alias_arg_name on alias_argument(name collate nocase);

-- useful indexes for sorting/filtering (if used in your queries)
-- table ALIAS
create index if not exists ix_alias_visible   on alias(hidden);
create index if not exists ix_alias_usage     on alias(exec_count);

-- table ALIAS_ARGUMENTS
create index if not exists ix_alias_argument on alias_argument(id_alias);

-- table ALIAS_USAGE
create index if not exists ix_alias_lastused  on alias_usage(time_stamp);
