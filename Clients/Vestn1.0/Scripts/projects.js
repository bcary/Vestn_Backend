
function getIdFromProjectTab(projectTab) {
    return projectTab.attr("id").substring(8, projectTab.attr("id").length);
}

function loadProjectsPartial() {
    setTimeout("addInnerTabs()", 200);

    //TAB NAVIGATION
    $(".menu > li").live("click", function (e) {
        goToProject(getIdFromProjectTab($(e.target)));
    });


    //INNER TAB NAVIGATION
    //TAB NAVIGATION
    $(".innerMenu > li").live("click", function (e) {
            selectInnerTabIndex(e.target.id.substring(16));

            //if no project elements active, make the first element active
            selectFirstProjectElementIfNoneActive()
    });
}

$(document).ready(function () {

    loadProjectsPartial();

    $(".projectContentFrame .mediaSelectionPane li:not(.projectElementAddNewSelector)").live("click", function (e) {
        if (isProjectElementOnPage(e.target.id.substring(23))) {
            selectProjectElement(e.target.id.substring(23));
        }
    });

    $(".projectContentFrame .mediaSelectionPane li.projectElementAddNewSelector").live("click", function (e) {
        selectProjectElement("new");
    });

    //Widtherize All;
    $('.experience-jobTitle').widtherize({ 'width': 420, 'maxSize': 32 });
    $('.experience-company').widtherize({ 'width': 420, 'maxSize': 32 });
    $('.mediaInformation-company').widtherize({ 'width': 275, 'maxSize': 20 });
    $('#userAboutInformationPartial .aboutLeftColumn .aboutPictureWrapper .email').widtherize({ 'width': 185, 'maxSize': 20 });
    $('#userAboutInformationPartial .aboutLeftColumn .aboutPictureWrapper .title').widtherize({ 'width': 185, 'maxSize': 26 });
    $('#userAboutInformationPartial .aboutLeftColumn .aboutPictureWrapper .name').widtherize({ 'width': 185, 'maxSize': 26 });
    $("#userAboutHeader span").widtherize({ 'width': 145, 'maxSize': 15 });

    refreshViewState();
    bindProjectPageButtons();
    setUpPageButtons();

});

function refreshViewState() {
    var projectElementId = $.bbq.getState("peid");
    var project = $.bbq.getState("proj");
    var innerTab = $.bbq.getState("iti");

    if ((projectElementId != undefined) && (isProjectElementOnPage(projectElementId))) {
        if (projectElementId != "new") {
            goToProjectElement($.bbq.getState("peid"));
        }
        else {
            goToProject(project);
            selectInnerTabIndex(innerTab);
            goToProjectElement("new");
        }
    }
    else if ((project != undefined) && (isProjectOnPage(project))) {
        goToProject(project);

        if ((innerTab != undefined) && (isInnerTab(innerTab))) {
            
            selectInnerTabIndex(innerTab);
        }
    }

    selectFirstProjectElementIfNoneActive();

}

function saveViewState(type, id) {
    //save the state of the current project in view
    switch (type) {
        case "projectElement":
            if (isProjectElementOnPage(id) || id == "new" || id == null) {
                $.bbq.pushState({ peid: id });
            }
            break;
        case "project":
            if (isProjectOnPage(id) || id == null) {
                $.bbq.pushState({ proj: id });
            }
            break;
        case "innerTab":
            if (isInnerTab(id) || id == null) {
                $.bbq.pushState({ iti: id });
            }
            break;
        default:
            break;
    }
}

function isProjectOnPage(projectId) {
    if ($("#projectTabs .menu #project_" + projectId).size() == 1) {
        return true;
    }
    else {
        return false;
    }
}
function isProjectElementOnPage(projectElementId) {
    if ($("#projectElementMedia_" + projectElementId).size() == 1) {
        return true;
    }
    else if (projectElementId == "new") {
        return true;
    }
    else {
        return false;
    }
}
function isInnerTab(innerTab) {

    switch (innerTab) {
        case "user-Information": return true; break;
        case "user-Experience": return true; break;
        case "project-Documents": return true; break;
        case "project-Pictures": return true; break;
        case "project-Videos": return true; break;
        case "project-Audio": return true; break;
        case "project-Information": return true; break;


        case "createProjectButton": return true; break;
        default: return false; break;
    }

    return false;

}

