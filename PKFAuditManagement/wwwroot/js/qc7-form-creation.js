$(document).ready(function () {
    // Function to update the Prior year’s recovery rate
    function updatePriorYearRecoveryRate() {
        var comp1 = parseFloat($("#comp1").val());
        var timeCosts = parseFloat($("#timeCosts").val());

        if (!isNaN(comp1) && !isNaN(timeCosts) && timeCosts !== 0) {
            var priorYearRecoveryRate = (comp1 / timeCosts).toFixed(2);
            $("#PriorYearRecoveryRate").val(priorYearRecoveryRate);
            $("#PriorYearRecoveryRateHidden").val(priorYearRecoveryRate);
        } else {
            $("#PriorYearRecoveryRate").val("");
        }
    }

    // Function to update the Proposed Recovery Rate
    function updateProposedRecoveryRate() {
        var comp1 = parseFloat($("#comp1").val());
        var budgetedTimeCost = parseFloat($("#budgetedTimeCost").val());

        if (!isNaN(comp1) && !isNaN(budgetedTimeCost) && budgetedTimeCost !== 0) {
            var proposedRecoveryRateCurrentYear = (comp1 / budgetedTimeCost).toFixed(2);
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
    $("#comp1, #budgetedTimeCost").on("input", function () {
        updateProposedRecoveryRate();
    });

    // Initial update of Prior year’s recovery rate
    updateProposedRecoveryRate();
});

// Toggling checkbox for suspicious transaction report for prior year displays the comment box
function toggleSuspiciousTransactionReportPriorYear() {
    var suspiciousTransactionReportPriorYearCheckbox = document.getElementById('suspiciousTransactionReportPriorYearCheckbox');
    var suspiciousTransactionReportPriorYearRow = document.getElementById('suspiciousTransactionReportPriorYearRow');
    var suspiciousTransactionReportFiledComment = document.getElementById('suspiciousTransactionReportFiledComment');

    if (suspiciousTransactionReportPriorYearCheckbox.checked) {
        suspiciousTransactionReportPriorYearRow.style.display = '';
        suspiciousTransactionReportFiledComment.disabled = false;
    } else {
        suspiciousTransactionReportPriorYearRow.style.display = 'none';
        suspiciousTransactionReportFiledComment.disabled = true;
        suspiciousTransactionReportFiledComment.value = '';
    }
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
        riskLevel.value = '';
    }
}

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

// Toggling checkbox for risks associated (conclusion section) displays the comment box
function toggleRisksAssociated() {
    var risksAssociatedCheckbox = document.getElementById('risksAssociatedCheckbox');
    var risksAssociatedRow = document.getElementById('risksAssociatedRow');
    var riskExplanationCurrentYearPriorYear = document.getElementById('riskExplanationCurrentYearPriorYear');
    var natureOfSafeguard = document.getElementById('natureOfSafeguard');
    var isSafeguardApplied = document.getElementById('isSafeguardApplied');

    if (risksAssociatedCheckbox.checked) {
        risksAssociatedRow.style.display = '';
        riskExplanationCurrentYearPriorYear.disabled = false;
        natureOfSafeguard.disabled = false;
        isSafeguardApplied.disabled = false;
    } else {
        risksAssociatedRow.style.display = 'none';
        riskExplanationCurrentYearPriorYear.disabled = true;
        natureOfSafeguard.disabled = true;
        isSafeguardApplied.disabled = true;
        // Reset values
        riskExplanationCurrentYearPriorYear.value = '';
        natureOfSafeguard.value = '';
        isSafeguardApplied.value = false;
    }
}