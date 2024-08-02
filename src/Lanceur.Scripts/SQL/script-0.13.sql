/*
 * This table contains additional arguments. It's a way to avoid cloning an 
 * alias just to have a sightly difference in the arguments
 */
create table alias_argument
(
    id       integer primary key,
    id_alias integer not null,
    name     text    not null,
    argument text    not null,
    foreign key (id_alias) references alias (id)
);