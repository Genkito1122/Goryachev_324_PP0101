const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.on("ReceiveMessage", (message) => {
});

connection.on("ReceiveAnnouncement", (announcement) => {
});

async function startConnection(classId) {
    try {
        await connection.start();
        await connection.invoke("JoinClassGroup", classId);
    } catch (err) {
        console.error(err);
    }
}