function getActiveProject() {
    $projectTab = $("#projectTabs .menu li.active");
    if ($projectTab.attr("id") != undefined) {
        return $("#projectTabs .menu li.active").attr("id").substring(8);
    }
    else return -1;
}

function getActiveInnerTab() {
    if ($("#projectContent_" + getActiveProject() + " .projectContentFrame.active").size() == 1) {
        return $("#projectContent_" + getActiveProject() + " .projectContentFrame.active").attr("class").split(" ")[1];
    }
    else {
        return null;
    }
}

function getActiveProjectElement() {
    if ($("#projectContent_" + getActiveProject() + " .projectContentFrame.active .media.active").size() == 1) {
        if ($("#projectContent_" + getActiveProject() + " .projectContentFrame.active .media.active").attr("class").split(" ")[1] == "pe_addNew") {
            return "pe_addNew";
        }
        else if ($("#projectContent_" + getActiveProject() + " .projectContentFrame.active .media.active").attr("id") != null) {
            return $("#projectContent_" + getActiveProject() + " .projectContentFrame.active .media.active").attr("id").substring(20)
        }
        return null;
    }
    else {
        return null;
    }
}

function changeActiveMedia(projectElementId) {

    //remove current active media
    $("#projectContent_" + getActiveProject() + " .projectContentFrame.active .media.active").css("display", "none").removeClass("active");


    //add new active media
    if (projectElementId != "new") {
        $("#projectElementMedia_" + projectElementId).css("display", "block").addClass("active");
    }
    else {
        $("#projectContent_" + getActiveProject() + " .projectContentFrame.active .media.pe_addNew").css("display", "block").addClass("active");
    }

    saveViewState("projectElement", projectElementId);
}

function changeActiveMediaTab(projectElementId) {

    //remove current active media
    $("#projectContent_" + getActiveProject() + " .projectContentFrame.active .mediaSelector.active").removeClass("active");
    $("#projectContent_" + getActiveProject() + " .projectContentFrame.active .mediaSelector.active").children(".mediaActiveIndicator").css("display", "none");

    //add new active media
    if (projectElementId != "new") {
        $("#projectElementSelector_" + projectElementId).addClass("active");
        $("#projectElementSelector_" + projectElementId).children(".mediaActiveIndicator").css("display", "block");
    }
    else {
        $("#projectContent_" + getActiveProject() + " .projectContentFrame.active .projectElementAddNewSelector").addClass("active");
        $("#projectContent_" + getActiveProject() + " .projectContentFrame.active .projectElementAddNewSelector").children(".mediaActiveIndicator").css("display", "block");
    }
}

function selectProjectElement(projectElementId) {

    //temporary fix-> this will remove the current html of the iframe and replace it with itself
    //we're doing this because the iframe renderes with an incorrect height the first time it loads
    if ($("projectElementMedia_" + projectElementId + " .documentViewer").size() == 1) {
        //$("#projectElementMedia_" + projectElementId + " .documentViewer").html($("#projectElementMedia_" + projectElementId + " iframe"));
    }


    changeActiveMedia(projectElementId);
    changeActiveMediaTab(projectElementId);
}

/*
function selectAddProjectElement() {
    changeActiveMedia($("#projectContent_" + getActiveProject() + " .projectContentFrame.active .pe_addNew"));
    changeActiveMediaTab($("#projectContent_" + getActiveProject() + " .projectContentFrame.active .projectElementAddNewSelector"));
}
*/
/*
function reloadAllDocuments(){

    $(".pe_doc .mediaViewer").each(function () {
        var htmlFromIframe = $(this).children("iframe");
        $(this).html(htmlFromIframe);
        //$(this).html($(this).children("iframe").html());
    });
}*/

function isProjectTabActive(projectId) {
    if ($("#project_" + projectId).hasClass("active")) {
        return true;
    }
    else {
        return false;
    }
}

/*
function getInnerTabFromProject(projectId) {
    //if no prjoect Id, just return active inner project tab of active project
    if (projectId == undefined) {
        projectId = getActiveProject();
    }

    if ($("#projectContent_" + projectId + " .projectContentFrame.active").size() == 1) {
        return $("#projectContent_" + projectId + " .projectContentFrame.active").attr("class").split(" ")[1];
    }
    else {
        return null;
    }
}*/

function getProjectIdFromProjectElement(projectElementId) {
    //if no element on page with projectElementId
    if (!isProjectElementOnPage(projectElementId)) {
        return null;
    }

    return $("#projectElementMedia_" + projectElementId).parent().parent().parent().attr("value");
}

