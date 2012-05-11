$(document).ready(function () {
    setTimeout('$(".manage-profile-picture").resize({ maxHeight: 150, maxWidth: 150 })', 150);
});

$("#inviteFriendEmailInput input").keyup(function () {
    if (($(this).val() != "") && ($("#inviteFriendNameInput input").val() != "")) {
        //$(this).css("outline", "#EEA53D solid 1px;");
        $("#inviteFriendButton").removeClass("blue");
        $("#inviteFriendButton").addClass("orange");
    }
    else {
        //$(this).css("outline", "#EEA53D solid 1px;");
        $("#inviteFriendButton").removeClass("orange");
        $("#inviteFriendButton").addClass("blue");
    }
});

$("#inviteFriendNameInput input").keyup(function () {
    if (($(this).val() != "") && ($("#inviteFriendEmailInput input").val() != "")) {
        //$(this).css("outline", "#EEA53D solid 1px;");
        $("#inviteFriendButton").removeClass("blue");
        $("#inviteFriendButton").addClass("orange");
    }
    else {
        //$(this).css("outline", "#EEA53D solid 1px;");
        $("#inviteFriendButton").removeClass("orange");
        $("#inviteFriendButton").addClass("blue");
    }
});

$("#inviteFriendButton").click(function () {
    if (($("#inviteFriendEmailInput input").val() != "") && ($("#inviteFriendNameInput input").val() != "")) {

        //make ajax call to invite friend
        $.ajax({
            url: "/User/InviteFriend",
            dataType: "json",
            type: "POST",
            data: {
                friendName: $("#inviteFriendNameInput input").val(),
                friendEmail: $("#inviteFriendEmailInput input").val()
            },
            success: function (result) {
                if (result["Error"]) {
                    popUpUnknownError();
                    return;
                }
                switch (result["InviteFriendStatus"]) {
                    case "emailSent":
                        popUp("An invite has been sent to the requested email address.", ["OK"]);
                        clearInviteFields();
                        break;
                    case "emailAlreadyRegistered":
                        popUpError_OkOrSendReport("This email is already registered to an account.");
                        break;
                    case "emailNotSent":
                        popUpError_OkOrSendReport("We could not send an invite to the requested email address.");
                        break;
                    case "fail":
                        popUpError_OkOrSendReport("Sorry, but we could not send your invite.");
                        break;
                    default:
                        popUpUnknownError();
                        break;
                }
            },
            error: function (result, status, error) {
                popUpUnknownError();
            }
        });
    }
});

function clearInviteFields() {

    $("#inviteFriendEmailInput input").val("");
    $("#inviteFriendNameInput input").val("");

    $("#inviteFriendButton").removeClass("orange");
    $("#inviteFriendButton").addClass("blue");
}


/*Email Verification*//*Email Verification*//*Email Verification*//*Email Verification*/

$("#resendVerificationEmailButton").click(function () {
    if($(this).hasClass("disabled")){
        return;
    }
    //disable button until ajax call returns
    toggleResendVerificationEmailButton("off");

    //make ajax call to resend email
    $.ajax({
        url: "/User/SendVerificationEmail",
        dataType: "json",
        type: "POST",
        data: {},
        success: function (result) {
            switch (result["VerificationEmailStatus"]) {
                case "isVerified": popUpError_OkOrSendReport("Your email is already verified! You should be able to <b>Go Public</b>.<br /><br />If you still can't send us a report and we'll fix this"); break;
                case "emailSent": popUp("Email Verification Sent"); break;
                case "emailNotSent": popUpError_OkOrSendReport("Email Verification Failed to send."); break;
                default: popUpError_OkOrSendReport("Email Verification Failed to send."); break;
            }
            toggleResendVerificationEmailButton("on");
        },
        error: function (result, status, error) {
            popUpUnknownError();
            toggleResendVerificationEmailButton("on");
        }
    });
});

function toggleResendVerificationEmailButton(string) {
    switch (string) {
        case "on":
            $("#resendVerificationEmailButton").removeClass("disabled");
            $("#resendVerificationEmailButton").addClass("orange");
            $("#resendVerificationEmailButton").removeClass("black");
            $("#resendVerificationEmailButton").html("Resend Verification Email");
            break;
        case "off":
            $("#resendVerificationEmailButton").addClass("disabled");
            $("#resendVerificationEmailButton").addClass("black");
            $("#resendVerificationEmailButton").removeClass("orange");
            $("#resendVerificationEmailButton").html("sending...");
            break;
        default: break;
    }
}

/*/Email Verification/*/


