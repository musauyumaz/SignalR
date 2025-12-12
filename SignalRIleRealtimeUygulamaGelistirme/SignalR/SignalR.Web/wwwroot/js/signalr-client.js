$(document).ready(function () {
    const connection = new signalR.HubConnectionBuilder().withUrl("/exampleTypeSafeHub").configureLogging(signalR.LogLevel.Information).build();
    
    
    function start(){
        connection.start().then(() => console.log("Hub ile Bağlantı Kuruldu"));
    }
    try {
        start();
    }catch (e) {
        setTimeout(() => start(),5000)
    }
    
    connection.on("ReceiveMessageForAllClient", (message) => {
        console.log("Gelen Mesaj : " + message);
        $("#divAllClientMessages").append("<li>" + message + "</li>");
    });

    connection.on("ReceiveMessageForCallerClient", (message) => {
        console.log("Gelen Mesaj : " + message);
        $("#divCallerClientMessages").append("<li>" + message + "</li>");
    });

    let spanClientCount = $("#spanConnectedClientCount");
    connection.on("ReceiveConnectedClientCountAllClient", (count) => {
        spanClientCount.text(count);
        console.log("Connected Client Count", count)
    })
    
    $("#btnSendMessageAllClient").click(function () {
        const message = "hello world all client";
        connection.invoke("BroadCastMessageToAllClient", message).catch(err => console.error("hata", err));
    });
    
    $("#btnSendMessageCallerClient").click(function () {
        const message = "hello world caller client";
        connection.invoke("BroadCastMessageToCallerClient", message).catch(err => console.error("hata", err));
    });
});