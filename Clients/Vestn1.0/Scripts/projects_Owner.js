/// <reference path="jeditable.js" />

function clearBindings() {

}



function setFileUploadSubmitButtonActive($button, active) {
    if (active) {
        $button.addClass("orange").removeClass("disabled").removeClass("black").css("display", "inline-block");
    }
    else {
        $button.removeClass("orange").addClass("disabled").addClass("black").css("display", "none");
    }
}

function flashErrorBackground($div, revertColor) {
    $div.stop().css("background-color", "#D64141");

    $div.delay('1000').animate({ backgroundColor: revertColor }, 500);
}

function validateFile(fileType) {

    var activeProjectId = getActiveProject();

    switch(fileType)
    {
        case "picture":
            var x = $('#addNewPictureToProject_' + activeProjectId + ' form .fileUploadField').val();
            var y = x.substring(x.length - 4, x.length).toLowerCase();

            $uploadButton = $("#addNewPictureToProject_" + activeProjectId + " form #buttonGroup a");

            if (y == ".png" || y == "jpeg" || y == ".jpg" || y == ".bmp") {
                setFileUploadSubmitButtonActive($uploadButton, true);
            }
            else {
                setFileUploadSubmitButtonActive($uploadButton, false);
                $('#addNewPictureToProject_' + activeProjectId + ' form .fileUploadField').val("");
                flashErrorBackground($("#addNewPictureToProject_" + activeProjectId + " form .acceptedFileTypesLabel"), '#CCCCCC');
            }
            break;
        case "document":
            var x = $('#addNewDocumentToProject_' + activeProjectId + ' form .fileUploadField').val();
            var y = x.substring(x.length - 4, x.length).toLowerCase();

            $uploadButton = $("#addNewDocumentToProject_" + activeProjectId + " form #buttonGroup a");

            if (y == ".csv" || y == ".doc" || y == "docx" || y == "html" || y == ".odp" || y == ".ods" || y == ".odt" || y == ".pdf" || y == ".pps" || y == ".ppt" || y == "pptx" || y == ".rtf" || y == ".sxc" || y == ".sxi" || y == ".sxw" || y == ".tsv" || y == ".txt" || y == ".wpd" || y == ".xls" || y == "xlsx") {
                setFileUploadSubmitButtonActive($uploadButton, true);
            }
            else {
                setFileUploadSubmitButtonActive($uploadButton, false);
                $('#addNewDocumentToProject_' + activeProjectId + ' form .fileUploadField').val("");
                flashErrorBackground($("#addNewDocumentToProject_" + activeProjectId + " form .acceptedFileTypesLabel"), '#CCCCCC');
            }
            break;
        case "audio":
            var x = $('#addNewAudioToProject_' + activeProjectId + ' form .fileUploadField').val();
            var y = x.substring(x.length - 4, x.length).toLowerCase();

            $uploadButton = $("#addNewAudioToProject_" + activeProjectId + " form #buttonGroup a");

            if (y == ".mp3" || y == ".wav" || y == ".m4a" || y == ".ogg") {
                setFileUploadSubmitButtonActive($uploadButton, true);
            }
            else {
                setFileUploadSubmitButtonActive($uploadButton, false);
                $('#addNewAudioToProject_' + activeProjectId + ' form .fileUploadField').val("");
                flashErrorBackground($("#addNewAudioToProject_" + activeProjectId + " form .acceptedFileTypesLabel"), '#CCCCCC');
            }
            break;
        case "video":
            var x = $('#addNewVideoToProject_' + activeProjectId + ' form .fileUploadField').val();
            var y = x.substring(x.length - 4, x.length).toLowerCase();

            $uploadButton = $("#addNewVideoToProject_" + activeProjectId + " form a");

            if (y == ".mov" || y == ".avi" || y == "peg4" || y == ".wmv" || y == ".flv" || y == "3gpp" || y == ".wmv" || y == "egps") {
                setFileUploadSubmitButtonActive($uploadButton, true);
            }
            else {
                setFileUploadSubmitButtonActive($uploadButton, false);
                $('#addNewVideoToProject_' + activeProjectId + ' form .fileUploadField').val("");
                flashErrorBackground($("#addNewVideoToProject_" + activeProjectId + " .acceptedFileTypesLabel"), '#CCCCCC');
            }
            break;
        default: break;
    }
}

