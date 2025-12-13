$(document).ready(function () {
    const connection = new signalR.HubConnectionBuilder().withUrl("/exampleTypeSafeHub").configureLogging(signalR.LogLevel.Information).build();

    let currentGroupList = []
    function refreshGroupList(){
        $("#groupList").empty();
        currentGroupList.forEach(group => {
            $("#groupList").append(`<p>${group}</p>`)
        })
    }
    
    $("#btnGroupAAdd").click(function () {
        
        if (currentGroupList.includes("GroupA")) return alert("Zaten Bu Grupta Bulunuyorsunuz")
        
        connection.invoke("AddUserToGroupAsync","GroupA").then(() => {
            currentGroupList.push("GroupA");
            refreshGroupList();
        })
    }) 
    $("#btnGroupARemove").click(function () {
        if (!currentGroupList.includes("GroupA")) return alert("Zaten Bu Grupta Bulunmuyorsun")
        connection.invoke("RemoveUserToGroupAsync","GroupA").then(() => {
            currentGroupList = currentGroupList.filter(group => group !== "GroupA");
            refreshGroupList();
        })
    })

    $("#btnGroupBAdd").click(function () {
        if (currentGroupList.includes("GroupB")) return alert("Zaten Bu Grupta Bulunuyorsunuz")
        connection.invoke("AddUserToGroupAsync","GroupB").then(() => {
            currentGroupList.push("GroupB");
            refreshGroupList();
        })
    })
    $("#btnGroupBRemove").click(function () {
        if (!currentGroupList.includes("GroupB")) return alert("Zaten Bu Grupta Bulunmuyorsun")
        connection.invoke("RemoveUserToGroupAsync","GroupB").then(() => {
            currentGroupList = currentGroupList.filter(group => group !== "GroupB");
            refreshGroupList();
        })
    })
    
    function start(){
        connection.start().then(() => {
            console.log("Hub ile Bağlantı Kuruldu");
            $("#divConnectionId").html(`Connection Id: ${connection.connectionId}`);
        });
    }
    try {
        start();
    }catch (e) {
        setTimeout(() => start(),5000)
    }
    
    connection.on("ReceiveMessageForAllClient", (message) => {
        console.log("All Client Gelen Mesaj : " + message);
        $("#divAllClientMessages").append("<li>" + message + "</li>");
    });

    connection.on("ReceiveMessageForCallerClient", (message) => {
        console.log("Caller Client Gelen Mesaj : " + message);
        $("#divCallerClientMessages").append("<li>" + message + "</li>");
    });

    connection.on("ReceiveMessageForOthersClient", (message) => {
        console.log("Others Clients Gelen Mesaj : " + message);
        $("#divOthersClientMessages").append("<li>" + message + "</li>");
    });

    connection.on("ReceiveMessageForIndividualClient", (message) => {
        console.log("Individual Gelen Mesaj : " + message);
        $("#divIndividualClientMessages").append("<li>" + message + "</li>");
    });
    
    connection.on("ReceiveMessageForGroupClients", (message) => {
        console.log("Group Gelen Mesaj : " + message);
        $("#divGroupClientMessages").append("<li>" + message + "</li>");
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
    $("#btnSendMessageOthersClient").click(function () {
        const message = "hello world others client";
        connection.invoke("BroadCastMessageToOthersClient", message).catch(err => console.error("hata", err));
    });
    $("#btnSendMessageIndividualClient").click(function () {
        const message = "Hello world individual client";
        const connectionId = $("#txtConnectionId").val();
        connection.invoke("BroadCastMessageToIndividualClient",connectionId, message).catch(err => console.error("hata", err));
    })
    $("#btnGroupASendMessage").click(function () {
        const message = "Group A Message";
        const connectionId = $("#txtConnectionId").val();
        connection.invoke("BroadcastMessageToGroupClients","GroupA", message).catch(err => console.error("hata", err));
    });
    $("#btnGroupBSendMessage").click(function () {
        const message = "Group B Message";
        const connectionId = $("#txtConnectionId").val();
        connection.invoke("BroadcastMessageToGroupClients","GroupB", message).catch(err => console.error("hata", err));
    })
});