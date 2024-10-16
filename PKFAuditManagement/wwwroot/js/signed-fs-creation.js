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

// Open PDF in a new tab
$(document).on('click', '.preview-doc', function () {
    var fileInput = $(this).siblings('input[type="file"]')[0];
    if (fileInput.files.length > 0) {
        var file = fileInput.files[0];
        var reader = new FileReader();

        reader.onload = function (e) {
            var blob = new Blob([e.target.result], { type: 'application/pdf' });
            var url = URL.createObjectURL(blob);
            window.open(url, '_blank');
        };

        reader.readAsArrayBuffer(file);
    } else {
        alert('No file selected.');
    }
});