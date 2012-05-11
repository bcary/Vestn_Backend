$(document).ready(function () {
    $("#btnRegister").click(function () {
        if ((($("#Email").val() != "") && ($("#Password").val() != ""))) {
            //make ajax call to submit the log on
            $.ajax({
                url: "/User/Register",
                dataType: "json",
                async: false,
                type: "POST",
                data: {
                    Email: $("#Email").val(),
                    Password: $("#Password").val()
                },
                success: function (data) {
                    if (data["Success"] == "true") {
                        //resumeUI();
                        //have to change this back to window.location.origin because otherwise we can't log in from the register page or another user's page. i realize that this breaks IE though..
                        window.location = window.location.protocol + "//" + window.location.host + "/User/Profile";
                        return false;
                        //window.location = "http://localhost:8011/TestURL";
                        //window.location.href = window.location.origin;
                        //window.location.pathnam = window.location.href;
                        // location.reload(true);
                        //return changeLocation("http://localhost:8011/User/Profile");
                    }
                    else {
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

function changeLocation(location) {
    window.location.href = "Profile";
}