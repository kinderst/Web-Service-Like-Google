<?php
//Scott Kinder, PHP Ajax to page for NBA Search PA #1
//Exists as a place for a ajax request to land and be handled
//Also defines a function to handle potentially malicious input

require_once("Player.php");

if (isset($_POST["playername"])) {
	$playerName = test_input($_POST["playername"]);

	//$results = search_player($playerName, $db);

	$player = new Player($playerName, null);
	//var_dump($player);

	//get an array of all players
	$allPlayers = $player->searchPlayers($player);
	//$player->searchPlayers($player);

	//writes out the HTML code for ajax response
	$player->writePlayers($allPlayers, $playerName);
	//foreach ($player->GetAllPlayers() as $)


	//write_players($results, $playerName);
}

//Corrects data by taking care of common malicious input
//Returns a sanitized result;
function test_input($data) {
	$oldData = $data;
	$data = trim($data);
	$data = stripslashes($data);
	$data = htmlspecialchars($data);
	return $data;
}