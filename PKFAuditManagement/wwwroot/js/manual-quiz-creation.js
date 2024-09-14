document.addEventListener('DOMContentLoaded', function () {
    var selectParticipantsBtn = document.getElementById('select-participants-btn');
    var participantsModal = document.getElementById('participants-modal');
    var closeModal = document.querySelector('.close-modal');
    var confirmParticipantsBtn = document.getElementById('confirm-participants-btn');
    var participantsList = document.querySelector('.participants-list');
    var participantsCountSpan = document.getElementById('participants-count');
    var selectedParticipants = []; // Stores selected participants across all pages
    var selectedParticipantsInput = document.getElementById('SelectedParticipants'); // Hidden input for selected participants
    const selectedParticipantsModal = document.getElementById('selectedParticipantsModal');
    const closeSelectedParticipantsModal = document.getElementById('closeSelectedParticipantsModal');
    const selectedParticipantsList = document.getElementById('selectedParticipantsList');
    let currentPage = 1;
    const emailsPerPage = 30;
    let totalPages = 1;
    var quizStartInput = document.getElementById('QuizStart');

    if (quizStartInput) {
        var now = new Date();

        // Convert to Singapore time (UTC+8)
        var singaporeOffset = 8 * 60; // offset in minutes
        var localOffset = now.getTimezoneOffset(); // offset in minutes
        var singaporeTime = new Date(now.getTime() + (singaporeOffset + localOffset) * 60000);

        // Format the date and time to yyyy-MM-ddTHH:mm format for the input value
        var year = singaporeTime.getFullYear();
        var month = String(singaporeTime.getMonth() + 1).padStart(2, '0');
        var day = String(singaporeTime.getDate()).padStart(2, '0');
        var hours = String(singaporeTime.getHours()).padStart(2, '0');
        var minutes = String(singaporeTime.getMinutes()).padStart(2, '0');
        var formattedDate = `${year}-${month}-${day}T${hours}:${minutes}`;

        // Set the min attribute to the formatted date-time for validation
        quizStartInput.min = formattedDate;
        quizStartInput.placeholder = formattedDate;
        quizStartInput.value = formattedDate;

        // Open the DateTime picker on focus or click anywhere in the input field
        quizStartInput.addEventListener('click', function () {
            quizStartInput.showPicker(); // Opens the date-time picker
        });
    }

    participantsCountSpan.addEventListener('click', function () {
        // Get selected participants from the hidden input field, split by ';', and filter out any empty entries
        const selectedParticipants = selectedParticipantsInput.value.split(';').filter(email => email.trim() !== '');

        // Clear the list before populating
        selectedParticipantsList.innerHTML = '';

        if (selectedParticipants.length > 0) {
            // Populate the list with selected participants
            selectedParticipants.forEach(email => {
                const listItem = document.createElement('li');
                listItem.textContent = email;
                selectedParticipantsList.appendChild(listItem);
            });
        } else {
            // Display a message if no participants are selected
            const emptyMessage = document.createElement('li');
            emptyMessage.textContent = 'No participants selected';
            selectedParticipantsList.appendChild(emptyMessage);
        }

        // Show the modal
        selectedParticipantsModal.style.display = 'block';
    });
    // Event listener to close the modal
    closeSelectedParticipantsModal.addEventListener('click', function () {
        selectedParticipantsModal.style.display = 'none';
    });

    // Close modal when clicking outside of the modal
    window.addEventListener('click', function (event) {
        if (event.target == selectedParticipantsModal) {
            selectedParticipantsModal.style.display = 'none';
        }
    });

    // Confirm button functionality to store selected participants
    confirmParticipantsBtn.addEventListener('click', function () {
        selectedParticipantsInput.value = selectedParticipants.join(';'); // Store selected participants in the hidden input
        participantsCountSpan.textContent = `${selectedParticipants.length} participants selected`;
        participantsModal.style.display = 'none';

        // Highlight the participants count when participants are selected
        if (selectedParticipants.length > 0) {
            participantsCountSpan.style.fontWeight = 'bold';
            participantsCountSpan.style.color = 'green';
        } else {
            participantsCountSpan.style.fontWeight = 'normal';
            participantsCountSpan.style.color = 'blue';
        }
    });

    // Participant Selection
    selectParticipantsBtn.addEventListener('click', function () {
        fetchParticipants(); // Fetch participants each time modal opens
        participantsModal.style.display = 'block';
    });

    // Close modal
    closeModal.addEventListener('click', function () {
        participantsModal.style.display = 'none';
    });

    window.addEventListener('click', function (event) {
        if (event.target == participantsModal) {
            participantsModal.style.display = 'none';
        }
    });

    function fetchParticipants(page = 1) {
        fetch('/Quizzes/GetAllUsers')
            .then(response => response.json())
            .then(users => {
                const startIndex = (page - 1) * emailsPerPage;
                const endIndex = page * emailsPerPage;
                const paginatedUsers = users.slice(startIndex, endIndex);

                // Clear the list
                participantsList.innerHTML = '';

                // Calculate total pages
                totalPages = Math.ceil(users.length / emailsPerPage);

                // Populate participants list with paginated data
                paginatedUsers.forEach(user => {
                    if (user.email) {
                        const isChecked = selectedParticipants.includes(user.email) ? 'checked' : '';
                        const userItem = `
                                                                        <label>
                                                                            <input type="checkbox" class="participant-checkbox" value="${user.email}" ${isChecked} /> ${user.email}
                                                                        </label>`;
                        participantsList.insertAdjacentHTML('beforeend', userItem);
                    }
                });

                // Update pagination display
                document.querySelector('.current-page').textContent = page;

                // Attach event listeners to checkboxes to update the global array
                const checkboxes = document.querySelectorAll('.participant-checkbox');
                checkboxes.forEach(checkbox => {
                    checkbox.addEventListener('change', function () {
                        if (checkbox.checked) {
                            if (!selectedParticipants.includes(checkbox.value)) {
                                selectedParticipants.push(checkbox.value);
                            }
                        } else {
                            selectedParticipants = selectedParticipants.filter(email => email !== checkbox.value);
                        }
                    });
                });
            })
            .catch(error => console.error('Error fetching participants:', error));
    }

    // Handle pagination for 'Next' button
    document.querySelector('.next-page').addEventListener('click', () => {
        if (currentPage < totalPages) {
            currentPage++;
            fetchParticipants(currentPage);
        }
    });

    // Handle pagination for 'Prev' button
    document.querySelector('.prev-page').addEventListener('click', () => {
        if (currentPage > 1) {
            currentPage--;
            fetchParticipants(currentPage);
        }
    });


    // Confirm button functionality to store selected participants
    confirmParticipantsBtn.addEventListener('click', function () {
        selectedParticipantsInput.value = selectedParticipants.join(';'); // Store selected participants in the hidden input
        participantsCountSpan.textContent = selectedParticipants.length + " participants selected";
        participantsModal.style.display = 'none';
    });

    //Function to check/uncheck all checkboxes in the current page
    function updateCheckboxes(checkedStatus) {
        const checkboxes = document.querySelectorAll('.participant-checkbox');
        checkboxes.forEach(checkbox => {
            checkbox.checked = checkedStatus;
        });
    }
    // Trigger file input click when 'Upload via Excel' button is clicked
    document.getElementById('upload-participants-btn').addEventListener('click', function () {
        document.getElementById('ExcelFile').click();
    });

    // Handle the file selection
    document.getElementById('ExcelFile').addEventListener('change', handleFileSelect);
    //Handle Excel
    async function handleFileSelect() {
        var file = document.getElementById('ExcelFile').files[0];
        if (!file) return;

        var reader = new FileReader();
        reader.onload = async function (event) {
            var data = new Uint8Array(event.target.result);
            var workbook = XLSX.read(data, { type: 'array' });

            // Get the first sheet
            var firstSheetName = workbook.SheetNames[0];
            var sheet = workbook.Sheets[firstSheetName];

            // Extract email addresses from the first column, excluding the header
            var sheetData = XLSX.utils.sheet_to_json(sheet, { header: 1 });
            var emails = [];

            for (var i = 1; i < sheetData.length; i++) {
                var email = sheetData[i][0];
                if (typeof email === 'string' && email.trim() !== '') {
                    emails.push(email.trim());
                }
            }

            // Send emails to backend for validation
            try {
                const response = await fetch('/Quizzes/EmailValidation', {
                    method: 'POST',  // Use POST method
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(emails)  // Send emails as JSON
                });

                if (response.ok) {
                    const result = await response.text(); // Expecting a string of emails delimited by ';'
                    document.getElementById('SelectedParticipants').value = result;
                    const validEmails = result.split(';').filter(email => email.trim() !== ''); // Split and filter out any empty entries
                    document.getElementById('participants-count').textContent = `${validEmails.length} participants selected`;
                    if (validEmails.length > 0) {
                        participantsCountSpan.style.fontWeight = 'bold';
                        participantsCountSpan.style.color = 'green';
                    } else {
                        participantsCountSpan.style.fontWeight = 'normal';
                        participantsCountSpan.style.color = 'blue';
                    }
                    console.log('Valid Emails:', result);
                } else {
                    console.error('Error validating emails');
                }
            } catch (error) {
                console.error('Error sending request:', error);
            }
        };

        reader.readAsArrayBuffer(file);
    }

});



