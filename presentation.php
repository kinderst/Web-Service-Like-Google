<?php
//Scott Kinder, Presentation Page for NBA Search PA#1
//Defines the common HTML data for pages for potential scalability by
//defining HTTP Header, and the Search Body

//writes the header for the page
function write_header() {
	?>
	<!DOCTYPE html>
	<html>
		<head>
			<meta charset="utf-8" />
			<title>NBA Player Search</title>
			<link href="nbasearch.css" type="text/css" rel="stylesheet" />
			<link href="http://www.logodesignlove.com/images/classic/nba-logo.jpg" 
				type="image/ico" rel="shortcut icon" />
			<script src="http://ajax.googleapis.com/ajax/libs/jquery/1.11.1/jquery.min.js"></script>
			<script src="nbasearch.js" type="text/javascript"></script>
		</head>
	<?php
}

//writes the body for the page
function write_body() {
	?>
	<body>
		<div id="contentbox">
			<img src='https://herleagueonlineteam.files.wordpress.com/2014/09/nba-logo-png.png'
					alt='NBA logo' />
			<div id="searcharea">
				<h2>Search NBA Players</h2>
				<input id="searchfield" type="text" value="Search..." 
						onfocus="if (this.value == 'Search...') {this.value = '';}" 
						onblur="if (this.value == '') {this.value = 'Search...';}">
				<input id="searchbutton" type="button" value="Go">
			</div>
		</div>
		<div id="results">
		</div>
	</body>
</html>
	<?php
}