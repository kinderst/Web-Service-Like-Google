/*
Scott Kinder
JQuery PA2 Search
*/

"use strict";

(function () {

    $(document).ready(function () {
        $.fn.getAjax("getCrawlerState", "thestate");

        $.fn.getAjax("getRamAvail", "ram");
        $.fn.getAjax("getCpuUtil", "cpu");

        $.fn.getAjax("totalUrlsCrawled", "totalurls");

        $.fn.getLastTenUrls();

        $.fn.getAjax("getQueueSize", "queuesize");
        $.fn.getAjax("getIndexSize", "indexsize");

        $.fn.getErrors();

        $("#cleartable").on("click", function () {
            $.fn.getAjax("clearTable", "response");
        });

        $("#clearurlqueue").on("click", function () {
            $.fn.getAjax("clearUrlQueue", "response");
        });

        $("#clearadminqueue").on("click", function () {
            $.fn.getAjax("clearAdminQueue", "response");
        });

        $("#stopworkers").on("click", function () {
            $.fn.getAjax("stopAllWorkers", "response");
        });

        $("#start").on("click", function () {
            $.fn.getAjax("startCrawler", "response");
        });

        $("#startwithurl").on("click", function () {
            $.fn.startCrawlerWithUrl();
        });

        $("#retrieve").on("click", function () {
            $.fn.retrievePage();
        });


    });

    $.fn.startCrawlerWithUrl = function () {
        var query = $("#starturl").val();
        $.ajax({
            type: "POST",
            url: "WebService1.asmx/startCrawlerWithUrl",
            data: "{ 'rootUrl': '" + query + "'}",
            contentType: "application/json; charset=utf-8",
            success: function (msg) {
                $.each(msg, function (key, val) {
                    $("#startresponse").html(val);
                });
            },
            error: function (msg) {
                alert("fail");
            }
        });
    };

    $.fn.getAjax = function (url, cssSelector) {
        $.ajax({
            type: "POST",
            url: "WebService1.asmx/" + url,
            contentType: "application/json; charset=utf-8",
            success: function (msg) {
                $.each(msg, function (key, val) {
                    $("#" + cssSelector).html(val);
                });
            },
            error: function (msg) {
                alert("fail");
            }
        });
    };

    $.fn.getLastTenUrls = function () {
        $.ajax({
            type: "POST",
            url: "WebService1.asmx/lastTenUrls",
            contentType: "application/json; charset=utf-8",
            success: function (msg) {
                $.each(msg, function (key, val) {
                    var counter = 1;
                    var arr = val.toString().split(/,/);
                    $.each(arr, function (key, val) {
                        $("#" + counter).html(val);
                        counter++;
                    });
                });
            },
            error: function (msg) {
                alert("fail");
            }
        });
    };

    $.fn.getErrors = function () {
        $.ajax({
            type: "POST",
            url: "WebService1.asmx/getErrors",
            contentType: "application/json; charset=utf-8",
            success: function (msg) {
                $.each(msg, function (key, val) {
                    var theString = "";
                    var arr = val.toString().split(/,/);
                    $.each(arr, function (key, val) {
                        theString += val + "<br />";
                    });
                    $("#errors").html(theString);
                });
            },
            error: function (msg) {
                alert("fail");
            }
        });
    };



    $.fn.retrievePage = function () {
        var query = $("#retrieveurl").val();
        $.ajax({
            type: "POST",
            url: "WebService1.asmx/retrieveIndex",
            data: "{ 'url': '" + query + "'}",
            contentType: "application/json; charset=utf-8",
            success: function (msg) {
                $.each(msg, function (key, val) {
                    $("#retrieveajax").html(val);
                });
            },
            error: function (msg) {
                alert("fail");
            }
        });
    };
})();