function postFile(fileType) {

    var $iframeObject = $('#iframe-post-form');
    var pauseMessageContent = 'Uploading...';

    switch (fileType) {
        case "audio":

            if ($('#addNewAudioToProject_' + getActiveProject() + ' form #buttonGroup a').hasClass("disabled")) {
                return;
            }

            $iframeObject.val('addProjectElement');
            $('#addNewAudioToProject_' + getActiveProject() + ' form').submit();
            pauseMessageContent = "Uploading audio file...";
            $('#addNewAudioToProject_' + getActiveProject() + ' form .fileUploadField').val("");
            break;
        case "document":

            if ($('#addNewDocumentToProject_' + getActiveProject() + ' form #buttonGroup a').hasClass("disabled")) {
                return;
            }

            $iframeObject.val('addProjectElement');
            $('#addNewDocumentToProject_' + getActiveProject() + ' form').submit();
            pauseMessageContent = "Uploading document file...";
            $('#addNewDocumentToProject_' + getActiveProject() + ' form .fileUploadField').val("");
            break;
        case "experience":

            if ($('#addNewExperienceToProject_' + getActiveProject() + ' form #buttonGroup a').hasClass("disabled")) {
                return;
            }

            $iframeObject.val('addProjectElement');
            $('#addNewExperienceToProject_' + getActiveProject() + ' form').submit();
            pauseMessageContent = "Adding experience item...";
            break;

        case "picture":

            if ($('#addNewPictureToProject_' + getActiveProject() + ' form #buttonGroup a').hasClass("disabled")) {
                return;
            }

            $iframeObject.val('addProjectElement');
            $('#addNewPictureToProject_' + getActiveProject() + ' form').submit();
            pauseMessageContent = "Uploading picture file...";
            $('#addNewPictureToProject_' + getActiveProject() + ' form .fileUploadField').val("");
            break;
        case "video":

            if ($('#addNewVideoToProject_' + getActiveProject() + ' form a').hasClass("disabled")) {
                return;
            }

            $('#addNewVideoToProject_' + getActiveProject() + ' form').submit();
            pauseMessageContent = "Uploading Video to YouTube...";
            $('#addNewVideoToProject_' + getActiveProject() + ' form .fileUploadField').val("");
            break;
        case "projectPicture":

            /*if (!$('#projectPictureUpload_' + getActiveProject() + ' form #buttonGroup a').hasClass("active")) {
            return;
            }*/

            $iframeObject.val('updateProjectPicture');
            $('#projectPictureUpload_' + getActiveProject() + ' form').submit();
            pauseMessageContent = "Uploading Project Picture...";
            $('#projectPictureUpload_' + getActiveProject() + ' form .projectPictureUploadInput').val("");
            break;
        case "profilePicture":

            $iframeObject.val('addProfilePicture');
            $('#userProfilePictureUploadForm').submit();
            pauseMessageContent = "Uploading Profile Picture...";
            $("#userProfilePictureUploadForm #userProfilePictureUploadInput").val("");
            break;

        case "resume":
            $iframeObject.val('addResume');
            $('#userResumeUploadForm').submit();
            pauseMessageContent = "Uploading Resume...";
            $("#userResumeUploadForm #resumeUploadInput").val("");
            break;

        default:
            break;
    }

    pauseUI(pauseMessageContent);

}

