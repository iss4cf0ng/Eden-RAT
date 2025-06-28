const socket = new WebSocket('wss://127.0.0.1:8080')
socket.onopen = function(event)
{
    console.log('Connected');
    socket.send('hello server');
}