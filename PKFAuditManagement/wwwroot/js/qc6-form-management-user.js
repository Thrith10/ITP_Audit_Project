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