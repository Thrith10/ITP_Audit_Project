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