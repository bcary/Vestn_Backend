$(document).ready(function () {
    $("#btnLogin").click(function () {
        if (($("#emailInput1").val() != "") && ($("#passwordInput1").val() != "")) {
            //make ajax call to submit the log on
            $.ajax({
                url: "/User/Logon",
                dataType: "json",
                type: "POST",
                data: {
                    username: $("#emailInput1").val(),
                    password: $("#passwordInput1").val(),
                    rememberme: $("#rememberMeCheckbox1").is(':checked')
                },
                success: function (data) {
                    if (data["LogOnResult"] == "Success") {
                        //resumeUI();
                        //have to change this back to window.location.origin because otherwise we can't log in from the register page or another user's page. i realize that this breaks IE though..
                        window.location = window.location.protocol + "//" + window.location.host + "/Test";
                    }
                    else {
                        resumeUI();
                        popUpError_OkOrSendReport(data["Error"]);
                    }
                },
                error: function (result, status, error) {
                }
            });
        }
        else {
            notify("Missing user name or password", 1000);
        }
    });
});