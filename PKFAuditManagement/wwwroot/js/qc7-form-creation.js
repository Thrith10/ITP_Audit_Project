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

        // Trigger the change event for the client select dropdown
        $('#clientSelect').trigger('change');

        // Hide the modal
        $('#clientSearchModal').modal('hide');
    }


}

// Handle the change event for the select element
$("#clientSelect").on('change', function () {
    var selectedValue = $(this).val();

    if (selectedValue) {
        // Make another AJAX call using the selected value
        $.ajax({
            url: '/QC7Form/RetrievePastQC7Data',
            type: 'GET',
            data: { selectedClient: selectedValue },
            success: function (data) {
                // Process the data returned from the second AJAX call
                console.log(data);

                // Redirect to the new page and trigger a full page reload
                window.location.href = '/QC7Form/RetrievePastQC7Data?selectedClient=' + encodeURIComponent(selectedValue);
            },
            error: function (xhr, status, error) {
                console.error("Error: " + error);
            }
        });
    }
});

// Handle the submission event validations
document.getElementById('qc7form').addEventListener('submit', function (event) {
    var selectElement = document.getElementById('clientSelect');
    if (selectElement.value === "") {
        selectElement.focus(); // Focus on the select element
        event.preventDefault(); // Prevent form submission
    } 
});


// Add event listener to the "Add Service" button
document.getElementById('addService').addEventListener('click', addService);

// Ensure input values are formatted to two decimal places on blur
$(document).on("blur", "#proposedFeeCurrentYear, #comp1, #timeCosts, #priorYearRate, #PriorYearRecoveryRateHidden, #ProposedFee, #budgetedTimeCost, #proposedRecoveryRateCurrentYear, #proposedRecoveryRateCurrentYearHidden, #auditFee, input[name^='Services'][name$='.Fee']", function () {
    var value = parseFloat($(this).val());
    if (!isNaN(value)) {
        $(this).val(convertToMoney(value));
    }
});

// Function to round down to two decimal places
function convertToMoney(val) {
    return (Math.floor(val * 100).toFixed(0) / 100).toFixed(2);
}

// Function to add a new service field
function addService() {
    const servicesContainer = document.getElementById('services');
    const lastServiceIndex = servicesContainer.children.length;

    const serviceCard = document.createElement('div');
    serviceCard.className = 'card border border-secondary p-3 mb-3';
    serviceCard.innerHTML = `
        <div class="row mb-3 service input-field card-body">
            <h6 class="card-title">Service ${lastServiceIndex + 1}</h6>
            <div class="col-sm-6">
                <label>Nature of Service:</label>
                <select class="form-control" name="Services[${lastServiceIndex}].NatureOfService" onchange="showOtherServiceInput(this)">
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
                    <input type="number" name="Services[${lastServiceIndex}].Fee" step="0.01" class="form-control fee-input" oninput="calculateTotalAndConcentration()" required>
                </div>
            </div>
            <div class="col-sm-12 mt-3" id="otherServiceInput-${lastServiceIndex}" style="display: none;">
                <label>Name of Non-Audit Service<span class="text-danger"> *</span></label>
                <input type="text" name="Services[${lastServiceIndex}].OtherService" class="form-control">
            </div>
            <div class="col-sm-12 mt-3">
                <button type="button" class="btn btn-danger" onclick="removeService(this)">Remove Service</button>
            </div>
        </div>
    `;

    servicesContainer.appendChild(serviceCard);
}

// Function to display additional text field based on service field selection
function showOtherServiceInput(selectElement) {
    const selectedValue = selectElement.value;
    const index = selectElement.name.split('[')[1].split(']')[0];
    const otherServiceInput = document.getElementById(`otherServiceInput-${index}`);
    const inputField = otherServiceInput.querySelector('input');
    if (otherServiceInput) {
        if (selectedValue === 'Other Non-Audit Services') {
            otherServiceInput.style.display = 'block';
            inputField.setAttribute('required', 'required'); // Add required attribute
        } else {
            otherServiceInput.style.display = 'none';
            inputField.removeAttribute('required'); // Remove required attribute
        }
    }
}

