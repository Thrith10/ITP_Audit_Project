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
    var quizEndInput = document.getElementById('QuizEnd');
    var addTopicBtn = document.getElementById('add-topic');
    var topicsContainer = document.getElementById('topics-container');
    const selectFeedbackFormBtn = document.getElementById('select-feedback-form-btn');
    const feedbackFormModal = document.getElementById('feedback-form-modal'); // Modal element for feedback forms
    const feedbackFormsList = document.getElementById('feedback-forms-list'); // Container for the list of feedback forms
    const closeFeedbackModal = document.getElementById('close-feedback-modal'); // Close button in the modal
    const confirmFeedbackFormBtn = document.getElementById('confirm-feedback-form-btn'); // Confirm selection button
    const selectedFeedbackFormIdInput = document.getElementById('SelectedFeedbackFormId'); // Hidden input for selected feedback form ID
    const selectedFeedbackFormText = document.getElementById('selected-feedback-form-text'); // Text element to show selected feedback form title
    const feedbackSelectedTick = document.getElementById('feedback-selected-tick'); // Green tick icon for selected form

 

    if (quizStartInput && quizEndInput) {
        var now = new Date();

        // Convert to Singapore time (UTC+8)
        var singaporeOffset = 8 * 60; // offset in minutes
        var localOffset = now.getTimezoneOffset(); // offset in minutes
        var singaporeTime = new Date(now.getTime() + (singaporeOffset + localOffset) * 60000);

        // Format the current date and time for Quiz Start (yyyy-MM-ddTHH:mm)
        var year = singaporeTime.getFullYear();
        var month = String(singaporeTime.getMonth() + 1).padStart(2, '0');
        var day = String(singaporeTime.getDate()).padStart(2, '0');
        var hours = String(singaporeTime.getHours()).padStart(2, '0');
        var minutes = String(singaporeTime.getMinutes()).padStart(2, '0');
        var formattedDateStart = `${year}-${month}-${day}T${hours}:${minutes}`;

        // Set Quiz Start input default
        quizStartInput.min = formattedDateStart;
        quizStartInput.placeholder = formattedDateStart;
        quizStartInput.value = formattedDateStart;

        // Open the DateTime picker on focus or click for Quiz Start
        quizStartInput.addEventListener('click', function () {
            quizStartInput.showPicker(); // Opens the date-time picker
        });

        // Set the Quiz End time to 23:59 on the same day for Singapore time
        var endHours = '23';
        var endMinutes = '59';
        var formattedDateEnd = `${year}-${month}-${day}T${endHours}:${endMinutes}`;

        // Set Quiz End input default
        quizEndInput.min = formattedDateEnd;
        quizEndInput.placeholder = formattedDateEnd;
        quizEndInput.value = formattedDateEnd;

        // Open the DateTime picker on focus or click for Quiz End
        quizEndInput.addEventListener('click', function () {
            quizEndInput.showPicker(); // Opens the date-time picker
        });
    }

    // Load feedback forms dynamically (replace with actual API endpoint)
    function loadFeedbackForms() {
        // Fetch available feedback forms from the server
        fetch('/Quizzes/GetFeedbackForms') // Example API endpoint
            .then(response => response.json())
            .then(data => {
                feedbackFormsList.innerHTML = ''; // Clear existing items
                data.forEach(form => {
                    const listItem = document.createElement('li');
                    listItem.className = 'list-group-item list-group-item-action';
                    listItem.textContent = form.title;
                    listItem.dataset.id = form.id;
                    listItem.addEventListener('click', function () {
                        // Remove 'active' class from all items and apply it only to the selected one
                        feedbackFormsList.querySelectorAll('.list-group-item').forEach(item => item.classList.remove('active'));
                        listItem.classList.add('active');

                        // Set the hidden input value with the selected form ID
                        selectedFeedbackFormIdInput.value = form.id;

                        // Update the display text to show the selected feedback form title
                        selectedFeedbackFormText.textContent = form.title;
                        selectedFeedbackFormText.classList.add('selected'); // Add class for styling

                        // Add a green tick beside the selected feedback form text
                        feedbackSelectedTick.style.display = 'inline'; // Show the green tick icon
                    });

                    feedbackFormsList.appendChild(listItem);
                });
            })
            .catch(error => console.error('Error fetching feedback forms:', error));
    }

    // Open modal and load feedback forms
    selectFeedbackFormBtn.addEventListener('click', function () {
        feedbackFormModal.style.display = 'block';
        loadFeedbackForms();
    });

    // Close modal
    closeFeedbackModal.addEventListener('click', function () {
        feedbackFormModal.style.display = 'none';
    });

    // Confirm selection and close modal
    confirmFeedbackFormBtn.addEventListener('click', function () {
        feedbackFormModal.style.display = 'none';
    });

    // Close modal when clicking outside the modal content
    window.addEventListener('click', function (event) {
        if (event.target == feedbackFormModal) {
            feedbackFormModal.style.display = 'none';
        }
    });

    // Add new topic
    addTopicBtn.addEventListener('click', function () {
        var index = topicsContainer.querySelectorAll('.topic-group').length;

        var newTopicHtml = `
        <div class="input-group mb-2 topic-group" data-index="${index}">
            <input type="text" class="form-control" name="Topics[${index}].Name" placeholder="Enter topic name" required />
            <button type="button" class="btn btn-danger btn-sm remove-topic">
                <i class="bi bi-trash"></i> Remove
            </button>
        </div>`;

        topicsContainer.insertAdjacentHTML('beforeend', newTopicHtml);
    });

    // Remove topic
    document.addEventListener('click', function (event) {
        if (event.target && event.target.classList.contains('remove-topic')) {
            var topicGroup = event.target.closest('.topic-group');
            topicGroup.remove();

            // Re-index topics to maintain consistency in form data
            var topicGroups = topicsContainer.querySelectorAll('.topic-group');
            topicGroups.forEach((topicGroup, index) => {
                topicGroup.setAttribute('data-index', index);
                var input = topicGroup.querySelector('input');
                input.setAttribute('name', `Topics[${index}].Name`);
            });
        }
    });


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


    //Function to check/uncheck all checkboxes in the current page
    function updateCheckboxes(checkedStatus) {
        const checkboxes = document.querySelectorAll('.participant-checkbox');
        checkboxes.forEach(checkbox => {
            checkbox.checked = checkedStatus;
        });
    }
    // Trigger file input click when 'Upload via Excel' button is clicked
    document.getElementById('upload-participants-btn').addEventListener('click', function () {
        console.log('Upload button clicked. Opening file selector.');
        document.getElementById('ExcelFile').click();
    });

    // Handle the file selection
    document.getElementById('ExcelFile').addEventListener('change', handleFileSelect);

    // Handle Excel file reading and validation
    async function handleFileSelect() {
        console.log('File selected. Handling file...');

        var file = document.getElementById('ExcelFile').files[0];

        if (!file) {
            console.error('No file selected. Exiting function.');
            return;
        }

        console.log('File details:', file);

        // Check if FileReader is supported
        if (!window.FileReader) {
            console.error('FileReader API is not supported by this browser.');
            return;
        }

        var reader = new FileReader();

        // Debug message to check if reader.onload is being triggered
        reader.onload = async function (event) {
            console.log('FileReader onload triggered.');

            var data = new Uint8Array(event.target.result);

            // Read the workbook from the file
            try {
                var workbook = XLSX.read(data, { type: 'array' });
                console.log('Workbook read successfully:', workbook);
            } catch (err) {
                console.error('Error reading the workbook:', err);
                return;
            }

            // Get the first sheet
            var firstSheetName = workbook.SheetNames[0];
            var sheet = workbook.Sheets[firstSheetName];

            // Extract email addresses from the first column, excluding the header
            var sheetData = XLSX.utils.sheet_to_json(sheet, { header: 1 });
            console.log('Extracted sheet data:', sheetData);

            var emails = [];

            for (var i = 1; i < sheetData.length; i++) {
                var email = sheetData[i][0];
                if (typeof email === 'string' && email.trim() !== '') {
                    emails.push(email.trim());
                }
            }

            console.log('Extracted emails:', emails);

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
                    console.log('Validation result:', result);

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

                } else {
                    console.error('Error validating emails:', response.statusText);
                }
            } catch (error) {
                console.error('Error sending request:', error);
            }
        };

        reader.onerror = function () {
            console.error('Error reading file:', reader.error);
        };

        // Read the file as an array buffer
        try {
            reader.readAsArrayBuffer(file);
            console.log('FileReader is reading the file...');
        } catch (err) {
            console.error('Error starting FileReader:', err);
        }
    }




