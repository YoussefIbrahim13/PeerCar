/* PeerCar AI Assistant — SignalR streaming chat */
(function () {
    "use strict";

    const list = document.getElementById("chatMessages");
    const form = document.getElementById("chatForm");
    const input = document.getElementById("userInput");
    const sendBtn = document.getElementById("sendBtn");
    const intro = document.getElementById("chatIntro");
    const statusDot = document.getElementById("chatStatus");
    const statusText = document.getElementById("chatStatusText");

    let streamBubble = null;   // bubble currently receiving chunks
    let streamBuffer = "";
    let typingRow = null;
    let busy = false;

    const time = () => new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
    const scrollDown = () => { list.scrollTop = list.scrollHeight; };

    // Escape first, then apply a tiny, safe subset of Markdown (bold, italics, code, links, lists, line breaks).
    function escapeHtml(s) {
        return s.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;").replace(/'/g, "&#39;");
    }
    function render(text) {
        let html = escapeHtml(text);
        html = html.replace(/\*\*(.+?)\*\*/g, "<strong>$1</strong>");
        html = html.replace(/(^|[^*])\*(?!\s)(.+?)\*(?!\*)/g, "$1<em>$2</em>");
        html = html.replace(/`([^`]+)`/g, "<code>$1</code>");
        html = html.replace(/\[([^\]]+)\]\((https?:\/\/[^\s)]+|\/[^\s)]*)\)/g, '<a href="$2" target="_blank" rel="noopener">$1</a>');
        html = html.replace(/(^|[\s(])(https?:\/\/[^\s<)]+)/g, '$1<a href="$2" target="_blank" rel="noopener">$2</a>');
        // bullet lists
        html = html.replace(/(?:^|\n)((?:[-•*] .+(?:\n|$))+)/g, (m, block) =>
            "\n<ul>" + block.trim().split(/\n/).map(l => "<li>" + l.replace(/^[-•*] /, "") + "</li>").join("") + "</ul>\n");
        return html.split(/\n{2,}/).map(p => p.trim() ? "<p>" + p.trim().replace(/\n/g, "<br>") + "</p>" : "").join("");
    }

    function addMessage(kind, text) {
        if (intro) intro.remove();
        const row = document.createElement("div");
        row.className = "pc-msg is-" + kind;
        const icon = kind === "user" ? "bi-person" : kind === "error" ? "bi-exclamation-triangle" : "bi-stars";
        row.innerHTML = `<div class="pc-msg-avatar"><i class="bi ${icon}"></i></div><div><div class="pc-bubble" dir="auto"></div><small class="pc-msg-time">${time()}</small></div>`;
        if (kind === "error") row.classList.add("is-bot");
        const bubble = row.querySelector(".pc-bubble");
        if (text != null) {
            if (kind === "user") bubble.textContent = text;
            else bubble.innerHTML = render(text);
        }
        list.appendChild(row);
        scrollDown();
        return bubble;
    }

    function showTyping() {
        hideTyping();
        if (intro) intro.remove();
        typingRow = document.createElement("div");
        typingRow.className = "pc-msg is-bot";
        typingRow.innerHTML = '<div class="pc-msg-avatar"><i class="bi bi-stars"></i></div><div class="pc-bubble"><span class="pc-typing"><span></span><span></span><span></span></span></div>';
        list.appendChild(typingRow);
        scrollDown();
    }
    function hideTyping() { if (typingRow) { typingRow.remove(); typingRow = null; } }

    function setBusy(b) { busy = b; updateSend(); }
    function updateSend() { sendBtn.disabled = busy || !input.value.trim() || connection.state !== signalR.HubConnectionState.Connected; }

    function setStatus(state, text) {
        statusDot.setAttribute("data-state", state);
        statusText.textContent = text;
        updateSend();
    }

    /* ---------- SignalR ---------- */
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .withAutomaticReconnect()
        .build();

    connection.on("ReceiveMessage", (user, message) => {
        if (user === "You") return; // already shown optimistically
        hideTyping();
        addMessage(user === "System" ? "error" : "bot", message);
        setBusy(false);
    });

    connection.on("StartAssistantMessage", () => {
        hideTyping();
        streamBuffer = "";
        streamBubble = addMessage("bot", "");
        streamBubble.classList.add("pc-caret");
    });

    connection.on("ReceiveChunk", chunk => {
        if (!streamBubble) return;
        streamBuffer += chunk;
        streamBubble.innerHTML = render(streamBuffer);
        const nearBottom = list.scrollHeight - list.scrollTop - list.clientHeight < 140;
        if (nearBottom) scrollDown();
    });

    connection.on("EndAssistantMessage", () => {
        if (streamBubble) streamBubble.classList.remove("pc-caret");
        streamBubble = null;
        setBusy(false);
    });

    connection.onreconnecting(() => setStatus("connecting", "Reconnecting…"));
    connection.onreconnected(() => setStatus("online", "Online · replies in seconds"));
    connection.onclose(() => setStatus("offline", "Offline — refresh to reconnect"));

    connection.start()
        .then(() => setStatus("online", "Online · replies in seconds"))
        .catch(err => { console.error(err); setStatus("offline", "Couldn't connect to the assistant"); });

    // Safety net: if the end-of-stream event never arrives, finish once the stream goes quiet.
    let idleTimer = null;
    const observer = new MutationObserver(() => {
        if (!streamBubble) return;
        clearTimeout(idleTimer);
        idleTimer = setTimeout(() => {
            if (streamBubble) streamBubble.classList.remove("pc-caret");
            streamBubble = null;
            setBusy(false);
        }, 4000);
    });
    observer.observe(list, { childList: true, subtree: true, characterData: true });

    /* ---------- Input ---------- */
    function send(text) {
        const message = (text ?? input.value).trim();
        if (!message || busy) return;
        addMessage("user", message);
        input.value = "";
        autoGrow();
        setBusy(true);
        showTyping();
        connection.invoke("SendMessage", message).catch(err => {
            console.error(err);
            hideTyping();
            addMessage("error", "تعذر إرسال الرسالة. حاول مرة أخرى.");
            setBusy(false);
        });
    }

    function autoGrow() {
        input.style.height = "auto";
        input.style.height = Math.min(input.scrollHeight, 160) + "px";
    }

    form.addEventListener("submit", e => { e.preventDefault(); send(); });
    input.addEventListener("input", () => { autoGrow(); updateSend(); });
    input.addEventListener("keydown", e => {
        if (e.key === "Enter" && !e.shiftKey && !e.isComposing) { e.preventDefault(); send(); }
    });
    document.querySelectorAll("[data-suggest]").forEach(b => b.addEventListener("click", () => send(b.getAttribute("data-suggest"))));
    document.getElementById("chatClear").addEventListener("click", () => {
        list.querySelectorAll(".pc-msg").forEach(m => m.remove());
        streamBubble = null;
        setBusy(false);
    });

    input.focus();
})();
