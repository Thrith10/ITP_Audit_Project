document.getElementById('addService').addEventListener('click', addService);

function addService() {
    const serviceDiv = document.createElement('div');
    serviceDiv.className = 'service input-field';
    serviceDiv.innerHTML = `
                <label>Nature of Service:</label>
                <input type="text" name="nature_of_service[]">
                <label>Fee:</label>
                <input type="number" name="fee[]" step="0.01">
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
    const feeConcentration = (totalFee / auditFee) * 100;
    document.getElementById('feeConcentration').value = feeConcentration.toFixed(2);
}

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