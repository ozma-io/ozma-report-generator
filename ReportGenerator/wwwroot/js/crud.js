function LoadList(objectName) {
    var loadListUrl = "/Admin/" + instanceName + "/Load" + objectName + "s";
    var tableId = "tableMainList" + objectName + "s";
    $.ajax({
        url: loadListUrl,
        method: "GET",
    }).done(function (partialViewResult) {
        $("#" + tableId).html(partialViewResult);
    });
}
function Add(objectName) {
    var addUrl = "/Admin/" + instanceName + "/Add" + objectName;
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
            LoadList(objectName);
            modalWindow.modal('hide');
            form.find("input[type=text], textarea").val("");
        }).fail(function (msg) {
            ShowErrorMessage('Ошибка: ' + msg.responseText);
        });
    }
}
function AddWithFile(objectName) {
    var addUrl = "/Admin/" + instanceName + "/Add" + objectName;
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
            ShowErrorMessage('Ошибка: ' + msg.responseText);
        });
    }
}
function Delete(objectName, id) {
    var deleteUrl = "/Admin/" + instanceName + "/Delete" + objectName;
    $.ajax({
        url: deleteUrl,
        method: "DELETE",
        data: {
            id: id
        }
    }).done(function () {
        LoadList(objectName);
    }).fail(function (msg) {
        ShowErrorMessage('Ошибка: ' + msg.responseText);
    });
}
$(document).ready(function () {
    LoadList("Scheme");
    LoadList("Template");
});