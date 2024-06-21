
$(document).ready(function () {
    toggleRiskLevel();
    toggleSectionBResult();
    toggleSignificantRisk();
    toggleSTR();
});

document.addEventListener("DOMContentLoaded", function () {
    var approveButton = document.getElementById("approveButton");
    var rejectButton = document.getElementById("rejectButton");

    approveButton.addEventListener("click", function (event) {
        event.preventDefault(); // Prevent form submission
        approveQC7Form(approveButton.value);
    });

    rejectButton.addEventListener("click", function (event) {
        event.preventDefault(); // Prevent form submission
        rejectQC7Form(rejectButton.value);
    });
});


// Function to display comment risk level field based on checkbox value
function toggleRiskLevel() {
    var pieCheckbox = $('#pieCheckbox');
    var riskLevelRow = $('#riskLevelRow');

    if (pieCheckbox.is(':checked')) {
        riskLevelRow.show();
    } else {
        riskLevelRow.hide();
    }
}

// Function to display comment for significant risk field based on checkbox value
function toggleSignificantRisk() {
    var significantRiskCheckbox = $('#significantRiskCheckbox');
    var significantRiskRow = $('#significantRiskRow');

    if (significantRiskCheckbox.is(':checked')) {
        significantRiskRow.show();
    } else {
        significantRiskRow.hide();
    }
}

// Function to display comment for suspicious transaction report field based on checkbox value
function toggleSTR() {
    var strCheckbox = $('#strCheckbox');
    var rationaleSTRRow = $('#rationaleSTRRow');

    if (strCheckbox.is(':checked')) {
        rationaleSTRRow.show();
    } else {
        rationaleSTRRow.hide();
    }
}


// This function approves the QC7 form selected from the Admin Dashboard QC7 Form Management page
function approveQC7Form(qc7FormId) {
    Swal.fire({
        title: 'Are you sure?',
        text: 'Do you want to approve this QC7 Form?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, approve it!',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            // Send a POST request to the server
            fetch("/QC7Form/ApproveQC7Form/" + qc7FormId, {
                method: 'POST',
            })
                .then(response => {
                    if (response.ok) {
                        // Show success message
                        Swal.fire({
                            icon: 'success',
                            title: 'Approved!',
                            text: 'The QC7 Form has been approved.',
                        }).then(() => {
                            // Redirect to main page
                            window.location.href = '/QC7Form/QC7FormApprovalManagement';
                        });
                    } else {
                        // Handle the error response
                        throw new Error('Failed to approve item');
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    Swal.fire({
                        icon: 'error',
                        title: 'Oops...',
                        text: 'Failed to approve item',
                    });
                });
        }
    });
}

// This function rejects the QC7 form selected from the Admin Dashboard QC7 Form Management page
function rejectQC7Form(qc7FormId) {
    Swal.fire({
        title: 'Are you sure?',
        text: 'Do you want to reject this QC7 Form?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, reject it!',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            // Prompt user for rejection reason
            Swal.fire({
                title: 'Rejection Reason',
                input: 'textarea',
                inputLabel: 'Enter your reason for rejection',
                inputPlaceholder: 'Enter reason here...',
                inputAttributes: {
                    autocapitalize: 'off',
                    rows: 4 // Number of visible rows
                },
                showCancelButton: true,
                confirmButtonText: 'Reject',
                cancelButtonText: 'Cancel',
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                showLoaderOnConfirm: true,
                preConfirm: (rejectionReason) => {
                    if (!rejectionReason) {
                        Swal.showValidationMessage('Rejection reason is required');
                    }
                    return rejectionReason;
                },
                allowOutsideClick: () => !Swal.isLoading()
            }).then((result) => {
                if (result.isConfirmed) {
                    // Send a POST request to the server with the rejection reason
                    fetch("/QC7Form/RejectQC7Form/" + qc7FormId, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({ QC7FormID: qc7FormId, RejectionReason: result.value })
                    })
                        .then(response => {
                            if (response.ok) {
                                // Show success message
                                Swal.fire({
                                    icon: 'success',
                                    title: 'Rejected!',
                                    text: 'The QC7 Form has been Rejected.',
                                }).then(() => {
                                    // Redirect to main page
                                    window.location.href = '/QC7Form/QC7FormApprovalManagement';
                                });
                            } else {
                                // Handle the error response
                                throw new Error('Failed to reject item');
                            }
                        })
                        .catch(error => {
                            console.error('Error:', error);
                            Swal.fire({
                                icon: 'error',
                                title: 'Oops...',
                                text: 'Failed to reject item',
                            });
                        });
                }
            });
        }
    });
}
