$(document).ready(function () {
    toggleRiskLevel();
    toggleSectionBResult();
    toggleSignificantRisk();
    toggleSTR();
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