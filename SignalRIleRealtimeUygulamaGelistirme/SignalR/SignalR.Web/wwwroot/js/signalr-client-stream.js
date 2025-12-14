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
    connection.on("ReceiveProductAsStreamForAllClient", (product) => {
        $("#streamBox").append(`<li>${product.id} - ${product.name} - ${product.price}</li>`)
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

    $("#btnProductFromClientToHub").click(function () {
        var productList = [
            {id: 1, name: "pen 1", price: 200},
            {id: 2, name: "pen 2", price: 300},
            {id: 3, name: "pen 3", price: 400},
            {id: 4, name: "pen 4", price: 500},
            {id: 5, name: "pen 5", price: 600},
            {id: 6, name: "pen 6", price: 700},
            {id: 7, name: "pen 7", price: 800},
            {id: 8, name: "pen 8", price: 900},
            {id: 9, name: "pen 9", price: 1000},
            {id: 10, name: "pen 10", price: 1100}
        ]

        const subject = new signalR.Subject();

        connection.send("BroadcastStreamProductToAllClient", subject).catch(err => console.error("hata", err));

        productList.forEach(product => {
            subject.next(product);
        });

        subject.complete();
    });

    $("#btnFromHubToClient").click(function () {
        connection.stream("BroadcastFromHubToClient", 25).subscribe({
            next: (message) => $("#streamBox").append(`<p>${message}</p>`)
        })
    })
});