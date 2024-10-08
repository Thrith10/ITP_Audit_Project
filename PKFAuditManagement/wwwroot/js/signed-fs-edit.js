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