// Function for removing service
function removeService(button) {
    // Find the parent card element
    const card = button.closest('.card');
    // Remove the card element
    card.remove();

    // Update indexes of remaining services
    const servicesContainer = document.getElementById('services');
    const serviceCards = servicesContainer.getElementsByClassName('card');
    for (let i = 0; i < serviceCards.length; i++) {
        const card = serviceCards[i];
        const titleElement = card.querySelector('.card-title');
        titleElement.textContent = `Service ${i + 1}`;

        const inputs = serviceCards[i].getElementsByTagName('input');
        for (let j = 0; j < inputs.length; j++) {
            const name = inputs[j].getAttribute('name');
            const newName = name.replace(/\[\d+\]/g, `[${i}]`);
            inputs[j].setAttribute('name', newName);
        }

        const selects = serviceCards[i].getElementsByTagName('select');
        for (let j = 0; j < selects.length; j++) {
            const name = selects[j].getAttribute('name');
            const newName = name.replace(/\[\d+\]/g, `[${i}]`);
            selects[j].setAttribute('name', newName);
        }

        const otherServiceInputs = serviceCards[i].querySelectorAll('[id^="otherServiceInput-"]');
        for (let j = 0; j < otherServiceInputs.length; j++) {
            const input = otherServiceInputs[j];
            input.id = `otherServiceInput-${i}`;
        }
    }

    calculateTotalAndConcentration();
}

// Function to calculate the total fee and fee concentration
function calculateTotalAndConcentration() {
    // Get all fee input elements
    const fees = document.querySelectorAll('input[name^="Services["][name$="].Fee"]');
    let totalFee = 0;
    // Calculate the total fee by summing up all fee values
    for (let fee of fees) {
        totalFee += parseFloat(fee.value) || 0;
    }
    // Set the total fee in the corresponding input field
    document.getElementById('grandTotal').value = totalFee.toFixed(2);
    document.getElementById('grandTotalHidden').value = totalFee.toFixed(2);

    // Get the audit fee value
    const auditFee = parseFloat(document.getElementById('auditFee').value) || 0;
    // Calculate the fee concentration
    const feeConcentration = totalFee / auditFee * 100;
    // Set the fee concentration in the corresponding input field
    document.getElementById('feeConcentration').value = feeConcentration.toFixed(2);
    document.getElementById('feeConcentrationHidden').value = feeConcentration.toFixed(2);
}


// Function for disabling all textareas and input on "Not Applicable" selection
function toggleSubForm(subFormIndex) {
    const checkbox = document.getElementById(`toggleSubForm${subFormIndex}`);
    const hiddenInput = document.querySelector(`input[name="SubForm${subFormIndex}NotApplicable"]`);

    hiddenInput.value = checkbox.checked ? 'true' : 'false';

    const tableContainer = document.getElementById(`tableContainer${subFormIndex}`);
    const inputs = tableContainer.getElementsByTagName('input');
    const textareas = tableContainer.getElementsByTagName('textarea');

    for (let i = 0; i < inputs.length; i++) {
        inputs[i].disabled = !inputs[i].disabled;
    }

    for (let i = 0; i < textareas.length; i++) {
        textareas[i].disabled = !textareas[i].disabled;
    }
}

