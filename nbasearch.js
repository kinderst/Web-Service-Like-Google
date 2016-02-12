/*
Scott Kinder
JQuery NBA Search page
*/

"use strict";

(function() {

	$(document).ready(function() {
		$("#searchbutton").on("click", function() {
			$.fn.searchPlayers();
		});
	});

	//searches for players, pastes the results into the proper space
	$.fn.searchPlayers = function() {
		//place load button
		$.fn.loadButton("#results");


		var playerName = $('#searchfield').val();

		//perform ajax request
		$.ajax({
			url: 'searchajax.php',
			type: 'post',
			data: { "playername": playerName},
			success: function(data)
			{
				$('#results').fadeOut(800, function() {
					$("#results").html(data);
					$("#results").fadeIn(800);
				});

			},
			error: function(error)
			{
				alert('something is wrong' + error);
			}
		});
		

	};

	//creates a load button in a specified place using the CSS selector
	$.fn.loadButton = function(place) {
		var loading = document.createElement("img");
		loading.id = "load";
		loading.src = "https://webster.cs.washington.edu/images/babynames/loading.gif";
		$(place).append(loading);
	};

	
})();