document.addEventListener('change', function (e) {
    if (e.target && e.target.matches('.question-type-select')) {
        var questionCard = e.target.closest('.card');
        var questionIndex = questionCard.getAttribute('data-index');
        var selectedType = e.target.value;

        // Call the function to update the fields based on the selected question type
        updateQuestionTypeFields(questionIndex, selectedType);
    }
});

function updateQuestionTypeFields(questionIndex, selectedType) {
    var optionsContainer = document.querySelector(`[data-index='${questionIndex}'] .options-container`);

    // Find or create the correct answer container (for True/False, MCQs, etc.)
    var correctAnswerContainer = document.querySelector(`[data-index='${questionIndex}'] .correct-answer-container`);

    // If the correct answer container doesn't exist, create it
    if (!correctAnswerContainer) {
        correctAnswerContainer = document.createElement('div');
        correctAnswerContainer.className = 'form-group correct-answer-container';
        var label = document.createElement('label');
        label.textContent = 'Select the correct answer:';
        correctAnswerContainer.appendChild(label);
        document.querySelector(`[data-index='${questionIndex}'] .card-body`).appendChild(correctAnswerContainer);
    } else {
        // Clear the existing correct answer container
        correctAnswerContainer.innerHTML = '';
        var label = document.createElement('label');
        label.textContent = 'Select the correct answer:';
        correctAnswerContainer.appendChild(label);
    }

    optionsContainer.innerHTML = ''; // Clear existing options

    // True/False question type handling
    if (selectedType == '1') { // True/False
        var trueOption = document.createElement('option');
        trueOption.value = 'True';
        trueOption.text = 'True';

        var falseOption = document.createElement('option');
        falseOption.value = 'False';
        falseOption.text = 'False';

        var select = document.createElement('select');
        select.className = 'form-control';
        select.name = `Questions[${questionIndex}].CorrectOptionText`;
        select.appendChild(trueOption);
        select.appendChild(falseOption);

        correctAnswerContainer.appendChild(select);
    }
    // Single Answer MCQ or Multi-Answer MCQ handling
    else if (selectedType == '2' || selectedType == '3') {
        // Create one initial option field for SingleAnswerMCQ and MultiAnswerMCQ
        createOptionField(questionIndex, 0);

        // Add the "Add Option" button
        var addOptionButton = document.createElement('button');
        addOptionButton.type = 'button';
        addOptionButton.className = 'btn btn-secondary btn-sm add-option';
        addOptionButton.textContent = 'Add Option';
        addOptionButton.setAttribute('data-question-index', questionIndex);
        optionsContainer.appendChild(addOptionButton);

        // Create a select or multi-select dropdown for correct answers
        var select = document.createElement('select');
        select.className = 'form-control';
        select.name = `Questions[${questionIndex}].CorrectOptionText`;
        if (selectedType == '3') {
            select.multiple = true; // Allow multiple selection for Multi-Answer MCQ
            select.classList.add('multi-select'); // Mark for Select2 initialization
        }
        select.required = true;

        correctAnswerContainer.appendChild(select);

        // Update the dropdown with the available options
        updateDropdown(questionIndex);

        // Initialize Select2 for multi-select dropdown
        if (selectedType == '3') {
            $(`[data-index='${questionIndex}'] .multi-select`).select2({
                placeholder: "Select correct answers",
                allowClear: true
            });
        }
    }
}

