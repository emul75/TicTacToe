let player;
let turnResult;
let id;
let playerSymbol = 'X';
let enemySymbol = 'O';
let joinDto;
const connection = new signalR
    .HubConnectionBuilder()
    .withUrl("/gameHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.on("getUserConnectionId", (ConnectionId) => {
    alert("your connection id " + ConnectionId);
});

connection.on("getUserConnectionId", (ConnectionId) => {
    alert("your connection id " + ConnectionId);
});

connection.on("enemyJoined", (gameId) => {
    alert("ENEMY said hemlo in ur gaem " + gameId);
});

connection.on("alert", (message) => {
    alert(message);
});

connection.on("enemyMove", (turnDto) => {
    document.getElementById(turnDto.x + "-" + turnDto.y).innerHTML = enemySymbol;

});

//connection.onclose(---);

async function connectSignalR() {
    await connection.start().then(function () {
        // alert("connected to hub");
    }).catch(function (err) {
        return console.error(err.toString());
    });
}


async function joinGroup() {
    gameId = document.getElementById("spangameid").innerText;
    console.log("Trying to join group with gameid = " + gameId)
    await connection.invoke("JoinGroup", gameId).catch(function (err) {
        return console.error(err.toString());
    });
}

async function notifyEnemy() {
    gameId = document.getElementById("spangameid").innerText;
    await connection.invoke("NotifyEnemy", gameId).catch(function (err) {
        return console.error(err.toString());
    });
}


async function makeTurnSignalR(element, x, y, gameId) {
    let turnDto = {
        PlayerId: player.id,
        GameId: gameId,
        X: x,
        Y: y
    };
    
    await connection.invoke("MakeTurn", turnDto).catch(function (err) {
        return console.error(err.toString());
    });
    element.innerHTML = playerSymbol;
}


function makeTurn(element, x, y, gameId) {
    let turnDto = {
        PlayerId: player.id,
        GameId: gameId,
        X: x,
        Y: y
    };

    $.ajax({
        url: "/api/game/maketurn",
        type: "POST",
        data: turnDto,
        success: (data) => {
            element.innerHTML = playerSymbol
            console.log(data)
            if (data === TurnResult.PlayerXWon) alert("x won");
            if (data === TurnResult.PlayerOWon) alert("o won");
            if (data === TurnResult.Draw) alert("draww");
        },
        error: function (error) {
            alert(error + "błomd fillField");
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
            id = player.id;
            findGames(player.id);
        },
        error: function (error) {
            alert(error.responseText);
            console.log(error);
        }
    });
}

function createGame() {
    let game = {
        Name: player.name + "'s game",
        PlayerId: player.id,
        BoardSize: document.getElementById("boardSizeInput").value
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
            playerSymbol = 'O';
            enemySymbol = 'X';
            document.getElementById("gameArea").innerHTML = data

            const fields = document.getElementsByClassName("field");

            for (let i = 0; i < fields.length; i++) {
                fields[i].style.width = "100px";
                fields[i].style.height = "100px";
            }
        },
        error: function (error) {
            alert(error);
        },
    });
}

function findGames(playerId) {
    $.ajax({
        url: "/api/game/new",
        type: "GET",
        // data: {name},
        success: function (data) {
            document.getElementById("gameArea").innerHTML = data
        },
        error: function (error) {
            alert(error.responseText);
            console.log(error);
        }
    });
}