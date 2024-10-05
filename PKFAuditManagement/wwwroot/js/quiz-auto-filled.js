function initializeQuizForm() {
    // Ensure the button exists before attaching the event listener
    const addQuestionButton = document.getElementById('quiz-add-question');

    if (addQuestionButton) {
        addQuestionButton.addEventListener('click', function () {
            addQuestion();
        });
    } else {
        console.error('Add Question button not found in the DOM');
    }

    // Attach existing event listeners for preloaded questions/options
    attachExistingEventListeners();
}

function addQuestion() {
    const container = document.getElementById('quiz-questions-container');
    const questionIndex = document.querySelectorAll('.quiz-question-card').length;
    const questionCard = `
        <div class="card mb-3 quiz-question-card" data-index="${questionIndex}">
            <div class="card-body quiz-question-card-body">
                <div class="d-flex justify-content-between align-items-center">
                    <h5 class="card-title">Question ${questionIndex + 1}:</h5>
                    <button type="button" class="btn btn-danger btn-sm quiz-remove-question" onclick="removeQuestion(${questionIndex})">
                        <i class="bi bi-trash"></i> Remove Question
                    </button>
                </div>
                <div class="form-group">
                    <input type="text" class="form-control" name="Questions[${questionIndex}].Description" required />
                </div>
                <div class="form-group">
                    <label>Options:</label>
                    <div class="quiz-options-container" id="quiz-options-container-${questionIndex}"></div>
                    <button type="button" class="btn btn-secondary btn-sm quiz-add-option" id="add-option-btn-${questionIndex}" onclick="addOption(${questionIndex})">Add Option</button>
                </div>
                <div class="form-group">
                    <label>Select the correct answer:</label>
                    <select class="form-control" name="Questions[${questionIndex}].CorrectOptionText" id="correct-answer-select-${questionIndex}"></select>
                </div>
            </div>
        </div>`;

    container.insertAdjacentHTML('beforeend', questionCard);
}

function attachExistingEventListeners() {
    const removeOptionButtons = document.querySelectorAll('.quiz-remove-option');
    const removeQuestionButtons = document.querySelectorAll('.quiz-remove-question');

    removeOptionButtons.forEach(button => {
        const questionIndex = button.closest('.quiz-question-card').getAttribute('data-index');
        const optionIndex = button.closest('.quiz-option-group').getAttribute('data-index');
        button.addEventListener('click', () => removeOption(questionIndex, optionIndex));
    });

    removeQuestionButtons.forEach(button => {
        const questionIndex = button.closest('.quiz-question-card').getAttribute('data-index');
        button.addEventListener('click', () => removeQuestion(questionIndex));
    });
}