// Function to create an option field dynamically
    function createOptionField(questionIndex, optionIndex) {
        var optionsContainer = document.querySelector(`[data-index='${questionIndex}'] .options-container`);

        var optionGroup = document.createElement('div');
        optionGroup.className = 'input-group mb-2 option-group';

        var input = document.createElement('input');
        input.type = 'text';
        input.className = 'form-control';
        input.name = `Questions[${questionIndex}].Options[${optionIndex}].OptionText`;
        input.setAttribute('data-option-index', optionIndex); // Tag the input with option index
        input.required = true;

        var removeButton = document.createElement('button');
        removeButton.type = 'button';
        removeButton.className = 'btn btn-danger btn-sm remove-option';
        removeButton.innerHTML = '<i class="bi bi-trash"></i>';
        removeButton.setAttribute('data-option-index', optionIndex); // Tag the remove button with the same option index

        optionGroup.appendChild(input);
        optionGroup.appendChild(removeButton);
        optionsContainer.insertBefore(optionGroup, optionsContainer.querySelector('.add-option')); // Insert before "Add Option" button

        updateDropdown(questionIndex); // Update the dropdown
    }

// Additional quiz logic for options and questions
function updateDropdown(questionIndex) {
    var optionsContainer = document.querySelector(`[data-index='${questionIndex}'] .options-container`);
    var select = document.querySelector(`[data-index='${questionIndex}'] select[name*='CorrectOptionText']`);

    // Ensure the select element exists before attempting to update it
    if (select === null) {
        console.error('CorrectOptionText select element not found for questionIndex:', questionIndex);
        return;
    }

    select.innerHTML = ''; // Clear existing options

    var inputs = optionsContainer.querySelectorAll('input[type="text"]');
    inputs.forEach(function (input) {
        var option = document.createElement('option');
        option.value = input.value; // Use input value as option value
        option.text = input.value;
        select.appendChild(option);
    });

    // Initialize Select2 for the multi-select if required
    if (select.classList.contains('multi-select')) {
        $(`[data-index='${questionIndex}'] .multi-select`).select2({
            placeholder: "Select correct answers",
            allowClear: true
        });
    }
}

