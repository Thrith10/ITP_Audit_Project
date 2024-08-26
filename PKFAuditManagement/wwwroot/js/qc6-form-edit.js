$(document).ready(function () {
    toggleRiskLevel();
    toggleSectionBResult();
    toggleSignificantRisk();
    toggleSTR();

    // Find all service select elements
    const serviceSelects = document.querySelectorAll('select[asp-for$="NatureOfService"]');

    // Loop through each select element and apply the logic
    serviceSelects.forEach(selectElement => {
        showOtherServiceInput(selectElement);
    });

    // Get the value from the hidden field
    const grandTotalHiddenValue = document.getElementById('grandTotalHidden').value;

    // Set the value in the visible, disabled field
    document.getElementById('grandTotal').value = parseFloat(grandTotalHiddenValue).toFixed(2);

    // Get the value from the hidden field
    const feeConcentrationHiddenValue = document.getElementById('feeConcentrationHidden').value;

    // Set the value in the visible, disabled field
    document.getElementById('feeConcentration').value = parseFloat(feeConcentrationHiddenValue).toFixed(2);

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

    // Function to update the Budgeted fee recovery rate
    function updateBudgetedFeeRecoveryRate() {
        var estimatedFee = parseFloat($("#EstimatedFee").val());
        var budgetedTimeCost = parseFloat($("#BudgetedTimeCost").val());
        if (!isNaN(estimatedFee) && !isNaN(budgetedTimeCost) && budgetedTimeCost !== 0) {
            var budgetedFeeRecoveryRate = (estimatedFee / budgetedTimeCost) * 100;
            budgetedFeeRecoveryRate = budgetedFeeRecoveryRate.toFixed(2);
            $("#BudgetedFeeRecoveryRate").val(budgetedFeeRecoveryRate);
            $("#BudgetedFeeRecoveryRateHidden").val(budgetedFeeRecoveryRate);
        } else {
            $("#BudgetedFeeRecoveryRate").val("");
        }
    }

    // Update the Budgeted fee recovery rate when Estimated fee or Budgeted time cost changes
    $("#EstimatedFee, #BudgetedTimeCost").on("input", function () {
        updateBudgetedFeeRecoveryRate();
    });


    // Initial update of Budgeted fee recovery rate
    updateBudgetedFeeRecoveryRate();

    var documentIndex = 1;
    $('#add-more-docs').on('click', function () {
        $('#other-documents-container').append(`
            <div class="document-row card border mb-2" data-index="${documentIndex}">
                <div class="card-body">
                    <input type="text" class="form-control mt-3 mb-3" name="OtherDocuments[${documentIndex}].DocumentName" placeholder="Document Name" required/>
                    <input type="file" class="form-control mb-3" name="OtherDocuments[${documentIndex}].File" accept="application/pdf" required/>
                    <button type="button" class="btn btn-primary btn-sm preview-doc">Preview</button>
                    <button type="button" class="btn btn-danger btn-sm remove-doc">Remove</button>
                </div>
            </div>
        `);
        documentIndex++;
    });

    // Remove a document row
    $(document).on('click', '.remove-doc', function () {
        // Remove the clicked document row
        $(this).closest('.document-row').remove();

        // Reindex the remaining document rows
        $('#other-documents-container .document-row').each(function (index) {
            $(this).attr('data-index', index); // Update the data-index attribute

            // Update the names of the input fields to match the new index
            $(this).find('input[name^="OtherDocuments"]').each(function () {
                var name = $(this).attr('name');
                var newName = name.replace(/\[.*?\]/, '[' + index + ']');
                $(this).attr('name', newName);
            });
        });

        // Decrease the global documentIndex variable 
        documentIndex--;
    });

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
});

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
// Add event listener to the "Add Service" button
document.getElementById('addService').addEventListener('click', addService);

