

//ensure that any click on a sh6 item gets passed through it. mostly an ie fix.
$(function () {
    $("sh6").click(function () {
        $(this).parent().trigger("click");
    });
    $("sh5").click(function () {
        $(this).parent().trigger("click");
    });
    $(".mediaTypeIndicator").click(function () {
        $(this).parent().trigger("click");
    });
});

//escape key should close out of any popUp
$(document).keyup(function (e) {
    if (e.keyCode == 27) {
        if ($("#popUp").css("display") == "block") {
            closePopUp(0);
        }

        if ($("#galleriaBox").css("visibility") == "visible") {
            hideAllGalleries();
        }
    } else if (e.keyCode == 37) { //left
        if ($("#galleriaBox").css("visibility") == "visible") {
            getCurrentGallery().prev();
        }
    } else if (e.keyCode == 39) { //right
        if ($("#galleriaBox").css("visibility") == "visible") {
            getCurrentGallery().next();
        }
    }
});

function pauseUI(message) {
    if (message != "") {
        notify(message, 0, true);
    }
    else {
        notify("Working...", 0, true);
    }
    $(".whiteHaze").css("display", "block");
}

function resumeUI() {
    closePopUp('instant');
    $('.whiteHaze').fadeOut('fast', function () { });
}

function updatePartial(id, htmlResult) {
    $(id).html(htmlResult);
    //loadProjectsPartial();
    //refreshViewState();
}

//In order to call this with a new button type, simply set the text of the button equal to a new case in popUpButtonPress(action) so that the button press can be registered.
function popUp(message, buttons, inputField, data) {
    $("#popUp .message").html(message);
    $("#popUp .buttons").html("");//reset html

    if (inputField == true) {
        $("#popUp textarea").css("display", "block");
    }
    else {
        $("#popUp textarea").css("display", "none");
    }

    // no buttons give, we have to close the popup after a timeout (or it will never be able to be closed)
    if ((buttons === undefined) || (buttons.length === undefined) || (buttons.length == 0)) {
        setTimeout("closePopUp('fast')", 1500);
    }
    else {
        $.each(buttons, function (index, value) {
            $("#popUp .buttons").append('<div id="popUpButton' + index + '" class="orange button">' + value + '</div>');

            $("#popUpButton" + index).click(function(){
                popUpButtonPress(value, data);
            });
        });
    }


    $("#popUp").css("display", "block");
    $(".whiteHaze").css("display", "block");
}

function popUpTextInput(message, buttons, textInputId, data) {
    $("#popUp .message").html(message);
    $("#popUp .buttons").html(""); //reset html

    if (inputField == true) {
        $("#popUp textarea").css("display", "block");
    }
    else {
        $("#popUp textarea").css("display", "none");
    }

    // no buttons give, we have to close the popup after a timeout (or it will never be able to be closed)
    if ((buttons === undefined) || (buttons.length === undefined) || (buttons.length == 0)) {
        setTimeout("closePopUp('fast')", 1500);
    }
    else {
        $.each(buttons, function (index, value) {
            $("#popUp .buttons").append('<div id="popUpButton' + index + '" class="orange button">' + value + '</div>');

            $("#popUpButton" + index).click(function () {
                popUpButtonPress(value, data);
            });
        });
    }


    $("#popUp").css("display", "block");
    $(".whiteHaze").css("display", "block");
}

function notify(message, ms, indicator) {
    $("#popUp .message").html(message);
    $("#popUp .buttons").html(""); //reset html
    $("#popUp textarea").css("display", "none");

    //TODO add indicator here
    if (indicator) {
        console.log("indicator");
    }

    //only close notify message if ms specified. otherwise, this is an indefinite notify msg
    if (ms > 0) {
        setTimeout("closePopUp('fast')", ms);
    }
    
    $("#popUp").css("display", "block");
}

function closePopUp(speed) {

    if (speed != "instant") {
        $('#popUp').fadeOut(speed, function () {
            $("#popUp messageText").val("");
            $("#popUp button").html("");
        });
    }
    else {
        $('#popUp').css("display", "none");
        $("#popUp messageText").val("");
        $("#popUp button").html("");
    }

    $('.whiteHaze').fadeOut('slow', function () { });
}

function sendErrorReport() {
    if ($("#popUp textarea").css("display") == "none") {
        $("#popUp textarea").css("display", "block");
        return;
    }
    else if ($("#popUp textarea").val() == ""){
        //no text in error report
        return;
    }
    else if ($("#popUp textarea").val() != "") {
        $("#popUp .textField").css("display", "none");
    
        //post the users input to Feedback table via User controller
        $.ajax({
            type: "POST",
            data: { "message": $("#popUp textarea").val(),
                "subject": "Error Report"
            },
            url: "/User/AddFeedback",
            dataType: "json"
        });
        
        setTimeout('notify("Report has been sent. Thank You!", 1500)', 500);
        closePopUp("fast");
        $("#popUp .textarea").css("display", "none");
    }
}

