$("#autocomplete").autocomplete({
    source: function (request, response) {
        $.ajax({
            url: '/QC6Form/GetAllClients',
            type: 'GET',
            success: function (data) {
                var matcher = new RegExp("^" + $.ui.autocomplete.escapeRegex(request.term), "i");
                response($.grep(data, function (item) {
                    return matcher.test(item);
                }));
            }
        });
    }
});

$("#industrySelect").select2({
    theme: "bootstrap",
    width: '100%',
    createSearchChoice: function (term) {
        return false;
    }
});

// Add event listener to the "Add Service" button
document.getElementById('addService').addEventListener('click', addService);

// Ensure input values are formatted to two decimal places on blur
$(document).on("blur", "#auditFee, #EstimatedFee, #BudgetedTimeCost, input[name^='Services'][name$='.Fee']", function () {
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

$(document).ready(function () {
    // Function to update the Budgeted fee recovery rate
    function updateBudgetedFeeRecoveryRate() {
        var estimatedFee = parseFloat($("#EstimatedFee").val());
        var budgetedTimeCost = parseFloat($("#BudgetedTimeCost").val());
        var commentBox = $("#commentBoxContainer");

        if (!isNaN(estimatedFee) && !isNaN(budgetedTimeCost) && budgetedTimeCost !== 0) {
            var budgetedFeeRecoveryRate = (estimatedFee / budgetedTimeCost) * 100;
            budgetedFeeRecoveryRate = budgetedFeeRecoveryRate.toFixed(2);
            $("#BudgetedFeeRecoveryRate").val(budgetedFeeRecoveryRate);
            $("#BudgetedFeeRecoveryRateHidden").val(budgetedFeeRecoveryRate);

            // Check if recovery rate is below 30%
            if (budgetedFeeRecoveryRate < 30) {
                commentBox.show(); // Show the comment box
            } else {
                commentBox.hide(); // Hide the comment box
            }

        } else {
            $("#BudgetedFeeRecoveryRate").val("");
            commentBox.hide(); // Hide the comment box if values are not valid
        }
    }

    // Disables or hides the sub forms for QC6 on click
    $('#toggleSubForm1').change(function () {
        if (this.checked) {
            // Hide the table
            $('#tableContainer1').hide();
        } else {
            // Show the table
            $('#tableContainer1').show();
        }
    });

    // Disables or hides the sub forms for QC6 on click
    $('#toggleSubForm2').change(function () {
        if (this.checked) {
            // Hide the table
            $('#tableContainer2').hide();
        } else {
            // Show the table
            $('#tableContainer2').show();
        }
    });

    // Update the Budgeted fee recovery rate when Estimated fee or Budgeted time cost changes
    $("#EstimatedFee, #BudgetedTimeCost").on("input", function () {
        updateBudgetedFeeRecoveryRate();
    });


    // Initial update of Budgeted fee recovery rate
    updateBudgetedFeeRecoveryRate();

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

    // Function to toggle the comment input field based on radio selection
    function toggleCommentInput() {
        var yesSelected = document.getElementById('outstandingUnpaidFeesYes').checked;
        var commentRow = document.getElementById('outstandingUnpaidFeesRow');
        var commentInput = document.getElementById('outstandingUnpaidFeesCommentInput');

        if (yesSelected) {
            commentRow.style.display = 'flex';  // Show the comment row
        } else {
            commentRow.style.display = 'none';  // Hide the comment row
            commentInput.value = '';  // Clear the comment input field
        }
    }

    // Attach event listeners to the radio buttons
    document.getElementById('outstandingUnpaidFeesYes').addEventListener('change', toggleCommentInput);
    document.getElementById('outstandingUnpaidFeesNo').addEventListener('change', toggleCommentInput);

    // Initial load: Call the function to ensure the correct visibility based on the current selection
    toggleCommentInput();

    // Toggling checkbox for any significant risk displays the comment box
    function toggleSignificantRisk() {
        var yesSelected = document.getElementById('anySignificantRiskYes').checked;
        var significantRiskRow = document.getElementById('significantRiskRow');
        var significantRiskComment = document.getElementById('significantRiskComment');

        if (yesSelected) {
            significantRiskRow.style.display = '';
            significantRiskComment.disabled = false;
        } else {
            significantRiskRow.style.display = 'none';
            significantRiskComment.disabled = true;
            significantRiskComment.value = '';
        }
    }

    // Attach event listeners to the radio buttons
    document.getElementById('anySignificantRiskYes').addEventListener('change', toggleSignificantRisk);
    document.getElementById('anySignificantRiskNo').addEventListener('change', toggleSignificantRisk);

    // Initial load: Call the function to ensure the correct visibility based on the current selection
    toggleSignificantRisk();

    // Check if either one is filled up
    document.getElementById('qc6Form').addEventListener('submit', function (event) {
        // Get the input fields
        const documentNameInput = document.querySelector('input[name="AdditionalDocuments[0].DocumentName"]');
        const fileInput = document.querySelector('input[name="AdditionalDocuments[0].File"]');
        const submitButton = document.getElementById('submit-btn');
        const tooltip = document.getElementById('tooltip');

        // Check if either input is filled
        const isDocumentNameFilled = documentNameInput.value.trim() !== '';
        const isFileInputFilled = fileInput.files.length > 0;

        // Prevent form submission if exactly one input is filled
        if (isDocumentNameFilled !== isFileInputFilled) {
            event.preventDefault();

            // Show the tooltip
            tooltip.style.display = 'block';

            // Scroll to the tooltip
            tooltip.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }

        // Add the .is-invalid class to the inputs
        if (isDocumentNameFilled) {
            fileInput.classList.add('is-invalid');
        } else if (isFileInputFilled) {
            documentNameInput.classList.add('is-invalid');
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

// Function to toggle the visibility of the reasons input field
function togglePredecessorReasonsInput() {
    var checkbox = document.getElementById("predecessorAuditorCheckbox");
    var reasonsContainer = document.getElementById("reasonsContainer");
    var reasonsInput = document.getElementById("ReasonsForDiscontinuance");

    if (checkbox.checked) {
        reasonsContainer.style.display = "";  // Show reasons input
        reasonsInput.disabled = false;
        reasonsInput.setAttribute("required", "required"); // Mark as required
    } else {
        reasonsContainer.style.display = "none";    // Hide reasons input
        reasonsInput.removeAttribute("required"); // Remove required attribute
        reasonsInput.disabled = true;
        reasonsInput.value = ""; // Clear the input value if hidden
    }
}

// Toggling checkbox for suspicious transaction report displays the comment box
function toggleRationaleSTR() {
    var strCheckbox = document.getElementById('strCheckbox');
    var rationaleSTRRow = document.getElementById('rationaleSTRRow');
    var rationaleSTRRowInput = document.getElementById('rationaleSTRRowInput');

    if (strCheckbox.checked) {
        rationaleSTRRow.style.display = '';
        rationaleSTRRowInput.disabled = false;
    } else {
        rationaleSTRRow.style.display = 'none';
        rationaleSTRRowInput.disabled = true;
        rationaleSTRRowInput.value = '';
    }
}
// Display NAS modal
document.getElementById('retrieveFeeDetailsButton').addEventListener('click', function (e) {
    // Prevent default button behavior
    e.preventDefault(); 

    // Retrieve the client name from the input field
    var clientName = document.getElementById('autocomplete').value;

    // Check if the input field is empty
    if (clientName.trim() === '') {
        alert('Please enter a client name.');
        return; // Exit the method if the field is empty
    }

    $.ajax({
        url: '/QC6Form/RetrieveNASFeeDetails',
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

            // Group fee details by QC6FormID
            var groupedFeeDetails = {};
            data.forEach(function (feeDetail) {
                if (!groupedFeeDetails.hasOwnProperty(feeDetail.qC6FormID)) {
                    groupedFeeDetails[feeDetail.qC6FormID] = [];
                }
                groupedFeeDetails[feeDetail.qC6FormID].push(feeDetail);
            });

            // Set the modal title with the client name
            var clientName = data[0].prospectiveClient;
            $('#feeDetailsModalLabel').text('Fee Details for Client: ' + clientName);

            // Build the tbody HTML
            var tbodyHtml = '';
            Object.keys(groupedFeeDetails).forEach(function (qc6FormID) {
                var feeDetailsList = groupedFeeDetails[qc6FormID];

                // Display the QC6 Form File reference, Client Name, and Financial Year End in the first row
                tbodyHtml += '<tr>' +
                    '<td rowspan="' + feeDetailsList.length + '">' +
                    '<a href="/QC6Form/ViewQC6Form?id=' + feeDetailsList[0].qC6FormID + '" style="color: #007bff; text-decoration: underline;">' +
                    feeDetailsList[0].fileReference +
                    '</a>' +
                    '</td>' +
                    '<td rowspan="' + feeDetailsList.length + '">' + feeDetailsList[0].prospectiveClient + '</td>' +
                    '<td rowspan="' + feeDetailsList.length + '">' + formatDateToDDMMYYYY(feeDetailsList[0].periodEnded) + '</td>' +
                    '<td>' + feeDetailsList[0].natureOfService + '</td>' +
                    '<td>' + feeDetailsList[0].fee + '</td>' +
                    '<td rowspan="' + feeDetailsList.length + '">' + feeDetailsList[0].grandTotal + '</td>' + 
                    '<td rowspan="' + feeDetailsList.length + '">' + feeDetailsList[0].auditFee + '</td>' + 
                    '<td rowspan="' + feeDetailsList.length + '">' + feeDetailsList[0].feeConcentration + '</td>' + 
                    '</tr>';

                // Display the fee details for this QC6 form in subsequent rows, except the first one
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

// Function to check for duplicate document names
function checkDuplicateDocuments() {
    const numOfDocuments = 5; // Replace with the actual number of documents
    const docInputs = [];

    for (let i = 0; i < numOfDocuments; i++) {
        const docInput = document.querySelector(`input[name="AdditionalDocuments[${i}].DocumentName"]`);
        if (docInput) {
            docInputs.push(docInput);
        }
    }
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
        const docName = input.value.trim().toLowerCase(); // Normalize to lowercase for comparison
        // Check if the current input is not a file input
        if (input.type !== 'file') {
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
        }
    });


    // Initialize Bootstrap tooltips for invalid inputs
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
document.getElementById('qc6Form').addEventListener('submit', function (event) {
    // Prevent default validation and handle manually
    event.preventDefault(); // Stop the form from submitting

    // Clear validation classes from file inputs without removing them
    const fileInputs = this.querySelectorAll('input[type="file"]');
    fileInputs.forEach(input => {
        input.classList.remove('is-invalid', 'is-valid', 'was-validated'); // Remove validation classes
        // Optionally remove tooltip attributes if needed
        input.removeAttribute('data-bs-toggle');
        input.removeAttribute('data-bs-placement');
        input.removeAttribute('title');
    });

    // Remove Bootstrap's automatic validation classes from the form
    this.classList.remove('was-validated');
    this.classList.remove('is-invalid');
    // Check for duplicates in document names
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