// Additional quiz logic for options and questions
function updateDropdown(questionIndex) {
    var optionsContainer = document.querySelector(`[data-index='${questionIndex}'] .options-container`);
    var select = document.querySelector(`[data-index='${questionIndex}'] select[name*='CorrectOptionText']`);
    select.innerHTML = ''; // Clear existing options

    var inputs = optionsContainer.querySelectorAll('input[type="text"]');
    inputs.forEach(function (input) {
        var option = document.createElement('option');
        option.value = input.value; // Use input value as option value
        option.text = input.value;
        select.appendChild(option);
    });
}

function getOptions(questionIndex) {
    var optionsContainer = document.querySelector(`[data-index='${questionIndex}'] .options-container`);
    return Array.from(optionsContainer.querySelectorAll('input[type="text"]')).map(input => input.value.trim());
}

function isOptionUnique(options, newValue) {
    return options.filter(option => option === newValue).length === 1;
}

function showInvalidOptionsPopup() {
    alert('Some options are invalid. Please correct duplicate options before submitting.');
}

document.addEventListener('input', function (e) {
    if (e.target && e.target.matches('.option-group input[type="text"]')) {
        var questionCard = e.target.closest('.card');
        var questionIndex = questionCard.getAttribute('data-index');
        var allOptions = getOptions(questionIndex);
        var newValue = e.target.value.trim();

        // Update dropdown for correct answer
        updateDropdown(questionIndex);

        // Remove existing validation classes
        var optionInputs = document.querySelectorAll(`[data-index='${questionIndex}'] .option-group input[type="text"]`);
        optionInputs.forEach(function (input) {
            input.classList.remove('is-invalid');
        });

        // Apply invalid class only to duplicate options
        allOptions.forEach(function (option) {
            if (!isOptionUnique(allOptions, option)) {
                var invalidInputs = Array.from(optionInputs).filter(input => input.value.trim() === option);
                invalidInputs.forEach(input => input.classList.add('is-invalid'));
            }
        });
    }
});

