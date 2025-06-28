import socket

# HTTP server settings
HTTP_SERVER_IP = '127.0.0.1'
HTTP_SERVER_PORT = 8080

def start_http_server():
    """Start a basic HTTP server to receive and respond with text data."""
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as server_socket:
        server_socket.bind((HTTP_SERVER_IP, HTTP_SERVER_PORT))
        server_socket.listen(1)
        print(f"HTTP server listening on {HTTP_SERVER_IP}:{HTTP_SERVER_PORT}")

        while True:
            # Accept a client connection
            client_socket, client_address = server_socket.accept()
            with client_socket:
                print(f"Connection from {client_address}")

                # Receive the HTTP request (max 1024 bytes)
                request = client_socket.recv(1024)
                print(f"Received request:\n{request.decode()}")

                # Prepare the body content (the response text)
                response_body = "Hello from HTTP server!"
                response_body_size = len(response_body)

                # Prepare the full HTTP response including headers and body
                response_headers = (
                    f"HTTP/1.1 200 OK\r\n"
                    f"Content-Type: text/plain\r\n"
                    f"Content-Length: {response_body_size}\r\n"
                    f"\r\n"  # Empty line to indicate the end of headers
                )
                response = response_headers + response_body

                # Send the response back to the client
                client_socket.sendall(response.encode())

if __name__ == '__main__':
    start_http_server()
