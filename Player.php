<?php
//Scott Kinder, NBA Search, Player
//This is a player class that has a player name and their respective stats
//player searching and reporting methods
class Player {

	private $name;
	private $stats;

	public function __construct($name, $stats) {
		$this->name = $name;
		$this->stats = $stats;
	}

	//returns name of player
	public function getName() {
		return $this->name;
	}

	//returns player stats
	public function getStats() {
		return $this->stats;
	}

	//
	public function writePlayers($allPlayers, $query) {
		if ($allPlayers) {
			foreach ($allPlayers as $player) {
				$playerStats = $player->getStats();

				//Get the proper url for their picture
				$playerImgName = $this->getImgUrl($player->getName());
				?>
				<div class="playerpanel">
					<img src=<?php echo($playerImgName); ?> alt="Player" />
					<div class="playerinfo">
						<table class="playerstatstable">
							<tbody>
								<tr class="caption">
									<th colspan="5"><?php echo($player->getName()); ?></th>
								</tr>
								<tr>
									<th>PPG</th>
									<th>GP</th>
									<th>FGP</th>
									<th>TPP</th>
									<th>FTP</th
								</tr>
								<tr>
									<td class="PPG"> <?php echo($playerStats["PPG"]) ?> </td>
									<td class="GP"> <?php echo($playerStats["GP"]) ?> </td>
									<td class="FGP"> <?php echo($playerStats["FGP"]) ?> </td>
									<td class="TPP"> <?php echo($playerStats["TPP"]) ?> </td>
									<td class="FTP"> <?php echo($playerStats["FTP"]) ?> </td> 
								</tr>
							</tbody>
						</table>
					</div>
				</div>
				<?php
			}
		} else {
			?>
			<p>Sorry, could not find any results for '<?php echo($query); ?>'</p>
			<?php
		}
	}

	//This function searches for all players like the name in the database with the given name
	//Returns an array of players
	public static function searchPlayers($player) {
		//create db connection
		$db_host = "info344user.cveib7cnjosi.us-west-2.rds.amazonaws.com:3306";
		$db_username = "info344user";
		$db_pass = "Sonny123!";
		$db_name = "info344";
		$db = new PDO("mysql:host=$db_host;dbname=$db_name", "$db_username", "$db_pass");


		$searchSql = "SELECT PlayerName, GP, FGP, TPP, FTP, PPG
						FROM nbastats
						WHERE PlayerName LIKE :playerName
						ORDER BY PPG DESC";
		$stmt = $db->prepare($searchSql);
		$playerSql = "%" . $player->getName() . "%";
		$stmt->bindParam(':playerName', $playerSql);
		$stmt->execute();
		$results = $stmt->fetchAll();

		//close db conn
		$db = null;

		foreach ($results as $playerInfo) {
			$stats["GP"] = $playerInfo["GP"];
			$stats["FGP"] = $playerInfo["FGP"];
			$stats["TPP"] = $playerInfo["TPP"];
			$stats["FTP"] = $playerInfo["FTP"];
			$stats["PPG"] = $playerInfo["PPG"];

			$allPlayers[] = new Player($playerInfo["PlayerName"], $stats);
		}
		
		return $allPlayers;
	}


	//Helper method to get the url for their image on stats.nba.com/players
	//Takes in a name, and returns the exact http url for the image
	private static function getImgUrl($name) {
		//checks if names in db are different than they are in url
		if ($name === 'Nene Hilario') {
			$name = 'Nene';
		} else if ($name === 'John Lucas') {
			$name = 'John III Lucas';
		} else if ($name === 'Wes Johnson') {
			$name = 'Wesley Johnson';
		} else if ($name === 'Luc Richard Mbah a Moute') {
			$name = 'Luc Mbah a Moute';
		} else if ($name === 'J.J. Barea') {
			$name = 'Jose Barea';
		}
		//replace funky parts in names like single quotes, periods; explode name
		$properName = str_replace('.', '', $name);
		$properName = str_replace('\'', '', $properName);
		$playerImgNames = explode(' ', strtolower($properName));

		//test size to see how many underscores and names to report
		if ($playerImgNames[3]) {
			$playerImgName = "http://i.cdn.turner.com/nba/nba/.element/img/2.0/sect/statscube/players/large/"
					. $playerImgNames[0] . "_" . $playerImgNames[1] . "_" . $playerImgNames[2] . "_"
					. $playerImgNames[3] . ".png";
		} else if ($playerImgNames[2]) {
			$playerImgName = "http://i.cdn.turner.com/nba/nba/.element/img/2.0/sect/statscube/players/large/"
					. $playerImgNames[0] . "_" . $playerImgNames[1] . "_" . $playerImgNames[2] . ".png";
		} else if ($playerImgNames[1]) {
			$playerImgName = "http://i.cdn.turner.com/nba/nba/.element/img/2.0/sect/statscube/players/large/"
					. $playerImgNames[0] . "_" . $playerImgNames[1] . ".png";
		} else {
			$playerImgName = "http://i.cdn.turner.com/nba/nba/.element/img/2.0/sect/statscube/players/large/"
					. $playerImgNames[0] . ".png";
		}

		return $playerImgName;
	}
}