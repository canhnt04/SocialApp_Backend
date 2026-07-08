// State storage for users A and B
const users = {
    A: {
        token: null,
        connection: null,
        userId: null,
        username: null,
        typingTimeout: null
    },
    B: {
        token: null,
        connection: null,
        userId: null,
        username: null,
        typingTimeout: null
    }
};

// Mock Profiles from seed database
const mockProfiles = {
    alice: { id: '11111111-1111-1111-1111-111111111111', username: 'alice' },
    bob: { id: '22222222-2222-2222-2222-222222222222', username: 'bob' },
    charlie: { id: '33333333-3333-3333-3333-333333333333', username: 'charlie' },
    david: { id: '44444444-4444-4444-4444-444444444444', username: 'david' },
    eve: { id: '55555555-5555-5555-5555-555555555555', username: 'eve' }
};

window.applyMockProfile = function(userKey) {
    const select = document.getElementById('mockProfile' + userKey);
    const profileKey = select.value;
    if (profileKey && mockProfiles[profileKey]) {
        document.getElementById('userId' + userKey).value = mockProfiles[profileKey].id;
        document.getElementById('username' + userKey).value = mockProfiles[profileKey].username;
        // Pre-fill Chat ID with the General Dev Chat
        document.getElementById('chatId' + userKey).value = '99999999-9999-9999-9999-999999999999';
        // Set mode to group
        document.getElementById('chatMode' + userKey).value = 'group';
        toggleChatMode(userKey);
    } else {
        document.getElementById('userId' + userKey).value = '';
        document.getElementById('username' + userKey).value = '';
        document.getElementById('chatId' + userKey).value = '';
    }
};

// Initialize random identities on load
window.addEventListener('DOMContentLoaded', () => {
    initializeRandomUser('A', 'alice_' + Math.floor(Math.random() * 1000));
    initializeRandomUser('B', 'bob_' + Math.floor(Math.random() * 1000));
});

// Setup click handlers for Auto Setup
document.getElementById('btnAutoSetupPrivate').addEventListener('click', () => autoSetup('private'));
document.getElementById('btnAutoSetupGroup').addEventListener('click', () => autoSetup('group'));

function initializeRandomUser(userKey, defaultName) {
    document.getElementById('userId' + userKey).value = uuidv4();
    document.getElementById('username' + userKey).value = defaultName;
}

function generateGuid(id) {
    document.getElementById(id).value = uuidv4();
}

function uuidv4() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

function showToast(message) {
    const container = document.getElementById('alertBox');
    const alert = document.createElement('div');
    alert.className = 'alert-msg';
    alert.innerText = message;
    container.appendChild(alert);
    setTimeout(() => {
        alert.style.opacity = '0';
        setTimeout(() => alert.remove(), 300);
    }, 4000);
}

function getGatewayUrl() {
    return document.getElementById('gatewayUrl').value.trim();
}

function toggleChatMode(userKey) {
    const mode = document.getElementById('chatMode' + userKey).value;
    const label = document.getElementById('chatIdLabel' + userKey);
    const btn = document.getElementById('btnVerify' + userKey);
    const joinRow = document.getElementById('groupJoinRow' + userKey);

    if (mode === 'group') {
        label.innerText = "Target Group ID (Group Chat Guid)";
        joinRow.style.display = "block";
        if (userKey === 'A') {
            btn.innerText = "Create Group";
            btn.className = "button btn-purple";
        }
    } else {
        label.innerText = "Target Chat ID (Private Chat Room Guid)";
        joinRow.style.display = "none";
        if (userKey === 'A') {
            btn.innerText = "Create with B";
            btn.className = "button btn-secondary";
        }
    }
}

// Get token via Developer endpoint
async function getToken(userKey) {
    const userId = document.getElementById('userId' + userKey).value.trim();
    const username = document.getElementById('username' + userKey).value.trim();
    const gatewayUrl = getGatewayUrl();

    if (!userId || !username) {
        showToast("Please provide both UserId Guid and Username.");
        return;
    }

    try {
        const response = await fetch(`${gatewayUrl}/api/v1/dev-auth/token`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                userId: userId,
                username: username,
                role: "User"
            })
        });

        if (!response.ok) {
            const errText = await response.text();
            throw new Error(errText || "Error obtaining token");
        }

        const data = await response.json();
        users[userKey].token = data.accessToken;
        users[userKey].userId = userId;
        users[userKey].username = username;

        showToast(`Token generated for User ${userKey} (${username})`);
        
        // Enable connect button
        document.getElementById('btnConnect' + userKey).disabled = false;
    } catch (error) {
        console.error(error);
        showToast(`Failed to generate token: ${error.message}`);
    }
}

