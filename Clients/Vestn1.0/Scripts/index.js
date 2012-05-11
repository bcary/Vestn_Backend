$(document).ready(function () {
    $("#moreTrendingButton").click(function () {
        expandTrendingItems();
    });

    $(".trendingHeader").click(function () {
        expandTrendingItems();
    });

});

function expandTrendingItems() {
    $(".trendingItem").css("display", "block");
    $("#moreTrendingButton").css("display", "none");
}