let player;
let turnResult;
let id;
let playerSymbol = 'X';
let joinDto;

// example GET:
//function getPlayerWithId1() {
//    $.ajax({
//        type: "GET",
//        url: "/api/player/1",
//        contentType: "application/json; charset=utf-8",
//        success: function (data) {
//            alert(JSON.stringify(data));
//            document.getElementById("name").innerHTML = myJson.name;
//        },
//        error: function (errMsg) {
//            alert(errMsg);
//        }
//    });
//}

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
        success: function (data) {
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