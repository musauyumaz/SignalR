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

    connection.on("ReceiveMessageAsStreamForAllClient", (name) => {
        $("#streamBox").append(`<li>${name}</li>`)
    })
    
    $("#btnFromClientToHub").click(function () {
        const names = $("#txtStream").val();
        const nameAsChunk = names.split(";")
        
        const subject = new signalR.Subject();
        
        connection.send("BroadcastStreamDataToAllClient", subject).catch(err => console.error("hata", err));
        
        nameAsChunk.forEach(name => {
            subject.next(name);
        });
        
        subject.complete();
    });
});