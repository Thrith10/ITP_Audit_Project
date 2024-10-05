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

    // Ensure sidebar is closed on page load
    var sidebar = document.querySelector('.sidebar'); // Adjust this selector to your sidebar
    var toggleSidebarBtn = document.querySelector('.toggle-sidebar-btn'); // The hamburger menu button
    if (toggleSidebarBtn) {
        toggleSidebarBtn.click();
    }

    $('.multi-select').select2({
        placeholder: "Select correct answers",
        allowClear: true
    });

    // Set current date and time for Quiz Start/End
    if (quizStartInput && quizEndInput) {
        var now = new Date();
        var singaporeOffset = 8 * 60; // UTC+8 offset
        var localOffset = now.getTimezoneOffset(); // Local offset
        var singaporeTime = new Date(now.getTime() + (singaporeOffset + localOffset) * 60000);

        var year = singaporeTime.getFullYear();
        var month = String(singaporeTime.getMonth() + 1).padStart(2, '0');
        var day = String(singaporeTime.getDate()).padStart(2, '0');
        var hours = String(singaporeTime.getHours()).padStart(2, '0');
        var minutes = String(singaporeTime.getMinutes()).padStart(2, '0');
        var formattedDateStart = `${year}-${month}-${day}T${hours}:${minutes}`;

        quizStartInput.min = formattedDateStart;
        quizStartInput.placeholder = formattedDateStart;

        // End time defaults to 23:59 of the same day
        var formattedDateEnd = `${year}-${month}-${day}T23:59`;
        quizEndInput.min = formattedDateEnd;
        quizEndInput.placeholder = formattedDateEnd;

        // Ensure the datetime picker opens on click
        quizStartInput.addEventListener('click', function () {
            quizStartInput.showPicker();
        });
        quizEndInput.addEventListener('click', function () {
            quizEndInput.showPicker();
        });
    }

    // Fetch participants and update participant count
    participantsCountSpan.addEventListener('click', function () {
        const selectedParticipants = selectedParticipantsInput.value.split(';').filter(email => email.trim() !== '');
        selectedParticipantsList.innerHTML = '';

        if (selectedParticipants.length > 0) {
            selectedParticipants.forEach(email => {
                const listItem = document.createElement('li');
                listItem.textContent = email;
                selectedParticipantsList.appendChild(listItem);
            });
        } else {
            const emptyMessage = document.createElement('li');
            emptyMessage.textContent = 'No participants selected';
            selectedParticipantsList.appendChild(emptyMessage);
        }

        selectedParticipantsModal.style.display = 'block';
    });

    // Modal close handlers
    closeSelectedParticipantsModal.addEventListener('click', function () {
        selectedParticipantsModal.style.display = 'none';
    });

    window.addEventListener('click', function (event) {
        if (event.target == selectedParticipantsModal) {
            selectedParticipantsModal.style.display = 'none';
        }
    });

    // Confirm button to update participant list and count
    confirmParticipantsBtn.addEventListener('click', function () {
        selectedParticipantsInput.value = selectedParticipants.join(';');
        participantsCountSpan.textContent = `${selectedParticipants.length} participants selected`;
        participantsModal.style.display = 'none';

        if (selectedParticipants.length > 0) {
            participantsCountSpan.style.fontWeight = 'bold';
            participantsCountSpan.style.color = 'green';
        } else {
            participantsCountSpan.style.fontWeight = 'normal';
            participantsCountSpan.style.color = 'blue';
        }
    });

    // Open participant selection modal
    selectParticipantsBtn.addEventListener('click', function () {
        fetchParticipants(); // Fetch participants on modal open
        participantsModal.style.display = 'block';
    });

    // Close modal functionality
    closeModal.addEventListener('click', function () {
        participantsModal.style.display = 'none';
    });

    window.addEventListener('click', function (event) {
        if (event.target == participantsModal) {
            participantsModal.style.display = 'none';
        }
    });

    // Fetch participants function
    function fetchParticipants(page = 1) {
        fetch('/Quizzes/GetAllUsers')
            .then(response => response.json())
            .then(users => {
                const startIndex = (page - 1) * emailsPerPage;
                const endIndex = page * emailsPerPage;
                const paginatedUsers = users.slice(startIndex, endIndex);

                participantsList.innerHTML = '';
                totalPages = Math.ceil(users.length / emailsPerPage);

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

                document.querySelector('.current-page').textContent = page;

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

    // Pagination controls
    document.querySelector('.next-page').addEventListener('click', () => {
        if (currentPage < totalPages) {
            currentPage++;
            fetchParticipants(currentPage);
        }
    });

    document.querySelector('.prev-page').addEventListener('click', () => {
        if (currentPage > 1) {
            currentPage--;
            fetchParticipants(currentPage);
        }
    });

    // Handle file upload for Excel
    document.getElementById('upload-participants-btn').addEventListener('click', function () {
        document.getElementById('ExcelFile').click();
    });

    document.getElementById('ExcelFile').addEventListener('change', handleFileSelect);

    async function handleFileSelect() {
        var file = document.getElementById('ExcelFile').files[0];
        if (!file) return;

        var reader = new FileReader();
        reader.onload = async function (event) {
            var data = new Uint8Array(event.target.result);
            var workbook = XLSX.read(data, { type: 'array' });
            var firstSheetName = workbook.SheetNames[0];
            var sheet = workbook.Sheets[firstSheetName];
            var sheetData = XLSX.utils.sheet_to_json(sheet, { header: 1 });
            var emails = [];

            for (var i = 1; i < sheetData.length; i++) {
                var email = sheetData[i][0];
                if (typeof email === 'string' && email.trim() !== '') {
                    emails.push(email.trim());
                }
            }

            try {
                const response = await fetch('/Quizzes/EmailValidation', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(emails)
                });

                if (response.ok) {
                    const result = await response.text();
                    document.getElementById('SelectedParticipants').value = result;
                    const validEmails = result.split(';').filter(email => email.trim() !== '');
                    document.getElementById('participants-count').textContent = `${validEmails.length} participants selected`;

                    if (validEmails.length > 0) {
                        participantsCountSpan.style.fontWeight = 'bold';
                        participantsCountSpan.style.color = 'green';
                    } else {
                        participantsCountSpan.style.fontWeight = 'normal';
                        participantsCountSpan.style.color = 'blue';
                    }
                } else {
                    console.error('Error validating emails');
                }
            } catch (error) {
                console.error('Error sending request:', error);
            }
        };

        reader.readAsArrayBuffer(file);
    }

    // Handle quiz question type and options
    document.addEventListener('change', function (e) {
        if (e.target && e.target.matches('.question-type-select')) {
            var questionCard = e.target.closest('.card');
            var questionIndex = questionCard.getAttribute('data-index');
            var selectedType = e.target.value;
            updateQuestionTypeFields(questionIndex, selectedType);
        }
    });

    function updateQuestionTypeFields(questionIndex, selectedType) {
        var optionsContainer = document.querySelector(`[data-index='${questionIndex}'] .options-container`);
        var correctAnswerContainer = document.querySelector(`[data-index='${questionIndex}'] .correct-answer-container`);

        if (!correctAnswerContainer) {
            correctAnswerContainer = document.createElement('div');
            correctAnswerContainer.className = 'form-group correct-answer-container';
            var label = document.createElement('label');
            label.textContent = 'Select the correct answer:';
            correctAnswerContainer.appendChild(label);
            document.querySelector(`[data-index='${questionIndex}'] .card-body`).appendChild(correctAnswerContainer);
        } else {
            correctAnswerContainer.innerHTML = '';
            var label = document.createElement('label');
            label.textContent = 'Select the correct answer:';
            correctAnswerContainer.appendChild(label);
        }

        optionsContainer.innerHTML = '';

        if (selectedType == '1') {
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
        } else if (selectedType == '2' || selectedType == '3') {
            createOptionField(questionIndex, 0);

            var addOptionButton = document.createElement('button');
            addOptionButton.type = 'button';
            addOptionButton.className = 'btn btn-secondary btn-sm add-option';
            addOptionButton.textContent = 'Add Option';
            addOptionButton.setAttribute('data-question-index', questionIndex);
            optionsContainer.appendChild(addOptionButton);

            var select = document.createElement('select');
            select.className = 'form-control';
            select.name = `Questions[${questionIndex}].CorrectOptionText`;
            if (selectedType == '3') {
                select.multiple = true;
                select.classList.add('multi-select');
            }
            select.required = true;
            correctAnswerContainer.appendChild(select);
            updateDropdown(questionIndex);

            if (selectedType == '3') {
                $(`[data-index='${questionIndex}'] .multi-select`).select2({
                    placeholder: "Select correct answers",
                    allowClear: true
                });
            }
        }
    }

    function createOptionField(questionIndex, optionIndex) {
        var optionsContainer = document.querySelector(`[data-index='${questionIndex}'] .options-container`);
        var optionGroup = document.createElement('div');
        optionGroup.className = 'input-group mb-2 option-group';

        var input = document.createElement('input');
        input.type = 'text';
        input.className = 'form-control';
        input.name = `Questions[${questionIndex}].Options[${optionIndex}].OptionText`;
        input.setAttribute('data-option-index', optionIndex);
        input.required = true;

        var removeButton = document.createElement('button');
        removeButton.type = 'button';
        removeButton.className = 'btn btn-danger btn-sm remove-option';
        removeButton.innerHTML = '<i class="bi bi-trash"></i>';
        removeButton.setAttribute('data-option-index', optionIndex);

        optionGroup.appendChild(input);
        optionGroup.appendChild(removeButton);
        optionsContainer.insertBefore(optionGroup, optionsContainer.querySelector('.add-option'));

        updateDropdown(questionIndex);
    }

    function updateDropdown(questionIndex) {
        var optionsContainer = document.querySelector(`[data-index='${questionIndex}'] .options-container`);
        var select = document.querySelector(`[data-index='${questionIndex}'] select[name*='CorrectOptionText']`);
        if (select === null) {
            console.error('CorrectOptionText select element not found for questionIndex:', questionIndex);
            return;
        }

        select.innerHTML = '';
        var inputs = optionsContainer.querySelectorAll('input[type="text"]');
        inputs.forEach(function (input) {
            var option = document.createElement('option');
            option.value = input.value;
            option.text = input.value;
            select.appendChild(option);
        });

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

    function showInvalidOptionsPopup() {
        alert('Some options are invalid. Please correct duplicate options before submitting.');
    }

    document.addEventListener('input', function (e) {
        if (e.target && e.target.matches('.option-group input[type="text"]')) {
            var questionCard = e.target.closest('.card');
            var questionIndex = questionCard.getAttribute('data-index');
            var allOptions = getOptions(questionIndex);
            var newValue = e.target.value.trim();

            updateDropdown(questionIndex);
            var optionInputs = document.querySelectorAll(`[data-index='${questionIndex}'] .option-group input[type="text"]`);
            optionInputs.forEach(function (input) {
                input.classList.remove('is-invalid');
            });

            allOptions.forEach(function (option) {
                if (!isOptionUnique(allOptions, option)) {
                    var invalidInputs = Array.from(optionInputs).filter(input => input.value.trim() === option);
                    invalidInputs.forEach(input => input.classList.add('is-invalid'));
                }
            });
        }
    });

    // Add new question dynamically
    document.addEventListener('click', function (e) {
        if (e.target && e.target.matches('.add-option')) {
            var questionIndex = e.target.getAttribute('data-question-index');
            var optionsContainer = document.querySelector(`[data-index='${questionIndex}'] .options-container`);
            var currentOptionsCount = optionsContainer.querySelectorAll('.option-group').length;

            if (currentOptionsCount < 5) {
                createOptionField(questionIndex, currentOptionsCount);
            }

            if (currentOptionsCount + 1 >= 5) {
                e.target.style.display = 'none';
            }
        } else if (e.target.matches('.remove-option') || e.target.closest('.remove-option')) {
            var removeButton = e.target.matches('.remove-option') ? e.target : e.target.closest('.remove-option');
            var questionIndex = removeButton.closest('.card').getAttribute('data-index');
            var optionGroup = removeButton.closest('.option-group');
            optionGroup.remove();

            var optionsContainer = document.querySelector(`[data-index='${questionIndex}'] .options-container`);
            var addOptionButton = optionsContainer.querySelector('.add-option');
            if (optionsContainer.querySelectorAll('.option-group').length < 5) {
                addOptionButton.style.display = 'block';
            }

            updateDropdown(questionIndex);
        } else if (e.target && e.target.matches('#add-question')) {
            addQuestionCard();
        } else if (e.target && e.target.matches('.remove-question')) {
            var questionCard = e.target.closest('.card');
            questionCard.remove();
            updateIndexes();
        }
    });

    function addQuestionCard() {
        var quizContainer = document.getElementById('quiz-container');
        var questionIndex = quizContainer.querySelectorAll('.card').length;

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
                    <option value="" disabled selected>Select the Question Type</option>
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

        if (!validateQuiz()) {
            e.preventDefault();
        }
    });

});

