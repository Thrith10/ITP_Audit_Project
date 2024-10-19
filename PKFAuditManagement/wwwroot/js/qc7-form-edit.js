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

$(document).ready(function () {
    toggleRiskLevel();
    toggleSectionBResult();
    toggleSTR();

    // Get the value from the hidden field
    const grandTotalHiddenValue = document.getElementById('grandTotalHidden').value;

    // Set the value in the visible, disabled field
    document.getElementById('grandTotal').value = parseFloat(grandTotalHiddenValue).toFixed(2);

    // Get the value from the hidden field
    const feeConcentrationHiddenValue = document.getElementById('feeConcentrationHidden').value;

    // Set the value in the visible, disabled field
    document.getElementById('feeConcentration').value = parseFloat(feeConcentrationHiddenValue).toFixed(2);

    // Get the value from the hidden field
    const priorYearHiddenValue = document.getElementById('PriorYearRecoveryRateHidden').value;

    // Set the value in the visible, disabled field
    document.getElementById('PriorYearRecoveryRate').value = parseFloat(priorYearHiddenValue).toFixed(2);

    // Disables or hides the sub forms for QC7 on click
    $('#toggleSubForm1').change(function () {
        if (this.checked) {
            // Hide the table
            $('#tableContainer1').hide();
        } else {
            // Show the table
            $('#tableContainer1').show();
        }
    });

    // Disables or hides the sub forms for QC7 on click
    $('#toggleSubForm2').change(function () {
        if (this.checked) {
            // Hide the table
            $('#tableContainer2').hide();
        } else {
            // Show the table
            $('#tableContainer2').show();
        }
    });

    // Function to update the Prior year’s recovery rate
    function updatePriorYearRecoveryRate() {
        var comp1 = parseFloat($("#comp1").val());
        var timeCosts = parseFloat($("#timeCosts").val());
        var commentBox = $("#commentBoxContainer");

        if (!isNaN(comp1) && !isNaN(timeCosts) && timeCosts !== 0) {
            var priorYearRecoveryRate = (comp1 / timeCosts) * 100;
            priorYearRecoveryRate = priorYearRecoveryRate.toFixed(2);
            $("#PriorYearRecoveryRate").val(priorYearRecoveryRate);
            $("#PriorYearRecoveryRateHidden").val(priorYearRecoveryRate);

            // Check if recovery rate is below 30%
            if (priorYearRecoveryRate < 30) {
                commentBox.show(); // Show the comment box
            } else {
                commentBox.hide(); // Hide the comment box
            }

        } else {
            $("#PriorYearRecoveryRate").val("");
            commentBox.hide(); // Hide the comment box if values are not valid
        }
    }

    // Function to update the Proposed Recovery Rate
    function updateProposedRecoveryRate() {
        var comp1 = parseFloat($("#proposedFeeCurrentYear").val());
        var budgetedTimeCost = parseFloat($("#budgetedTimeCost").val());

        if (!isNaN(comp1) && !isNaN(budgetedTimeCost) && budgetedTimeCost !== 0) {
            var proposedRecoveryRateCurrentYear = (comp1 / budgetedTimeCost) * 100;
            proposedRecoveryRateCurrentYear = proposedRecoveryRateCurrentYear.toFixed(2);
            $("#proposedRecoveryRateCurrentYear").val(proposedRecoveryRateCurrentYear);
            $("#proposedRecoveryRateCurrentYearHidden").val(proposedRecoveryRateCurrentYear);
        } else {
            $("#proposedRecoveryRateCurrentYear").val("");
        }
    }

    // Update the Prior year’s recovery rate when comp1 (prior year's fee) or time costs changes
    $("#comp1, #timeCosts").on("input", function () {
        updatePriorYearRecoveryRate();
    });

    // Initial update of Prior year’s recovery rate
    updatePriorYearRecoveryRate();

    // Update the Prior year’s recovery rate when comp1 (prior year's fee) or time costs changes
    $("#proposedFeeCurrentYear, #budgetedTimeCost").on("input", function () {
        updateProposedRecoveryRate();
    });

    // Initial update of Prior year’s recovery rate
    updateProposedRecoveryRate();

    // Function to toggle the comment input field based on radio selection
    function toggleUnpaidAuditFeeCommentInput() {
        var yesSelected = document.getElementById('outstandingUnpaidAuditFeesYes').checked;
        var commentRow = document.getElementById('outstandingUnpaidFeesRow');

        if (yesSelected) {
            commentRow.style.display = 'flex';  // Show the comment row
        } else {
            commentRow.style.display = 'none';  // Hide the comment row
        }
    }

    // Attach event listeners to the radio buttons
    document.getElementById('outstandingUnpaidAuditFeesYes').addEventListener('change', toggleUnpaidAuditFeeCommentInput);
    document.getElementById('outstandingUnpaidAuditFeesNo').addEventListener('change', toggleUnpaidAuditFeeCommentInput);

    // Initial load: Call the function to ensure the correct visibility based on the current selection
    toggleUnpaidAuditFeeCommentInput();

    // Function to toggle the comment input field based on radio selection
    function toggleSuspiciousTransactionReportPriorYearComment() {
        var yesSelected = document.getElementById('anySuspiciousTransactionReportFiledYes').checked;
        var commentRow = document.getElementById('suspiciousTransactionReportPriorYearRow');

        if (yesSelected) {
            commentRow.style.display = 'flex';  // Show the comment row
        } else {
            commentRow.style.display = 'none';  // Hide the comment row
        }
    }

    // Attach event listeners to the radio buttons
    document.getElementById('anySuspiciousTransactionReportFiledYes').addEventListener('change', toggleSuspiciousTransactionReportPriorYearComment);
    document.getElementById('anySuspiciousTransactionReportFiledNo').addEventListener('change', toggleSuspiciousTransactionReportPriorYearComment);

    // Initial load: Call the function to ensure the correct visibility based on the current selection
    toggleSuspiciousTransactionReportPriorYearComment();

    // Function to toggle the comment input field based on radio selection
    function toggleUnpaidNonAuditFeeCommentInput() {
        var yesSelected = document.getElementById('anyOutstandingUnpaidNonAuditFeesYes').checked;
        var commentRow = document.getElementById('outstandingUnpaidNonAuditFeesRow');

        if (yesSelected) {
            commentRow.style.display = 'flex';  // Show the comment row
        } else {
            commentRow.style.display = 'none';  // Hide the comment row
        }
    }

    // Attach event listeners to the radio buttons
    document.getElementById('anyOutstandingUnpaidNonAuditFeesYes').addEventListener('change', toggleUnpaidNonAuditFeeCommentInput);
    document.getElementById('anyOutstandingUnpaidNonAuditFeesNo').addEventListener('change', toggleUnpaidNonAuditFeeCommentInput);

    // Initial load: Call the function to ensure the correct visibility based on the current selection
    toggleUnpaidNonAuditFeeCommentInput();

    // Toggling checkbox for risks associated (conclusion section) displays the comment box
    function toggleRisksAssociated() {
        var yesSelected = document.getElementById('anyRisksYes').checked;
        var risksAssociatedRow = document.getElementById('risksAssociatedRow');
        var riskExplanationCurrentYearPriorYear = document.getElementById('riskExplanationCurrentYearPriorYear');
        var natureOfSafeguard = document.getElementById('natureOfSafeguard');

        if (yesSelected) {
            risksAssociatedRow.style.display = '';
            riskExplanationCurrentYearPriorYear.disabled = false;
            natureOfSafeguard.disabled = false;
        } else {
            risksAssociatedRow.style.display = 'none';
            riskExplanationCurrentYearPriorYear.disabled = true;
            natureOfSafeguard.disabled = true;
            // Reset values
            riskExplanationCurrentYearPriorYear.value = '';
            natureOfSafeguard.value = '';
        }
    }

    // Attach event listeners to the radio buttons
    document.getElementById('anyRisksYes').addEventListener('change', toggleRisksAssociated);
    document.getElementById('anyRisksNo').addEventListener('change', toggleRisksAssociated);

    // Initial load: Call the function to ensure the correct visibility based on the current selection
    toggleRisksAssociated();

    // Toggling comment display for safeguards that should be applied
    function toggleSafeguards() {
        var yesSelected = document.getElementById('anySafeguardsYes').checked;
        var safeguardsAppliedRow = document.getElementById('safeguardsAppliedRow');
        var safeguardsApplied = document.getElementById('safeguardsApplied');

        if (yesSelected) {
            safeguardsAppliedRow.style.display = '';
            safeguardsApplied.disabled = false;
        } else {
            safeguardsAppliedRow.style.display = 'none';
            safeguardsApplied.disabled = true;
        }
    }

    // Attach event listeners to the radio buttons
    document.getElementById('anySafeguardsYes').addEventListener('change', toggleSafeguards);
    document.getElementById('anySafeguardsNo').addEventListener('change', toggleSafeguards);

    // Initial load: Call the function to ensure the correct visibility based on the current selection
    toggleSafeguards();

    // Open PDF in a new tab
    $(document).on('click', '.preview-doc', function () {
        // Find the closest <tr> and then look for the file input within it
        var fileInput = $(this).closest('tr').find('input[type="file"]')[0];

        if (fileInput && fileInput.files.length > 0) {  // Check if fileInput is defined
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

    // Clear the file input and strike through the document link when the 'Delete Files' button is clicked
    $(document).on('click', '.clear-doc', function () {
        // Get the file input and document link elements
        var fileInput = $('#file-input');
        var documentLink = $('#current-document-link');
        var noFileSpan = $('#no-file-span');
        var deleteHiddenInput = $('#delete-existing-file');

        // If the document link already indicates deletion, do nothing
        if (documentLink.length && documentLink.hasClass('deleted')) {
            return; // Skip if already marked as deleted
        }

        // Strike through the existing document link if it exists
        if (documentLink.length) {
            documentLink.css('text-decoration', 'line-through'); // Strike through the link
            documentLink.text(documentLink.text() + ' (Deleted)'); // Append '(Deleted)' to indicate removal
            documentLink.css('color', 'red'); // Change the color to red
            documentLink.addClass('deleted'); // Add a class to indicate it's been processed
        } else if (noFileSpan.length) {
            noFileSpan.text('No file uploaded'); // Show "No file uploaded" text if no document exists
        }

        // Clear the file input value
        fileInput.val('');
        deleteHiddenInput.val('true');

        // If a new file is uploaded, reset the hidden input value to "false"
        $('#file-input').on('change', function () {
            var deleteHiddenInput = $('#delete-existing-file');
            var documentLink = $('#current-document-link');

            // Remove the deleted class and restore the link text if it was previously marked
            if (documentLink.length && documentLink.hasClass('deleted')) {
                documentLink.text(documentLink.text().replace(' (Deleted)', '')); // Remove the "(Deleted)" text
                documentLink.css('text-decoration', 'none'); // Remove the strike-through
                documentLink.css('color', ''); // Reset color
                documentLink.removeClass('deleted'); // Remove the deleted class
            }

            deleteHiddenInput.val('false'); // Reset the delete flag
        });
    });

    // Clear the file input when the 'Clear' button is clicked
    $(document).on('click', '.remove-uploaded-doc', function () {
        // Find the closest file input within the same row
        var fileInput = $(this).closest('tr').find('input[type="file"]');

        if (fileInput.val()) {
            // Clear the file input value
            fileInput.val('');
        } else {
            alert('No file selected.');
        }
    });

    // Calculate document indices on load
    updateDocumentIndices();
});

// Add event listener to the "Add More Documents" button
document.getElementById('add-more-docs').addEventListener('click', addDocument);

// Array to hold the filenames of deleted documents
let deletedDocumentFilenames = [];

// Function to add a new document row
function addDocument() {
    const docsContainer = document.getElementById('additional-docs-body');
    const existingRowCount = docsContainer.querySelectorAll('tr.additional-doc-row').length; // Count only additional document rows
    const noDocsRow = document.getElementById('no-docs-row');

    // Check if the 'No additional documents available' row exists and hide it
    if (noDocsRow) {
        noDocsRow.style.display = 'none';
    }

    const newRow = document.createElement('tr');
    newRow.classList.add('additional-doc-row'); // Add a class to easily identify additional document rows
    newRow.innerHTML = `
        <td>
            <input asp-for="AdditionalDocuments[${existingRowCount}].DocumentName" type="text" class="form-control" name="AdditionalDocuments[${existingRowCount}].DocumentName" placeholder="Document Name" required />
        </td>
        <td>
            <span class="no-file-text">No file uploaded</span>
        </td>
        <td>
            <input asp-for="AdditionalDocuments[${existingRowCount}].File" type="file" class="form-control" name="AdditionalDocuments[${existingRowCount}].File" accept="application/pdf" required />
        </td>
        <td>
            <button type="button" class="btn btn-secondary btn-sm remove-uploaded-doc mr-2">Clear</button>
            <button type="button" class="btn btn-primary btn-sm preview-doc">Preview</button>
            <button type="button" class="btn btn-danger btn-sm delete-row" onclick="removeDocument(this)">Delete Row</button>
        </td>
    `;

    docsContainer.appendChild(newRow);
}

// Function to remove a document row
function removeDocument(button) {
    const row = button.closest('tr');
    const documentFilenameInput = row.querySelector('.document-filename');

    // Get the document filename from the input
    if (documentFilenameInput) {
        const documentFilename = documentFilenameInput.value;

        // Add the document filename to the deletedDocumentFilenames array
        if (documentFilename) {
            deletedDocumentFilenames.push(documentFilename);
        }
    }

    row.remove();

    // Update the indices of the additional document rows
    updateDocumentIndices();

    // Update the hidden input with the deleted document filenames
    updateDeletedDocumentsInput();
}

// Function to update the hidden input with deleted document filenames
function updateDeletedDocumentsInput() {
    const deletedDocumentsInput = document.getElementById('deletedDocuments');
    if (deletedDocumentsInput) {
        // Join the deleted document filenames array into a comma-separated string
        deletedDocumentsInput.value = deletedDocumentFilenames.join(',');
    }
}

// Function to update the indices of the additional document rows
function updateDocumentIndices() {
    const docsContainer = document.getElementById('additional-docs-body');
    const additionalDocRows = docsContainer.querySelectorAll('tr.additional-doc-row'); // Select only additional document rows

    additionalDocRows.forEach((row, index) => {
        const inputs = row.querySelectorAll('input');
        inputs.forEach(input => {
            // Update 'name' attribute
            const name = input.getAttribute('name');
            if (name) {
                const newName = name.replace(/\[\d+\]/g, `[${index}]`); // Update index in 'name' attribute
                input.setAttribute('name', newName);
            }

            // Update 'asp-for' attribute
            const aspFor = input.getAttribute('asp-for');
            if (aspFor) {
                const newAspFor = aspFor.replace(/\[\d+\]/g, `[${index}]`); // Update index in 'asp-for' attribute
                input.setAttribute('asp-for', newAspFor);
            }
        });
    });
}

// Function to check for duplicate document names
function checkDuplicateDocuments() {
    const docInputs = document.querySelectorAll('input[name^="AdditionalDocuments"][name$=".DocumentName"]');
    const docNames = [];
    let hasDuplicates = false;

    // Remove any previous error tooltips, validation styles, and Bootstrap's is-valid/was-validated classes
    docInputs.forEach(input => {
        input.classList.remove('is-invalid', 'is-valid', 'was-validated');
        input.removeAttribute('data-bs-toggle');
        input.removeAttribute('data-bs-placement');
        input.removeAttribute('title');
    });

    // Loop through the document name inputs to find duplicates
    docInputs.forEach(input => {
        const docName = input.value.trim().toLowerCase(); // Normalise to lowercase for comparison
        if (docNames.includes(docName)) {
            // If duplicate, add error styling and tooltip
            input.classList.add('is-invalid');
            input.setAttribute('data-bs-toggle', 'tooltip');
            input.setAttribute('data-bs-placement', 'top');
            input.setAttribute('title', 'Duplicate document name');
            hasDuplicates = true;
        } else {
            docNames.push(docName);
        }
    });

    // Initialise Bootstrap tooltips for invalid inputs
    if (hasDuplicates) {
        const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
        const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));
    }

    // Scroll to the first invalid input if duplicates are found
    if (hasDuplicates) {
        const firstInvalidElement = document.querySelector('.is-invalid');
        if (firstInvalidElement) {
            firstInvalidElement.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    }

    return !hasDuplicates; // Return false if duplicates were found
}

// Hook into the form submission event
document.getElementById('qc7Form').addEventListener('submit', function (event) {
    // Prevent default validation and handle manually
    event.preventDefault(); // Stop the form from submitting

    // Remove Bootstrap's automatic validation classes from the form
    this.classList.remove('was-validated');

    // Check for duplicates
    if (!checkDuplicateDocuments()) {
        // Prevent form submission if duplicates were found
        event.stopPropagation();
    } else {
        // No duplicates, remove any automatic is-valid classes and submit the form programmatically
        const inputs = this.querySelectorAll('input');
        inputs.forEach(input => {
            input.classList.remove('is-valid'); // Remove Bootstrap's automatic valid class
        });

        this.submit();
    }
});

// Ensure input values are formatted to two decimal places on blur
$(document).on("blur", "#auditFee, input[name^='Services'][name$='.Fee']", function () {
    var value = parseFloat($(this).val());
    if (!isNaN(value)) {
        $(this).val(convertToMoney(value));
    }
});

// Add event listener to the "Add Service" button
document.getElementById('addService').addEventListener('click', addService);

// Function to add a new service field
function addService() {
    const servicesContainer = document.getElementById('services');
    const currentServices = servicesContainer.getElementsByClassName('service');
    const nextIndex = currentServices.length; // This will give the correct index for the next service

    const serviceCard = document.createElement('div');
    serviceCard.className = 'card border border-secondary p-3 mb-3';
    serviceCard.innerHTML = `
        <div class="row mb-3 service input-field card-body">
            <h6 class="card-title">Service ${nextIndex + 1}</h6>
            <input type="hidden" asp-for="Services[${nextIndex}].QC6FormFeeDetailID" />
            <div class="col-sm-6">
                <label>Nature of Service:</label>
                <select class="form-control" name="Services[${nextIndex}].NatureOfService" onchange="showOtherServiceInput(this)">
                    <option value="Tax Services">Tax Services</option>
                    <option value="Accounting">Accounting</option>
                    <option value="Payroll">Payroll</option>
                    <option value="Secretarial Services">Secretarial Services</option>
                    <option value="Other Non-Audit Services">Other Non-Audit Services</option>
                </select>
            </div>
            <div class="col-sm-6">
                <label>Fee:<span class="text-danger"> *</span></label>
                <div class="input-group">
                    <span class="input-group-text">$</span>
                    <input type="number" name="Services[${nextIndex}].Fee" step="0.01" class="form-control fee-input" oninput="calculateTotalAndConcentration()" required>
                </div>
            </div>
            <div class="col-sm-12 mt-3" id="otherServiceInput-${nextIndex}" style="display: none;">
                <label>Name of Non-Audit Service<span class="text-danger"> *</span></label>
                <input type="text" name="Services[${nextIndex}].OtherService" class="form-control">
            </div>
            <div class="col-sm-12 mt-3">
                <button type="button" class="btn btn-danger" onclick="removeService(this)">Remove Service</button>
            </div>
        </div>
    `;

    servicesContainer.appendChild(serviceCard);
}

// Function for removing service
function removeService(button) {
    const card = button.closest('.card');
    const serviceIdInput = card.querySelector('input[name$="QC6FormFeeDetailID"]');

    if (serviceIdInput) {
        const removedServicesContainer = document.getElementById('removedServicesContainer');
        const removedServiceInput = document.createElement('input');
        removedServiceInput.type = 'hidden';
        removedServiceInput.name = 'RemovedServices[]';
        removedServiceInput.value = serviceIdInput.value;
        removedServicesContainer.appendChild(removedServiceInput);
    }

    // Remove the card element
    card.remove();

    // Update the indexes of remaining services
    const servicesContainer = document.getElementById('services');
    const serviceCards = servicesContainer.getElementsByClassName('card');
    for (let i = 0; i < serviceCards.length; i++) {
        const card = serviceCards[i];
        const titleElement = card.querySelector('.card-title');
        titleElement.textContent = `Service ${i + 1}`;

        const otherServiceInputs = serviceCards[i].querySelectorAll('[id^="otherServiceInput-"]');
        for (let j = 0; j < otherServiceInputs.length; j++) {
            const input = otherServiceInputs[j];
            input.id = `otherServiceInput-${i}`;
        }
    }

    // Recalculate the total fee after removal
    calculateTotalAndConcentration();
}

// Function to display additional text field based on service field selection
function showOtherServiceInput(selectElement) {
    const selectedValue = selectElement.value;
    const index = selectElement.name.split('[')[1].split(']')[0];
    const otherServiceInput = document.getElementById(`otherServiceInput-${index}`);

    if (otherServiceInput) {
        const inputField = otherServiceInput.querySelector('input');
        if (selectedValue === 'Other Non-Audit Services') {
            otherServiceInput.style.display = 'block';
            inputField.setAttribute('required', 'required');
        } else {
            otherServiceInput.style.display = 'none';
            inputField.removeAttribute('required');
        }
    }
}


// Function to round down to two decimal places
function convertToMoney(val) {
    return (Math.floor(val * 100).toFixed(0) / 100).toFixed(2);
}

// Function to calculate the total fee and fee concentration
function calculateTotalAndConcentration() {
    // Get all fee input elements with class 'fee-input'
    const fees = document.querySelectorAll('.fee-input');
    let totalFee = 0;

    // Calculate the total fee by summing up all fee values
    fees.forEach(fee => {
        totalFee += parseFloat(fee.value) || 0;
    });

    // Set the total fee in the corresponding input field
    document.getElementById('grandTotal').value = totalFee.toFixed(2);
    document.getElementById('grandTotalHidden').value = totalFee.toFixed(2);

    // Get the audit fee value
    const auditFee = parseFloat(document.getElementById('auditFee').value) || 0;

    // Calculate the fee concentration if audit fee is greater than 0
    let feeConcentration = auditFee > 0 ? (totalFee / auditFee) * 100 : 0;

    // Set the fee concentration in the corresponding input field
    document.getElementById('feeConcentration').value = feeConcentration.toFixed(2);
    document.getElementById('feeConcentrationHidden').value = feeConcentration.toFixed(2);
}

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

// Function to display comment for suspicious transaction report field based on checkbox value
function toggleSTR() {
    var strCheckbox = $('#isSuspiciousTransactionReportFiled');
    var rationaleSTRRow = $('#strRationale');

    if (strCheckbox.is(':checked')) {
        rationaleSTRRow.show();
    } else {
        rationaleSTRRow.hide();
    }
}

// Function to removing all values for Q1-Q4 for Section B of TNA Assessment
function toggleSectionBResult() {
    var engagementType = $('#engagementType').val();

    if (engagementType === "Non-Audit") {
        $('#q1Yes').prop('checked', false);
        $('#q1No').prop('checked', false);
        $('#q2Yes').prop('checked', false);
        $('#q2No').prop('checked', false);
        $('#q3Yes').prop('checked', false);
        $('#q3No').prop('checked', false);
        $('#q4Yes').prop('checked', false);
        $('#q4No').prop('checked', false);

        $('input[name="TNATNEAssessment.SectionB.Q1"]').prop('disabled', true);
        $('input[name="TNATNEAssessment.SectionB.Q2"]').prop('disabled', true);
        $('input[name="TNATNEAssessment.SectionB.Q3"]').prop('disabled', true);
        $('input[name="TNATNEAssessment.SectionB.Q4"]').prop('disabled', true);
    }
}

// Handles the change of selection for the TNE Form Assessment Dropdown Selection 
document.getElementById('engagementType').addEventListener('change', function () {
    var q1Yes = document.getElementById('q1Yes');
    var q1No = document.getElementById('q1No');
    var q2Yes = document.getElementById('q2Yes');
    var q2No = document.getElementById('q2No');
    var q3Yes = document.getElementById('q3Yes');
    var q3No = document.getElementById('q3No');
    var q4Yes = document.getElementById('q4Yes');
    var q4No = document.getElementById('q4No');

    if (this.value === 'Audit') {
        // Enable Q1 to Q4
        q1Yes.disabled = false;
        q1No.disabled = false;
        q2Yes.disabled = false;
        q2No.disabled = false;
        q3Yes.disabled = false;
        q3No.disabled = false;
        q4Yes.disabled = false;
        q4No.disabled = false;

        // Reset selections to No
        q1Yes.checked = false;
        q1No.checked = true;
        q2Yes.checked = false;
        q2No.checked = true;
        q3Yes.checked = false;
        q3No.checked = true;
        q4Yes.checked = false;
        q4No.checked = true;

    } else if (this.value === 'Non-Audit') {

        // Clear selections for Q2 to Q4
        q1Yes.checked = false;
        q1No.checked = false;
        q2Yes.checked = false;
        q2No.checked = false;
        q3Yes.checked = false;
        q3No.checked = false;
        q4Yes.checked = false;
        q4No.checked = false;

        // Disable Q1 to Q4
        q1Yes.disabled = true;
        q1No.disabled = true;
        q2Yes.disabled = true;
        q2No.disabled = true;
        q3Yes.disabled = true;
        q3No.disabled = true;
        q4Yes.disabled = true;
        q4No.disabled = true;
    }
});

// Display NAS modal
document.getElementById('retrieveFeeDetailsButton').addEventListener('click', function (e) {

    e.preventDefault(); // Prevent default button behavior

    $.ajax({
        url: '/QC7Form/RetrieveNASFeeDetails',
        method: 'GET',
        success: function (data) {
            console.log('Data:', data);

            // Group fee details by QC7FormID
            var groupedFeeDetails = {};
            data.forEach(function (feeDetail) {
                if (!groupedFeeDetails.hasOwnProperty(feeDetail.qC7FormID)) {
                    groupedFeeDetails[feeDetail.qC7FormID] = [];
                }
                groupedFeeDetails[feeDetail.qC7FormID].push(feeDetail);
            });
            console.log('Grouped Fee Details:', groupedFeeDetails);

            // Build the tbody HTML
            var tbodyHtml = '';
            Object.keys(groupedFeeDetails).forEach(function (qc7FormID) {
                var feeDetailsList = groupedFeeDetails[qc7FormID];
                console.log('QC7FormID:', qc7FormID, 'Fee Details List:', feeDetailsList);

                // Display the QC7 Form File reference in one row
                tbodyHtml += '<tr>' +
                    '<td rowspan="' + feeDetailsList.length + '">' + feeDetailsList[0].fileReference + '</td>' +
                    '<td>' + feeDetailsList[0].fee + '</td>' +
                    '<td>' + feeDetailsList[0].natureOfService + '</td>' +
                    '</tr>';

                // Display the fee details for this QC7 form in subsequent rows
                for (var i = 1; i < feeDetailsList.length; i++) {
                    var feeDetail = feeDetailsList[i];
                    tbodyHtml += '<tr>' +
                        '<td>' + feeDetail.fee + '</td>' +
                        '<td>' + feeDetail.natureOfService + '</td>' +
                        '</tr>';
                }
            });

            // Set the tbody HTML to the table
            $('#feeDetailsTable tbody').html(tbodyHtml);

            // Show the modal
            $('#feeDetailsModal').modal('show');
        },

        error: function (error) {
            console.log('Error:', error);
        }
    });
});