function share(data) {
    switch(data["linkType"]){
        case "profile": link = $("#profileURL").val();  break;
        case "project": link = $("#profileURL").val() + "#peid=null&iti=project-Information&proj=" + getActiveProject(); break;
        case "view": link= $("#profileURL").val() + "#peid=" + getActiveProjectElement() + "&iti=" + getActiveInnerTab() + "&proj=" + getActiveProject(); break;
    }

     $.ajax({
        type: "POST",
        data: { "link": link,
            "email" : data.email
        },
        url: "/User/Share",
        dataType: "json",
        success: function(data){
        if(data["Error"]){
            popUpError_OkOrSendReport(data["Error"]);
            return;
        }
            notify("Message Delivered.", 1500);
        },
        error: function(){
            notify("Message Delivered.", 1500);
        }
    });
}

function sendFeedback() {

    if ($("#popUp textarea").css("display") == "none") {
        return;
    }
    else if ($("#popUp textarea").val() == "") {
        //no text in feedback
        return;
    }
    else if ($("#popUp textarea").val() != "") {

        //post the users input to Feedback table and send email to us via User controller
        $.ajax({
            type: "POST",
            data: { "message": $("#popUp textarea").val(),
                "subject": "Feedback"
            },
            url: "/User/AddFeedback",
            dataType: "json"
        });

        setTimeout('notify("Thanks for the feedback", 1000)', 500);
        closePopUp("fast");

        $("#popUp textarea").val("");

    }

}

function addConnectionPopup() {
    $.ajax({
        type: "POST",
        data: { "userId": $("#profileId").val() },
        url: "/User/AddConnection",
        dataType: "json",
        success: function (result) {
            if (result["Error"]) {
                setTimeout('notify("' + result["Error"] + '", 1000)', 500);
            } else {
                setTimeout('notify("Connection Added!", 1000)', 500);
            }
            closePopUp("fast");
        },
        error: function (result) {
            setTimeout('notify("Error adding connection...", 1000)', 500);
            closePopUp("fast");
        }
    });

}

function addConnection() {
    $.ajax({
        type: "POST",
        data: { "userId": $("#profileId").val() },
        url: "/User/AddConnection",
        dataType: "json",
        success: function (result) {
            if (result["Error"]) {
                setTimeout('notify("' + result["Error"] + '", 1000)', 500);
            } else {
                setTimeout('notify("Connection Added!", 1000)', 500);
            }
            closePopUp("fast");
        },
        error: function (result) {
            setTimeout('notify("Error adding connection...", 1000)', 500);
            closePopUp("fast");
        }
    });

}

function sendHelpRequest() {

    if ($("#popUp textarea").css("display") == "none") {
        return;
    }
    else if ($("#popUp textarea").val() == "") {
        //no text in error report
        return;
    }
    else if ($("#popUp textarea").val() != "") {

        //post the users input to Feedback table and send email to us via User controller
        $.ajax({
            type: "POST",
            data: { "message": $("#popUp textarea").val(),
                "subject": "Help Request"
            },
            url: "/User/AddFeedback",
            dataType: "json"
        });

        setTimeout('notify("Help request has been sent. We will contact you through your registered email.", 1500)', 500);
        closePopUp("fast");

        $("#popUp textarea").val("");

    }

}

function confirmDeleteProject() {

    $.ajax({
        type: "POST",
        data: { 'projectId': getActiveProject() },
        url: "/Project/DeleteProject",
        dataType: "json",
        success: function (result) {
            if (result["Error"]) {
                updatePartial("#Partial_Projects", result["UpdatedPartial"]);
                popUpError_OkOrSendReport(result["Error"]);
                return;
            }



        },
        error: function (result, status, error) {
            updatePartial("#Partial_Projects", result["UpdatedPartial"]);
            popUpUnknownError();
            return;
        }
    });

    //assume delete is going to be successful
    $("#projectContent_" + getActiveProject()).remove();
    $("#projectTabs .menu #project_" + getActiveProject()).remove();
    goToProject(0); //go to About project
    //setTimeout('notify("project successfully deleted", 2500)', 1000);
}

function confirmDeleteProjectElement() {

    $.ajax({
        type: "POST",
        data: { 'projectId': getActiveProject(),
            'projectElementId': getActiveProjectElement()
        },
        url: "/Project/DeleteProjectElement",
        dataType: "json",
        success: function (result) {

            if (result["Error"]) {
                updatePartial("#Partial_Projects", result["UpdatedPartial"]);
                popUpError_OkOrSendReport(result["Error"]);
                return;
            }

        },
        error: function (result, status, error) {
            updatePartial("#Partial_Projects", result["UpdatedPartial"]);
            popUpUnknownError();
            return;
        }
    });

    //assume delete is going to be successful
    $("#projectElementSelector_" + getActiveProjectElement()).remove();
    $("#projectElementMedia_" + getActiveProjectElement()).remove();
}

