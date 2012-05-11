$(function(){
	bindHoverActionsForSelectUser();
	
	bindClickActionsForSelectUser();
	
	$(".pricing-getStartedButton").click(function(){
		navigateTo("signUp");
	});
});

function bindHoverActionsForSelectUser(){
	$("#pricing-individualPortal").hover(function(){
		$("#pricing-individualDescription").addClass("active");
		$("#pricing-organizationDescription").removeClass("active");
		
	},function(){
		$("#pricing-individualDescription").removeClass("active");
	});
	
	$("#pricing-organizationPortal").hover(function(){
		$("#pricing-organizationDescription").addClass("active");
		$("#pricing-individualDescription").removeClass("active");
	},function(){
		$("#pricing-organizationDescription").removeClass("active");
	});
}

function bindClickActionsForSelectUser(){
	$("#pricing-individualPortal").click(function(){
		removeSelectUser();
		$("#pricing-individualContent").addClass("active");
	});
	
	$("#pricing-organizationPortal").click(function(){
		removeSelectUser();
		$("#pricing-organizationContent").addClass("active");
	});
}

function removeSelectUser(){
	$("#pricing-selectUser").removeClass("active");
	$("#pricing-userDescriptions").removeClass("active");
	$("#pricing-organizationDescription").removeClass("active");
	$("#pricing-individualDescription").removeClass("active");
}