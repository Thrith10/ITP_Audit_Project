// This function deletes the QC6 form selected from the User Dashboard QC6 Form Management page
function confirmDelete(deleteUrl) {
    Swal.fire({
        title: 'Are you sure?',
        text: 'You will not be able to recover the QC6 Form!',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Yes, delete it!',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            // Send a DELETE request to the server
            fetch(deleteUrl, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json'
                },
            })
                .then(response => {
                    if (response.ok) {
                        // Show success message
                        Swal.fire({
                            icon: 'success',
                            title: 'Deleted!',
                            text: 'The QC6 Form has been deleted.',
                        }).then(() => {
                            // Refresh
                            location.reload();
                        });
                    } else {
                        // Handle the error response
                        throw new Error('Failed to delete item');
                    }
                })
                .catch(error => {
                    // Handle any errors
                    console.error('Error:', error);
                    Swal.fire({
                        icon: 'error',
                        title: 'Oops...',
                        text: 'Failed to delete item',
                    });
                });
        }
    });
}

const notyf = new Notyf({
    position: { x: 'center', y: 'top' }
});

// Read the data-message attribute value for QC6FormUpdateToastMessage
var QC6FormUpdateToastMessage = document.getElementById("QC6FormUpdateToastMessage").getAttribute("data-message");

// Read the data-message attribute value for ToastMessage
var toastMessage = document.getElementById("toastMessage").getAttribute("data-message");

if (QC6FormUpdateToastMessage) {
    // Display the QC6FormUpdateToastMessage
    notyf.success(QC6FormUpdateToastMessage);
} else if (toastMessage) {
    // Display the toastMessage
    notyf.success(toastMessage);
}
