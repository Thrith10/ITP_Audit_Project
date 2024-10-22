let selectedUserId = null;

// Function to show Swal.fire modal and set the selected user ID
function showSuspendModal(userId) {
    selectedUserId = userId;

    // Show Swal.fire modal
    Swal.fire({
        title: 'Are you sure?',
        text: "Do you really want to suspend this user?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Yes, suspend them!',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            confirmSuspendUser();
        }
    });
}

// Function to confirm and suspend the user
function confirmSuspendUser() {
    if (!selectedUserId) return;

    $.ajax({
        url: '@Url.Action("SuspendUser", "Admin")',
        type: 'POST',
        data: {
            userId: selectedUserId,
            __RequestVerificationToken: $('input[name=__RequestVerificationToken]').val()
        },
        success: function (data) {
            if (data.success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Success',
                    text: data.message,
                    confirmButtonText: 'OK'
                }).then(() => {
                    location.reload(); // Reload the page after successful suspension
                });
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: data.message,
                    confirmButtonText: 'OK'
                });
            }
        },
        error: function (xhr, status, error) {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'An error occurred while processing your request. Please try again.',
                confirmButtonText: 'OK'
            });
            console.error(error);
        }
    });
}

function showActivateModal(userId) {
    Swal.fire({
        title: 'Are you sure?',
        text: "Do you really want to activate this user?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#28a745',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Yes, activate them!',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            activateUser(userId);
        }
    });
}

function activateUser(userId) {
    $.ajax({
        url: '@Url.Action("ActivateUser", "Admin")',
        type: 'POST',
        data: {
            userId: userId,
            __RequestVerificationToken: $('input[name=__RequestVerificationToken]').val()
        },
        success: function (data) {
            if (data.success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Success',
                    text: data.message,
                    confirmButtonText: 'OK'
                }).then(() => {
                    location.reload(); // Reload the page after successful activation
                });
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: data.message,
                    confirmButtonText: 'OK'
                });
            }
        },
        error: function (xhr, status, error) {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'An error occurred while processing your request. Please try again.',
                confirmButtonText: 'OK'
            });
            console.error(error);
        }
    });
}
