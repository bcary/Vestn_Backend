/*
submitUploadForm({
    form: '#formId',
    progress: function (evt) { },
    complete: function (evt) { },
    error: function (evt) { },
    abort: function (evt) { }
});
*/

function submitUploadForm(parameters) {
    var xhr = new XMLHttpRequest();
    var form = $(parameters.form);
    var fd = new FormData();

    var hiddenFields = $('input[type="hidden"]', form);
    var fileFields = $('input[type="file"]', form);
    var otherFields = $('input[type!="hidden"][type!="file"]', form);

    $.each(hiddenFields, function (i, hiddenField) {
        fd.append(hiddenField.name, hiddenField.value);
    });

    $.each(fileFields, function (i, fileField) {
        for (var j in fileField.files) {
            if (j == 0) {
                fd.append(fileField.name, fileField.files[j]);
            } else {
                fd.append(fileField.name + '-' + j, fileField.files[j]);
            }

        }
    });

    $.each(otherFields, function (i, otherField) {
        fd.append(otherField.name, otherField.value);
    });

    xhr.upload.addEventListener("progress", parameters.progress, false);
    xhr.addEventListener("load", parameters.complete, false);
    xhr.addEventListener("error", parameters.error, false);
    xhr.addEventListener("abort", parameters.abort, false);

    xhr.open("POST", form.attr("action"));
    xhr.send(fd);
}