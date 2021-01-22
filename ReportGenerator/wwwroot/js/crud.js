function LoadList() {
    $.ajax({
        url: loadListUrl,
        method: "GET",
    }).done(function (partialViewResult) {
        $("#tableMainList").html(partialViewResult);
    });
}
function Add() {
    var form = $("#addForm");
    form.validate();
    if (form.valid()) {
        var data = form.serialize();
        $.ajax({
            url: addUrl,
            method: "POST",
            contentType: "application/x-www-form-urlencoded; charset=UTF-8",
            data: data
        }).done(function () {
            LoadList();
            $('#addModal').modal('hide');
            form.find("input[type=text], textarea").val("");
        }).fail(function (msg) {
            ShowErrorMessage('Ошибка: ' + msg.responseText);
        });
    }
}
function AddWithFile() {
    var form = $("#addForm");
    form.validate();
    if (form.valid()) {
        var formData = new FormData(document.getElementById("addForm"));
        $.ajax({
            url: addUrl,
            method: "POST",
            processData: false,
            contentType: false,
            data: formData
        }).done(function () {
            LoadList();
            $('#addModal').modal('hide');
            form.find("input[type=text], input[type=file], textarea").val("");
        }).fail(function (msg) {
            ShowErrorMessage('Ошибка: ' + msg.responseText);
        });
    }
}
function Delete(id) {
    $.ajax({
        url: deleteUrl,
        method: "DELETE",
        data: {
            id: id
        }
    }).done(function () {
        LoadList();
    }).fail(function (msg) {
        ShowErrorMessage('Ошибка: ' + msg.responseText);
    });
}
$(document).ready(function () {
    LoadList();
});