$(function(){
	
	$("#progressbar").progressbar({
		value:10
	});
	
	$("#signUp-createAccountForm").live("submit", function(){
		$("#signUp-createAccountForm input").blur();
		console.log("Signed Up!");
		return false;
	})
});