document.addEventListener('click', function (e) {
    if (e.target && e.target.matches('.add-question')) {
        var quizContainer = document.getElementById('quiz-container');
        var newIndex = quizContainer.children.length;
        var newCard = document.createElement('div');
        newCard.className = 'card mb-3';
        newCard.setAttribute('data-index', newIndex);
        newCard.innerHTML = `
                                                                        <div class="card-body">
                                                                            <div class="d-flex justify-content-between align-items-center">
                                                                                <h5 class="card-title">Question ${newIndex + 1}:</h5>
                                                                                <button type="button" class="btn btn-danger btn-sm remove-question">
                                                                                    <i class="bi bi-trash"></i> Remove Question
                                                                                </button>
                                                                            </div>
                                                                            <div class="form-group">
                                                                                <input type="text" class="form-control" name="Questions[${newIndex}].Description" required />
                                                                            </div>
                                                                            <div class="form-group">
                                                                                <label>Options:</label>
                                                                                <div class="options-container">
                                                                                    <div class="input-group mb-2 option-group">
                                                                                        <input type="text" class="form-control" name="Questions[${newIndex}].Options[0].OptionText" data-option-index="0" required />
                                                                                        <button type="button" class="btn btn-danger btn-sm remove-option">
                                                                                            <i class="bi bi-trash"></i>
                                                                                        </button>
                                                                                    </div>
                                                                                </div>
                                                                                <button type="button" class="btn btn-secondary btn-sm add-option" data-question-index="${newIndex}">
                                                                                    <i class="bi bi-plus-lg"></i> Add Option
                                                                                </button>
                                                                            </div>
                                                                            <div class="form-group">
                                                                                <label>Select the correct answer:</label>
                                                                                <select class="form-control" name="Questions[${newIndex}].CorrectOptionText" required>
                                                                                    <option value="">Select an option</option>
                                                                                </select>
                                                                            </div>
                                                                        </div>
                                                                    `;
        quizContainer.appendChild(newCard);
    } else if (e.target && e.target.matches('.add-option')) {
        var questionIndex = e.target.getAttribute('data-question-index');
        var optionsContainer = document.querySelector(`[data-index='${questionIndex}'] .options-container`);
        var newIndex = optionsContainer.children.length;
        var newOptionGroup = document.createElement('div');
        newOptionGroup.className = 'input-group mb-2 option-group';
        newOptionGroup.innerHTML = `
                                                                        <input type="text" class="form-control" name="Questions[${questionIndex}].Options[${newIndex}].OptionText" data-option-index="${newIndex}" required />
                                                                        <button type="button" class="btn btn-danger btn-sm remove-option">
                                                                            <i class="bi bi-trash"></i>
                                                                        </button>
                                                                    `;
        optionsContainer.appendChild(newOptionGroup);
        updateDropdown(questionIndex);

        // Hide the add option button if the number of options reaches 5
        if (newIndex >= 4) {
            e.target.style.display = 'none';
        }
    } else if (e.target && e.target.matches('.remove-question')) {
        var cardToRemove = e.target.closest('.card');
        cardToRemove.remove();
        updateIndexes();
    } else if (e.target && e.target.matches('.remove-option')) {
        var optionGroupToRemove = e.target.closest('.option-group');
        var questionIndex = e.target.closest('.card').getAttribute('data-index');
        optionGroupToRemove.remove();
        updateDropdown(questionIndex);

        // Re-enable the add option button if options are fewer than 5
        var optionsContainer = document.querySelector(`[data-index='${questionIndex}'] .options-container`);
        var addOptionButton = document.querySelector(`[data-index='${questionIndex}'] .add-option`);
        if (optionsContainer.children.length < 5) {
            addOptionButton.style.display = 'block';
        }
    }
});

