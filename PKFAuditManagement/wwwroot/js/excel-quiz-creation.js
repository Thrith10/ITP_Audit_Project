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
        var quizStart = quizInfoSheet['B4'] ? quizInfoSheet['B4'].w : '';

        // Extract data from Questions (Sheet 3)
        var questionsSheet = workbook.Sheets['Questions'];
        var questions = [];
        var row = 2;

        while (questionsSheet['A' + row]) {
            var question = questionsSheet['A' + row] ? questionsSheet['A' + row].v : '';
            var type = questionsSheet['B' + row] ? questionsSheet['B' + row].v : '';
            switch (type.trim().toLowerCase()) {
                case "truefalse":
                    type = 1; // TrueFalse
                    break;
                case "singleanswermcq":
                    type = 2; // SingleAnswerMCQ
                    break;
                case "multianswermcq":
                    type = 3; // MultiAnswerMCQ
                    break;
                default:
                    alert(`Invalid question type: ${type}`);
                    return;
            }

            var optionA = questionsSheet['C' + row] ? questionsSheet['C' + row].v : '';
            var optionB = questionsSheet['D' + row] ? questionsSheet['D' + row].v : '';
            var optionC = questionsSheet['E' + row] ? questionsSheet['E' + row].v : '';
            var optionD = questionsSheet['F' + row] ? questionsSheet['F' + row].v : '';
            var optionE = questionsSheet['G' + row] ? questionsSheet['G' + row].v : '';

            var correctAnswer = '';
            var correctOption = questionsSheet['H' + row] ? questionsSheet['H' + row].v : '';

            if (type === 3 && correctOption.includes(',')) {
                const selectedOptions = correctOption.split(',').map(opt => opt.trim());
                correctAnswer = selectedOptions.map(opt => {
                    switch (opt) {
                        case 'A': return optionA;
                        case 'B': return optionB;
                        case 'C': return optionC;
                        case 'D': return optionD;
                        case 'E': return optionE;
                        default: return ''; // Handle invalid options gracefully
                    }
                }).filter(Boolean).join(',');
            }
            else {
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
                    default:
                        correctAnswer = ''; // Handle invalid single options
                        break;
                }
            }


            questions.push({
                description: question,
                type: type,
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

        // Call the backend to load ManualQuizCreation page
        const excelQuizData = {
            Title: quizTitle,
            Description: description,
            QuizStart: quizStart,
            Questions: questions,
            Participants: participants
        };

        const response = await fetch('/Quizzes/LoadManualQuizCreation', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(excelQuizData)
        });

        if (response.ok) {
            // Redirect to the new page
            window.location.href = "/Quizzes/QuizAutoFilled";
        } else {
            alert('Error loading quiz form.');
        }
    };

    reader.readAsArrayBuffer(file);
}
