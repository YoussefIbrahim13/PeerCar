
let assistantMessageBuffer = "";
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

const chatMessages = document.getElementById("chatMessages");
const chatForm = document.getElementById("chatForm");
const userInput = document.getElementById("userInput");

let currentAssistantMessageDiv = null;


connection.on("ReceiveMessage", (user, message) => {
    appendMessage(user === "Assistant" ? "assistant-msg" : "user-msg", message);
});

connection.on("StartAssistantMessage", () => {
    assistantMessageBuffer = ""; 

    const time = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    const msgHtml = `
        <div class="message assistant-msg shadow-sm" style="direction: rtl;">
            <div class="text" id="latest-msg"></div>
            <small class="time">${time}</small>
        </div>
    `;
    chatMessages.insertAdjacentHTML('beforeend', msgHtml);
    currentAssistantMessageDiv = document.getElementById("latest-msg");
    currentAssistantMessageDiv.removeAttribute("id");
    chatMessages.scrollTop = chatMessages.scrollHeight;
});
connection.on("ReceiveChunk", (chunk) => {
    if (currentAssistantMessageDiv) {
        assistantMessageBuffer += chunk;

        currentAssistantMessageDiv.innerHTML = assistantMessageBuffer;

        chatMessages.scrollTop = chatMessages.scrollHeight;
    }
});
function appendMessage(className, text) {
    const time = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    const msgHtml = `
        <div class="message ${className} shadow-sm">
            <div class="text">${text}</div>
            <small class="time">${time}</small>
        </div>
    `;
    chatMessages.insertAdjacentHTML('beforeend', msgHtml);
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

connection.start().catch(err => console.error(err));

chatForm.addEventListener("submit", (e) => {
    e.preventDefault();
    const message = userInput.value.trim();
    if (message) {
        connection.invoke("SendMessage", message).catch(err => console.error(err));
        userInput.value = "";
    }
});