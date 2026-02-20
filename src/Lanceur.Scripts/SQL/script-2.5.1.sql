/*
 * A prior migration script used this approach on the alias table but failed
 * to drop the temporary table at the end. This script performs that cleanup.
 */

drop table if exists temp_alias;