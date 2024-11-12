// Add event listeners for resize functionality
document.addEventListener('DOMContentLoaded', function () {
    const chatbotPopup = document.getElementById('chatbot-popup');
    const chatBox = document.getElementById('chat-box');
    let isResizing = false;
    let lastDownX = 0;
    let lastDownY = 0;

    // Start resizing when mouse is down on the handle
    chatbotPopup.addEventListener('mousedown', function (e) {
        if (e.offsetX < 30 && e.offsetY < 30) { // Adjust if necessary to make handle easier to grab
            isResizing = true;
            lastDownX = e.clientX;
            lastDownY = e.clientY;
            document.addEventListener('mousemove', resize);
            document.addEventListener('mouseup', stopResize);
        }
    });

    // Function to resize the chat-box
    function resize(e) {
        if (!isResizing) return;

        const widthChange = e.clientX - lastDownX; // Change in width
        const heightChange = e.clientY - lastDownY; // Change in height

        const newWidth = chatbotPopup.clientWidth - widthChange;
        const newHeight = chatbotPopup.clientHeight - heightChange;

        // Prevent width from going below minimum size
        if (newWidth > 100) {
            chatbotPopup.style.width = `${newWidth}px`;
        }

        // Prevent height from going below minimum size
        if (newHeight > 150) { // Minimum height for the entire popup
            chatbotPopup.style.height = `${newHeight}px`;

            // Adjust chat box height based on the new popup height
            const chatInputHeight = document.querySelector('.chat-input').offsetHeight;
            const maxHeight = newHeight - chatInputHeight;

            // Set a minimum height for the chat box
            if (maxHeight > 100) {
                chatBox.style.height = `${maxHeight}px`;
            }
        }

        // Update last positions
        lastDownX = e.clientX;
        lastDownY = e.clientY;
    }

    // Stop resizing
    function stopResize() {
        isResizing = false;
        document.removeEventListener('mousemove', resize);
        document.removeEventListener('mouseup', stopResize);
    }
});

// Flag to track if the chatbot has been opened
let hasOpened = false;

// Add event listeners for buttons and input field of the chatbot function
document.getElementById('chatbot-toggle-btn').addEventListener('click', toggleChatbot);
document.getElementById('close-btn').addEventListener('click', toggleChatbot);
document.getElementById('send-btn').addEventListener('click', sendMessage);
document.getElementById('user-input').addEventListener('keypress', function (e) {
    if (e.key === 'Enter') {
        sendMessage();
    }
});

// Toggle the chatbot popup
function toggleChatbot() {
    const chatbotPopup = document.getElementById('chatbot-popup');
    if (chatbotPopup.classList.contains('visible')) {
        chatbotPopup.classList.remove('visible');
    } else {
        chatbotPopup.classList.add('visible');
        if (!hasOpened) {
            showIntroductionMessage();
            hasOpened = true;
        }
        document.getElementById('user-input').disabled = true; // Disable chat input initially
    }
}

// Function to show the introductory message and options
function showIntroductionMessage() {
    const introMessage = 'Hello, I am the audit chatbot trained on SSQM1, SSQM2, EP100, and EP200. To begin, please click an area you would like to explore:';
    appendMessage('bot', introMessage); // Display the bot message with the introductory text

    // Enhanced exit message with an additional explanation
    const exitMessage = 'If you wish to exit at any time and reset the conversation, just type "exit". This will allow you to choose an area to explore again.';
    appendMessage('bot', exitMessage); // Show the enhanced exit message

    showDefaultOptions(); // Show the clickable options
}
// Variable to hold the current selection
let currentSelection = null;

// Function to display clickable options after the introductory message
function showDefaultOptions() {
    const options = [
        { label: 'SSQM1', response: 'You selected SSQM1' },
        { label: 'SSQM2', response: 'You selected SSQM2' },
        { label: 'EP 100', response: 'You selected EP 100' },
        { label: 'EP 200', response: 'You selected EP 200' }
    ];

    const buttonContainer = document.createElement('div');
    buttonContainer.classList.add('button-container'); // Apply custom styling to align buttons

    options.forEach(option => {
        const button = document.createElement('button');
        button.classList.add('chat-option-button');
        button.textContent = option.label;
        button.addEventListener('click', function () {
            appendMessage('user', option.label); // Show user choice
            appendMessage('bot', option.response); // Show bot response
            // Save the current selection
            currentSelection = option.label;
            document.getElementById('user-input').disabled = false; // Enable chat input after a choice
            buttonContainer.remove(); // Remove buttons after a choice
        });
        buttonContainer.appendChild(button);
    });

    const chatBox = document.getElementById('chat-box');
    chatBox.appendChild(buttonContainer);
    chatBox.scrollTop = chatBox.scrollHeight;
}

// Function to send a message from the user
function sendMessage() {
    const userInput = document.getElementById('user-input').value.trim();
    if (userInput !== '') {
        appendMessage('user', userInput); // Show user message

        if (userInput.toLowerCase() === 'exit') {
            resetChat(); // Reset the chat if the user types 'exit'
        } else {
            respondToUser(userInput.toLowerCase()); // Get the bot's response
        }

        document.getElementById('user-input').value = ''; // Clear input field
    }
}

// Function to show the selection buttons again without clearing the conversation history
function showSelectionButtons() {
    if (currentSelection === null) {
        // If no selection has been made yet, show the default options
        showDefaultOptions();
    } else {
        // If a selection has been made, show the current selection message
        const currentMessage = `You previously selected ${currentSelection}. To explore another area, please click one of the options below:`;
        appendMessage('bot', currentMessage); // Show the current selection message
        showDefaultOptions(); // Show the clickable options again
    }

}
// Function to reset the chat and show the selection buttons again
function resetChat() {
    // Clear the chat box and reset the current selection
    const chatBox = document.getElementById('chat-box');
    chatBox.innerHTML = ''; // Clear chat history
    currentSelection = null; // Reset the current selection

    // Show the introduction message and options again
    showIntroductionMessage();
}


// Function to get the chatbot's response via an AJAX request
function respondToUser(userInput) {
    // Append a loading message for the bot response
    const loadingMessage = appendMessage('bot', createLoadingIndicator());

    // Make AJAX request
    $.ajax({
        url: '/Chatbot/GetChatResponse',
        type: 'POST',
        data: {
            'userInput': userInput,
            'currentSelection': currentSelection 
        },
        success: function (response) {
            // Replace the loading indicator with the actual response using jQuery's .html() to display as HTML
            $(loadingMessage).remove(); // Remove the loading message
            appendMessage('bot', response);
        },
        error: function (xhr, status, error) {
            console.error('Error:', error);
            $(loadingMessage).html('Sorry, there was an error processing your request.');
        }
    });
}

// Function to create a loading indicator with a GIF
function createLoadingIndicator() {
    return `<img src="/img/loading.gif" alt="Loading..." class="loading-indicator" width="24" height="24"/>`;
}
function appendMessage(sender, message) {
    const chatBox = $('#chat-box');
    const messageElement = $('<div></div>');
    messageElement.addClass(sender === 'user' ? 'user-message' : 'bot-message');
    messageElement.html(message); // Allow HTML content
    chatBox.append(messageElement);
    chatBox.scrollTop(chatBox[0].scrollHeight);

    return messageElement[0]; // Return the DOM element for further manipulation if needed
}