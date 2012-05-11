$(function(){
	
	$("#login-loginButton").live("click", function(){
		$("#login-loginForm input").blur();
		
		//JSON call here
		
		if ((($("#login-email").val() != "") && ($("#login-password").val() != ""))) {
            //make ajax call to submit the log on
            $.ajax({
                url: saasLocation + "/User/LogOn",
                dataType: "json",
                async: false,
                type: "POST",
                data: {
                    username: $("#login-email").val(),
                    password: $("#login-password").val()
                },
                success: function (data) {
                    if (data["Success"] == "true") {
                        //resumeUI();
                        //have to change this back to window.location.origin because otherwise we can't log in from the register page or another user's page. i realize that this breaks IE though..
						console.log("yo");
						console.log(data["Response"].id);
                        //window.location = window.location.protocol + "//" + window.location.host + "Profile.html?id="+data["Response"].id;
						window.location = window.location.protocol + "//" + "/F:/VestnFE/Profile.html?id="+data["Response"].id;
                        console.log(window.location);
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
		
		$("#login-statusText").fadeIn("fast");
		$(".login-inputField").hide();
		
		
		console.log("Logged In!");
		return false;
	});
	
	$("#login-minimizeButton").live("click", function(){
		$("#loginTemplate").fadeOut("fast");
	});
	
});