$(document).ready(function () {
    toggleRiskLevel();
    toggleSectionBResult();
    toggleSuspiciousTransactionReport();
    calculateTotalAndConcentration();


    // Function to update the Prior year’s recovery rate
    function updatePriorYearRecoveryRate() {
        var comp1 = parseFloat($("#comp1").val());
        var timeCosts = parseFloat($("#timeCosts").val());
        var commentBox = $("#commentBoxContainer");

        if (!isNaN(comp1) && !isNaN(timeCosts) && timeCosts !== 0) {
            var priorYearRecoveryRate = (comp1 / timeCosts) * 100;
            priorYearRecoveryRate = priorYearRecoveryRate.toFixed(2);
            $("#priorYearRate").val(priorYearRecoveryRate);
            $("#PriorYearRecoveryRateHidden").val(priorYearRecoveryRate);

            // Check if recovery rate is below 30%
            if (priorYearRecoveryRate < 30) {
                commentBox.show(); // Show the comment box
            } else {
                commentBox.hide(); // Hide the comment box
            }

        } else {
            $("#priorYearRate").val("");
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
        var commentInput = document.getElementById('outstandingUnpaidAuditFeesComment');

        if (yesSelected) {
            commentRow.style.display = 'flex';  // Show the comment row
        } else {
            commentRow.style.display = 'none';  // Hide the comment row
            commentInput.value = '';  // Clear the comment input field
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
        var commentInput = document.getElementById('suspiciousTransactionReportFiledComment');

        if (yesSelected) {
            commentRow.style.display = 'flex';  // Show the comment row
        } else {
            commentRow.style.display = 'none';  // Hide the comment row
            commentInput.value = '';  // Clear the comment input field
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
        var commentInput = document.getElementById('outstandingUnpaidAuditFeesCommentInput');

        if (yesSelected) {
            commentRow.style.display = 'flex';  // Show the comment row
        } else {
            commentRow.style.display = 'none';  // Hide the comment row
            commentInput.value = '';  // Clear the comment input field
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

    // Add event listener to the "Add More Documents" button
    document.getElementById('add-more-docs').addEventListener('click', addDocument);

    // Function to add a new document row
    function addDocument() {
        const docsContainer = document.getElementById('additional-docs-body');
        const existingRowCount = docsContainer.querySelectorAll('tr.additional-doc-row').length; // Count only additional document rows

        const newRow = document.createElement('tr');
        newRow.classList.add('additional-doc-row'); // Add a class to easily identify additional document rows
        newRow.innerHTML = `
        <td style="width: 30%;">
            <input asp-for="AdditionalDocuments[${existingRowCount}].DocumentName" type="text" class="form-control" name="AdditionalDocuments[${existingRowCount}].DocumentName" placeholder="Document Name" required />
        </td>
        <td style="width: 40%;">
            <input asp-for="AdditionalDocuments[${existingRowCount}].File" type="file" class="form-control" name="AdditionalDocuments[${existingRowCount}].File" accept="application/pdf" required />
        </td>
        <td style="width: 30%;">
            <button type="button" class="btn btn-secondary btn-sm remove-uploaded-doc mr-2">Clear</button>
            <button type="button" class="btn btn-primary btn-sm preview-doc">Preview</button>
            <button type="button" class="btn btn-danger btn-sm delete-row" onclick="removeDocument(this)">Delete Row</button>
        </td>
    `;

        docsContainer.appendChild(newRow);
    }

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
});

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

// Toggling checkbox for suspicious transaction report (conclusion section) displays the comment box
function toggleSuspiciousTransactionReport() {
    var isSuspiciousTransactionReportFiled = document.getElementById('isSuspiciousTransactionReportFiled');
    var strRationale = document.getElementById('strRationale');
    var strRationaleComment = document.getElementById('strRationaleComment');

    if (isSuspiciousTransactionReportFiled.checked) {
        strRationale.style.display = '';
        strRationaleComment.disabled = false;
    } else {
        strRationale.style.display = 'none';
        strRationaleComment.disabled = true;
        strRationaleComment.value = '';
    }
}

// Toggling checkbox for risk level displays the comment box
function toggleRiskLevel() {
    var pieCheckbox = document.getElementById('pieCheckbox');
    var riskLevelRow = document.getElementById('riskLevelRow');
    var riskLevel = document.getElementById('riskLevel');

    if (pieCheckbox.checked) {
        riskLevelRow.style.display = '';
        riskLevel.disabled = false;
    } else {
        riskLevelRow.style.display = 'none';
        riskLevel.disabled = true;
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

// Display NAS modal
document.getElementById('retrieveFeeDetailsButton').addEventListener('click', function (e) {
    // Prevent default button behavior
    e.preventDefault();

    // Retrieve the client name from the input field
    var clientName = document.getElementById('clientSelect').value;

    // Check if the input field is empty
    if (clientName.trim() === '') {
        alert('Please enter a client name.');
        return; // Exit the method if the field is empty
    }

    $.ajax({
        url: '/QC7Form/RetrieveNASFeeDetails',
        method: 'GET',
        data: {
            clientName: clientName
        },
        success: function (data) {

            if (!data.success) {
                // Alert if no client is found
                alert(data.message);
                return; // Exit the function if no client found
            }

            var data = data.data;

            // Group fee details by QC7FormID
            var groupedFeeDetails = {};
            data.forEach(function (feeDetail) {
                if (!groupedFeeDetails.hasOwnProperty(feeDetail.qC7FormID)) {
                    groupedFeeDetails[feeDetail.qC7FormID] = [];
                }
                groupedFeeDetails[feeDetail.qC7FormID].push(feeDetail);
            });

            // Set the modal title with the client name
            var clientName = data[0].client;
            $('#feeDetailsModalLabel').text('Fee Details for Client: ' + clientName);

            // Build the tbody HTML
            var tbodyHtml = '';
            Object.keys(groupedFeeDetails).forEach(function (qc7FormID) {
                var feeDetailsList = groupedFeeDetails[qc7FormID];

                // Display the QC7 Form File reference, Client Name, and Financial Year End in the first row
                tbodyHtml += '<tr>' +
                    '<td rowspan="' + feeDetailsList.length + '">' +
                    '<a href="/QC7Form/ViewQC7Form?id=' + feeDetailsList[0].qC7FormID + '" style="color: #007bff; text-decoration: underline;">' +
                    feeDetailsList[0].fileReference +
                    '</a>' +
                    '</td>' +
                    '<td rowspan="' + feeDetailsList.length + '">' + feeDetailsList[0].client + '</td>' +
                    '<td rowspan="' + feeDetailsList.length + '">' + formatDateToDDMMYYYY(feeDetailsList[0].periodEnded) + '</td>' +
                    '<td>' + feeDetailsList[0].natureOfService + '</td>' +
                    '<td>' + feeDetailsList[0].fee + '</td>' +
                    '<td rowspan="' + feeDetailsList.length + '">' + feeDetailsList[0].grandTotal + '</td>' +
                    '<td rowspan="' + feeDetailsList.length + '">' + feeDetailsList[0].auditFee + '</td>' +
                    '<td rowspan="' + feeDetailsList.length + '">' + feeDetailsList[0].feeConcentration + '</td>' +
                    '</tr>';

                // Display the fee details for this QC7 form in subsequent rows, except the first one
                for (var i = 1; i < feeDetailsList.length; i++) {
                    var feeDetail = feeDetailsList[i];
                    tbodyHtml += '<tr>' +
                        '<td>' + feeDetail.natureOfService + '</td>' +
                        '<td>' + feeDetail.fee + '</td>' +
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

// Function to format the date as dd/mm/yyyy
function formatDateToDDMMYYYY(dateString) {
    var date = new Date(dateString); // Create a Date object from the string
    var day = String(date.getDate()).padStart(2, '0'); // Get the day, add leading zero if needed
    var month = String(date.getMonth() + 1).padStart(2, '0'); // Get the month, add leading zero if needed (0-based index)
    var year = date.getFullYear(); // Get the full year

    return day + '/' + month + '/' + year; // Return the formatted date
}