/*
 * Roll back script 0.8 and add keywords as it is used.
 */

delete from alias_usage 
where
	id_alias in (
    select id from alias where file_name in ('add', '=', 'import' , 'quit', 'sessions', 'setup', 'version')
);

delete from alias where file_name in ('add', '=', 'import' , 'quit', 'sessions', 'setup', 'version');
delete from alias_name where name in ('add', '=', 'import' , 'quit', 'sessions', 'setup', 'version');