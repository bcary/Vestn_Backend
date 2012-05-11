var jsonResultUser = "";
var jsonResultProjects = [];
//TODO: update this...
var userID;

$(document).ready(function () {

	userID = getUrlVars()["id"];
 

	//initializeArtifacts();
	loadFrame();//for left nav bar and header
	renderEssentials();
	
	$('#scrollbarProjectsPane').tinyscrollbar();
	
	$(".timelineIcon").live("click", function () {
		//var elementClasses = $(this).attr('class');
		toggleExpand($(this));
	});
	
	$(".artifactTitle").live("click", function () {
		//var elementClasses = $(this).attr('class');
		toggleExpand($(this).parent().parent().find('.timelineIcon'));
	});
	
	/*$(".projects").sortable({
				connectWith: ".projects"
	}).disableSelection();*/
	$("#leftColProjects").sortable({
		update: function(event, ui) {
			var idString = "";
			$.each($("#leftColProjects").children(), function(index, value) {
			  	idString += value.id.substring(15)+",";
			});
			idString = idString.substring(0, idString.length - 1);
			updateProjectOrder(idString);
		}
	});
		
	
	//essentials
	$("#essentialsHeading").click(function () {
		essentialsElementClicked($(this).find('#leftColAbout'));
		return false;
	});
	$("#leftColAbout").click(function () {
		essentialsElementClicked($(this));
		return false;
	});
	
	$("#leftColResume").click(function () {
		essentialsElementClicked($(this));
		return false;
	});
	
	$("#leftColExperiences").click(function () {
		essentialsElementClicked($(this));
		return false;
	});
	
	$("#leftColReferences").click(function () {
		essentialsElementClicked($(this));
		return false;
	});
	
	//projects
	$("#projectsHeading").click(function () {		
		if (!$(this).find('#'+$(this).children().children().attr('id')).hasClass('active')) {
			//this gets all projects
			var arr = [];
			var projects = $("#leftColProjects");
			projects.children('ul').each(function() {
				/*var projectID = $(this).attr('id');
				var projectName = $(this).text();
				arr.push({
					'id':projectID,
					'name':projectName
				})*/
				arr.push({
					'project':$(this)
				})
			});

			//for debugging -- gets project name and id of given project in array
			/*var string = "";
			$.each(arr, function(i, v) {
			    string=("Project ID: "+v.id+"\nProjectName: "+v.name);
			});*/

			//gets the id of the first div
			var divID = arr[0].project.attr('id');

			//gets the id of the first project, not of the associated div
			var projectID = divID.substring(15);

			projectSelected(projectID);
			toggleExpandNavigation(arr[0].project);
			renderOrInitializeProject(projectID);
			
			/*clearActive();
			clearProjects();
			$("#"+arr[0].id).addClass("active");
			$("#projectsHeading").addClass("active");*/
		}
		return false;
		
	});
	$("#leftcol-list .category.projects ul").click(function () {
		if (!$(this).hasClass("active")) {
			//if the project isn't already selected
			var divID = $(this).attr('id');// this is the project div's id
			var projectID = divID.substring(15);//this is the projectID from the back end
			renderOrInitializeProject(projectID);
		}
		toggleExpandNavigation($(this));
		return false;
	});
	
	$("#leftcol-list .category.projects.artifacts li").click(function () {
		//artifacts
		alert($(this).attr("id"));
		return false;
	});
	
	//settings
	$("#settingsHeading").click(function () {
		collapseAllProjectsNav();
		clearActive();
		clearProjects();
		$("#settingsHeading").addClass("active");
		$("#leftColChangePassword").addClass("active");
	});
	$("#leftColChangePassword").click(function () {
		collapseAllProjectsNav();
		clearActive();
		clearProjects();
		$("#leftColChangePassword").addClass("active");
		$("#settingsHeading").addClass("active");
		return false;
	});
	
	$("#leftColChangeURL").click(function () {
		collapseAllProjectsNav();
		clearActive();
		clearProjects();
		$("#leftColChangeURL").addClass("active");
		$("#settingsHeading").addClass("active");
		return false;
	});
	
	$(".inLineEdit").editable(saasLocation + "/User/EditProfile", {
        tooltip: 'Click to edit',
        select: true,
        submitdata: {
            userId: userID
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
	
});

function essentialsElementClicked(element) {
	//if already selected, do nothing.
	if (!element.hasClass("active")) {
		// if not selected, check to see if the essentials section is selected
		if (element.parent().parent().hasClass("active")) {
			//scroll to correct location instead of re-rendering shit
		}
		else {
			//render essentials
			collapseAllProjectsNav();
			clearProjects();
			renderEssentials();
		}
		clearActive();
		element.addClass("active");
		$("#essentialsHeading").addClass("active");
	}
}

//expects projectID, not divID
function projectSelected(projectID) {
	clearProjects();
	clearActive();
	$("#projectsHeading").addClass("active");
	$("#leftColProject_"+projectID).addClass("active");
}

function clearActive() {
	//clear essentials
	$("#essentialsHeading").removeClass("active");
	$("#leftColAbout").removeClass("active");
	$("#leftColResume").removeClass("active");
	$("#leftColExperiences").removeClass("active");
	$("#leftColReferences").removeClass("active");
	
	//clear projects
	$("#projectsHeading").removeClass("active");
	$(".projects").removeClass("active");
	
	//clear settings
	$("#settingsHeading").removeClass("active");
	$("#leftColChangePassword").removeClass("active");
	$("#leftColChangeURL").removeClass("active");
}

function clearActiveArtifacts() {
	$('#leftcol-list .category.projects ul').children().each(function () {
	    if (!$(this).hasClass('expanded')) {
			$(this).removeClass("active");
		}
	});
	
	$('#projects').find('.rightColEntry').each(function () {
		$(this).find('.artifactTitle').removeClass('active');
	});
}

function clearActiveEssentials() {
	$('#leftcol-list .category.essentials').children().children().each(function () {
	    if (!$(this).hasClass('expanded')) {
			$(this).removeClass("active");
		}
	});
	
	$('#projects').find('.rightColEntry').each(function () {
		$(this).find('.artifactTitle').removeClass('active');
	});
}

function toggleExpand(element) {
	if (!element.hasClass('aboutIcon')) {
		var parent = element.parent();
		if (parent.hasClass('expanded')) {
			//collapse	
			$("#"+parent.attr('id')+" .artifactContent").css("overflow", "hidden");
			$("#"+parent.attr('id')+" .artifactContent").css("height", "40px");
			parent.removeClass('expanded');
		
		}
		else {
			$("#"+parent.attr('id')+" .artifactContent").css("height", "");
			parent.addClass('expanded');
		}
		// re-add waypoints
		$('#projects').find('.rightColEntry').each(function () {
			addProjectWaypoint(parent.attr('id'));
		});
		
	}
}
/*//don't know how to do this shit yet
function toggleExpandDescription(element) {
	if (element.hasClass('expandedDescription')) {
		//collapse
		$(".artifactDescription").css("height", "15px");
		element.removeClass('expandedDescription');
	}
	else {
		$(".artifactDescription").css("height", "");
		element.addClass('expandedDescription');
	}
}*/

function toggleExpandNavigation(element) {
	if (element.hasClass('expanded')) {
		//collapse	
		collapseNav(element);
	}
	else {
		//expand
		expandNav(element);
	}
}

function collapseNav(element) {
	element.children().css("display","none");
	element.removeClass('expanded');
}

function collapseAllProjectsNav() {
	$('#leftcol-list .category.projects ul').children().each(function () {
	    collapseNav($(this));
	});
}

function expandNav(element) {
	collapseAllProjectsNav();
	element.children().css("display","block");
	element.addClass('expanded');
}

function renderEssentials() {
	createAbout(jsonResultUser.UserID);
	renderTemplate("templates/artifact_templates/aboutTemplate.html", jsonResultUser, "About");

	//TODO: test to see if this if statement actually works...
	if (jQuery.inArray("resume", jsonResultUser.Response[0])) {
		createResume();
		renderTemplate("templates/artifact_templates/resumeTemplate.html", jsonResultUser, "Resume");
	}
	else {
		createAddResume();
		renderTemplate("templates/artifact_templates/addResumeTemplate.html", jsonResultUser, "addResume");
	}
	createAddExperience();
	renderTemplate("templates/artifact_templates/addExperienceTemplate.html", jsonResultUser, "addExperience");
	createAddReferences();
	renderTemplate("templates/artifact_templates/addReferencesTemplate.html", jsonResultUser, "addReferences");
	
	var offset = $('div#projects div.rightColEntry:last').height();
	$("#projects").append("<div id='projectBottomBuffer' style='margin-top:-"+offset+"px'></div>");
	
	//TODO: clean this up once it works
	addEssentialsWaypoint("About");
	addEssentialsWaypoint("Resume");
	addEssentialsWaypoint("addExperience");
	addEssentialsWaypoint("addReferences");
}

function createAbout(userID) {
	if ($("#About").length == 0) {
		$("#projects").append("<div class='rightColEntry'><div id='About'></div>");
		$("#About").addClass("expanded");
	}
}

function createAddResume() {
	if ($("#addResume").length == 0) {
		$("#projects").append("<div class='rightColEntry'><div id='addResume'></div>");
	}
}

function createResume() {
	if ($("#Resume").length == 0) {
		$("#projects").append("<div class='rightColEntry'><div id='Resume'></div>");
		$("#Resume").addClass('expanded');
	}
}

function createAddExperience() {
	if ($("#addExperience").length == 0) {
		$("#projects").append("<div class='rightColEntry'><div id='addExperience'></div>");
	}
}

function createAddReferences() {
	if ($("#addReferences").length == 0) {
		$("#projects").append("<div class='rightColEntry'><div id='addReferences'></div>");
	}
}

function loadFrame() {
		$.ajax({
		    type:'get',
			async: false,
		    dataType: 'json',
		    data: {
		       id: userID
		   },
		   url: saasLocation + "/User/GetUserInformation"
		}).done(function(data) {
		    jsonResultUser = data;
		
			//load header
			var firstName = jsonResultUser.Response[0].firstName;
			var lastName = jsonResultUser.Response[0].lastName;
			$("#firstName").text(firstName);
			$("#lastName").text(lastName);
			$(".headerProfilePic").append('<img src='+jsonResultUser.Response[0].profilePictureThumbnail+' style="max-width:100%; max-height:100%" />');
			
			$.each(jsonResultUser.Response[0].projects, function(index, value) {
			  	//if (value.name != "About") {
					var projID = value.id;
					//$("#leftColProjects").append("<li id='leftColProject_"+value.id+"' class='category projects'>"+value.name+"</li>");
					$("#leftColProjects").append("<ul id='leftColProject_"+projID+"' class='category projects artifacts'>"+value.name+"</ul>");
					if (value.artifacts.length > 0) {
						$.each(value.artifacts, function(index, value) {
							$("#leftColProject_"+projID).append("<li id='leftColProjectArtifact_"+value.id+"'>"+value.title+"</li>");
						});
					}
					else {
						// empty project

						//$("#projectsHeading").append("</ul>");
					}
				//}
			});
			
		});	
}

//expects projectID, not divID
function renderOrInitializeProject(projectID) {
	//if we already got the project's json object, we'll just re-render it form the projects array
	var flag = false;
	$.each(jsonResultProjects, function(index, value) {
	  	var backEndID = value.Response[0].id;
		if (backEndID == projectID) {
			projectSelected(projectID);
			loadProject(jsonResultProjects[index].Response[0]);
			flag = true;
			return false;
		}		
	});

	//if we don't already have the project's json object, we'll call the back end and load it
	if (flag == false) {
		projectSelected(projectID);
		initializeProject(projectID);
	}
}

//expects projectID, not divID
function initializeProject(projectID) {
	//get jsonResult for artifacts of particular project...represented by projectID
	$.ajax({
	   type:'get',
	   dataType: 'json',
	   data: {
	       id: projectID
	   },
	   url: saasLocation + "/Project/GetProject"
	}).done(function(data) {
	    var jsonResult = data;
		loadProject(jsonResult.Response[0]);
		jsonResultProjects.push(jsonResult);
	});
}

function loadProject(jsonProject) {
	$.each(jsonProject.artifacts, function(index, value) {
		var type = value.type;
		var artifactID = value.id;
		if (type == "document") {
			createDocument(artifactID);
			renderTemplate("templates/artifact_templates/documentTemplate.html", jsonProject.artifacts[index], "artifact_"+artifactID);
		}
		else if (type == "picture") {
			createPicture(artifactID);
			renderTemplate("templates/artifact_templates/pictureTemplate.html", jsonProject.artifacts[index], "artifact_"+artifactID);
		}
		else if (type == "video") {
			createVideo(artifactID);
			renderTemplate("templates/artifact_templates/videoTemplate.html", jsonProject.artifacts[index], "artifact_"+artifactID);
		}
		else if (type == "audio") {
			createAudio(artifactID);
			renderTemplate("templates/artifact_templates/audioTemplate.html", jsonProject.artifacts[index], "artifact_"+artifactID);
		}

		$("#leftcol-list .category.projects ul").children().children().first().addClass('active');
		addProjectWaypoint("artifact_"+artifactID);
	  	//$("#projects").append("<div class='rightColEntry'><div id='addResume'></div>");
		//$("#leftColProjects").append("<li id='leftColProject_"+value.id+"' class='category projects entry'>"+value.name+"</li>");

	});
	
	$("#projects").append("<div id='projectBottomBuffer' style='margin-top:-10px'></div>");
}

function addProjectWaypoint(divID) {
	$('#'+divID).waypoint(function(event, direction) {
		var artifactID = $(this).attr('id').substring(9);
		
		clearActiveArtifacts();
		$("#projectsHeading").addClass('active');
		$(this).find('.artifactTitle').addClass('active');
		$("#leftColProjectArtifact_"+artifactID).addClass('active');
	}, {
	   offset: '12%'
	})
}

function addEssentialsWaypoint(divID) {
	$('#'+divID).waypoint(function(event, direction) {
		
		clearActiveEssentials();
		$("#essentialsHeading").addClass('active');
		$(this).find('.artifactTitle').addClass('active');
		$("#leftCol"+$(this).attr('id')).addClass('active');
		
	}, {
	   offset: '12%'
	})
}

function updateProjectOrder(order) {
	$.ajax({
	   type:'get',
	   dataType: 'json',
	   data: {
	        id: userID,
			order: order
	   },
	   url: saasLocation + "/User/UpdateProjectOrder"
	}).done(function(data) {
		var jsonResult = data;
	    if (!jsonResult.Success) {
			alert("Something broke. Blame Brian.");
		}
	});
}

function createDocument(documentID) {
	$("#projects").append("<div class='rightColEntry'><div id='artifact_"+documentID+"'></div>");
	$("#artifact_"+documentID).addClass('expanded');
}

function createPicture(pictureID) {
	$("#projects").append("<div class='rightColEntry'><div id='artifact_"+pictureID+"'></div>");
	$("#artifact_"+pictureID).addClass('expanded');
}

function createVideo(videoID) {
	$("#projects").append("<div class='rightColEntry'><div id='artifact_"+videoID+"'></div>");
	$("#artifact_"+videoID).addClass('expanded');
}

function createAudio(audioID) {
	$("#projects").append("<div class='rightColEntry'><div id='artifact_"+audioID+"'></div>");
	$("#artifact_"+audioID).addClass('expanded');
}

/*function initializeArtifacts() {
	var jsonResult = {Description:"You can upload any file type.", 
						Document:"https://vestnstorage.blob.core.windows.net/pdfs/0bec8333-b0e1-4b17-b14c-ef4f7f073a9a.pdf", 
						Experiences:"i did this cool shit", 
						picture:"https://vestnstorage.blob.core.windows.net/thumbnails/8c42bb53-ff70-47c2-9877-b6440c4d8c2a.jpeg", 
						pictureDescription:"Vestn rager! alskdfjasldjfsVestn rager! alskdfjasldjfsVestn rager! alskdfjasldjfsVestn rager! alskdfjasldjfsVestn rager! alskdfjasldjfsVestn rager!",
						Video:"http://www.youtube.com/embed/Mtz4NOfbgv4", 
						VideoDescription:"Some bullshit"};
	renderTemplate("templates/artifact_templates/addArtifactTemplate.html", jsonResult, "addArtifact");
	renderTemplate("templates/artifact_templates/mediaProcessingTemplate.html", jsonResult, "mediaProcessing");
	
	renderTemplate("templates/artifact_templates/experienceTemplate.html", jsonResult, "experiences");
	
	
	renderTemplate("templates/artifact_templates/collectionTemplate.html", jsonResult, "collection");
	
}*/

function clearProjects() {
	$('#projects').children().each(function () {
	    $(this).remove();
	});
	//re-add projectsbuffer
	$("#projects").append("<div id='projectsBuffer'></div>");
	$("#projects").append("<div id='timeLine'></div>");
}









