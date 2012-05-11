
$(document).ready(function () {

    $("#talentCreateTab").click(function () {
        $("#individualCreateAccountForm").css("display", "block");
        $("#educationCreateAccountForm").css("display", "none");

        $("#educationCreateTab").css("opacity", ".75");
        $("#educationCreateTab img").removeClass("rotateBy180").addClass("rotateBy90");
        $("#talentCreateTab").css("opacity", "1");
        $("#talentCreateTab img").addClass("rotateBy180").removeClass("rotateBy90");

    });

    $("#educationCreateTab").click(function () {
        $("#educationCreateAccountForm").css("display", "block");
        $("#individualCreateAccountForm").css("display", "none");

        $("#talentCreateTab").css("opacity", ".75");
        $("#talentCreateTab img").removeClass("rotateBy180").addClass("rotateBy90");
        $("#educationCreateTab").css("opacity", "1");
        $("#educationCreateTab img").addClass("rotateBy180").removeClass("rotateBy90");

    });


});