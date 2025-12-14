$(document).ready(function () {
    const connection = new signalR.HubConnectionBuilder().withUrl("/exampleTypeSafeHub").configureLogging(signalR.LogLevel.Information).build();

    async function start() {
        try {
            await connection.start().then(() => {
                console.log("Hub ile bağlantı kuruldu");
                $("#connectionId").html(`Connection Id: ${connection.connectionId}`)
            });
        } catch (err) {
            console.error("hub ile bağlantı kurulamadı", err)
            setTimeout(() => start(), 5000)
        }
    }

    connection.onclose(async () => {
        await start();
    });

    start();
});