function updateIndexes() {
    var cards = document.querySelectorAll('#quiz-container .card');
    cards.forEach(function (card, index) {
        card.setAttribute('data-index', index);
        card.querySelector('.card-title').textContent = `Question ${index + 1}:`;
        var inputs = card.querySelectorAll('input, select');
        inputs.forEach(function (input) {
            var name = input.getAttribute('name');
            if (name) {
                var newName = name.replace(/Questions\[\d+\]/, `Questions[${index}]`);
                input.setAttribute('name', newName);
            }
        });
    });
}

function validateQuiz() {
    var isValid = true;
    var cards = document.querySelectorAll('#quiz-container .card');
    cards.forEach(function (card) {
        var questionIndex = card.getAttribute('data-index');
        var allOptions = getOptions(questionIndex);
        var optionInputs = card.querySelectorAll('.option-group input[type="text"]');

        allOptions.forEach(function (option) {
            if (!isOptionUnique(allOptions, option)) {
                var invalidInputs = Array.from(optionInputs).filter(input => input.value.trim() === option);
                invalidInputs.forEach(input => input.classList.add('is-invalid'));
                isValid = false;
            }
        });
    });

    if (!isValid) {
        showInvalidOptionsPopup();
    }

    return isValid;
}




document.querySelector('#submit-button').addEventListener('click', function (e) {
    if (!validateQuiz()) {
        e.preventDefault(); // Prevent form submission if there are invalid options
    }
});