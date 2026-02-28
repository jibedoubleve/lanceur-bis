/* Remove invalid records from the alias_argument table.  
 * Each alias argument must be associated with a valid alias,  
 * and an ID of 0 is not considered valid.  
 */
delete
from alias_argument
where id_alias = 0;