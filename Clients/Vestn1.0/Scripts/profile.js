/// <reference path="widtherize" />

function assignShortcuts() {
    $(document).keydown(function (e) {

        if ($("input").is(":focus")) {
            return;
        }

        if (e.keyCode == 37) {
            currentProjectElementSelectorIndex = $("#projectElementSelector_" + getActiveProjectElement()).index();
            if (currentProjectElementSelectorIndex >= 0) {
                if (currentProjectElementSelectorIndex >= 1) {
                    newProjectElementId = $("#projectElementSelector_" + getActiveProjectElement()).parent().children()[currentProjectElementSelectorIndex - 1].id.substring(23);
                    goToProjectElement(newProjectElementId);
                }
            }
            return false;
        }

        if (e.keyCode == 38) {
            //$("#projectTabs ul li.active").moveUp();

        }
        if (e.keyCode == 40) {
            //$("#projectTabs ul li.active").moveDown();

        }

        if (e.keyCode == 39) {
            currentProjectElementSelectorIndex = $("#projectElementSelector_" + getActiveProjectElement()).index();
            if (currentProjectElementSelectorIndex >= 0) {
                if (currentProjectElementSelectorIndex + 2 < $("#projectElementSelector_" + getActiveProjectElement()).parent().children().size()) {
                    newProjectElementId = $("#projectElementSelector_" + getActiveProjectElement()).parent().children()[currentProjectElementSelectorIndex + 1].id.substring(23);
                    goToProjectElement(newProjectElementId);
                }
            }
            return false;
        }

    });
}

function shareEmailIsValid() {

    /*$(".error").hide();*/
    var hasError = false;
    var emailReg = /^([\w-\.]+@([\w-]+\.)+[\w-]{2,4})?$/;

    var emailaddressVal = $("#shareDropdownEmailText input").val();
    if (emailaddressVal == '') {
        /*$("#UserEmail").after('<span class="error">Please enter your email address.</span>');*/
        //$("#invalidEmailText").fadeIn("fast");
        hasError = true;
    }

    else if (!emailReg.test(emailaddressVal)) {
        /*$("#UserEmail").after('<span class="error">Enter a valid email address.</span>');*/
        //$("#invalidEmailText").fadeIn("fast");
        hasError = true;
    }

    if (hasError == true) {
        $("#invalidEmailText").fadeIn("fast");
        $("#shareDropdownEmailText input").focus();
        return false;
    }
    else {
        $("#invalidEmailText").fadeOut("fast")
        return true;
    }

}

var mouse_is_inside = false;
var dropdown_visible = false;
var mouse_is_inside_gallery = false;

$(document).ready(function () {

    //assign shortcuts
    assignShortcuts();

    $("#galleriaBox").hover(function () {
        mouse_is_inside_gallery = true;
    }, function () {
        mouse_is_inside_gallery = false;
    });

    $("#shareDropdown").hover(function () {
        mouse_is_inside = true;
    }, function () {
        mouse_is_inside = false;
    });

    $("#shareButton").hover(function () {
        mouse_is_inside = true;
    }, function () {
        mouse_is_inside = false;
    });

    $("#popUp").hover(function () {
        mouse_is_inside = true;

    }, function () {
        mouse_is_inside = false;
    });

    $("body").mouseup(function () {
        if (!mouse_is_inside) {
            $('#shareDropdown').hide();
            dropdown_visible = false;
        }

        if (!mouse_is_inside_gallery) {
            hideAllGalleries();
        }
    });

    $("#shareButton").click(function () {

        if (dropdown_visible) {
            $("#shareDropdown").fadeOut("fast");
            dropdown_visible = false;
        }
        else {
            $("#shareDropdown").fadeIn("slow");
            dropdown_visible = true;
        }

    });

    $("#shareDropdownEmailText input").focus(function () {
        if ($(this).val() == "recipient's email") {
            $(this).val('');
        }
    });

    $("#shareDropdownEmailText input").blur(function () {
        if ($(this).val() == '') {
            $(this).val("recipient's email");
        }
        else {
            shareEmailIsValid();
        }
    });

    $("#shareDropdownProfileButton").click(function () {
        if (shareEmailIsValid()) {
            $("#invalidEmailText").fadeOut("fast");
            var inputEmail = $("#shareDropdownEmailText input").val();
            var data = { email: inputEmail, linkType: "profile" };
            popUp("This will send an email to " + inputEmail + " containing a link to this profile.", ["Share Profile", "Never mind"], false, data);
        }
    });

    $("#shareDropdownProjectButton").click(function () {
        if (shareEmailIsValid()) {
            $("#invalidEmailText").fadeOut("fast");
            var inputEmail = $("#shareDropdownEmailText input").val();
            var data = { email: inputEmail, linkType: "project" };
            popUp("This will send an email to " + inputEmail + " containing a link to this project.", ["Share Project", "Never mind"], false, data);
        }
    });

    $("#shareDropdownElementButton").click(function () {
        if (shareEmailIsValid()) {
            $("#invalidEmailText").fadeOut("fast");
            var inputEmail = $("#shareDropdownEmailText input").val();
            var data = { email: inputEmail, linkType: "view" };
            popUp("This will send an email to " + inputEmail + " containing a link to this page.", ["Share View", "Never mind"], false, data);
        }
    });

    /*right now not using a hashchange function -> only reloading page on a legitimate page refresh. otherwise, goToProjectElement will have a hashchange effect
    $(window).bind("hashchange", function (e) {

    var projectElementId = $.bbq.getState("peid")

    if (projectElementId != null) {

    if (isProjectElementOnPage(projectElementId)) {
    //goToProjectElement($.bbq.getState("peid"));
    }
    else {
    // do nothing, bad projectElementId
    }
    }

    });*/

    //pop up get help 
    $("#feedbackToUser").click(function () {
        popUp("Tell this person what you think of their work!", ["Send Feedback", "Cancel"], true);

    });

    hideAllGalleries();
});


function showGallery(projectId, pictureIndex) {

    $(".whiteHaze").show();

    

    $('#galleriaBox').css({
        'z-index': '30002',

        position:'relative',
        'background-color': 'black',
        '-moz-box-shadow': '0 0 20px #444',
        '-webkit-box-shadow': '0 0 20px #444',
        'box-shadow': '0 0 20px #444'
    });

    var gloaded = 0;

    $('#galleriaBox').galleria({
        height:'600px',
        width:'1250px',
        dataSource: '#galleria_' + projectId,
        extend: function (options) {
            this.bind('loadfinish', function (e) {
                if (gloaded == 0) {
                    goToCurrentGalleryImageIndex(pictureIndex);
                    gloaded = 1;
                    $('#galleriaBox').css({
                        'visibility': 'visible'
                    });
                }
            });
        }
    });
}

function goToCurrentGalleryImageIndex(pictureIndex) {
    getCurrentGallery().show(pictureIndex);
}

function getCurrentGallery() {
    var numberOfGalleries = Galleria.get().length;
    return Galleria.get(numberOfGalleries - 1);
}

function hideAllGalleries() {

    $('#galleriaBox').html('');

    if ($("#galleriaBox").css("visibility") == "visible") {
        $(".whiteHaze").hide();
    }

    $('#galleriaWrapper').css({
        'visibility':'hidden'
    });

    $('#galleriaBox').css({
        'visibility':'hidden'
    });
}