function getOptions(questionIndex) {
    var optionsContainer = document.querySelector(`[data-index='${questionIndex}'] .options-container`);
    return Array.from(optionsContainer.querySelectorAll('input[type="text"]')).map(input => input.value.trim());
}

function isOptionUnique(options, newValue) {
    return options.filter(option => option === newValue).length === 1;
}
    function showInvalidOptionsPopup(message) {
        // Set the message in the modal body dynamically
        document.getElementById('invalidOptionsMessage').innerHTML = message;

        // Show the modal with custom positioning
        var invalidOptionsModal = new bootstrap.Modal(document.getElementById('invalidOptionsModal'), {
            backdrop: false, // No overlay
            keyboard: false  // Prevent closing on keyboard actions
        });
        invalidOptionsModal.show();
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

// Event listener to handle the addition of new option fields
    document.addEventListener('click', function (e) {
        if (e.target && e.target.matches('.add-option')) {
            var questionIndex = e.target.getAttribute('data-question-index');
            var optionsContainer = document.querySelector(`[data-index='${questionIndex}'] .options-container`);
            var currentOptionsCount = optionsContainer.querySelectorAll('.option-group').length;

            // Add a new option field only if the current count is less than 5
            if (currentOptionsCount < 5) {
                createOptionField(questionIndex, currentOptionsCount);
            }

            // Hide the "Add Option" button if the max of 5 options is reached
            if (currentOptionsCount + 1 >= 5) {
                e.target.style.display = 'none';
            }
        } else if (e.target.matches('.remove-option') || e.target.closest('.remove-option')) {
            // Find the button element (either the one clicked or the parent of the icon)
            var removeButton = e.target.matches('.remove-option') ? e.target : e.target.closest('.remove-option');
            var questionIndex = removeButton.closest('.card').getAttribute('data-index');
            var optionIndex = removeButton.getAttribute('data-option-index'); // Get the corresponding option index

            // Find and remove the option group with the same data-option-index
            var optionGroup = removeButton.closest('.option-group');
            optionGroup.remove();

            // Re-enable the "Add Option" button if the options are fewer than 5
            var optionsContainer = document.querySelector(`[data-index='${questionIndex}'] .options-container`);
            var addOptionButton = optionsContainer.querySelector('.add-option');
            if (optionsContainer.querySelectorAll('.option-group').length < 5) {
                addOptionButton.style.display = 'block';
            }

            updateDropdown(questionIndex); // Update the options dropdown after removal
        }
        else if (e.target && e.target.matches('#add-question')) {
            addQuestionCard();
        }
        // Handle the "Remove Question" button click
        else if (e.target && e.target.matches('.remove-question')) {
            var questionCard = e.target.closest('.card');
            questionCard.remove();
            updateQuestionIndexes(); // Update indexes after question removal
        }
    });

    function addQuestionCard() {
        var quizContainer = document.getElementById('quiz-container');
        var questionIndex = quizContainer.querySelectorAll('.card').length;

        // Create a new card for the question
        var newCard = document.createElement('div');
        newCard.className = 'card mb-3';
        newCard.setAttribute('data-index', questionIndex);
        newCard.innerHTML = `
        <div class="card-body">
            <div class="d-flex justify-content-between align-items-center">
                <h5 class="card-title">Question ${questionIndex + 1}:</h5>
                <button type="button" class="btn btn-danger btn-sm remove-question">
                    <i class="bi bi-trash"></i> Remove Question
                </button>
            </div>
            <div class="form-group">
                <input type="text" class="form-control" name="Questions[${questionIndex}].Description" required />
            </div>
            <div class="form-group">
                <label for="QuestionType">Question Type</label>
                <select class="form-control question-type-select" name="Questions[${questionIndex}].Type" required>
                    <option value="" disabled selected>Select the Question Type</option> <!-- Placeholder option -->
                    <option value="1">True/False</option>
                    <option value="2">Single Answer MCQ</option>
                    <option value="3">Multi-Answer MCQ</option>
                </select>
            </div>
            <div class="form-group options-container">
            </div>
        </div>
    `;
        quizContainer.appendChild(newCard);
    }
    // Function to update question indexes after removing a question
    function updateQuestionIndexes() {
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
            showInvalidOptionsPopup("Some options are invalid. Please correct duplicate options before submitting.");
        }

        return isValid;
    }

    // Ensure that when the form is submitted, the multi-select values are properly serialized
    document.querySelector('#submit-button').addEventListener('click', function (e) {
        // Serialize all Select2 multi-selects for multi-answer MCQ
        var multiSelects = document.querySelectorAll('.multi-select');
        multiSelects.forEach(function (multiSelect) {
            var questionIndex = multiSelect.closest('.card').getAttribute('data-index');
            var selectedOptions = $(multiSelect).find(':selected').map(function () {
                return $(this).val();
            }).get();

            var existingHiddenInput = multiSelect.parentElement.querySelector(`input[name='Questions[${questionIndex}].CorrectOptionTexts']`);
            if (existingHiddenInput) {
                existingHiddenInput.remove();
            }

            var hiddenInput = document.createElement('input');
            hiddenInput.type = 'hidden';
            hiddenInput.name = `Questions[${questionIndex}].CorrectOptionTexts`;
            hiddenInput.value = selectedOptions.join(';');
            multiSelect.parentElement.appendChild(hiddenInput);
        });

        // Validation for feedback form and topics
        const feedbackFormIdInput = document.getElementById('SelectedFeedbackFormId');
        const topicsContainer = document.getElementById('topics-container');
        const topicInputs = topicsContainer.querySelectorAll('input[type="text"]');
        const hasTopics = Array.from(topicInputs).some(input => input.value.trim() !== '');
        const hasFeedbackForm = feedbackFormIdInput && feedbackFormIdInput.value.trim() !== '';

        // Display errors in a modal and prevent form submission if topics or feedback form are missing
        if (!hasTopics || !hasFeedbackForm) {
            let errorMessage = '';
            if (!hasTopics) errorMessage += 'Please add at least one topic.<br>';
            if (!hasFeedbackForm) errorMessage += 'Please select a feedback form.<br>';

            showInvalidOptionsPopup(errorMessage); // Display the error message(s) in a modal
            e.preventDefault(); // Prevent form submission
        } else if (!validateQuiz()) {
            e.preventDefault(); // Prevent form submission if there are other validation errors
        }
    });


});