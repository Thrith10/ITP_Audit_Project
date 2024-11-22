document.addEventListener('DOMContentLoaded', function () {
    //Feedback
    const feedbackFormListContainer = document.getElementById('feedback-form-list-container');
    const closeFeedbackFormModal = document.getElementById('close-feedback-form-modal');
    const confirmFeedbackFormSelectionBtn = document.getElementById('confirm-feedback-form-selection-btn');
    const selectedFeedbackFormIdInput = document.getElementById('SelectedFeedbackFormId');
    const searchFeedbackFormInput = document.getElementById('search-feedback-form-input');
    const prevFeedbackFormPageBtn = document.getElementById('prev-feedback-form-page');
    const nextFeedbackFormPageBtn = document.getElementById('next-feedback-form-page');
    const currentFeedbackFormPageNumber = document.getElementById('current-feedback-form-page-number');
    const selectedFeedbackFormText = document.getElementById('selected-feedback-form-text'); // Ensure this targets the correct element
    const feedbackFormSelectedTick = document.getElementById('feedback-selected-tick'); // Green tick icon for selected form


    let allFeedbackForms = [];
    let currentFeedbackFormPage = 1;
    const feedbackFormsPerPage = 10;

    //Participant Portion
    const selectParticipantsBtn = document.getElementById('select-participants-btn');
    const participantsModal = new bootstrap.Modal(document.getElementById('participants-modal'), {
        backdrop: false, // Prevents closing on clicking outside if desired
        keyboard: true
    }); const closeParticipantsModal = document.getElementById('close-participants-modal');
    const confirmParticipantsBtn = document.getElementById('confirm-participants-btn');
    const participantsList = document.getElementById('participants-list');
    const selectedParticipantsInput = document.getElementById('SelectedParticipants');
    const selectedParticipantsText = document.getElementById('selected-participants-text');
    const participantsSelectedTick = document.getElementById('participants-selected-tick');
    let selectedParticipants = [];
    let allParticipants = [];
    let currentPage = 1;
    const participantsPerPage = 10;
    const searchBox = document.getElementById('search-participants');
    const selectAllBtn = document.getElementById('select-all-btn');
    let isSelectAll = false; // To track the select/deselect state

    //Check selected participants modal
    const selectedParticipantsModal = new bootstrap.Modal(document.getElementById("selected-participants-modal"), {
        backdrop: false // This disables the backdrop
    });
    const selectedParticipantsList = document.getElementById("selected-participants-list");
    const closeParticipantsModalBtn = document.getElementById("close-participants-modal-btn");

    //Self-Assessment
    const selfAssessmentFormListContainer = document.getElementById('self-assessment-form-list-container');
    const closeSelfAssessmentFormModal = document.getElementById('close-self-assessment-form-modal');
    const confirmSelfAssessmentFormSelectionBtn = document.getElementById('confirm-self-assessment-form-selection-btn');
    const selectedSelfAssessmentFormIdInput = document.getElementById('SelectedSelfAssessmentFormId');
    const searchSelfAssessmentFormInput = document.getElementById('search-self-assessment-form-input');
    const prevSelfAssessmentFormPageBtn = document.getElementById('prev-self-assessment-form-page');
    const nextSelfAssessmentFormPageBtn = document.getElementById('next-self-assessment-form-page');
    const currentSelfAssessmentFormPageNumber = document.getElementById('current-self-assessment-form-page-number');
    const selectedSelfAssessmentFormText = document.getElementById('selected-self-assessment-form-text'); // Display element for selected form
    const selfAssessmentFormSelectedTick = document.getElementById('self-assessment-selected-tick'); // Green tick icon

    // Ensure default values for pagination and form data
    let allSelfAssessmentForms = [];
    let currentSelfAssessmentFormPage = 1;
    const selfAssessmentFormsPerPage = 10;

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
    function loadFeedbackForms() {
        fetch('/Quizzes/GetFeedbackForms') // Replace with the actual API endpoint
            .then(response => response.json())
            .then(data => {
                allFeedbackForms = data;
                currentFeedbackFormPage = 1;
                renderFeedbackFormList();
            })
            .catch(error => console.error('Error fetching feedback forms:', error));
    }

    function renderFeedbackFormList() {
        const searchTerm = searchFeedbackFormInput.value.toLowerCase();
        const filteredFeedbackForms = allFeedbackForms.filter(form =>
            form.title.toLowerCase().includes(searchTerm)
        );

        const startIndex = (currentFeedbackFormPage - 1) * feedbackFormsPerPage;
        const endIndex = startIndex + feedbackFormsPerPage;
        const paginatedFeedbackForms = filteredFeedbackForms.slice(startIndex, endIndex);

        feedbackFormListContainer.innerHTML = '';

        paginatedFeedbackForms.forEach(form => {
            const listItem = document.createElement('li');
            listItem.className = 'list-group-item list-group-item-action';
            listItem.textContent = form.title;
            listItem.dataset.id = form.id;

            listItem.addEventListener('click', function () {
                feedbackFormListContainer.querySelectorAll('.list-group-item').forEach(item => item.classList.remove('active'));
                listItem.classList.add('active');

                // Set the hidden input value with the selected form ID
                selectedFeedbackFormIdInput.value = form.id;

                // Update the display text to show the selected feedback form title
                selectedFeedbackFormText.textContent = form.title; // Update `selected-feedback-form-text` element
                selectedFeedbackFormText.classList.add('selected'); // Add class for styling

                // Show the green tick beside the selected feedback form text
                feedbackFormSelectedTick.style.display = 'inline';
            });

            feedbackFormListContainer.appendChild(listItem);
        });

        updateFeedbackFormPaginationControls(filteredFeedbackForms.length);
    }

    function updateFeedbackFormPaginationControls(totalItems) {
        const totalPages = Math.ceil(totalItems / feedbackFormsPerPage);

        prevFeedbackFormPageBtn.disabled = currentFeedbackFormPage === 1;
        nextFeedbackFormPageBtn.disabled = currentFeedbackFormPage === totalPages || totalPages === 0;

        currentFeedbackFormPageNumber.textContent = `Page ${currentFeedbackFormPage} of ${totalPages}`;
    }

    searchFeedbackFormInput.addEventListener('input', function () {
        currentFeedbackFormPage = 1;
        renderFeedbackFormList();
    });

    prevFeedbackFormPageBtn.addEventListener('click', function () {
        if (currentFeedbackFormPage > 1) {
            currentFeedbackFormPage--;
            renderFeedbackFormList();
        }
    });

    nextFeedbackFormPageBtn.addEventListener('click', function () {
        const totalPages = Math.ceil(
            allFeedbackForms.filter(form => form.title.toLowerCase().includes(searchFeedbackFormInput.value.toLowerCase())).length / feedbackFormsPerPage
        );
        if (currentFeedbackFormPage < totalPages) {
            currentFeedbackFormPage++;
            renderFeedbackFormList();
        }
    });

    document.getElementById('select-feedback-form-btn').addEventListener('click', function () {
        document.getElementById('feedback-form-modal').style.display = 'block';
        loadFeedbackForms();
    });

    closeFeedbackFormModal.addEventListener('click', function () {
        document.getElementById('feedback-form-modal').style.display = 'none';
    });

    confirmFeedbackFormSelectionBtn.addEventListener('click', function () {
        document.getElementById('feedback-form-modal').style.display = 'none';
    });

    window.addEventListener('click', function (event) {
        const feedbackFormModal = document.getElementById('feedback-form-modal');
        if (event.target === feedbackFormModal) {
            feedbackFormModal.style.display = 'none';
        }
    });
    //Self-Assessment
    function loadSelfAssessmentForms() {
        fetch('/Quizzes/GetSelfAssessmentForms') // Replace with the actual endpoint for self-assessment forms
            .then(response => response.json())
            .then(data => {
                allSelfAssessmentForms = data;
                currentSelfAssessmentFormPage = 1;
                renderSelfAssessmentFormList();
            })
            .catch(error => console.error('Error fetching self-assessment forms:', error));
    }

    function renderSelfAssessmentFormList() {
        const searchTerm = searchSelfAssessmentFormInput.value.toLowerCase();
        const filteredSelfAssessmentForms = allSelfAssessmentForms.filter(form =>
            form.title.toLowerCase().includes(searchTerm)
        );

        const startIndex = (currentSelfAssessmentFormPage - 1) * selfAssessmentFormsPerPage;
        const endIndex = startIndex + selfAssessmentFormsPerPage;
        const paginatedSelfAssessmentForms = filteredSelfAssessmentForms.slice(startIndex, endIndex);

        selfAssessmentFormListContainer.innerHTML = '';

        paginatedSelfAssessmentForms.forEach(form => {
            const listItem = document.createElement('li');
            listItem.className = 'list-group-item list-group-item-action';
            listItem.textContent = form.title;
            listItem.dataset.id = form.id;

            listItem.addEventListener('click', function () {
                selfAssessmentFormListContainer.querySelectorAll('.list-group-item').forEach(item => item.classList.remove('active'));
                listItem.classList.add('active');

                // Set the hidden input value with the selected form ID
                selectedSelfAssessmentFormIdInput.value = form.id;

                // Update the display text to show the selected self-assessment form title
                selectedSelfAssessmentFormText.textContent = form.title;
                selectedSelfAssessmentFormText.classList.add('selected'); // Add class for styling

                // Show the green tick beside the selected form text
                selfAssessmentFormSelectedTick.style.display = 'inline';
            });

            selfAssessmentFormListContainer.appendChild(listItem);
        });

        updateSelfAssessmentFormPaginationControls(filteredSelfAssessmentForms.length);
    }

    function updateSelfAssessmentFormPaginationControls(totalItems) {
        const totalPages = Math.ceil(totalItems / selfAssessmentFormsPerPage);

        prevSelfAssessmentFormPageBtn.disabled = currentSelfAssessmentFormPage === 1;
        nextSelfAssessmentFormPageBtn.disabled = currentSelfAssessmentFormPage === totalPages || totalPages === 0;

        currentSelfAssessmentFormPageNumber.textContent = `Page ${currentSelfAssessmentFormPage} of ${totalPages}`;
    }

    searchSelfAssessmentFormInput.addEventListener('input', function () {
        currentSelfAssessmentFormPage = 1;
        renderSelfAssessmentFormList();
    });

    prevSelfAssessmentFormPageBtn.addEventListener('click', function () {
        if (currentSelfAssessmentFormPage > 1) {
            currentSelfAssessmentFormPage--;
            renderSelfAssessmentFormList();
        }
    });

    nextSelfAssessmentFormPageBtn.addEventListener('click', function () {
        const totalPages = Math.ceil(
            allSelfAssessmentForms.filter(form => form.title.toLowerCase().includes(searchSelfAssessmentFormInput.value.toLowerCase())).length / selfAssessmentFormsPerPage
        );
        if (currentSelfAssessmentFormPage < totalPages) {
            currentSelfAssessmentFormPage++;
            renderSelfAssessmentFormList();
        }
    });

    document.getElementById('select-self-assessment-form-btn').addEventListener('click', function () {
        document.getElementById('self-assessment-form-modal').style.display = 'block';
        loadSelfAssessmentForms();
    });

    closeSelfAssessmentFormModal.addEventListener('click', function () {
        document.getElementById('self-assessment-form-modal').style.display = 'none';
    });

    confirmSelfAssessmentFormSelectionBtn.addEventListener('click', function () {
        document.getElementById('self-assessment-form-modal').style.display = 'none';
    });

    window.addEventListener('click', function (event) {
        const selfAssessmentFormModal = document.getElementById('self-assessment-form-modal');
        if (event.target === selfAssessmentFormModal) {
            selfAssessmentFormModal.style.display = 'none';
        }
    });



  


    // Open Participants Modal
    selectParticipantsBtn.addEventListener('click', function () {
        participantsModal.show();
        loadParticipants();
    });

    selectParticipantsBtn.addEventListener('click', function () {
        participantsModal.show();
        loadParticipants();
    });
    // Load participants from the server
    function loadParticipants() {
        fetch('/Quizzes/GetAllWithUserRole')
            .then(response => response.json())
            .then(users => {
                allParticipants = users;
                currentPage = 1;
                renderParticipantsList();
            })
            .catch(error => console.error('Error fetching participants:', error));
    }

    // Render the participants list based on the current page and search term
    function renderParticipantsList() {
        const searchTerm = searchBox.value.toLowerCase();
        const filteredParticipants = allParticipants.filter(user =>
            user.email.toLowerCase().includes(searchTerm)
        );

        const startIndex = (currentPage - 1) * participantsPerPage;
        const endIndex = startIndex + participantsPerPage;
        const paginatedParticipants = filteredParticipants.slice(startIndex, endIndex);

        participantsList.innerHTML = '';

        paginatedParticipants.forEach(user => {
            const listItem = document.createElement('li');
            listItem.className = 'list-group-item list-group-item-action participant-item';
            listItem.textContent = user.email;
            listItem.dataset.id = user.UserId;

            // Highlight if already selected
            if (selectedParticipants.includes(user.email)) {
                listItem.classList.add('selected');
            }

            // Toggle selection on click
            listItem.addEventListener('click', function () {
                if (selectedParticipants.includes(user.email)) {
                    selectedParticipants = selectedParticipants.filter(email => email !== user.email);
                    listItem.classList.remove('selected');
                } else {
                    selectedParticipants.push(user.email);
                    listItem.classList.add('selected');
                }
                updateParticipantsCount();
            });

            participantsList.appendChild(listItem);
        });

        updatePaginationControls(filteredParticipants.length);
        updateSelectAllButtonText();
    }

    // Update the participants count display
    function updateParticipantsCount() {
        participantsCountSpan.textContent = `${selectedParticipants.length} participants selected`;
        participantsCountSpan.style.fontWeight = selectedParticipants.length > 0 ? 'bold' : 'normal';
    }

    // Toggle between Select All and Deselect All
    selectAllBtn.addEventListener('click', function () {
        if (isSelectAll) {
            selectedParticipants = []; // Clear selection
            participantsList.querySelectorAll('.participant-item').forEach(item => {
                item.classList.remove('selected');
            });
            isSelectAll = false;
        } else {
            selectedParticipants = allParticipants.map(user => user.email); // Select all emails
            participantsList.querySelectorAll('.participant-item').forEach(item => {
                item.classList.add('selected');
            });
            isSelectAll = true;
        }
        updateSelectAllButtonText();
        updateParticipantsCount();
    });

    // Update Select All / Deselect All button text
    function updateSelectAllButtonText() {
        selectAllBtn.textContent = isSelectAll ? 'Deselect All' : 'Select All';
    }

    // Update pagination controls
    function updatePaginationControls(totalItems) {
        const totalPages = Math.ceil(totalItems / participantsPerPage);

        document.getElementById('prev-page').disabled = currentPage === 1;
        document.getElementById('next-page').disabled = currentPage === totalPages || totalPages === 0;

        document.getElementById('current-page-number').textContent = `Page ${currentPage} of ${totalPages}`;
    }

    // Event listener for search input to filter participants in real-time
    searchBox.addEventListener('input', function () {
        currentPage = 1; // Reset to first page on new search
        renderParticipantsList();
    });

    // Pagination control buttons
    document.getElementById('prev-page').addEventListener('click', function () {
        if (currentPage > 1) {
            currentPage--;
            renderParticipantsList();
        }
    });

    document.getElementById('next-page').addEventListener('click', function () {
        const totalPages = Math.ceil(
            allParticipants.filter(user => user.email.toLowerCase().includes(searchBox.value.toLowerCase())).length / participantsPerPage
        );
        if (currentPage < totalPages) {
            currentPage++;
            renderParticipantsList();
        }
    });

    // Initial load
    loadParticipants();
    // Real-time filter for participants based on search input
    document.getElementById('search-participants').addEventListener('input', function () {
        const searchTerm = this.value.toLowerCase();

        // Filter the full list of participants to match the search term
        const filteredUsers = allParticipants.filter(user => user.email.toLowerCase().includes(searchTerm));

        // Re-render the participants list with filtered users
        renderParticipantsList(filteredUsers);
    });

    // Confirm participant selection
    confirmParticipantsBtn.addEventListener('click', function () {
        selectedParticipantsInput.value = selectedParticipants.join(';'); // Store selected participants in hidden input
        participantsModal.hide();
        updateParticipantsCount();
    });

    // Function to update the participants count display
    function updateParticipantsCount() {
        selectedParticipantsText.textContent = `${selectedParticipants.length} participants selected`;
        participantsSelectedTick.style.display = selectedParticipants.length > 0 ? 'inline' : 'none'; // Show green tick if participants are selected
        if (selectedParticipants.length > 0) {
            selectedParticipantsText.classList.add('selected');
        } else {
            selectedParticipantsText.classList.remove('selected');
        }
    }

    // Close modal when clicking outside of modal content
    window.addEventListener('click', function (event) {
        if (event.target == participantsModal) {
            participantsModal.hide();
        }
    });
    // Event listener to open the modal when "selected-participants-text" is clicked
    selectedParticipantsText.addEventListener("click", function () {
        // Clear the list before populating
        selectedParticipantsList.innerHTML = "";

        // Populate the modal list with selected participants
        if (selectedParticipants.length === 0) {
            selectedParticipantsList.innerHTML = "<li class='list-group-item'>No participants selected.</li>";
        } else {
            selectedParticipants.forEach(participant => {
                const listItem = document.createElement("li");
                listItem.className = "list-group-item";
                listItem.textContent = participant; // Use participant name or email as needed
                selectedParticipantsList.appendChild(listItem);
            });
        }

        // Show the modal
        selectedParticipantsModal.show();
    });

    // Event listener to close the modal
    closeParticipantsModalBtn.addEventListener("click", function () {
        selectedParticipantsModal.hide();
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

        // Validation for feedback form, self-assessment form
        const feedbackFormIdInput = document.getElementById('SelectedFeedbackFormId');
        const selfAssessmentFormIdInput = document.getElementById('SelectedSelfAssessmentFormId'); // Ensure this input exists for self-assessment selection
 
        const hasFeedbackForm = feedbackFormIdInput && feedbackFormIdInput.value.trim() !== '';
        const hasSelfAssessmentForm = selfAssessmentFormIdInput && selfAssessmentFormIdInput.value.trim() !== '';

        // Display errors in a modal and prevent form submission if feedback form, or self-assessment form are missing
        if (!hasFeedbackForm || !hasSelfAssessmentForm) {
            let errorMessage = '';
            if (!hasFeedbackForm) errorMessage += 'Please select a feedback form.<br>';
            if (!hasSelfAssessmentForm) errorMessage += 'Please select a self-assessment form.<br>';

            showInvalidOptionsPopup(errorMessage); // Display the error message(s) in a modal
            e.preventDefault(); // Prevent form submission
        } else if (!validateQuiz()) {
            e.preventDefault(); // Prevent form submission if there are other validation errors
        }
    });

});

