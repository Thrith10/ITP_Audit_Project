async function processExcel() {
    var file = document.getElementById('ExcelQuizUpload').files[0];
    if (!file) {
        alert("Please select an Excel file.");
        return;
    }

    var reader = new FileReader();
    reader.onload = async function (e) {
        var data = new Uint8Array(e.target.result);
        var workbook = XLSX.read(data, { type: 'array' });

        // Extract data from QuizInfo (Sheet 2)
        var quizInfoSheet = workbook.Sheets['QuizInfo'];
        var quizTitle = quizInfoSheet['B2'] ? quizInfoSheet['B2'].v : '';
        var description = quizInfoSheet['B3'] ? quizInfoSheet['B3'].v : '';
        var quizStart = quizInfoSheet['B4'] ? quizInfoSheet['B4'].w: '';

        // Extract data from Questions (Sheet 3)
        var questionsSheet = workbook.Sheets['Questions'];
        var questions = [];
        var row = 2;

        while (questionsSheet['A' + row]) {
            var question = questionsSheet['A' + row] ? questionsSheet['A' + row].v : '';
            var optionA = questionsSheet['B' + row] ? questionsSheet['B' + row].v : '';
            var optionB = questionsSheet['C' + row] ? questionsSheet['C' + row].v : '';
            var optionC = questionsSheet['D' + row] ? questionsSheet['D' + row].v : '';
            var optionD = questionsSheet['E' + row] ? questionsSheet['E' + row].v : '';
            var optionE = questionsSheet['F' + row] ? questionsSheet['F' + row].v : '';
            var correctAnswer = '';

            var correctOption = questionsSheet['G' + row] ? questionsSheet['G' + row].v : '';
            switch (correctOption) {
                case 'A':
                    correctAnswer = optionA;
                    break;
                case 'B':
                    correctAnswer = optionB;
                    break;
                case 'C':
                    correctAnswer = optionC;
                    break;
                case 'D':
                    correctAnswer = optionD;
                    break;
                case 'E':
                    correctAnswer = optionE;
                    break;
            }

            questions.push({
                description: question,
                optionA: optionA,
                optionB: optionB,
                optionC: optionC,
                optionD: optionD,
                optionE: optionE,
                correctAnswer: correctAnswer
            });

            row++;
        }

        // Extract data from Participants (Sheet 4)
        var participantsSheet = workbook.Sheets['Participants'];
        var participants = [];
        row = 2;

        while (participantsSheet['A' + row]) {
            participants.push(participantsSheet['A' + row].v);
            row++;
        }

        // Call the backend to load ManualQuizCreation PartialView
        loadQuizForm(quizTitle, description, quizStart, questions, participants);
    };

    reader.readAsArrayBuffer(file);
}

function loadQuizForm(quizTitle, description, quizStart, questions, participants) {
    var excelQuizData = {
        Title: quizTitle,
        Description: description,
        QuizStart: quizStart,
        Questions: questions,
        Participants: participants
    };

    $.ajax({
        url: '/Quizzes/LoadManualQuizCreation',
        type: 'POST',
        data: JSON.stringify(excelQuizData),
        contentType: 'application/json',
        success: function (result) {

            $('#quiz-form-container').html(result);
             initializeQuizForm();

        },
        error: function () {
            alert('Error loading quiz form.');
        }
    });
}

