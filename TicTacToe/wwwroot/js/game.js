let player;
let playerSymbol = 'X';
let turnCounter = -1;



const connection = new signalR
    .HubConnectionBuilder()
    .withUrl("/gameHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();



connection.on("enemyJoined", (gameId) => {
    console.log("ENEMY joined ");
    updateGameDetails();
    updatePlayerSymbol();
});

connection.on("enemyLeft", () => {
    alert("ENEMY left the game.");
    findGames();
});

connection.on("consoleLog", (message) => {
    console.log("Hub: " + message);
});

connection.on("turnResult", (turnDto, turnResultEnum) => {
    updateGameDetails();
    if (document.getElementById(turnDto.x + "-" + turnDto.y).innerHTML === "") {
        document.getElementById(turnDto.x + "-" + turnDto.y).innerHTML = document.getElementById("currentTurn").innerText;

    }
    switch (turnResultEnum) {
        case TurnResult.PlayerXWon:
            alert("player X won");
            findGames();
            connection.stop();
            break;
        case TurnResult.PlayerOWon:
            alert("player O won");
            findGames();
            connection.stop();
            break;
        case TurnResult.Draw:
            alert("draw");
            findGames();
            connection.stop();
            break;
    }

});




async function connectSignalR() {
    await connection.start().then(function () {
        // alert("connected to hub");
    }).catch(function (err) {
        return console.error(err.toString());
    });
}

async function joinGroup() {
    let gameId = document.getElementById("currentGameId").innerText;
    console.log("Trying to join group with gameid = " + gameId)
    await connection.invoke("JoinGroup", gameId).catch(function (err) {
        return console.error(err.toString());
    });
}

async function notifyEnemy() {
    let gameId = document.getElementById("currentGameId").innerText;
    await connection.invoke("NotifyEnemy", gameId).catch(function (err) {
        return console.error(err.toString());
    });
}




async function makeTurn(element, x, y, gameId) {
    if (turnCounter < 0) return;
    if (element.innerHTML !== "") return;

    let currentTurn = document.getElementById("currentTurn").innerText;
    if (currentTurn !== playerSymbol) return;

    let turnDto = {
        PlayerId: player.id,
        GameId: gameId,
        X: x,
        Y: y
    };

    await connection.invoke("MakeTurn", turnDto).catch(function (err) {
        return console.error(err.toString());
    });

    updateGameDetails();
    element.innerHTML = currentTurn;

}

function leaveGame() {
    let gameId = document.getElementById("currentGameId").innerText;
    console.log("Trying to delete game " + gameId);
    $.ajax({
        url: "/api/game/leave/",
        type: "DELETE",
        data: {id: gameId},
        success: async function () {
            console.log("Game " + gameId + " has been deleted.");
            findGames();
            await connection.invoke("EnemyLeft", gameId);
            await connection.stop();
        },
        error: function (error) {
            alert(error.responseText);
            console.log(error);
        }
    });
}

function login() {
    const name = document.getElementById("nickInput").value;
    
    $.ajax({
        url: "/api/player",
        type: "POST",
        data: {name},
        success: function (data) {
            player = data;
            document.getElementById("nick").innerHTML = data.name;
            findGames();
        },
        error: function (error) {
            alert(error.responseText);
            console.log(error);
        }
    });
}

function createGame() {
    let boardSize = parseInt(document.getElementById("boardSizeInput").value);

    if (!Number.isInteger(boardSize) || boardSize < 1) {
        alert("Incorrect size");
        return;
    }

    let game = {
        Name: player.name + "'s game",
        PlayerId: player.id,
        BoardSize: boardSize
    };
    $.ajax({
        url: '/api/game/create',
        type: "POST",
        data: game,
        success: function (data) {
            document.getElementById("gameArea").innerHTML = data;
            const fields = document.getElementsByClassName("field");

            for (let i = 0; i < fields.length; i++) {
                fields[i].style.width = "100px";
                fields[i].style.height = "100px";
            }
            connectSignalR().then(joinGroup);
        },
        error: function (error) {
            alert(error);
        },
    });
}


function joinGame(id = document.getElementById("gameIdInput").value) {
    let joinDto = {
        GameId: id,
        PlayerId: player.id,
    };

    $.ajax({
        url: '/api/game/join',
        type: "POST",
        data: joinDto,
        success: function (data) {
            document.getElementById("gameArea").innerHTML = data

            const fields = document.getElementsByClassName("field");

            for (let i = 0; i < fields.length; i++) {
                fields[i].style.width = "100px";
                fields[i].style.height = "100px";
            }
            connectSignalR().then(joinGroup).then(notifyEnemy);
            updateGameDetails();
            updatePlayerSymbol();
        },
        error: function (error) {
            alert(error);
        },
    });
}

function reconnect(id = document.getElementById("currentGameId").value) {
    let joinDto = {
        GameId: id,
        PlayerId: player.id,
    };

    $.ajax({
        url: '/api/game/reconnect',
        type: "POST",
        data: joinDto,
        success: function (data) {
            document.getElementById("gameArea").innerHTML = data

            const fields = document.getElementsByClassName("field");

            for (let i = 0; i < fields.length; i++) {
                fields[i].style.width = "100px";
                fields[i].style.height = "100px";
            }
            connectSignalR().then(joinGroup);
            try{
                updateGameDetails();
            } catch {
                console.log("w8in for 2nd player");
            }
            updatePlayerSymbol();
        },
        error: function (error) {
            alert(error);
        },
    });
}

function findGames() {
    $.ajax({
        url: "/api/game/new",
        type: "GET",
        data: {playerId: player.id},
        success: function (data) {
            document.getElementById("gameArea").innerHTML = data
        },
        error: function (error) {
            alert(error.responseText);
            console.log(error);
        }
    });
}

function updateGameDetails() {
    let gameId = document.getElementById("currentGameId").innerText;
    $.ajax({
        url: "/api/game/details/" + gameId,
        type: "GET",
        success: function (data) {
            document.getElementById("gameDetails").innerHTML = data;
            turnCounter = document.getElementById("turnCounter").innerText;
        },
        error: function (error) {
            alert(error.responseText);
            console.log(error);
        }
    });
}

function updatePlayerSymbol(){
    if (document.getElementById("playerXName").innerText === player.name){
        playerSymbol = 'X';
    }
    if (document.getElementById("playerOName").innerText === player.name){
        playerSymbol = 'O';
    }
}