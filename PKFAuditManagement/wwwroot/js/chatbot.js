// Add event listeners for resize functionality
document.addEventListener('DOMContentLoaded', function () {
    const chatbotPopup = document.getElementById('chatbot-popup');
    let isResizing = false;
    let lastDownX = 0;
    let lastDownY = 0;

    // Start resizing when mouse is down on the handle
    chatbotPopup.addEventListener('mousedown', function (e) {
        if (e.offsetX < 15 && e.offsetY < 15) {
            isResizing = true;
            lastDownX = e.clientX;
            lastDownY = e.clientY;
            document.addEventListener('mousemove', resize);
            document.addEventListener('mouseup', stopResize);
        }
    });

    // Function to resize the widget
    function resize(e) {
        if (!isResizing) return;

        const width = chatbotPopup.clientWidth - (e.clientX - lastDownX);
        const height = chatbotPopup.clientHeight - (e.clientY - lastDownY);

        // Prevent width and height from going below minimum size
        if (width > 100 && height > 100) {
            chatbotPopup.style.width = `${width}px`;
            chatbotPopup.style.height = `${height}px`;
        }

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
    if (chatbotPopup.style.display === 'block') {
        chatbotPopup.style.display = 'none';
    } else {
        chatbotPopup.style.display = 'block';
        if (!hasOpened) {
            showDefaultMessage();
            hasOpened = true; // Set flag to true once the chatbot has been opened
        }
    }
}

// Function to show the default message when the chatbot opens
function showDefaultMessage() {
    const defaultMessage = 'Hello! How can I assist you today?'; // Default message
    appendMessage('bot', defaultMessage);
}

// Function to send a message from the user
function sendMessage() {
    const userInput = document.getElementById('user-input').value.trim();
    if (userInput !== '') {
        appendMessage('user', userInput);
        respondToUser(userInput.toLowerCase());
        document.getElementById('user-input').value = '';
    }
}

// Function to get the chatbot's response via an AJAX request
function respondToUser(userInput) {
    const loadingIndicator = document.getElementById('loader');
    loadingIndicator.style.display = 'block'; // Show loading indicator

    // Make AJAX request
    $.ajax({
        url: '/Chatbot/GetChatResponse',
        type: 'POST',
        data: { 'userInput': userInput },
        success: function (response) {
            // Append bot response to chat
            appendMessage('bot', response);
        },
        error: function (xhr, status, error) {
            console.error('Error:', error);
        },
        complete: function () {
            loadingIndicator.style.display = 'none'; // Hide loading indicator after response is received
        }
    });
}

// Function to append a message to the chat box
function appendMessage(sender, message) {
    const chatBox = document.getElementById('chat-box');
    const messageElement = document.createElement('div');
    messageElement.classList.add(sender === 'user' ? 'user-message' : 'bot-message');
    messageElement.innerHTML = message;
    chatBox.appendChild(messageElement);
    chatBox.scrollTop = chatBox.scrollHeight;
}