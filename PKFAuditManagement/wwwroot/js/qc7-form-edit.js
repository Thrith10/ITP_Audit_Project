$(document).ready(function () {
    toggleRiskLevel();
    toggleSectionBResult();
    toggleSignificantRisk();
    toggleSTR();
    toggleConSTR();

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

        if (!isNaN(comp1) && !isNaN(timeCosts) && timeCosts !== 0) {
            var priorYearRecoveryRate = (comp1 / timeCosts) * 100;
            priorYearRecoveryRate = priorYearRecoveryRate.toFixed(2);
            $("#PriorYearRecoveryRate").val(priorYearRecoveryRate);
            $("#PriorYearRecoveryRateHidden").val(priorYearRecoveryRate);
        } else {
            $("#PriorYearRecoveryRate").val("");
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
    var strCheckbox = $('#isSuspiciousTransactionReportFiled');
    var rationaleSTRRow = $('#strRationale');

    if (strCheckbox.is(':checked')) {
        rationaleSTRRow.show();
    } else {
        rationaleSTRRow.hide();
    }
}

// Function to display comment for suspicious transaction report field in conclusion based on checkbox value
function toggleConSTR() {
    var strCheckbox = $('#anySuspiciousTransactionReportFiled');
    var rationaleSTRRow = $('#suspiciousTransactionReportPriorYearRow');

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