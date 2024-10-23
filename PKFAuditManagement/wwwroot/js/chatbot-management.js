document.getElementById('add-document-btn').addEventListener('click', function () {
    var formData = new FormData();
    var fileInput = document.getElementById('new-document');
    var documentName = document.getElementById('document-name').value;

    // Ensure a file is selected
    if (!fileInput.files.length) {
        Swal.fire("Error", "Please select a PDF document to upload.", "error");
        return;
    }

    // Ensure the document name is provided
    if (!documentName.trim()) {
        Swal.fire("Error", "Please enter a document name.", "error");
        return;
    }

    // Check if the selected file is a PDF
    if (!fileInput.files[0].name.endsWith('.pdf')) {
        Swal.fire("Error", "Only PDF documents are allowed.", "error");
        return;
    }

    // Ask for confirmation before uploading
    Swal.fire({
        title: "Are you sure?",
        text: "Do you want to upload this document?",
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Yes, upload it!",
        cancelButtonText: "Cancel"
    }).then((result) => {
        if (result.isConfirmed) {
            // Show the loading spinner
            document.getElementById('loading-spinner').style.display = 'block';

            // Add the file and document name to the FormData
            formData.append('file', fileInput.files[0]);
            formData.append('documentName', documentName);

            // AJAX request to upload the file
            var xhr = new XMLHttpRequest();
            xhr.open('POST', '/Chatbot/UploadDocument', true);

            // Define the callback when the upload is complete
            xhr.onload = function () {
                // Hide the loading spinner
                document.getElementById('loading-spinner').style.display = 'none';

                if (xhr.status === 200) {
                    Swal.fire("Success", "Document uploaded successfully.", "success").then(() => {
                        // Optionally, refresh the document list or update the UI
                        location.reload();
                    });
                } else {
                    Swal.fire("Error", "Failed to upload document: " + xhr.responseText, "error");
                }
            };

            // Handle network errors
            xhr.onerror = function () {
                // Hide the loading spinner
                document.getElementById('loading-spinner').style.display = 'none';
                Swal.fire("Error", "Network error. Please try again.", "error");
            };

            // Send the form data via the request
            xhr.send(formData);
        } else {
            Swal.fire("Upload canceled", "Your document was not uploaded.", "info");
        }
    });
});

// Function to remove document
function removeDocument(docId) {
    // Show confirmation dialog
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            // Show loading spinner
            document.getElementById('loading-spinner').style.display = 'block';

            var xhr = new XMLHttpRequest();
            xhr.open('DELETE', `/Chatbot/DeleteDocument/${docId}`, true);
            xhr.onload = function () {
                // Hide loading spinner
                document.getElementById('loading-spinner').style.display = 'none';

                if (xhr.status === 200) {
                    // Show success message
                    Swal.fire(
                        'Deleted!',
                        'Document removed successfully.',
                        'success'
                    ).then(() => {
                        location.reload(); // Reload the page after confirmation
                    });
                } else {
                    // Show error message
                    Swal.fire(
                        'Error!',
                        'Failed to remove document',
                        'error'
                    );
                }
            };
            xhr.send();
        }
    });
}


// Function to open document in new tab 
function previewDocument(filePath) {
    // Open the document in a new tab
    window.open(filePath, '_blank');
}