// Store selected client
let selectedClient = null;

// Function to filter clients
function filterClients() {
    const searchInput = document.getElementById('searchClientInput').value.toLowerCase();
    const clientItems = document.querySelectorAll('#clientList .list-group-item');

    clientItems.forEach(item => {
        const clientName = item.getAttribute('data-client').toLowerCase();
        if (clientName.includes(searchInput)) {
            item.style.display = 'block';
        } else {
            item.style.display = 'none';
        }
    });
}

// Function to select a client
function selectClient(clientName) {
    selectedClient = clientName;

    // Highlight the selected client
    const clientItems = document.querySelectorAll('#clientList .list-group-item');
    clientItems.forEach(item => {
        item.classList.remove('active');
        if (item.getAttribute('data-client') === clientName) {
            item.classList.add('active');
        }
    });

    // Enable the "Ok" button
    document.getElementById('confirmClientBtn').disabled = false;
}

// Function to confirm selection and update the dropdown
function confirmSelection() {
    if (selectedClient) {
        // Find the client dropdown and set the selected value
        const clientSelect = document.getElementById('clientSelect');
        for (let i = 0; i < clientSelect.options.length; i++) {
            if (clientSelect.options[i].value === selectedClient) {
                clientSelect.selectedIndex = i;
                break;
            }
        }

        // Hide the modal
        $('#clientSearchModal').modal('hide');
    }
}

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