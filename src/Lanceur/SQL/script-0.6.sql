/*
 * Add into the database the default values into the Settings table
 */

 insert into settings (s_key, s_value) values ('IdSession', '1');
 -- AltGr + Space   : Key: 18 - Modifier: 3
 -- Win + Shift + R : Key: 61 - Modifier: 12
 insert into settings (s_key, s_value) values ('HotKey.Key', '18');
 insert into settings (s_key, s_value) values ('HotKey.ModifierKeys', '3');
 insert into settings (s_key, s_value) values ('Window.Position.Left', '600');
 insert into settings (s_key, s_value) values ('Window.Position.Top', '150');


/*
 * Put all the alias in lower case
 */
update alias_name set name = lower(name);

drop trigger if exists on_alias_name_update;
create trigger on_alias_name_update after update on alias_name
begin
	update alias_name set name = lower(new.name) where id = old.id;
end;

/*
 * In table settings, the s_key should be unique.
 */

create table settings_bak as select * from settings;
drop table settings;

create table settings (
    id    integer primary key,
    s_key   text unique,
    s_value text
);
insert into settings select * from settings_bak;
drop table settings_bak;