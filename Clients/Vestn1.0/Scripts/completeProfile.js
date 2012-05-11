$(function () {

    $("#getStarted form input#agreement").change(function () {
        checkSubmitReady();
    });

    $("#getStarted form input").keyup(function () {
        checkSubmitReady();
    });

    $("#getStarted form input").change(function () {
        checkSubmitReady();
    });

    $("#getStarted form input").focus(function () {
        checkSubmitReady();
    });

    $("#completeProfileSubmitButton").click(function () {
        submitCompleteProfile();
    });

});

/*
function validatePhone() {
    
    if ($("#getStarted form input#phoneNumber").val() != "") {
       
        var phoneNumberPattern = /^\(?(\d{3})\)?[- ]?(\d{3})[- ]?(\d{4})$/;
        if (phoneNumberPattern.test($("#getStarted form input#phoneNumber").val())) {
            $("#getStarted form input#phoneNumber").css("outline", "none");
            return true;
        }
        else {
            $("#getStarted form input#phoneNumber").css("outline", "2px solid #E75547");
            return false;
        }
    }
    return true;
}*/


function checkSubmitReady() {
    var submitReady = true;

    $("#getStarted form input.required").each(function () {
        if ($(this).val() == "") {
            submitReady = false;
        }
        else {
            $(this).css("outline", "none");
        }
    });

    if (!$("#getStarted form input#agreement").is(":checked")) {
        submitReady = false;
    }

    if (submitReady) {
        $("#completeProfileSubmitButton").addClass("orange");
    }
    else {
        $("#completeProfileSubmitButton").removeClass("orange");
    }
}


function submitCompleteProfile() {

    checkSubmitReady();

    if ($("#completeProfileSubmitButton").hasClass("orange")) {
        makeAjaxCallToSubmitProfile();
    }
    else {
        //user doesn't have all required fields filled out
        $("#getStarted form input.required").each(function () {
            if ($(this).val() == "") {
                $(this).css("outline", "2px solid #E75547");
            }
            else {
                $(this).css("outline", "none");
            }
        });
    }
}


//after complete profile button is clicked, fill the information from the form into 
function fillUserInformationFromCompleteProfile(userInformation) {
    $("#nameBar div #firstName").text(userInformation["FirstName"]);
    $("#nameBar div #lastName").text(userInformation["LastName"]);

    $("#userAboutHeader span").html(userInformation["FirstName"] + " " + userInformation["LastName"]);
    $("#userAboutInformationPartial .aboutPictureWrapper .name").html(userInformation["FirstName"] + " " + userInformation["LastName"]);

    $("#userAboutInformationPane #school").text(userInformation["School"]);
    $("#userAboutInformationPane #major").text(userInformation["Major"]);
    $("#userAboutInformationPane #location").text(userInformation["Location"]);
}

function makeAjaxCallToSubmitProfile() {
    $.ajax({
        url: "/User/CompleteProfile",
        dataType: "json",
        type: "POST",
        data: {
            firstName: $("#firstNameCompleteProfile").val(),
            lastName: $("#lastNameCompleteProfile").val(),
            location: $("#locationCompleteProfile").val(),
            school: $("#schoolCompleteProfile").val(),
            major: $("#majorCompleteProfile").val(),
            checkbox: $("#agreement").val()
        },
        success: function (result) {
            if (result["Error"]) {
                popUp(result["Error"], ["OK"]);
                return;
            }

            if (result["UserInformation"]) {
                fillUserInformationFromCompleteProfile(result["UserInformation"]);

                $("#getStarted").fadeOut();
                $(".haze").hide();
                $(".helpElementContainer").fadeIn();
            }
            else {
                popUpUnknownError();
            }

        },
        error: function (result, status, error) {
            popUpUnknownError();
        }
    });

    //comment out since we dont have video ready yet
   // popUp("Do you want to watch Vestn Tutorial", ["Yes", "No"], false); 

}