function moveProject(direction) {
    $movingElement = $("#project_" + getActiveProject());

    $ajaxURL = "";

    if ($movingElement.index() == 0) {
        //return because we shouldn't be able to move the About project
        return;
    }

    if (direction == "left") {
        if ($movingElement.index() > 1) {//>1 because we need to not move before about project

            $movingElement.moveUp();

            //move to next page if movingElement going to next page (is currently the last item on the page to the left since we just called moveUp)
            if (($movingElement.index()) % 5 == 4) {
                goToProject($movingElement.attr("id").substring(8)); //move left and select the moving element project
                //set up movement buttons on tab bar - this MUST be called after we set the new active project
                setUpProjectMoveButtons($("#project_" + newProjectId).index());
            }

            $ajaxURL = "/Project/MoveProjectPrevious";
        }
    }
    else if (direction == "right") {
        sizeOfList = $movingElement.parent().children().size();

        //subtracting 1 for zero indexing
        if ($movingElement.index() < sizeOfList - 1) {

            $movingElement.moveDown();

            //move to next page if movingElement going to next page (is currently the first item on the page to the right since we just called moveDown)
            if (($movingElement.index()) % 5 == 0) {
                goToProject($movingElement.attr("id").substring(8)); //move right and select the moving element project

                //set up movement buttons on tab bar - this MUST be called after we set the new active project
                setUpProjectMoveButtons($("#project_" + newProjectId).index());
            }

            $ajaxURL = "/Project/MoveProjectNext";
        }
    }

    if ($ajaxURL == "") {
        return;
    }

    //make ajax call to update DB
    $.ajax({
        type: "POST",
        url: $ajaxURL,
        data: {
            projectId: getActiveProject()
        },
        dataType: "json",
        success: function (result) {

            if (result["Error"]) {
                popUpError_OkOrSendReport(result["Error"]);
            }

        },
        error: function (jsondata) {
            popUpUnknownError();
        }
    });

}

function moveProjectElement(direction) {
    $movingElement = $("#projectContent_" + getActiveProject() + " .projectContentFrame.active .chooser .mediaSelectionPane ul .mediaSelector.active");
    $movingElementType = $("#projectContent_" + getActiveProject() + " .projectContentFrame.active .chooser .mediaSelectionPane ul .mediaSelector.active").attr("class").split(" ")[0];

    $ajaxURL = "";

    if (direction == "up") {
        if ($movingElement.index() > 0) {
            $movingElement.moveUp();

            $ajaxURL = "/Project/MoveProjectElementPrevious";

        }
    }
    else if (direction == "down") {
        sizeOfList = $movingElement.parent().children().size();

        //subtracting 1 for zero indexing and another for Add New chooser.
        if ($movingElement.index() < sizeOfList - 2) {
            $movingElement.moveDown();
            
            $ajaxURL = "/Project/MoveProjectElementNext";
        }
    }


    if ($ajaxURL == "") {
        return;
    }

    //make ajax call to update DB
    $.ajax({
        type: "POST",
        url: $ajaxURL,
        data: {
            projectId: getActiveProject(),
            projectElementId: getActiveProjectElement(),
            projectElementType: $movingElementType
        },
        dataType: "json",
        success: function (result) {

            if (result["Error"]) {
                popUpError_OkOrSendReport(result["Error"]);
            }

        },
        error: function (jsondata) {
            popUpUnknownError();
        }
    });
}

function setUpProjectMoveButtons(indexOfTab) {
    if (indexOfTab == 0) {
        $("#moveProjectButtonWrapper").css("display", "none");
        return;
    }
    
    if (indexOfTab == 1) {
        $("#moveProjectRightButton").css("display", "block");
        $("#moveProjectLeftButton").css("display", "none");
    }
    else if (indexOfTab == $("#projectTabs ul").children().size() - 1) {
        $("#moveProjectRightButton").css("display", "none");
        $("#moveProjectLeftButton").css("display", "block");
    }
    else {
        $("#moveProjectRightButton").css("display", "block");
        $("#moveProjectLeftButton").css("display", "block");
    }

    visibleIndexOfTab = (indexOfTab) % 5;
    var moveToPosition = (135 * visibleIndexOfTab) + -2;
    
    $("#moveProjectButtonWrapper").css("left", moveToPosition + "px");
    $("#moveProjectButtonWrapper").css("display", "block");
}

function bindProjectMoveButtons() {
    $("#projectTabs ul li").click(function () {
        setUpProjectMoveButtons($(this).index());
    });

    $("#moveProjectRightButton").click(function () {
        moveProject("right");
        setUpProjectMoveButtons($("#projectTabs ul li.active").index());
    });

    $("#moveProjectLeftButton").click(function () {
        moveProject("left");
        setUpProjectMoveButtons($("#projectTabs ul li.active").index());
    });
}



