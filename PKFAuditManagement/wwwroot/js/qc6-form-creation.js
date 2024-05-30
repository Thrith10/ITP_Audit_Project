document.getElementById('addService').addEventListener('click', addService);

function addService() {
    const serviceDiv = document.createElement('div');
    serviceDiv.className = 'row mb-3 service input-field';
    serviceDiv.innerHTML = `
        <div class="col-sm-6">
            <label>Nature of Service:</label>
            <input type="text" name="nature_of_service[]" class="form-control">
        </div>
        <div class="col-sm-6">
            <label>Fee:</label>
            <input type="number" name="fee[]" step="0.01" class="form-control">
        </div>
    `;
    document.getElementById('services').appendChild(serviceDiv);
}

function calculateTotalAndConcentration() {
    const fees = document.getElementsByName('fee[]');
    let totalFee = 0;
    for (let fee of fees) {
        totalFee += parseFloat(fee.value) || 0;
    }
    document.getElementById('grandTotal').value = totalFee.toFixed(2);

    const auditFee = parseFloat(document.getElementById('auditFee').value) || 0;
    const feeConcentration = totalFee / auditFee * 100;
    document.getElementById('feeConcentration').value = feeConcentration.toFixed(2);
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

        if (!isNaN(estimatedFee) && !isNaN(budgetedTimeCost) && budgetedTimeCost !== 0) {
            var budgetedFeeRecoveryRate = (estimatedFee / budgetedTimeCost).toFixed(2);
            $("#BudgetedFeeRecoveryRate").val(budgetedFeeRecoveryRate);
            $("#BudgetedFeeRecoveryRateHidden").val(budgetedFeeRecoveryRate);
        } else {
            $("#BudgetedFeeRecoveryRate").val("");
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
});



// Toggling checkbox for transnational audit displays the comment box
function toggleTransnationalAuditRow() {
    var transnationalAuditCheckbox = document.getElementById('transnationalAuditCheckbox');
    var transnationalAuditRow = document.getElementById('transnationalAuditRow');
    var transnationalAuditComment = document.getElementById('transnationalAuditComment');

    if (transnationalAuditCheckbox.checked) {
        transnationalAuditRow.style.display = '';
        transnationalAuditComment.disabled = false;
    } else {
        transnationalAuditRow.style.display = 'none';
        transnationalAuditComment.disabled = true;
        transnationalAuditComment.value = '';
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
        riskLevel.value = '';
    }
}

// Toggling checkbox for any significant risk displays the comment box
function toggleSignificantRisk() {
    var significantRiskCheckbox = document.getElementById('significantRiskCheckbox');
    var significantRiskRow = document.getElementById('significantRiskRow');
    var significantRiskComment = document.getElementById('significantRiskComment');

    if (significantRiskCheckbox.checked) {
        significantRiskRow.style.display = '';
        significantRiskComment.disabled = false;
    } else {
        significantRiskRow.style.display = 'none';
        significantRiskComment.disabled = true;
        significantRiskComment.value = '';
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
