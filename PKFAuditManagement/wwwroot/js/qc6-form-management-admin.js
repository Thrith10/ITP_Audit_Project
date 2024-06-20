// This function deletes the QC6 form selected from the Admin Dashboard QC6 Form Management page
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

// This function changes the status to "Approved" for the selected QC6 Form
document.getElementById('engagementTable').addEventListener('click', function (e) {
    if (e.target && e.target.matches('.approveLink')) {
        e.preventDefault();

        var engagementId = e.target.getAttribute('data-engagement-id');

        fetch('/QC6Form/ApproveQC6Form/' + engagementId, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                console.log('Approved successfully');
                window.location.reload();
            })
            .catch(error => {
                console.error('Error:', error);
            });
    }
});

const notyf = new Notyf({
    position: { x: 'center', y: 'top' }
});

// Read the data-message attribute value
var toastMessage = document.getElementById("toastMessage").getAttribute("data-message");

if (toastMessage) {
    // Display the toast message
    notyf.success(toastMessage);
}

// Read the data-message attribute value for approval/rejection of form
var approvalToastMessage = document.getElementById("approvalToastMessage").getAttribute("data-message");

if (approvalToastMessage) {
    // Display the toast message
    if (toastType === "success") {
        // Display success toast
        notyf.success(approvalToastMessage);
    } else if (toastType === "error") {
        // Display error toast
        notyf.error(approvalToastMessage);
    }
}

// Read the data-message attribute value
var QC6FormUpdateToastMessage = document.getElementById("QC6FormUpdateToastMessage").getAttribute("data-message");

if (QC6FormUpdateToastMessage) {
    // Display the toast message
    notyf.success(QC6FormUpdateToastMessage);
}