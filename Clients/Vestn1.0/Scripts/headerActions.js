$("#headerActionsForm").click(function () {
    displayLoginFrameOverlay();
});

function displayLoginFrameOverlay() {
    $("#loginFramePlaceholder").css("display", "none");

    $("#loginFrameOverlay").css("display", "block");
    $("#loginFrameOverlay").animate({
        opacity: 1
    }, 200);
}

$("#labelForRememberMeCheckbox").click(function () {
    $("#rememberMeCheckbox").click();
});

$("#headerDropDown").hover(function () {
    $(".dropdown dd ul").toggle();
});

$(document).ready(function () {

    $("#headerActionsForm").keypress(function (e) {
        if ((e.which && e.which == 13) || (e.keyCode && e.keyCode == 13)) {
            $('#btnLogin').click();
            return false;
        } else {
            return true;
        }
    });

    $("#btnLogin").click(function () {
        if (($("#emailInput").val() != "") && ($("#passwordInput").val() != "")) {

            pauseUI("Logging In...");

            //make ajax call to submit the log on
            $.ajax({
                url: "/User/LogOnOld",
                dataType: "json",
                type: "POST",
                data: {
                    username: $("#emailInput").val(),
                    password: $("#passwordInput").val(),
                    rememberme: $("#rememberMeCheckbox").is(':checked')
                },
                success: function (data) {
                    if (data["LogOnResult"] == "Success") {
                        //resumeUI();
                        //have to change this back to window.location.origin because otherwise we can't log in from the register page or another user's page. i realize that this breaks IE though..
                        window.location = window.location.protocol + "//" + window.location.host + "/User";
                    }
                    else {
                        resumeUI();
                        popUpError_OkOrSendReport(data["Error"]);
                    }
                },
                error: function (result, status, error) {
                    resumeUI();
                    popUpUnknownError();
                }
            });
        }
        else {
            notify("Missing user name or password", 1000);
        }
    });
});