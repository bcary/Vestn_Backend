$(function(){
	populateInnerCircle();
	
	populateOuterCircle();
	
	bindPersonHoverActions();
	
	$("#about-Everyone-Message").show();
});

function bindPersonHoverActions(){
	$(".about-circle ul li").hover(function(){
		showInnerMessage($(this).attr("id").substring(6));
	},function(){
		hideInnerMessage($(this).attr("id").substring(6));
	});
}

function showInnerMessage(person){
	$("#about-Everyone-Message").hide();
	$("#about-" + person + "-Message").show();
}

function hideInnerMessage(person){
	$("#about-" + person + "-Message").hide();
	$("#about-Everyone-Message").show();
}

function populateInnerCircle(){
	var people = [
		{Name: "Conner", Position: "0"},
		{Name: "Brian", Position: "245"},
		{Name: "Phil", Position: "135"},
	];

	  var circleCenterX = 130;//165/2 - 35 = 130
	  var circleCenterY = 130;
	  var radius = 165;
	  for(var i = 0; i<people.length; i++)
	  {
	    var person = people[i];
	    var left = getLeftFromAngle(circleCenterX, person["Position"], radius);
	    var top = getTopFromAngle(circleCenterY, person["Position"], radius);
		$("#about-" + person["Name"]).css("left", left + "px");
		$("#about-" + person["Name"]).css("top", top + "px");  
	  }
}

function populateOuterCircle(){
	var people = [
		{Name: "Future5", Position: "120"},
		{Name: "Future6", Position: "155"},
		/*{Name: "Kyle", Position: "120"},
		{Name: "Jake", Position: "155"},*/
		{Name: "Steven", Position: "10"},
		{Name: "Future1", Position: "345"},
		{Name: "Future2", Position: "280"},
		{Name: "Future3", Position: "250"},
		{Name: "Future4", Position: "90"}
	];
	
	  var circleCenterX = 235;//middle=540/2=270   -   width of object / 2 = 70 / 2 = 35  === 235
	  var circleCenterY = 235;
	  var radius = 270;
	  for(var i = 0; i<people.length; i++)
	  {
	    var person = people[i];
	    var left = getLeftFromAngle(circleCenterX, person["Position"], radius);
	    var top = getTopFromAngle(circleCenterY, person["Position"], radius);
		$("#about-" + person["Name"]).css("left", left + "px");
		$("#about-" + person["Name"]).css("top", top + "px");  
	  }
}

function getLeftFromAngle(offset, angle, radius){
	console.log(offset + ", " + angle);
	return Math.round(offset+radius*Math.cos((angle-90)/180*Math.PI));
}

function getTopFromAngle(offset, angle, radius){
	return Math.round(offset+radius*Math.sin((angle-90)/180*Math.PI));
}