// Create a connection with SignalR
function connectWebSocket(userKey) {
    const token = users[userKey].token;
    const gatewayUrl = getGatewayUrl();

    if (!token) {
        showToast("Acquire a token first!");
        return;
    }

    const statusBadge = document.getElementById('statusBadge' + userKey);
    const statusText = statusBadge.querySelector('.status-text');
    statusText.innerText = "Connecting...";

    const connection = new signalR.HubConnectionBuilder()
        .withUrl(`${gatewayUrl}/chatHub`, {
            accessTokenFactory: () => token
        })
        .withAutomaticReconnect()
        .build();

    // Set up event listeners
    connection.on("ReceiveMessage", (messageDto) => {
        appendMessage(userKey, messageDto);
    });

    connection.on("MessageSent", (messageDto) => {
        appendMessage(userKey, messageDto);
    });

    // Group Activity Listeners
    connection.on("UserJoined", (userId, chatId) => {
        appendStatus(userKey, `User ${userId.substring(0, 8)} joined the room`);
    });

    connection.on("UserLeft", (userId, chatId) => {
        appendStatus(userKey, `User ${userId.substring(0, 8)} left the room`);
    });

    connection.on("UserTyping", (typingUsername, chatId) => {
        const typingDiv = document.getElementById('typing' + userKey);
        typingDiv.innerText = `${typingUsername} is typing...`;
        
        if (users[userKey].typingTimeout) clearTimeout(users[userKey].typingTimeout);
        users[userKey].typingTimeout = setTimeout(() => {
            typingDiv.innerText = '';
        }, 3000);
    });

    connection.onreconnecting((error) => {
        statusBadge.className = "status-badge";
        statusText.innerText = "Reconnecting...";
    });

    connection.onreconnected((connectionId) => {
        statusBadge.className = "status-badge connected";
        statusText.innerText = "Connected";
    });

    connection.onclose((error) => {
        statusBadge.className = "status-badge";
        statusText.innerText = "Disconnected";
        document.getElementById('msgInput' + userKey).disabled = true;
        document.getElementById('btnSend' + userKey).disabled = true;
    });

    connection.start()
        .then(() => {
            statusBadge.className = "status-badge connected";
            statusText.innerText = "Connected";
            showToast(`WebSocket connected for User ${userKey}!`);
            
            users[userKey].connection = connection;
            
            // Enable inputs
            document.getElementById('msgInput' + userKey).disabled = false;
            document.getElementById('btnSend' + userKey).disabled = false;
            document.getElementById('btnConnect' + userKey).innerText = "Reconnect";
        })
        .catch(err => {
            console.error(err);
            statusBadge.className = "status-badge";
            statusText.innerText = "Failed";
            showToast(`WebSocket Connection Failed: ${err.message}`);
        });
}

// Verification wrapper
function createOrVerifyChat(userKey) {
    const mode = document.getElementById('chatMode' + userKey).value;
    if (mode === 'group') {
        createGroupChat(userKey);
    } else {
        createPrivateChat(userKey);
    }
}

// Call HTTP REST endpoint to create private chat
async function createPrivateChat(userKey) {
    const token = users[userKey].token;
    const gatewayUrl = getGatewayUrl();
    const userIdA = document.getElementById('userIdA').value.trim();
    const userIdB = document.getElementById('userIdB').value.trim();

    if (!token) {
        showToast("Authenticate first!");
        return;
    }

    try {
        const response = await fetch(`${gatewayUrl}/api/v1/Chat/private/create-or-get`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({
                userId1: userIdA,
                userId2: userIdB
            })
        });

        if (!response.ok) {
            const errText = await response.text();
            throw new Error(errText || "Error creating/fetching chat");
        }

        const chatInfo = await response.json();
        const chatId = chatInfo.chatId || chatInfo.id;
        
        document.getElementById('chatIdA').value = chatId;
        document.getElementById('chatIdB').value = chatId;
        
        showToast(`Chat room successfully verified! ChatId: ${chatId}`);
        
        // Auto-join the chat room on WS
        await joinGroupHub('A');
        await joinGroupHub('B');
    } catch (error) {
        console.error(error);
        showToast(`Failed to create chat room: ${error.message}`);
    }
}

// Call HTTP REST endpoint to create group chat
async function createGroupChat(userKey) {
    const token = users[userKey].token;
    const gatewayUrl = getGatewayUrl();
    const userIdA = document.getElementById('userIdA').value.trim();
    const userIdB = document.getElementById('userIdB').value.trim();

    if (!token) {
        showToast("Authenticate first!");
        return;
    }

    try {
        const response = await fetch(`${gatewayUrl}/api/v1/Chat/groups`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({
                name: "Dev Team Chat Room",
                members: [
                    { userId: userIdA },
                    { userId: userIdB }
                ]
            })
        });

        if (!response.ok) {
            const errText = await response.text();
            throw new Error(errText || "Error creating group chat");
        }

        const chatInfo = await response.json();
        const groupId = chatInfo.id || chatInfo.chatId;

        document.getElementById('chatIdA').value = groupId;
        document.getElementById('chatIdB').value = groupId;

        showToast(`Group Chat successfully created! GroupId: ${groupId}`);
        
        // Automatically join the group room on WS
        await joinGroupHub('A');
        await joinGroupHub('B');
    } catch (error) {
        console.error(error);
        showToast(`Failed to create group: ${error.message}`);
    }
}

