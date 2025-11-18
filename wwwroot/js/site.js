// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
window.togglePasswordVisibility = function(inputId, visible) {
    const input = document.getElementById(inputId) || document.querySelector('input[name="Usuario.Contrasenia"]');
    if (input) {
        input.setAttribute('type', visible ? 'text' : 'password');
    }
};
