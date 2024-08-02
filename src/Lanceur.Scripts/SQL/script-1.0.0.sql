/*
 * Remove session feature:
 * 
 * 1. Alter tables to drop references to `alias_session`.
 * 2. Drop the `alias_session` table.
 * 3. Update views to remove or replace `alias_session` references.
 */

----------------------------------------
---- DROP THE VIEWS                 ----
----------------------------------------

drop view if exists stat_usage_per_app_v;
drop view if exists stat_usage_per_day_v;
drop view if exists stat_usage_per_month_v;
drop view if exists stat_usage_per_day_of_week_v;
drop view if exists stat_usage_per_hour_in_day_v;
drop view if exists data_not_used_v;
drop view if exists stat_execution_count_v;
drop view if exists stat_history_v;
drop view if exists data_doubloons_v;

----------------------------------------
---- TABLE ALIAS_USAGE              ----
----------------------------------------

/* Remove all usage of session other than main one*/
delete
from alias_usage
where id_session > (select min(id) from alias_session);

drop table if exists alias_usage_temp;
create table alias_usage_temp
(
    id         integer primary key,
    id_alias   integer,
    time_stamp timestamp default current_timestamp,
    foreign key (id_alias) references alias
);

insert into alias_usage_temp(id, id_alias, time_stamp)
select id, id_alias, time_stamp
from alias_usage;

drop table alias_usage;
alter table alias_usage_temp
    rename to alias_usage;

----------------------------------------
---- TABLE ALIAS                    ----
----------------------------------------
/* Remove all usage of session other than main one*/
delete
from alias
where id_session > (select min(id) from alias_session);

drop table if exists alias_temp;
create table alias_temp
(
    id                    integer primary key,
    arguments             text,
    file_name             text,
    notes                 text,
    run_as                text,
    start_mode            text,
    working_dir           text,
    icon                  text,
    hidden                integer not null default 0 check (hidden in (0, 1)),
    lua_script            text,
    thumbnail             text,
    exec_count            number,
    confirmation_required integer          default 0
);

insert into alias_temp(id, arguments, file_name, notes, run_as, start_mode, working_dir, icon, hidden, lua_script,
                       thumbnail, exec_count, confirmation_required)
select id,
       arguments,
       file_name,
       notes,
       run_as,
       start_mode,
       working_dir,
       icon,
       hidden,
       lua_script,
       thumbnail,
       exec_count,
       confirmation_required
from alias;

drop table alias;
alter table alias_temp
    rename to alias;
----------------------------------------
---- TABLE ALIAS_SESSION            ----
----------------------------------------

drop table alias_session;

----------------------------------------
---- RECREATE THE VIEWS             ----
----------------------------------------

create view stat_usage_per_app_v as
select id_alias        as id_alias,
       count(id_alias) as count
from alias_usage
group by id_alias;

create view stat_usage_per_day_v as
select count(*)                         as exec_count,
       strftime('%Y-%m-%d', time_stamp) as day
from alias_usage
group by strftime('%Y-%m-%d', time_stamp)
order by time_stamp;

create view stat_usage_per_month_v as
select count(*)                         as exec_count,
       strftime('%Y-%m-01', time_stamp) as month
from alias_usage
where strftime('%Y-%m-%d', time_stamp) < strftime('%Y-%m-01', date())
group by strftime('%Y-%m-01', time_stamp)
order by time_stamp;

create view stat_usage_per_day_of_week_v as
select sum(exec_count) as exec_count,
       day_of_week     as day_of_week,
       day_name        as day_name
from (select *
      from (select count(*) as exec_count,
                   case cast(strftime('%w', time_stamp) as integer)
                       when 0 then 7
                       else cast(strftime('%w', time_stamp) as integer)
                       end  as day_of_week,
                   case cast(strftime('%w', time_stamp) as integer)
                       when 0 then 'Sunday'
                       when 1 then 'Monday'
                       when 2 then 'Tuesday'
                       when 3 then 'Wednesday'
                       when 4 then 'Thursday'
                       when 5 then 'Friday'
                       when 6 then 'Saturday'
                       else 'error'
                       end  as day_name
            from alias_usage
            group by strftime('%w', time_stamp))
      union all
      select w.exec_count  as exec_count,
             w.day_of_week as day_of_week,
             w.day_name    as day_name
      from helper_day_in_week w)
group by day_of_week
order by day_of_week;

create view stat_usage_per_hour_in_day_v as
select sum(exec_count) as exec_count,
       hour_in_day     as hour_in_day
from (select *
      from (select count(*)                      as exec_count,
                   strftime('%H:00', time_stamp) as hour_in_day
            from alias_usage
            group by strftime('%H:00', time_stamp))
      union all
      select h.exec_count  as exec_count,
             h.hour_in_day as hour_in_day
      from helper_hour_in_day h)
group by hour_in_day
order by hour_in_day;

create view data_not_used_v as
select a.id,
       group_concat(an.name, ', ') as keywords,
       a.file_name,
       a.arguments,
       a.run_as,
       a.working_dir
from alias a
         inner join alias_name an on a.id = an.id_alias
         left join stat_execution_count_v s on a.id = s.id_keyword
where s.exec_count is null
   or s.exec_count = 0
group by a.id;

create view stat_execution_count_v as
select id_keyword as id_keyword,
       count(*)   as exec_count,
       keywords   as keywords
from stat_history_v sh
group by id_keyword
order by exec_count desc;

create view stat_history_v as
select s.id                        as id_keyword,
       group_concat(sn.name, ', ') as keywords,
       su.time_stamp               as time_stamp
from alias_usage su
         inner join alias s on su.id_alias = s.id
         inner join alias_name sn on s.id = sn.id_alias
group by su.time_stamp;

create view data_doubloons_v as
select d.*,
       an.name
from (select a.*
      from alias a
               inner join (select file_name   as file_name,
                                  arguments   as arguments,
                                  run_as      as run_as,
                                  working_dir as working_dir
                           from alias
                           group by file_name,
                                    arguments,
                                    run_as,
                                    working_dir
                           having count(*) > 1) d on
          a.file_name = d.file_name
              and a.run_as = d.run_as
              and ((a.working_dir = d.working_dir) is null or a.working_dir = d.working_dir)
              and ((a.arguments = d.arguments) is null or a.arguments = d.arguments)
      order by a.file_name) d
         inner join alias_name an on d.id = an.id_alias
group by d.id;