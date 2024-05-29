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
    }
}