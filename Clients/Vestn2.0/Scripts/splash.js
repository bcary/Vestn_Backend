$(function () {
	
	bindBBQ();
	
	initializeSplashPages();
	
	
	bindNavigation();
	
	$("#splashContainer #login").load('templates/splash_templates/login.html');
	
	bindLogin();
	
	
	loadPages();
});

function loadPages(){
	
	$.each($("#splashMain .mainContent"), function(){
		var pageName = $(this).attr("id").substring(14);
		$(this).load("templates/splash_templates/" + pageName + ".html");
	});
	
//	$("#splashMain .mainContent").load('templates/splash_templates/' + navigationItem + '.html?4');
}

function bindBBQ(){
	$(window).bind( 'hashchange', function(e) {
		
		var page = $.bbq.getState()["page"];
		
		if(page!=null){
			navigateMenuTo(page);
		}
	});
	
	//now that we have bound this we need to navigate to the current state if exists, or to tour if it doesn't
	var pageState = $.bbq.getState()["page"];
	if(pageState!=null){
		$(window).trigger( 'hashchange' );
	}
	else{
		pageState = {page: "tour"};
		$.bbq.pushState(pageState);
		$(window).trigger( 'hashchange' );
	}
}

function bindLogin(){
	$("#memberLoginButton").click(function(){
		$("#loginTemplate").fadeIn("fast");
		$("#loginTemplate #login-email").focus();	
	});
}

function bindNavigation(){
	$("#splashContainer .navItem").click(function(){
		var navigateToNavItem = $(this).attr("id").substring(8);
		
		var newState = {page: navigateToNavItem};
		$.bbq.pushState(newState);
	});
}

function navigateTo(navigateToNavItem){
	var newState = {page: navigateToNavItem};
	$.bbq.pushState(newState);
}

function navigateMenuTo(navigationItem){
	clearActive();
	
	$("#navItem-" + navigationItem).addClass("active");
	
	$("#splashContent-" + navigationItem).addClass("active");
}

function clearActive() {
	//remove active nav item's template from page
	//todo
	
	
	//clear active nav item
	$(".navItem.active").removeClass("active");
	
	//clear active splashContent template
	$(".mainContent.active").removeClass("active");
}

function initializeSplashPages() {
	//don't need to do it this way because we're not passing in information
	//var jsonResult = {Description:"You can upload any file type."};
	//renderTemplate("templates/splash_templates/Tour.html", jsonResult, "splashMain .mainContent");
	
	
	//$("#splashMain .mainContent").load('templates/splash_templates/tour.html');
}














