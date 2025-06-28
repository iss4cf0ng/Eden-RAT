import socket

# HTTP server settings
HTTP_SERVER_IP = '127.0.0.1'
HTTP_SERVER_PORT = 8080

def send_http_post(data):
    """Send a POST request with data to the server and receive the response."""
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as client_socket:
        client_socket.connect((HTTP_SERVER_IP, HTTP_SERVER_PORT))
        print(f"Connected to {HTTP_SERVER_IP}:{HTTP_SERVER_PORT}")

        # Prepare request headers
        data_bytes = data.encode()  # Convert to bytes
        content_length = len(data_bytes)
        
        request = (
            "POST / HTTP/1.1\r\n"
            "Host: 127.0.0.1\r\n"
            "Content-Type: text/plain\r\n"
            f"Content-Length: {content_length}\r\n"
            "\r\n"
        ).encode() + data_bytes  # Append the body after headers

        # Send the HTTP POST request
        client_socket.sendall(request)

        # Receive the response from the server
        response = client_socket.recv(1024)
        response_str = response.decode()

        # Separate headers and body (headers end with \r\n\r\n)
        headers, body = response_str.split("\r\n\r\n", 1)

        # Print the headers and body
        print(f"Response headers:\n{headers}")
        print(f"Response body:\n{body}")

if __name__ == '__main__':
    send_http_post("Hello, this is POST data!")
