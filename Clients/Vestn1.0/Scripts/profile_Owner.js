/// <reference path="jeditable.js" />


function addProfilePicture(result) {
    $("#userProfilePicture img").attr("width", "175");
    $("#userProfilePicture img").attr("height", "175");
    $("#userProfilePicture img").attr("src", result["PictureLocation"]);
    
    $("#helpElementPicture").fadeOut();
}

function addResume(result) {
    updatePartial("#Partial_Resume", result["UpdatedPartial"]);
    
    //navigate to new resume
    goToProject($("#projectTabs ul li.aboutProjectTab").val())

    //set up movement buttons on tab bar - this MUST be called after we set the new active project
    setUpProjectMoveButtons(0);
    
    selectInnerTabIndex("user-Resume")
}

function addProjectElement(result) {
    updatePartial("#Partial_Projects", result["UpdatedPartial"]);
}

function updateProjectPicture(result) {
    $("#projectPictureUpload_" + result["ProjectId"]).parent().children(".aboutPicture").children("img").attr("src", result["PictureLocation"])
}

function changeURL() {
    $("#changeURLTextField").hide();
    $("#saveURLButton").hide();
    $("#changeURLButton").show();
    $("#profileURLText").hide();

    userID = $("#editProfileUserId").val();
    value = $("#changeURLTextField").val();

    $.ajax({
        type: "POST",
        data: { "userID": userID,
            "id": "profileURL",
            "value": value
        },
        url: "/User/EditProfile",
        dataType: "json",
        success: function (data) {
            if (data["Error"]) {
                $("#profileURLText").show();
                popUpError_OkOrSendReport(data["Error"]);
                return;
            }
            notify("URL Changed.", 1500);
            $("#profileURLText").text(data["Value"]);
            $("#profileURLText").show();
        },
        error: function () {
            notify("Unknown Error Occurred.", 1500);
        }
    });
}

function addUserTag() {

    $.ajax({
        type: "POST",
        data: { "value": $("#tags").val() },
        url: "/User/AddUserTag",
        dataType: "json",
        success: function (data) {
            if (data["Error"]) {
                popUpError_OkOrSendReport(data["Error"]);
                return;
            }
            
            notify(data["tagStatus"], 1500);
            $("#tags").val("")
        },
        error: function () {
            notify("Unknown Error Occurred.", 1500);
        }
    });
}

function addProjectTag() {

    $.ajax({
        type: "POST",
        data: { "value": $("#tags").val(), "projectId": getActiveProject() },
        url: "/Project/AddProjectTag",
        dataType: "json",
        success: function (data) {
            if (data["Error"]) {
                popUpError_OkOrSendReport(data["Error"]);
                return;
            }

            notify(data["tagStatus"], 1500);
            $("#tags").val("")
        },
        error: function () {
            notify("Unknown Error Occurred.", 1500);
        }
    });
}

// autocomplete tags
$(function () {

    $.ajax({
        type: "GET",
        data: { "limit": 10
        },
        url: "/Resources/AutocompleteTags",
        dataType: "json",
        success: function (result) {
            $("#tags").autocomplete({
                source: result
            });
        },
        error: function (result, status, error) {
            popUpError_OkOrSendReport(result["Error"]);
            return;
        }
    });
});

$(function () {
    var availableCities = [
			"Lincoln, Nebraska",
			"Omaha, Nebraska",
			"Bellevue, Nebraska",
			"Grand Island, Nebraska",
			"Kearney, Nebraska",
		];
        $("#locationCompleteProfile").autocomplete({
            source: availableCities
            });
});

$(function () {
    var availableUniversities = [
		"University of Nebraska - Lincoln",
        "University of Nebraska at Kearney",
        "University of Nebraska at Omaha",
        "Bellevue University",
        "Southeast Community College",
		"Creighton University",
		"hastings College",
		"Nebraska Wesleyan University",
		"Nebraska Methodist College",
	];
    $("#schoolCompleteProfile").autocomplete({
        source: availableUniversities
    });
});

