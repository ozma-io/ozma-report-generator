// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function ShowMessage(messageText) {
    $('#alert-modal-body').html(messageText);
    $('#alert-modal-body').css("color", "black");
    $('#alert-modal').modal('show');
}
function ShowErrorMessage(messageText) {
    $('#alert-modal-body').html(messageText);
    $('#alert-modal-body').css("color", "red");
    $('#alert-modal').modal('show');
}
function CheckCharacters(event) {
    if (event.which == 32) {
        event.preventDefault();
        return false;
    }
}