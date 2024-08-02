/*
 * Allow to have lua script to manipulate the command
 * the user entered for automation purpose
 */
alter table alias
    add lua_script text;