$(document).ready(function () {

    $("#iframe-post-form").load(function () {

        var result = $.parseJSON($("#iframe-post-form").contents().find("body").text());

        if (result["Error"]) {
            popUpError_OkOrSendReport("Error<br /><br />(" + result["Error"] + ")");
            return;
        }

        switch ($("#iframe-post-form").val()) {
            case "addProfilePicture": addProfilePicture(result); break;
            case "addResume": addResume(result); break;
            case "addProjectElement":
                addProjectElement(result);
                goToProjectElement(result["ProjectElementId"]);
                break;
            case "updateProjectPicture": updateProjectPicture(result); break;
            default: break;
        }



        resumeUI();

    });

    $(".userConnectionsImage").resize({ maxHeight: 50, maxWidth: 50 });

    $("#viewResume").hover(function () {
        $("#viewResumeClickToChangeLabel").css("display", "block");
    },
    function () {
        $("#viewResumeClickToChangeLabel").css("display", "none");
    });

    $("#userProfilePicture").click(function () {
        $("#userProfilePictureUploadInput").click();
    });

    $("#userProfilePicture").hover(function () {
        $(".editProfilePictureLabel").html("Click to change");
        $(".editProfilePictureLabel").addClass("hovered");
    },
    function () {
        $(".editProfilePictureLabel").html("CHANGE");
        $(".editProfilePictureLabel").removeClass("hovered");
    });



    ///User/EditProfile****************************************************
    $(".inLineEdit").editable("/User/EditProfile", {
        tooltip: 'Click to edit',
        select: true,
        submitdata: {
            userId: $("#editProfileUserId").val()
        },
        /*submit: 'save',
        cancel: 'cancel',*/
        placeholder: 'click here',
        indicator: "Saving...",
        onblur: 'submit',
        intercept: function (jsondata) {
            var jsonObject = jQuery.parseJSON(jsondata);
            console.log(jsonObject);
            if (jsonObject["Status"] == 1) {
                if (jsonObject["Id"] == "firstName") {
                    $("#userAboutHeader span").html(jsonObject["Value"] + " " + $("#lastName").html());
                    $("#userAboutInformationPartial .aboutPictureWrapper .name").html(jsonObject["Value"] + " " + $("#lastName").html());
                }
                else if (jsonObject["Id"] == "lastName") {
                    $("#userAboutHeader span").html($("#firstName").html() + " " + jsonObject["Value"]);
                    $("#userAboutInformationPartial .aboutPictureWrapper .name").html($("#firstName").html() + " " + jsonObject["Value"]);
                }
                return jsonObject["Value"];
            }
            else {
                if (jsonObject["Id"] == null) {
                    popUpError_OkOrSendReport("Update Failed.");
                    return this.revert;
                } else {
                    popUp("Update Failed. " + jsonObject["Error"] + ".", ["OK"]);
                    return this.revert;
                }
            }
        }
    });


    //edit profile description -> requires autogrow
    $(".inLineEditUserDescription").editable("/User/EditProfile", {
        tooltip: 'Click to edit',
        type: 'autogrow',
        onblur: 'submit',
        data: function (value, settings) {
            var retval = value.replace(/<br[\s\/]?>/gi, '\n');
            return retval;
        },
        submitdata: {
            userId: $("#editProfileUserId").val()
        },
        autogrow: {
            lineHeight: 18,
            minHeight: 32
        },
        method: 'post',
        submit: 'save',
        cancel: 'cancel',
        placeholder: 'click here',
        indicator: "Saving...",
        intercept: function (jsondata) {
            var jsonObject = jQuery.parseJSON(jsondata);
            console.log(jsonObject);

            if (jsonObject["Status"] == 1) {
                if (jsonObject["Id"] == "firstName") {
                    $("#userAboutHeader span").html(jsonObject["Value"] + " " + $("#lastName").html());
                    $("#userAboutInformationPartial .aboutPictureWrapper .name").html(jsonObject["Value"] + " " + $("#lastName").html());
                }
                else if (jsonObject["Id"] == "lastName") {
                    $("#userAboutHeader span").html($("#firstName").html() + " " + jsonObject["Value"]);
                    $("#userAboutInformationPartial .aboutPictureWrapper .name").html($("#firstName").html() + " " + jsonObject["Value"]);
                }
                else if (jsonObject["Id"] == "description") {
                    //this will automatically repopulate correctly (and with html <br /> and not /n)
                }

                return jsonObject["Value"];
            }
            else {
                if (jsonObject["Id"] == null) {
                    popUpError_OkOrSendReport("Update Failed.");
                    return this.revert;
                } else {
                    popUp("Update Failed. " + jsonObject["Error"] + ".", ["OK"]);
                    return this.revert;
                }
            }

        }


        //,
        //indicator: "<img src='../../Content/Images/indicator.gif",
    });

    $(".inLineEditDate").editable("/User/EditProfile", {
        tooltip: 'Click to edit',
        onblur: 'submit',
        select: true,
        submitdata: {
            userId: $("#editProfileUserId").val()
        },
        submitBy: "change",
        cssclass: 'editable',
        width: '99%',
        placeholder: 'click here',
        indicator: "Saving...",
        onblur: 'submit',
        onsubmit: function (settings, original) {
            if (isValidBirthdate(original)) {
                return true;
            } else {
                //display your message
                return false;
            }
        },
        intercept: function (jsondata) {
            var jsonObject = jQuery.parseJSON(jsondata);
            console.log(jsonObject);

            if (jsonObject["Status"] == 1) {
                return jsonObject["Value"];
            }
            else {
                if (jsonObject["Error"] == null) {
                    popUpError_OkOrSendReport("Update Failed.");
                } else {
                    popUp("Update Failed. " + jsonObject["Error"] + ".", ["OK"]);
                }
                return this.revert;
            }

        }


        //,
        //indicator: "<img src='../../Content/Images/indicator.gif",
    });

    $(".inLineSelect").editable("/User/EditProfile", {
        data: "{'yes':'yes','no':'no'}",
        type: "select",
        select: true,
        submit: 'save',
        onblur: 'submit',
        submitdata: {
            userId: $("#editProfileUserId").val()
        },
        cssclass: 'editable',
        width: '99%'
    });
    ///****************************************************



    /*$(".inLineSelectProject").editable("/User/EditProject", {
    data: "{'yes':'yes','no':'no'}",
    type: "select",
    onblur: 'submit',
    submit: 'save',
    submitdata: {
    userId: $("#editProfileUserId").val()
    },
    cssclass: 'editable',
    width: '99%'
    });*/
    ///****************************************************

    $("#changeURLButton").click(function () {
        /*$("#changeURLTextField").fadeIn("slow");
        $("#changeURLTextField").focus();
        $("#changeURLButton").fadeOut("fast");
        $("#profileURLText").fadeOut("fast");
        $("#saveURLButton").fadeIn("slow");*/

        $("#changeURLTextField").show();
        $("#changeURLTextField").focus();
        $("#changeURLButton").hide();
        $("#profileURLText").hide();
        $("#saveURLButton").show();

    });

    //using .mousedown instead of .click because it fires before blur??
    $("#saveURLButton").mousedown(function () {
        if ($("#changeURLTextField").val() != "") {
            popUp("This will change your URL to www.vestn.com/v/" + $("#changeURLTextField").val() + ". Your old URL will become inactive, so you might have to redistribute your new URL to others. Are you sure you want to do this?", ["Yes! Change my URL", "Never mind"]);
        }

    });

    $("#changeURLTextField").blur(function () {
        /*$("#changeURLTextField").fadeOut("fast");
        $("#saveURLButton").fadeOut("fast");
        $("#changeURLButton").fadeIn("slow");
        $("#profileURLText").fadeIn("fast");*/

        $("#changeURLTextField").hide();
        $("#saveURLButton").hide();
        $("#changeURLButton").show();
        $("#profileURLText").show();

    });

    $('#changeURLTextField').keypress(function (event) {
        var keycode = (event.keyCode ? event.keyCode : event.which);
        if (keycode == '13' && $("#changeURLTextField").val() != "") {
            popUp("This will change your URL to www.vestn.com/v/" + $("#changeURLTextField").val() + ". Your old URL will become inactive, so you might have to redistribute your new URL to others. Are you sure you want to do this?", ["Yes! Change my URL", "Never mind"]);
        }
    });

    /*$('#tags').keypress(function (event) {
    var keycode = (event.keyCode ? event.keyCode : event.which);
    if (keycode == '13' && $("#tags").val() != "") {
    addUserTag(); or addProjectTag();
    }
    });*/

    $("#userTagButton").click(function () {
        if ($("#tags").val() != "") {
            addUserTag();
        }
    });

    $("#projectTagButton").click(function () {


        if ($("#tags").val() == "") {
            notify("Enter a tag in the text field before clicking the Add Project button", 2500);
        }
        else if ($("#projectTabs ul li")[0].value == getActiveProject()) {
            notify("You can't add a tag to the About project", 1500);
        }
        else {
            addProjectTag();
        }
    });

    $("#helpSwitch").click(function () {
        if ($(this).hasClass("active")) {
            $(".helpElementContainer").hide();
            $(this).removeClass("active");
        }
        else {
            $(".helpElementContainer").show();
            $(this).addClass("active");
        }
    });

    $(".helpElement").live("click", function () {
        $(this).fadeOut();
        if (!$("#resetHelpSwitch").hasClass("active")) {
            $("#resetHelpSwitch").addClass("active");
            $("#resetHelpSwitch").fadeIn();
        }
    });

    $(".helpElement").live({
        mouseenter:
    function () {
        $(this).children("img").css("display", "block");
    },
        mouseleave:
    function () {
        $(this).children("img").css("display", "none");
    }
    });

    $("#resetHelpSwitch").click(function () {
        //don't do anything if button not active
        if (!$(this).hasClass("active")) {
            return;
        }

        popUp("This will reset <b><i>all</i></b> help tips on <b><i>all</i></b> pages. <br />Are you sure you want to do this?", ["Yes! Show me help tips", "Never mind"]);


    });

    // initialize tooltip
    $(".cooltooltip").tooltip({
        offset: [10, 2],
        relative: 'false',
        effect: 'slide',
        position: 'bottom center'
        // add dynamic plugin with optional configuration for bottom edge
    });

    // initialize tooltip
    $(".tipHelpElementAboutProject").tooltip({
        offset: [50, 0],
        relative: 'false',
        effect: 'slide',
        position: 'top right'
        // add dynamic plugin with optional configuration for bottom edge
    });
    // initialize tooltip
    $(".tipHelpElementRenameProject").tooltip({
        offset: [50, 200],
        relative: 'false',
        effect: 'slide',
        position: 'top center'
        // add dynamic plugin with optional configuration for bottom edge
    });

    $(".tipHelpElementExperience").tooltip({
        offset: [50, 200],
        relative: 'false',
        effect: 'slide',
        position: 'top center'
        // add dynamic plugin with optional configuration for bottom edge
    });


    $(".tipHelpElementAbout").tooltip({
        offset: [50, 0],
        relative: 'false',
        effect: 'slide',
        position: 'top right'
        // add dynamic plugin with optional configuration for bottom edge
    });

    $(".tipHelpElementProject").tooltip({
        offset: [0, 0],
        relative: 'false',
        effect: 'slide',
        position: 'bottom left'
        // add dynamic plugin with optional configuration for bottom edge
    });

    $(".tipHelpElementProject").tooltip({
        offset: [0, 200],
        relative: 'false',
        effect: 'slide',
        position: 'bottom center'
        // add dynamic plugin with optional configuration for bottom edge
    });

    $(".tipHelpElementPicture").tooltip({
        offset: [0, 200],
        relative: 'false',
        effect: 'slide',
        position: 'bottom center'
        // add dynamic plugin with optional configuration for bottom edge
    });


});
