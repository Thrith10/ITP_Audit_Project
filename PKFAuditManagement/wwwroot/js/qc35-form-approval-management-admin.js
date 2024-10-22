// This function deletes the QC35 form selected from the Admin Dashboard QC35 Form Management page
function confirmDelete(deleteUrl) {
    Swal.fire({
        title: 'Are you sure?',
        text: 'You will not be able to recover the QC35 Form!',
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
                            text: 'The QC35 Form has been deleted.',
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

document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".approveLink").forEach(function (element) {
        element.addEventListener("click", function (event) {
            event.preventDefault();
            var engagementId = event.target.getAttribute("data-engagement-id");
            document.getElementById("approveFormId").value = engagementId;
            var form = document.getElementById("approveForm");
            form.action = '/QC35Form/ApproveQC35Form/' + engagementId;
            form.submit();
        });
    });
});

