/*
 * Add a new column to setup a confirmation before
 * executing an alias
 */

alter table alias add confirmation_required integer default 0;