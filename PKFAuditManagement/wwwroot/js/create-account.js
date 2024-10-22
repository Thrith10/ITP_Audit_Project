document.addEventListener("DOMContentLoaded", function () {
    var successMessage = '@TempData["SuccessMessage"]';
    var errorMessage = '@TempData["ErrorMessage"]';
});

function togglePassword(id) {
    var field = document.getElementById(id);
    var icon = field.nextElementSibling.querySelector('i');
    if (field.type === "password") {
        field.type = "text";
        icon.classList.remove('fa-eye');
        icon.classList.add('fa-eye-slash');
    } else {
        field.type = "password";
        icon.classList.remove('fa-eye-slash');
        icon.classList.add('fa-eye');
    }
}
