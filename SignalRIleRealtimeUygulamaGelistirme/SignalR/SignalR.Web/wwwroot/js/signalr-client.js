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
    })

    var spanClientCount = $("#spanConnectedClientCount");
    connection.on("ReceiveConnectedClientCountAllClient", (count) => {
        spanClientCount.text(count);
        console.log("Connected Client Count", count)
    })
    
    $("#btnSendMessageAllClient").click(function () {
        const message = "hello world";
        connection.invoke("BroadCastMessageToAllClient", message).catch(err => console.error("hata", err));
    })
});