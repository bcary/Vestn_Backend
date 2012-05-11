var saasLocation;
$(function(){
	saasLocation = "http://localhost:8011";
	//saasLocation = "http://334133ec016d4caba36ad6f29a3c54d3.cloudapp.net";
});


//Parameters: ID of Div to render to, Name of HTML File, Data
function renderTemplate(renderFromHtml, data, renderToHtml){
  $.get(renderFromHtml, function(renderFromHtml_html){
    $("#" + renderToHtml).html($.templates(renderFromHtml_html).render(data));
  });
}

//http://papermashup.com/read-url-get-variables-withjavascript/
function getUrlVars() {
    var vars = {};
    var parts = window.location.href.replace(/[?&]+([^=&]+)=([^&]*)/gi, function(m,key,value) {
        vars[key] = value;
    });
    return vars;
}

