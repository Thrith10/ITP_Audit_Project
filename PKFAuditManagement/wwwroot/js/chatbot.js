// Add event listeners for resize functionality
document.addEventListener('DOMContentLoaded', function () {


    const chatbotPopup = document.getElementById('chatbot-popup');
    const chatBox = document.getElementById('chat-box');
    let isResizing = false;
    let lastDownX = 0;
    let lastDownY = 0;
    const chatMenu = document.getElementById('chat-menu');
    const menuBtn = document.getElementById('menu-btn');
    const closeMenuBtn = document.getElementById('close-menu-btn');

    // Toggle the visibility of the chat menu
    menuBtn.addEventListener('click', function () {
        chatMenu.style.display = chatMenu.style.display === 'block' ? 'none' : 'block';
    });

    // Close the menu when the close button is clicked
    closeMenuBtn.addEventListener('click', function () {
        chatMenu.style.display = 'none';
    });

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
    const introMessage = 'Hello, I am PKF-CAP\'s chatbot trained on audit documents. To begin, please click an area you would like to explore by clicking on the menu button on the bottom left and selecting a topic. By default, I do not have a knowledge base and you have to navigate to the "Manage Chatbots" page as an administrator to add documents to my knowledge base.';
    appendMessage('bot', introMessage); // Display the bot message with the introductory text
}
// Variable to hold the current selection
let selectedTopic = null;

// Handle menu option clicks
const menuOptions = document.querySelectorAll('.menu-option');
menuOptions.forEach(option => {
    option.addEventListener('click', function () {
        document.getElementById('user-input').disabled = false;
        selectedTopic = option.textContent; // Get the selected topic text
        appendMessage('user', selectedTopic); // Show the user's choice
        appendMessage('bot', `You selected ${selectedTopic}`); // Show the bot's response
        document.getElementById('chat-menu').style.display = 'none'; // Hide the menu after selection
    });
});

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

// Function to get the chatbot's response via an AJAX request
function respondToUser(userInput) {
    // Append a loading message for the bot response
    const loadingMessage = appendMessage('bot', createLoadingIndicator());

    // Make AJAX request
    $.ajax({
        url: '/Chatbot/GetNewChatResponse',
        type: 'POST',
        data: {
            'userInput': userInput,
            'currentSelection': selectedTopic 
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