$(document).ready(function () {

    $(".aboutPicture").click(function () {

        $(this).parent().children(".informationPictureUpload").children("form").children(".projectPictureUploadInput").click();
        //$(".projectPictureUploadInput").click();
    });

    $(".aboutPicture").hover(function () {
        $(".editProjectPictureLabel").html("Click to change");
        $(".editProjectPictureLabel").addClass("hovered");
    },
    function () {
        $(".editProjectPictureLabel").html("CHANGE");
        $(".editProjectPictureLabel").removeClass("hovered");
    });

    clearBindings();

    bindProjectMoveButtons();


    $(".submitButtonForVideoLink").live("click", function () {
        var projectId = $(this).attr("id").substring(25);
        if ($("#addNewVideoLinkToProject_" + projectId + " form .newVideoLinkUpload").val() != "") {
            if ($("#addNewVideoLinkToProject_" + projectId + " form .newVideoLinkUpload").val().substring(0, 16) == "http://youtu.be/") {
                $("#addNewVideoLinkToProject_" + projectId + " form").submit();
                pauseUI("Uploading Content...");
            }
            else {
                popUpError_OkOrSendReport("Not a valid youtube link.", 1500);
            }
        }
        else {
            notify("No link specified.", 1500);
        }
    });

    //Depreciated. TODO refactor this / do more elegantly
    $("#innerProjectTabDelete").click(function () {
        //make sure this is not the about project
        if ($("#project_" + getActiveProject()).hasClass("aboutProjectTab")) {
            popUp("You cannot delete the About project");
            return;
        }

        popUp("Really delete entire<br /><b><i>" + $("#project_" + getActiveProject()).text() + "</b></i><br />project?", ["Delete Project", "Cancel"]);
    });

    $(".innerProjectElementTabButton.delete").click(function () {
        popUp("Really delete<br /><b><i>" + $("#projectElementMedia_" + getActiveProjectElement() + " .mediaInformation #title").text() + "</b></i><br />project element?", ["Delete Element", "Cancel"]);
    });

    $(".innerProjectElementTabButton.moveUp").click(function () {
        moveProjectElement("up");
    });
    $(".innerProjectElementTabButton.moveDown").click(function () {
        moveProjectElement("down");
    });



    // Project
    ///Project/EditEntireProject****************************************************
    $(".inLineEditProject").editable("/Project/EditProject", {
        tooltip: 'Click to edit',
        onblur: 'submit',
        select: true,
        data: function (value, settings) {
            var retval = value.replace(/<br[\s\/]?>/gi, '\n');
            return retval;
        },
        submitdata: function () {
            return { projectId: getActiveProject() };
        },
        method: 'post',
        submit: 'save',
        cancel: 'cancel',
        cssclass: 'editable',
        width: '99%',
        placeholder: 'click here',
        indicator: "Saving...",
        intercept: function (jsondata) {
            result = jQuery.parseJSON(jsondata);

            switch (result["UpdateStatus"]) {
                case "updated":

                    //update project tab if updating project name
                    if (result["UpdateType"] == "name") {
                        $("#project_" + getActiveProject()).html("<sh6>" + result["UpdateValue"].substring(0, 10) + "</sh6>");
                    }

                    return result["UpdateValue"];
                    break;

                case "notUpdated_TooLong":
                    popUpError_OkOrSendReport("Update Failed. " + result['UpdateType'] + " must be less than 100 characters.");
                    return this.revert;
                    break;

                case "notUpdated":
                    popUpError_OkOrSendReport("Update Failed.");
                    return this.revert;
                    break;

                default:
                    popUpError_OkOrSendReport("Update Failed.");
                    return this.revert;
                    break;
            }

        }
    });

    //edit project description text area autogrow
    $(".inLineEditProjectDescription").editable("/Project/EditProject", {
        tooltip: 'Click to edit',
        type: 'autogrow',
        onblur: 'submit',
        data: function (value, settings) {
            var retval = value.replace(/<br[\s\/]?>/gi, '\n');
            return retval;
        },
        submitdata: function () {
            return { projectId: getActiveProject() };
        },
        method: 'post',
        autogrow: {
            lineHeight: 18,
            minHeight: 32
        },
        submit: 'save',
        cancel: 'cancel',
        cssclass: 'editable',
        placeholder: 'click here',
        indicator: "Saving...",
        intercept: function (jsondata) {
            result = jQuery.parseJSON(jsondata);

            switch (result["UpdateStatus"]) {
                case "updated":
                    return result["UpdateValue"];
                    break;

                case "notUpdated":
                    popUpError_OkOrSendReport("Update Failed.");
                    return this.revert;
                    break;

                default:
                    popUpError_OkOrSendReport("Update Failed.");
                    return this.revert;
                    break;
            }

        }
    });




    // Project Element
    ///User/EditProject****************************************************
    $(".inLineEditProjectElement").editable("/Project/EditProjectElement", {
        tooltip: 'Click to edit',
        onblur: 'submit',
        submitdata: function () {
            return { projectElementId: getActiveProjectElement() };
        },
        cssclass: 'editable',
        select: true,
        width: '99%',
        placeholder: 'click here',
        indicator: "Saving...",
        intercept: function (jsondata) {
            result = jQuery.parseJSON(jsondata);

            switch (result["UpdateStatus"]) {
                case "updated":

                    //update some content here
                    if (result["UpdateType"] == "title") {
                        $("#projectElementSelector_" + getActiveProjectElement() + " sh5").html(result["UpdateValue"].split(' ')[0].toLowerCase().substring(0, 9));
                        //Galleria fields update
                        if (result["ProjectElementType"] == "ProjectElement_Picture") {
                            $("#galleriaImage_" + getActiveProjectElement()).attr("title", result["UpdateValue"]);
                        }

                    }
                    if (result["UpdateType"] == "company") {
                        $("#projectElementSelector_" + getActiveProjectElement() + " sh5").html(result["UpdateValue"].substring(0, 9).toLowerCase()); //selector
                        $("#projectElementMedia_" + getActiveProjectElement() + " #company").html(result["UpdateValue"]); //both the display in the media viewer and media information
                    }
                    if (result["UpdateType"] == "jobTitle") {
                        $("#projectElementMedia_" + getActiveProjectElement() + " #jobTitle").html(result["UpdateValue"]); //both the display in the media viewer and media information
                    }
                    $('.experience-company').widtherize({ 'width': 420, 'maxSize': 32 });
                    $('.experience-jobTitle').widtherize({ 'width': 420, 'maxSize': 32 });
                    $('.mediaInformation-company').widtherize({ 'width': 275, 'maxSize': 20 });
                    
                    return result["UpdateValue"];
                    break;

                case "notUpdated_TooLong":
                    popUpError_OkOrSendReport("Update Failed. " + result['UpdateType'] + " must be less than 100 characters.");
                    return this.revert;
                    break;

                case "notUpdated_DateError":
                    popUpError_OkOrSendReport("Update Failed. " + result['UpdateType'] + " must be a valid date.");
                    return this.revert;
                    break;

                case "notUpdated":
                    popUpError_OkOrSendReport("Update Failed.");
                    return this.revert;
                    break;

                default:
                    popUpError_OkOrSendReport("Update Failed.");
                    return this.revert;
                    break;
            }

        }
    });

    //edit project element description text area autogrow
    $(".inLineEditProjectElementDescription").editable("/Project/EditProjectElement", {
        tooltip: 'Click to edit',
        type: 'autogrow',
        onblur: 'submit',
        data: function (value, settings) {
            var retval = value.replace(/<br[\s\/]?>/gi, '\n');
            return retval;
        },
        submitdata: function () {
            return { projectElementId: getActiveProjectElement() };
        },
        method: 'post',
        autogrow: {
            lineHeight: 18,
            minHeight: 32
        },
        submit: 'save',
        cancel: 'cancel',
        cssclass: 'editable',
        placeholder: 'click here',
        indicator: "Saving...",
        intercept: function (jsondata) {
            result = jQuery.parseJSON(jsondata);

            switch (result["UpdateStatus"]) {
                case "updated":

                    //Galleria fields update
                    if (result["ProjectElementType"] == "ProjectElement_Picture") {
                        $("#galleriaImage_" + getActiveProjectElement()).attr("alt", result["UpdateValue"]);
                    }
                    return result["UpdateValue"];
                    break;

                case "notUpdated_TooLong":
                    popUpError_OkOrSendReport("Update Failed. " + result['UpdateType'] + " must be less than 100 characters.");
                    return this.revert;
                    break;

                case "notUpdated_DateError":
                    popUpError_OkOrSendReport("Update Failed. " + result['UpdateType'] + " must be a valid date.");
                    return this.revert;
                    break;

                case "notUpdated":
                    popUpError_OkOrSendReport("Update Failed.");
                    return this.revert;
                    break;

                default:
                    popUpError_OkOrSendReport("Update Failed.");
                    return this.revert;
                    break;
            }

        }
    });


    //bind inLineEdit ONLY to inLineEdits within the projects partial view
    $("#userAboutInformationPartial .inLineEdit").editable("/User/EditProfile", {
        tooltip: 'Click to edit',
        onblur: 'submit',
        submitdata: {
            userId: $("#editProfileUserId").val()
        },
        /*submit: 'save',
        cancel: 'cancel',*/
        cssclass: 'editable',
        select: true,
        width: '99%',
        placeholder: 'click here',
        indicator: "Saving...",
        intercept: function (jsondata) {
            var jsonObject = jQuery.parseJSON(jsondata);
            console.log(jsonObject);

            if (jsonObject["Status"] == 1) {

                if (jsonObject["Id"] == "email") {
                    $('#userAboutInformationPartial .aboutLeftColumn .aboutPictureWrapper .email').html(jsonObject["Value"]);
                    $('#userAboutInformationPartial .aboutLeftColumn .aboutPictureWrapper .email').widtherize({ 'width': 185, 'maxSize': 20 });
                }
                if (jsonObject["Id"] == "title") {
                    $('#userAboutInformationPartial .aboutLeftColumn .aboutPictureWrapper .title').html(jsonObject["Value"]);
                    $('#userAboutInformationPartial .aboutLeftColumn .aboutPictureWrapper .title').widtherize({ 'width': 185, 'maxSize': 26 });
                }
                return jsonObject["Value"];
            }
            else {
                if (jsonObject["Id"] == null) {
                    popUpError_OkOrSendReport("Update Failed.");
                    return this.revert;
                } else {
                    popUpError_OkOrSendReport("Update Failed. " + jsonObject["Error"]);
                    return this.revert;
                }
            }

        }


        //,
        //indicator: "<img src='../../Content/Images/indicator.gif",
    });

    $("#innerProjectTabShare").click(function () {
        $("#innerProjectTabShare").css("display", "none");
        $('#shareLinkText').fadeIn("fast", function () {
            $("#shareLinkText input").val("vestn.com/v/" + $(".sessionVariables .session_profileURL").text() + "#peid=null&iti=project-Information&proj=" + getActiveProject());
            $("#shareLinkText input").select();
        });
    });

    $("#shareLinkText input").blur(function () {
        $("#shareLinkText").css("display", "none");
        $('#innerProjectTabShare').fadeIn("fast", function () {
        });
    });

    //This function get called by the Add button in project video frame. It will call a code behind function to generate the token + URL value
    $(".showUploadNewVideoButton").click(function () {
        $.ajax({
            type: "POST",
            url: "/User/GetYoutubeURLandToken",
            data: {},
            dataType: "json",
            success: function (jsondata) {

                if (jsondata["postURL"] != "" && jsondata["token"] != "") {

                    $("input[id=hdtoken]").val(jsondata["token"]);

                    var url = jsondata["postURL"] + "?nexturl=" + window.location.protocol + "//" + window.location.host + "/project/AddVideoElement?ProjectID=" + $('#hdModelId').val();
                    $("#UploadFileForm").attr("action", url);

                }

            },
            error: function (jsondata) {
                popUp(jsondata["error"]);
            }
        });
    });



});

function createProject() {
    pauseUI("Creating Project..");
    $.ajax({
        type: "POST",
        data: null,
        url: "/Project/CreateProject",
        dataType: "json",
        success: function (result) {
            resumeUI();
            if (result["Error"]) {
                popUpUnknownError();
                return;
            }
            updatePartial("#Partial_Projects", result["UpdatedPartial"]);
            goToProject(result["ProjectId"]);
            //set up movement buttons on tab bar - this MUST be called after we set the new active project
            //setUpProjectMoveButtons($("#project_" + newProjectId).index());

            $("#helpElementProjects").fadeOut();
        },
        error: function (result, status, error) {
            resumeUI();
            popUpUnknownError();
            return;
        }
    });
}

function updateProject(projectModel) {
    $.ajax({
        type: "POST",
        data: { projectModel: projectModel },
        url: "/Project/UpdateProject",
        dataType: "json",
        success: function (result) {
            if (result["Error"]) {
                popUpUnknownError();
                return;
            }
            updatePartial("#Partial_Projects", result.responseText);
        },
        error: function (result, status, error) {
            popUpUnknownError();
        }
    });
}
