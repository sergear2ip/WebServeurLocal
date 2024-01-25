<?php
	//header('Content-Type: text/html; charset=UTF-8');
	echo "<!DOCTYPE html>";
echo "<html><head>";
echo "Le php est bien interprété";
$matable ="testos";
$db = new SQLite3('madb_sqlite.db');
$query = "CREATE TABLE $matable(
            ID bigint(20) NOT NULL PRIMARY KEY,
            auteur bigint(20) NOT NULL,            
            date datetime,
            content longtext,
            titre text,
            guid VARCHAR(255)            
            )";
$results = $db->exec($query);
//echo $db;
phpinfo();
echo "</head></html>";
?>