function sendFeedbackToUser() {

    $.ajax({
        type: "POST",
        data: { "message": $("#popUp textarea").val(),
            "friendUsername": $("#profileUsername").val()
        },
        url: "/User/SendFeedbackToUser",
        dataType: "json",
        success: function (result) {

            if (result["FeedbackStatus"]) {
                notify("Your feedback has been sent!", 1500);
                $("#popUp textarea").val("");
                return;
            } else if (result["Error"]) {
                popUpError_OkOrSendReport(result["Error"]);
                return;
            }

        },
        error: function (result, status, error) {
            popUpError_OkOrSendReport(result["Error"]);
            return;
        }
    });
}

//template for basic OK or Send Report button, with input textarea for feedback, popUp
function popUpError_OkOrSendReport(message) {
    popUp(message, ["OK", "Send Report"], true);
}

function popUpUnknownError() {
    popUp("Something broke that we don't know about.<br /><br />Please send us a report detailing what happened.", ["OK", "Send Report"], true);
}

//In order to call this with a new button type, simply set the text of the button equal to a new case in popUpButtonPress(action) so that the button press can be registered.
function popUpButtonPress(action, data) {
    var returnAction;
    switch (action) {
        case "OK": closePopUp("slow"); break;
        case "Yes": closePopUp("fast"); displayTutorialVideo(); break;
        case "No": closePopUp("fast"); break;
        case "Cancel": closePopUp("fast"); break;
        case "Delete Project": closePopUp("instant"); confirmDeleteProject(); break;
        case "Delete Element": closePopUp("instant"); confirmDeleteProjectElement(); break;
        case "Send Report":  sendErrorReport(); break;
        case "Something is not right here": closePopUp("fast"); sendErrorReport(); break;
        case "Yes! Show me help tips": closePopUp("fast"); $(".helpElement").fadeIn(); $("#resetHelpSwitch").fadeOut(); $("#resetHelpSwitch").removeClass("active"); break;
        case "Never mind": closePopUp("fast"); break;
        case "Send Request": sendHelpRequest(); break;
        case "Submit Feedback": sendFeedback(); break;
        case "Share Profile": share(data); closePopUp("instant"); break;
        case "Share Project": share(data); closePopUp("instant"); break;
        case "Share View": share(data); closePopUp("instant"); break;
        case "Yes! Change my URL": changeURL(); closePopUp("instant"); break;
        case "Send Feedback": sendFeedbackToUser(); closePopUp("instant"); break;
        default: closePopUp("slow"); break;
    }
}

//This pop up the tutorial video

//comment out since we dont have video ready yet
// function   displayTutorialVideo(){
//     $("#completeProfileSubmitButton").YouTubePopup({ 'youtubeId': 'CxRMFwPpkBE', 'title': 'Vestn Tutorial', 'color': 'white' }).click();
// }

$(document).ready(function () {

    var ievs = (/MSIE (\d+\.\d+);/.test(navigator.userAgent));

    if (ievs) {
        var iev = new Number(RegExp.$1);
        if (iev <= 9) {
            popUpError_OkOrSendReport("Internet Explorer is NOT supported at this time<br /><br />Please use Chrome, Firefox, or Safari when accessing this site.");
        }
    }

    //pop up feedback
    $("#sideFeedbackButton").click(function () {
        popUp("Let us know what we could do better.", ["Submit Feedback", "Cancel"], true);
    });

    //pop up get help 
    $("#sideHelpButton").click(function () {
        popUp("Having trouble? Let us know what's up.<br /><br />Responses will be sent to your registered email address", ["Send Request", "Cancel"], true);
    });

    $("#forgotPasswordButton").click(function () {
        if ($("#emailForgotPassword").val() != "") {

            //make ajax call to invite friend
            $.ajax({
                url: "/User/ForgotPassword",
                dataType: "json",
                type: "POST",
                data: {
                    email: $("#emailForgotPassword").val()
                },
                success: function (result) {
                    if (result["Error"]) {
                        popUpUnknownError();
                        return;
                    }
                    switch (result["ForgotPasswordStatus"]) {
                        case "emailSent":
                            popUp("Instructions to recover your password have been sent to your email", ["OK"]);
                            $("#forgotPasswordBox").fadeOut();
                            break;
                        case "emailNotSent":
                            popUp("We couldn't find that email in our database. You should sign up!", ["OK"]);
                            $("#forgotPasswordBox").fadeOut();
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


    // $(".youtube").YouTubePopup({idAttribute: 'id','title':'Vestn Tutorial', 'color': 'white'});

    

});
