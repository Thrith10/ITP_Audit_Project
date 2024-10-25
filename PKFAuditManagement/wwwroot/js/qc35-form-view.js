
document.addEventListener("DOMContentLoaded", function () {
    // DOM elements
    const dropArea = document.getElementById("drop-area");
    const inputFile = document.getElementById("input-file");
    const imageView = document.getElementById("img-view");
    const uploadIcon = document.getElementById("upload-icon");
    const placeholderText = document.getElementById("placeholder-text");
    const uploadedImage = document.getElementById("uploaded-image");
    const cancelUploadButton = document.getElementById("cancel-upload");
    const editImageBtn = document.getElementById("editImageBtn");
    const imageUploadDiv = document.querySelector(".imageUpload:not(.s3-image-container)");
    const s3ImageContainer = document.getElementById("s3ImageContainer");
    const s3Image = document.getElementById("s3-image");

    // Initialize Viewer.js on the uploaded or existing image
    let viewer;

    function initializeViewer(targetImage) {
        if (viewer) {
            viewer.destroy(); // Destroy existing Viewer instance if it exists
        }
        viewer = new Viewer(targetImage, { // Initialize Viewer on the specific image
            toolbar: true,
            navbar: false,
            title: false,
        });
    }

    // Activate Viewer.js on the existing S3 image if present
    if (s3Image) {
        initializeViewer(s3Image);
    }

    // Handle drag-and-drop image upload
    dropArea.addEventListener("dragover", function (e) {
        e.preventDefault();
        dropArea.style.borderColor = "#4A90E2"; // Change border color on hover
    });

    dropArea.addEventListener("dragleave", function (e) {
        e.preventDefault();
        dropArea.style.borderColor = "#bbb5ff"; // Reset border color when not dragging
    });

    dropArea.addEventListener("drop", function (e) {
        e.preventDefault();
        dropArea.style.borderColor = "#bbb5ff"; // Reset border color when dropping the file
        inputFile.files = e.dataTransfer.files;
        uploadImage();
    });

    // Handle file upload for new images
    function uploadImage() {
        if (inputFile.files && inputFile.files[0]) {
            let imgLink = URL.createObjectURL(inputFile.files[0]);

            // Set the uploaded image src and show the image
            uploadedImage.src = imgLink;
            uploadedImage.style.display = "block";

            // Hide placeholder icon and text
            uploadIcon.style.display = "none";
            placeholderText.style.display = "none";

            // Show the Cancel Upload button
            cancelUploadButton.style.display = "inline-block";

            // Initialize Viewer.js on the newly uploaded image
            initializeViewer(uploadedImage);
        }
    }

    // Handle Cancel Upload button click
    cancelUploadButton.addEventListener("click", function () {
        // Reset the input file element
        inputFile.value = "";

        // Hide the uploaded image
        uploadedImage.style.display = "none";
        uploadedImage.src = ""; // Clear the image source

        // Show placeholder icon and text again
        uploadIcon.style.display = "block";
        placeholderText.style.display = "block";

        // Hide the Cancel Upload button
        cancelUploadButton.style.display = "none";

        // Destroy Viewer.js instance as there's no image to view
        if (viewer) {
            viewer.destroy();
        }
    });

    // Edit Image Button: Directly switch to upload state and clear existing image
    editImageBtn.addEventListener("click", function () {
        // Hide the existing S3 image container
        if (s3ImageContainer) {
            s3ImageContainer.remove(); // Completely remove the existing image container
        }

        // Show the image upload div
        imageUploadDiv.style.display = "block";

        // Hide the Edit Image button itself
        editImageBtn.style.display = "none";
    });
});

function displayBusyIndicator() {
    console.log("Spinner activated"); // This log should appear when the button is clicked
    document.getElementById("loading").style.display = "block";
}

// This function approves the QC35 form selected from the Admin Dashboard QC35 Form Management page
function approveQC35Form(qc35FormId) {
    console.log("approveQC35Form function called");  // Add this to see if it's being triggered
    // Show the loading spinner


    Swal.fire({
        title: 'Are you sure?',
        text: 'Do you want to approve this QC35 Form?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, approve it!',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            displayBusyIndicator();
            // Send a POST request to the server
            $.ajax({
                url: '/QC35Form/ApproveQC35Form/' + qc35FormId,
                type: 'POST',
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() // Anti-forgery token
                },
                success: function (response) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Approved!',
                        text: 'The QC35 Form has been approved.',
                    }).then(() => {
                        window.location.href = '/QC35Form/QC35FormApprovalManagement'; // Redirect to the management page
                    });
                },
                error: function (xhr, status, error) {
                    if (xhr.status === 403) {
                        Swal.fire({
                            icon: 'error',
                            title: 'Forbidden',
                            text: 'The request was invalid. Please check that you are authorized to approve this form and try again.',
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

// This function rejects the QC35 form selected from the Admin Dashboard QC35 Form Management page
function rejectQC35Form(qc35FormId) {
    Swal.fire({
        title: 'Are you sure?',
        text: 'Do you want to reject this QC35 Form?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, reject it!',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            displayBusyIndicator();
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
                        url: '/QC35Form/RejectQC35Form/' + qc35FormId,
                        type: 'POST',
                        contentType: 'application/json',
                        data: JSON.stringify({ QC35FormID: qc35FormId, RejectionReason: result.value }),
                        success: function (response) {
                            Swal.fire({
                                icon: 'success',
                                title: 'Rejected!',
                                text: 'The QC35 Form has been rejected.',
                            }).then(() => {
                                window.location.href = '/QC35Form/QC35FormApprovalManagement';
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

