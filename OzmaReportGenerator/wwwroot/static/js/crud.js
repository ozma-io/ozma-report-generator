function LoadList(objectName) {
    var loadListUrl = "./Load" + objectName + "s";
    var tableId = "tableMainList" + objectName + "s";
    $.ajax({
        url: loadListUrl,
        method: "GET",
    }).done(function (partialViewResult) {
        $("#" + tableId).html(partialViewResult);
    }).fail(function (msg) {
        if (msg.responseText == 'relog') {
            document.location.reload();
        } else {
            ShowErrorMessage('Ошибка: ' + msg.responseText);
        }
    });
}
function Add(objectName) {
    var addUrl = "./Add" + objectName;
    var modalWindow = $("#add" + objectName + "Modal");
    var form = $("#add" + objectName + "Form");
    form.validate();
    if (form.valid()) {
        var data = form.serialize();
        $.ajax({
            url: addUrl,
            method: "POST",
            contentType: "application/x-www-form-urlencoded; charset=UTF-8",
            data: data
        }).done(function () {
            if (objectName == "Schema") {
                LoadSchemaNamesList();
            }
            LoadList(objectName);
            modalWindow.modal('hide');
            form.find("input[type=text], textarea").val("");
        }).fail(function (msg) {
            if (msg.responseText == 'relog') {
                document.location.reload();
            } else {
                ShowErrorMessage('Ошибка: ' + msg.responseText);
            }
        });
    }
}
function AddWithFile(objectName) {
    var addUrl = "./Add" + objectName;
    var modalWindow = $("#add" + objectName + "Modal");
    var form = $("#add" + objectName + "Form");
    form.validate();
    if (form.valid()) {
        var formData = new FormData(document.getElementById("add" + objectName + "Form"));
        $.ajax({
            url: addUrl,
            method: "POST",
            processData: false,
            contentType: false,
            data: formData
        }).done(function () {
            LoadList(objectName);
            modalWindow.modal('hide');
            form.find("input[type=text], input[type=file], textarea").val("");
        }).fail(function (msg) {
            if (msg.responseText == 'relog') {
                document.location.reload();
            } else {
                ShowErrorMessage('Ошибка: ' + msg.responseText);
            }
        });
    }
}
function Delete(objectName, id) {
    var deleteUrl = "./Delete" + objectName;
    $.ajax({
        url: deleteUrl,
        method: "DELETE",
        data: {
            id: id
        }
    }).done(function () {
        if (objectName == "Schema") {
            LoadSchemaNamesList();
        }
        LoadList(objectName);
    }).fail(function (msg) {
        if (msg.responseText == 'relog') {
            document.location.reload();
        } else {
            ShowErrorMessage('Ошибка: ' + msg.responseText);
        }
    });
}
function LoadSchemaNamesList() {
    var url = "./GetSchemaNamesList";
    $.ajax({
        url: url,
        method: "GET",
        dataType: 'json',
    }).done(function (response) {
        $('#SchemaId').html("");
        var options = '';
        for (var i = 0; i < response.length; i++) {
            options += '<option value="' + response[i].value + '">' + response[i].text + '</option>';
        }
        $('#SchemaId').append(options);  
    }).fail(function (msg) {
        if (msg == 'relog') {
            document.location.reload();
        } else {
            ShowErrorMessage('Ошибка: ' + msg.responseText);
        }
    });
}
$(document).ready(function () {
    LoadList("Schema");
    LoadList("Template");
    LoadSchemaNamesList();
});