function getInnerProjectTabFromProjectElement(projectElementId) {
    //if no element on page with projectElementId
    if (!isProjectElementOnPage(projectElementId)) {
        return null;
    }

    switch ($("#projectElementMedia_" + projectElementId).attr("class").split(" ")[1]) {

        case "pe_doc": return "project-Documents";
            break;
        case "pe_pic": return "project-Pictures";
            break;
        case "pe_vid": return "project-Videos";
            break;
        case "pe_audio": return "project-Audio";
            break;
        case "pe_exp": return "user-Experience";
            break;

        default: return null;
            break;
    }
}

function bindProjectPageButtons() {
    $("#movePageLeftButton").click(function () {
        pageLeftProjects();
        setUpPageButtons();
    });
    $("#movePageRightButton").click(function () {
        pageRightProjects();
        setUpPageButtons();
    });
}

function goToProject(newProjectId) {
    //if no project on page with newProjectId, go to About Project
    if (!isProjectOnPage(newProjectId)) {
        newProjectId = $("#projectTabs .menu li").first().attr("id").substring(8);
    }


    var oldProjectId = getActiveProject();

    //go to new project page -- this MUST be called before we change active project
    goToProjectPage(getPageOfProject(newProjectId));

    if (newProjectId != oldProjectId) {

        $("#project_" + oldProjectId).removeClass("active");
        $("#projectContent_" + oldProjectId).css("display", "none");
        $("#project_" + newProjectId).addClass("active");
        $("#projectContent_" + newProjectId).css("display", "block");

        //Set up new inner tabs
        setUpInnerTabs();

        saveViewState("project", newProjectId);
        saveViewState("innerTab", getActiveInnerTab());
        saveViewState("projectElement", getActiveProjectElement());
    }

    //set up project paging buttons
    setUpPageButtons();

}

function goToProjectElement(projectElementId) {

    //if no element on page with projectElementId || if no element ID is provided
    //continue and set projectElementId to first element on page. if first element can't be found, don't do anything.
    if (projectElementId == undefined ||
    projectElementId == null ||
    !isProjectElementOnPage(projectElementId)
    ) {
        if ((projectElementId = getFirstProjectElementIdFromCurrentFrame()) == null) {
            return;
        }
    }

    //deal with projectElement="new" (which is considered to be isProjectElementOnPage->TRUE)
    if (projectElementId == "new") {
        selectProjectElement("new");
        return;
    }

    //projectElementId = projectElementId;
    var projectElementInnerTab = getInnerProjectTabFromProjectElement(projectElementId);
    var projectId = getProjectIdFromProjectElement(projectElementId);


    //if switching to projectElement in same project
    if(projectId != getActiveProject())
    {
        goToProject(projectId);
    }

    if(projectElementInnerTab != getActiveInnerTab())
    {
        selectInnerTabIndex(projectElementInnerTab);
    }

    if(projectElementId != getActiveProjectElement())
    {
        selectProjectElement(projectElementId);
    }
}

function selectFirstProjectElementIfNoneActive() {
    //if there is a project with content active on the page with id = projectId
    if ($("#projectContent_" + getActiveProject() + " .projectContentFrame.active").size() == 1) {
        if ($("#projectContent_" + getActiveProject() + " .projectContentFrame.active .media.active").size() == 0) {

            var firstMediaId = $("#projectContent_" + getActiveProject() + " .projectContentFrame.active .media").first().attr("id")
            var projectElementId;
            //if existing project element
            if(firstMediaId != null){
                projectElementId = firstMediaId.substring(20);
            }

            else {//if new project element
                projectElementId = "new";
            }
            
            goToProjectElement(projectElementId);
        }
    }
}



//////////////INNER TABS SELECTION

function selectInnerTabIndex(selection) {

    //remove active tab and content
    $(".innerMenu >li.active").removeClass("active");
    $("#projectContent_" + getActiveProject() + " .projectContentFrame").removeClass("active");

    //add active class immediately then fade in new tab and content
    $("#projectContent_" + getActiveProject() + " ." + selection).addClass("active").fadeIn();
    $("#innerProjectTab-" + selection).addClass("active");

    //adjust arrow on innerMenu
    var innerTabLocationOffsetForArrow = -4 + $("#innerProjectTab-" + selection).offset().top - 176;

    $("#projectInnerTabArrowStart").animate({
        top: innerTabLocationOffsetForArrow
    }, 250);
    $("#projectInnerTabArrowEnd").animate({
        top: innerTabLocationOffsetForArrow
    }, 250);

    if (getActiveInnerTab() != "project-Information") {
        $("#innerProjectTabDelete").fadeOut();
    }
    else {
        $("#innerProjectTabDelete").fadeIn();
    }

    saveViewState("innerTab", selection);
    saveViewState("projectElement", getActiveProjectElement());

}

