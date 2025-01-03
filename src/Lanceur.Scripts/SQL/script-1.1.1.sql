/*
 * Remove obsolete configuration rows from the 'settings' table.
 * These rows are no longer needed as the corresponding data is now stored in the 'json' row.
 */
delete from settings
where s_key in (
    'IdSession',
    'HotKey.Key',
    'Window.Position.Left',
    'Window.Position.Top',
    'RestartDelay',
    'HotKey.ModifierKey'
);