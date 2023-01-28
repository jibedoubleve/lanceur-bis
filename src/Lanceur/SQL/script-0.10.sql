/*
 * Add into the database the default values into the Settings table
 */

 insert into settings (s_key, s_value)
select
	'HotKey.ModifierKey',
  s_value
from settings  
where s_key = 'HotKey.ModifierKeys';

delete from settings where s_key = 'HotKey.ModifierKeys';
