/*
Scott Kinder
JQuery PA2 Search
*/

"use strict";

(function () {

    $(document).ready(function () {
        $("#queryarea").keyup(function (e) {
            fetchResults();
            $.fn.fetchTable();
            $.fn.fetchPlayer();
        });
    });

    //Fetches the results and places them in the DOM
    function fetchResults() {
        var query = $("#queryarea").val();
        $.ajax({
            type: "POST",
            url: "WebService2.asmx/searchTrie",
            data: "{ 'search': '" + query + "'}",
            contentType: "application/json; charset=utf-8",
            success: function (msg) {
                $.each(msg, function (key, val) {
                    for (var i = 0; i < 10; i++) {
                        //if there is a search result
                        if (val[i]) {
                            $("#" + i).css("display", "initial");
                            $("#" + i).text(val[i]);
                        } else {
                            $("#" + i).css("display", "none");
                        }
                    }
                });
            },
            error: function (msg) {
                alert("fail");
            }
        });
    }

    //Fetches the results and places them in the DOM
    $.fn.fetchTable = function () {
        var query = $("#queryarea").val();
        $.ajax({
            type: "POST",
            url: "WebService2.asmx/searchTable",
            data: "{ 'search': '" + query + "'}",
            contentType: "application/json; charset=utf-8",
            success: function (msg) {
                $.each(msg, function (key, val) {
                    var i = 1;
                    $.each(val, function (key, val) {
                        $("#s" + i + " > a").text(val["Title"]);
                        $("#s" + i + " > a").attr("href", decodeURIComponent(val["Key"]));
                        $("#s" + i + " > h5").text(decodeURIComponent(val["Key"]));
                        $("#s" + i + " > h6").text(val["Date"]);
                        i++;
                    });
                });
            },
            error: function (msg) {
                alert("fail");
            }
        });
    };

    $.fn.fetchPlayer = function () {
        var query = $("#queryarea").val();
        $.ajax({
            url: 'http://52.11.96.255/PA1/searchajax.php',
            type: 'post',
            data: { "playername": query },
            success: function (data) {
                if (data.toLowerCase().indexOf(query.toLowerCase()) >= 0) {
                    $("#playerarea").html(data);
                } else {
                    $("#playerarea").empty();
                }
            },
            error: function (error) {
                alert('something is wrong' + error);
            }
        });
    };
})();