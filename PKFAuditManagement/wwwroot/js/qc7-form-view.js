
$(document).ready(function () {
    toggleRiskLevel();
    toggleSignificantRisk();
    toggleSTR();
    toggleConSTR();
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
    var significantRiskCheckbox = $('#risksAssociatedCheckbox');
    var significantRiskRow = $('#risksAssociatedRow');

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

// Function to display comment for suspicious transaction report field in conclusion based on checkbox value
function toggleConSTR() {
    var strCheckbox = $('#conStrCheckbox');
    var rationaleSTRRow = $('#conRationaleSTRRow');

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
            $.ajax({
                url: '/QC7Form/ApproveQC7Form/' + qc7FormId,
                type: 'POST',
                success: function (response) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Approved!',
                        text: 'The QC7 Form has been approved.',
                    }).then(() => {
                        window.location.href = '/QC7Form/QC7FormApprovalManagement';
                    });
                },
                error: function (xhr, status, error) {
                    if (xhr.status === 403) {
                        Swal.fire({
                            icon: 'error',
                            title: 'Forbidden',
                            text: 'The request was invalid. Please check that you are authorised to approve this form and try again.',
                        });
                    } else {
                        console.error('Error:', error);
                        Swal.fire({
                            icon: 'error',
                            title: 'Oops...',
                            text: 'Failed to approve item. Please try again later.',
                        });
                    }
                }
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
                    $.ajax({
                        url: '/QC7Form/RejectQC7Form/' + qc7FormId,
                        type: 'POST',
                        contentType: 'application/json',
                        data: JSON.stringify({ QC7FormID: qc7FormId, RejectionReason: result.value }),
                        success: function (response) {
                            Swal.fire({
                                icon: 'success',
                                title: 'Rejected!',
                                text: 'The QC7 Form has been rejected.',
                            }).then(() => {
                                window.location.href = '/QC7Form/QC7FormApprovalManagement';
                            });
                        },
                        error: function (xhr) {
                            if (xhr.status === 403) {
                                Swal.fire({
                                    icon: 'error',
                                    title: 'Forbidden',
                                    text: 'The request was invalid. Please check that you are authorised to reject this form and try again.',
                                });
                            } else {
                                console.error('Error:', xhr.responseText);
                                Swal.fire({
                                    icon: 'error',
                                    title: 'Oops...',
                                    text: 'Failed to reject item. Please try again later.',
                                });
                            }
                        }
                    });
                }
            });
        }
    });
}