/*Go Public*//*Go Public*//*Go Public*//*Go Public*//*Go Public*//*Go Public*/
$("#goPublicButton").click(function () {
    if (!$(this).hasClass("disabled")) {
        $.ajax({
            url: "/User/MakeProfilePublic",
            dataType: "json",
            type: "POST",
            data: {},
            success: function (result) {
                switch (result["MadePublicStatus"]) {
                    case "profileAlreadyPublic": popUpError_OkOrSendReport("Your account is already <b>Public</b> silly.<br /><br />If it isn't showing up give it a few minutes. If it still isn't showing up, send us a report."); break;
                    case "profileMadePublic":
                        popUp("Your account is now live for the world to see.<br /><br />(Start showing off)");
                        $("#goPublicButton").css("display", "none");
                        $("#hideProfileButton").fadeIn("slow", function () { });
                        $("#profileStatusHidden").css("display", "none");
                        $("#profileStatusPublic").fadeIn("slow", function () { });
                        break;
                    case "userEmailNotVerified": popUpError_OkOrSendReport("Could not make public because your email isn't verified. <br />Verify your email then make your account public."); break;
                    case "profileNotMadePublic": popUpError_OkOrSendReport("Could not make public for some reason. We're looking into it.."); break;
                    case "error": popUpError_OkOrSendReport("Server Error trying to make your profile public. We're working on it!"); break;
                    default: popUpUnknownError(); break;
                }
            },
            error: function (result, status, error) {
                popUpUnknownError();
            }
        });

    }
    else {
        popUpError_OkOrSendReport("You can't make your profile <b>Public</b> because your email isn't verified. <br />Try verifying your email, or if you have, refreshing the page. If this still doesn't work, send us a report!");
    }
});

$("#hideProfileButton").click(function () {
    $.ajax({
        url: "/User/HidePublicProfile",
        dataType: "json",
        type: "POST",
        data: {},
        success: function (result) {
            if (result["Error"]) {
                popUpUnknownError();
                return;
            }
            switch (result["HidePublicProfileStatus"]) {
                case "profileAlreadyHidden": popUpError_OkOrSendReport("Your account is already <b>Hidden</b> silly.<br /><br />If it isn't showing up as hidden give it a few minutes. If it still isn't showing up, send us a report.."); break;
                case "profileMadeHidden":
                    popUp("Your account is now hidden from the world.<br /><br />(stealth mode)");
                    $("#hideProfileButton").css("display", "none");
                    $("#goPublicButton").fadeIn("slow", function () { });
                    $("#profileStatusPublic").css("display", "none");
                    $("#profileStatusHidden").fadeIn("slow", function () { });
                    break;
                case "userEmailNotVerified": popUpError_OkOrSendReport("Your account is hidden because your email is not verified. <br />Verify your email to make your account public."); break;
                case "profileNotMadeHidden": popUpError_OkOrSendReport("Could not make public for some reason. We're looking into it.."); break;
                case "error": popUpUnknownError(); break;
                default: popUpUnknownError(); break;
            }
        },
        error: function (result, status, error) {
            popUpUnknownError();
        }
    });
});

/*/Go Public/*/


/*Copy link to clipboard*//*Copy link to clipboard*//*Copy link to clipboard*/

$("#copyLinkButton").click(function () {
    $("#copyLinkButton").css("display", "none");
    $('#copyLinkText').fadeIn("fast", function () {
        $("#copyLinkText input").val("vestn.com/v/" + $(".sessionVariables .session_profileURL").text());
        $("#copyLinkText input").select();
    });
});

$("#copyLinkText input").blur(function () {
    $("#copyLinkText").css("display", "none");
    $('#copyLinkButton').fadeIn("fast", function () {
    });
});
/*/Copy link to clipboard/*/

/*Feedback Submit*//*Feedback Submit*//*Feedback Submit*//*Feedback Submit*//*Feedback Submit*/

$("#submitFeedbackButton").click(function () {
    if (!(($("#submitFeedbackText").val() == "") || ($("#submitFeedbackText").val() == "Example: Great site! Could you please add company profiles?"))) {
        $.ajax({
            url: "/User/AddFeedback",
            dataType: "json",
            type: "POST",
            data: {
                message: $("#submitFeedbackText").val(),
                subject: "Feedback_HomePage"
            },
            success: function (result) {
                if (result["Error"]) {
                    popUpError_OkOrSendReport(result["Error"]);
                    return;
                }
                switch (result["FeedbackStatus"]) {
                    case "success": notify("Thanks for your help!", 1500); break;
                    default: popUpUnknownError(); break;
                }

                $("#submitFeedbackText").val("");

            },
            error: function (result, status, error) {
                popUpUnknownError();
            }
        });

    }
    else {
        notify("Type some feedback first.", 1500);
    }
});

$("#submitFeedbackText").blur(function () {
    if ($("#submitFeedbackText").val() == "") {
        $("#submitFeedbackText").val("Example: Great site! Could you please add company profiles?");
    }
    $("#submitFeedbackText").css("color", "#777");
});

$("#submitFeedbackText").focus(function () {
    if ($("#submitFeedbackText").val() == "Example: Great site! Could you please add company profiles?") {
        $("#submitFeedbackText").val("");
    }
    $("#submitFeedbackText").css("color", "#3C3C3C");
});

/*/Feedback Submit/*/