// Function to add a new service field
function addService() {
    const servicesContainer = document.getElementById('services');
    const lastServiceIndex = servicesContainer.children.length;

    const serviceCard = document.createElement('div');
    serviceCard.className = 'card border border-secondary p-3 mb-3';
    serviceCard.innerHTML = `
        <div class="row mb-3 service input-field card-body">
            <h6 class="card-title">Service ${lastServiceIndex}</h6>
            <input type="hidden" asp-for="Services[${lastServiceIndex - 1}].QC6FormFeeDetailID" />
            <div class="col-sm-6">
                <label>Nature of Service:</label>
                <select class="form-control" asp-for="Services[${lastServiceIndex - 1}].NatureOfService" name="Services[${lastServiceIndex - 1}].NatureOfService" onchange="showOtherServiceInput(this)">
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
                    <input type="number" name="Services[${lastServiceIndex - 1}].Fee" step="0.01" class="form-control fee-input" oninput="calculateTotalAndConcentration()" required>
                </div>
            </div>
            <div class="col-sm-12 mt-3" id="otherServiceInput-${lastServiceIndex - 1}" style="display: none;">
                <label>Name of Non-Audit Service<span class="text-danger"> *</span></label>
                <input type="text" name="Services[${lastServiceIndex - 1}].OtherService" class="form-control">
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
    // Find the parent card element
    const card = button.closest('.card');

    // Get the service ID input value before removing the card
    const serviceIdInput = card.querySelector('input[name$="QC6FormFeeDetailID"]');

    if (serviceIdInput) {
        // Create a hidden input for the removed service ID
        const removedServicesContainer = document.getElementById('removedServicesContainer');
        const removedServiceInput = document.createElement('input');
        removedServiceInput.type = 'hidden';
        removedServiceInput.name = 'RemovedServices[]';
        removedServiceInput.value = serviceIdInput.value; // Pass the service ID here

        // Append the hidden input to the container
        removedServicesContainer.appendChild(removedServiceInput);
    }

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

    // Recalculate the total fee after removal
    calculateTotalAndConcentration();
}

// Function to display additional text field based on service field selection
function showOtherServiceInput(selectElement) {
    const selectedValue = selectElement.value;

    // Extract the index from the element's name (e.g., "Services[0].NatureOfService")
    const index = selectElement.name.split('[')[1].split(']')[0];

    // Find the "Other Service" input field associated with this select element
    const otherServiceInput = document.getElementById(`otherServiceInput-${index}`);

    const inputField = otherServiceInput.querySelector('input');

    if (otherServiceInput) {
        // Show or hide the "Other Service" input field based on the selected value
        if (selectedValue === 'Other Non-Audit Services') {
            otherServiceInput.style.display = 'block';
            inputField.setAttribute('required', 'required'); // Add required attribute
        } else {
            otherServiceInput.style.display = 'none';
            inputField.removeAttribute('required'); // Remove required attribute
        }
    }
}

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

    e.preventDefault(); // Prevent default button behavior

    $.ajax({
        url: '/QC6Form/RetrieveNASFeeDetails',
        method: 'GET',
        success: function (data) {
            console.log('Data:', data);

            // Group fee details by QC6FormID
            var groupedFeeDetails = {};
            data.forEach(function (feeDetail) {
                if (!groupedFeeDetails.hasOwnProperty(feeDetail.qC6FormID)) {
                    groupedFeeDetails[feeDetail.qC6FormID] = [];
                }
                groupedFeeDetails[feeDetail.qC6FormID].push(feeDetail);
            });
            console.log('Grouped Fee Details:', groupedFeeDetails);

            // Build the tbody HTML
            var tbodyHtml = '';
            Object.keys(groupedFeeDetails).forEach(function (qc6FormID) {
                var feeDetailsList = groupedFeeDetails[qc6FormID];
                console.log('QC6FormID:', qc6FormID, 'Fee Details List:', feeDetailsList);

                // Display the QC6 Form File reference in one row
                tbodyHtml += '<tr>' +
                    '<td rowspan="' + feeDetailsList.length + '">' + feeDetailsList[0].fileReference + '</td>' +
                    '<td>' + feeDetailsList[0].fee + '</td>' +
                    '<td>' + feeDetailsList[0].natureOfService + '</td>' +
                    '</tr>';

                // Display the fee details for this QC6 form in subsequent rows
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