// Join Hub Group (websocket command)
async function joinGroupHub(userKey) {
    const connection = users[userKey].connection;
    const chatId = document.getElementById('chatId' + userKey).value.trim();

    if (!connection) {
        showToast(`User ${userKey} WebSocket connection is not active!`);
        return;
    }

    if (!chatId) {
        showToast("Create or enter a valid Group ID first.");
        return;
    }

    try {
        // Call hub method JoinChat(string chatId)
        await connection.invoke("JoinChat", chatId);
        showToast(`User ${userKey} joined group: ${chatId.substring(0,8)}...`);
    } catch (error) {
        console.error(error);
        showToast(`Failed to join group on Hub: ${error.message}`);
    }
}

function syncChatId(userKey) {
    const sourceKey = userKey === 'A' ? 'B' : 'A';
    const sourceChatId = document.getElementById('chatId' + sourceKey).value.trim();
    document.getElementById('chatId' + userKey).value = sourceChatId;
    showToast(`Synced Room ID from User ${sourceKey}`);
}

// Send a message via websocket connection
async function sendMessage(userKey) {
    const connection = users[userKey].connection;
    const mode = document.getElementById('chatMode' + userKey).value;
    const chatId = document.getElementById('chatId' + userKey).value.trim();
    const recipientId = document.getElementById('userId' + (userKey === 'A' ? 'B' : 'A')).value.trim();
    const senderUsername = users[userKey].username;
    const content = document.getElementById('msgInput' + userKey).value.trim();

    if (!connection) {
        showToast("WebSocket connection is not active!");
        return;
    }

    if (!chatId) {
        showToast("Verify/Create a Chat Room ID first.");
        return;
    }

    if (!content) return;

    try {
        // Invoke Hub method: SendMessage(string chatId, string content)
        await connection.invoke("SendMessage", chatId, content);
        document.getElementById('msgInput' + userKey).value = '';
    } catch (error) {
        console.error(error);
        showToast(`Error sending message: ${error.message}`);
    }
}

// Send typing indicator through websocket
function handleTyping(userKey) {
    const connection = users[userKey].connection;
    const mode = document.getElementById('chatMode' + userKey).value;
    const chatId = document.getElementById('chatId' + userKey).value.trim();
    const recipientId = document.getElementById('userId' + (userKey === 'A' ? 'B' : 'A')).value.trim();
    const senderUsername = users[userKey].username;

    if (!connection) return;

    const now = Date.now();
    if (!users[userKey].lastTypingSent || (now - users[userKey].lastTypingSent) > 1500) {
        users[userKey].lastTypingSent = now;
        if (chatId) {
            connection.invoke("Typing", chatId)
                .catch(err => console.error(err));
        }
    }
}

// Append message to UI (Secured: using createElement & textContent to prevent XSS)
function appendMessage(userKey, messageDto, labelSuffix = '') {
    const container = document.getElementById('messages' + userKey);
    const direction = messageDto.senderId === users[userKey].userId ? 'self' : 'other';
    
    const messageElement = document.createElement('div');
    messageElement.className = `message-bubble ${direction}`;
    
    const time = new Date(messageDto.createdAt || Date.now()).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    
    // Build secure message elements
    const headerDiv = document.createElement('div');
    headerDiv.className = 'message-header';
    const strong = document.createElement('strong');
    strong.textContent = `${messageDto.senderUsername}${labelSuffix}`;
    headerDiv.appendChild(strong);
    
    const contentDiv = document.createElement('div');
    contentDiv.textContent = messageDto.content;
    
    const timeDiv = document.createElement('div');
    timeDiv.className = 'message-time';
    timeDiv.textContent = time;
    
    messageElement.appendChild(headerDiv);
    messageElement.appendChild(contentDiv);
    messageElement.appendChild(timeDiv);
    
    container.appendChild(messageElement);
    container.scrollTop = container.scrollHeight;
}

function appendStatus(userKey, text) {
    const container = document.getElementById('messages' + userKey);
    const statusElement = document.createElement('div');
    statusElement.className = 'message-bubble status';
    statusElement.textContent = text;
    container.appendChild(statusElement);
    container.scrollTop = container.scrollHeight;
}

// Quick setup logic
async function autoSetup(mode) {
    showToast("Initializing identities...");
    
    // Set chat mode inputs
    document.getElementById('chatModeA').value = mode;
    document.getElementById('chatModeB').value = mode;
    toggleChatMode('A');
    toggleChatMode('B');

    // Clear previous chats (Secured: using textContent = '')
    document.getElementById('messagesA').textContent = '';
    document.getElementById('messagesB').textContent = '';

    // Get tokens
    await getToken('A');
    await getToken('B');
    
    // Connect WebSockets
    showToast("Connecting User A and User B WebSockets...");
    connectWebSocket('A');
    connectWebSocket('B');
    
    // Wait 2 seconds for WS connection to establish, then register the chat room
    setTimeout(async () => {
        showToast("Creating a shared chat room...");
        createOrVerifyChat('A');
        showToast("⚡ Setup Complete! Send messages below.");
    }, 2000);
}
