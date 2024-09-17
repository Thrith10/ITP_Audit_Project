document.addEventListener('DOMContentLoaded', function () {
    // Tab switching logic
    var manualTab = document.getElementById('manual-tab');
    var excelTab = document.getElementById('excel-tab');
    var manualContent = document.getElementById('manual-content');
    var excelContent = document.getElementById('excel-content');

    // Ensure sidebar is closed on page load
    var sidebar = document.querySelector('.sidebar'); // Adjust this selector to your sidebar
    var toggleSidebarBtn = document.querySelector('.toggle-sidebar-btn'); // The hamburger menu button
    if (toggleSidebarBtn) {
        toggleSidebarBtn.click();
    }

    // Switch to Manual Quiz Creation tab
    manualTab.addEventListener('click', function () {
        manualTab.classList.add('active');
        excelTab.classList.remove('active');
        manualContent.classList.add('active');
        excelContent.classList.remove('active');
    });

    // Switch to Excel Quiz Creation tab
    excelTab.addEventListener('click', function () {
        excelTab.classList.add('active');
        manualTab.classList.remove('active');
        excelContent.classList.add('active');
        manualContent.classList.remove('active');
    });
});