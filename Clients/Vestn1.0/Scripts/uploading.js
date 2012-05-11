var int = self.setInterval("checkfile()", 5000);

function clock() {
    var d = new Date();
    var t = d.toLocaleTimeString();
    alert(t);
}

function checkfile() {

    $.ajax({
        type: "POST",
        data: { "url": "http://maxcdn.crazyleafdesign.com/blog/wp-content/uploads/2008/09/cool-wallpapers-for-designers-29.jpg"
        },
        url: "/User/CheckFileExist",
        dataType: "json",
        success: function (data) {
            if (data["status"] == 200) {
                alert("file exist");
                return;
            }
            else {
                alert("file not exist");
            }
        },
        error: function () {

        }
    });
}

$(document).ready(function () {

    $("#btnCheck").click(function () {

        checkfile();

    });
});
