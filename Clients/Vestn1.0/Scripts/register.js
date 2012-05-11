

$(document).ready(function () {
    $("#formUserNameInput").focus(function () {
        $("#formVestnProfileLabel").hide();
        $("#formVestnURL").show();
    });

    $("#formUserNameInput").blur(function () {
        if ($("#formUserNameInput").val() == "") {
            $("#formVestnProfileLabel").show();
            $("#formVestnURL").hide();
        }
    });

    $("#requestBetaAccessKeyButton").click(function () {
        if ($("#formEmailInput").val() != "") {
            pauseUI("Requesting Key...");
            //make ajax call to send key to the user (if available), if unavailable let user know they've been added to a waiting list
            $.ajax({
                url: "/User/RequestBetaKey",
                dataType: "json",
                type: "POST",
                data: {
                    requesterEmail: $("#formEmailInput").val()
                },
                success: function (result) {
                    resumeUI();
                    if (result["Error"]) {
                        popUpUnknownError();
                        return;
                    }
                    switch (result["RequestBetaKeyStatus"]) {
                        case "emailSent":
                            popUp("A beta key has been sent to the requested email address.", ["OK"]);
                            break;
                        case "emailAddedToQueue":
                            popUp("Sorry, but we are out of Beta Keys!<br /><br />Your email has been added to a waiting list and you will receive your Beta Key as we make them available.", ["OK"]);
                            break;
                        case "emailAlreadyRegistered":
                            popUpError_OkOrSendReport("This email is already registered to an account.<br /><br />Send us a report if this is a mistake. Include your email address in the body");
                            break;
                        case "emailNotSent":
                            popUpError_OkOrSendReport("We could not send an email to the requested email address.");
                            break;
                        case "fail":
                            popUpError_OkOrSendReport("Sorry, but we can not send you a beta key at this time");
                            break;
                        default:
                            popUpUnknownError();
                            break;
                    }
                },
                error: function (result, status, error) {
                    resumeUI();
                    popUpUnknownError();
                }
            });
        }
        else {
            popUp("Please enter a valid email address");
        }
    });
});