function setUpInnerTabs() {
    //determine based on the project which projects have which tabs visible.  this way i can have a tab index of 1-10 but only 1,2,3,7,9 are visible (and clickable and therefore only this content is viewable)
    //!!another way to do this is to set up the names of the tabs and ignore the paring of content (how its set up to do this currently by naming the content sections project1Content-[index] instead of project1Content-["title" i.e. Information]
    removeExistingTabs();

    //add new tabs back in after old tabs removed
    addInnerTabs();
}

function removeExistingTabs() {
    $(".innerMenu >li").css("display", "none");
}

function addInnerTab(innerTabId) {
//if($("#" + innerTabId).
    $("#" + innerTabId).css("display", "block").animate({
        opacity: 1
    }, 150);
}

function addInnerTabs() {

    $("#projectContent_" + getActiveProject() + " .projectContentFrame").each(function () {

        //split gives us the second class which we need to have to know which type of element collection it is
        if ($(this).attr("class") != null) {

            innerTabName = $(this).attr("class").split(' ')[1];

            addInnerTab("innerProjectTab-" + innerTabName);
        }
    });

    //active tab associated with active projectContent
    selectInnerTabIndex($("#projectContent_" + getActiveProject() + " .projectContentFrame.active").attr("class").split(' ')[1]);

    

    //disable delete button if about project
    if ($("#project_" + getActiveProject()).hasClass("aboutProjectTab") ||
        getActiveInnerTab() != "project-Information") {
        $("#innerProjectTabDelete").fadeOut();
    }
    else {
        $("#innerProjectTabDelete").fadeIn();
    }
}


///////////////END INNER TABS SELECTION




//***PAGING SECTION***///
function getIdFromProjectTabIndex(index) {
    return $("#projectTabs .menu li")[index].id.substring(8);
}

//make sure to call this when the old project is still active!
function getCurrentPage() {
    return getPageOfProject(getActiveProject());
}

function setUpPageButtons() {
    var currentPage = getCurrentPage();    
    if (($("#projectTabs .menu li").size() / 5) > 1) {
        $("#movePageRightButton").css("display", "block");
    }
    if (currentPage > 1) {
        $("#movePageLeftButton").css("display", "block");
    }
    //$("#movePageLeftButton").css("display", "block");
    //$("#movePageRightButton").css("display", "block");

    if (currentPage <= 1) {
        $("#movePageLeftButton").css("display", "none");
        if (($("#projectTabs .menu li").size() / 5) <= 1) {
            $("#movePageRightButton").css("display", "none");
        }
    }
    else if (currentPage >= ($("#projectTabs .menu li").size() / 5)) {
        $("#movePageRightButton").css("display", "none");
    }
}

//return the page (1-number of pages) of project tab for project with id = projectId
function getPageOfProject(projectId) {
    $projectTab = $("#project_" + projectId);

    var indexOfProjectTab = $projectTab.index()

    if (indexOfProjectTab == undefined) {
        return 1;
    }

    return parseInt(indexOfProjectTab / 5) + 1;

}

//go to project page (1 - number of pages)
function goToProjectPage(pageNumber) {

    var newPageLeftOffset = 0;

    newPageLeftOffset = (-675 * (pageNumber - 1));

    $("#projectTabs .menu").css("left", newPageLeftOffset + "px");
}

function pageLeftProjects() {
    var currentPage = getCurrentPage();

    if (currentPage > 1) {
        var goToProjectAtIndex = (currentPage - 1) * 5 - 1;

        var newProjectId = getIdFromProjectTabIndex(goToProjectAtIndex);

        goToProject(newProjectId);
    }
}

function pageRightProjects() {
    var currentPage = getCurrentPage();

    var goToProjectAtIndex = (currentPage) * 5;
    var newProjectId = getIdFromProjectTabIndex(goToProjectAtIndex);

    goToProject(newProjectId);
}

//**END PAGING SECTION**//
