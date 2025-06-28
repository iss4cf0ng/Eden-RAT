import socket
import struct

DNS_SERVER_IP = '0.0.0.0'
DNS_SERVER_PORT = 5353
TXT_RESPONSE = "Hello, Client!"  # Simulated response data

def parse_dns_query(data):
    """Extract the domain name and query type from the DNS request."""
    domain_parts = []
    i = 12  # DNS queries start after the header (12 bytes)
    while data[i] != 0:  # Read domain name
        length = data[i]
        domain_parts.append(data[i + 1 : i + 1 + length].decode())
        i += length + 1
    domain_name = ".".join(domain_parts)
    
    query_type = struct.unpack("!H", data[i+1:i+3])[0]  # Extract query type
    return domain_name, query_type

def build_dns_response(request, txt_data):
    """Create a DNS response with a TXT record."""
    transaction_id = request[:2]  # Transaction ID (first 2 bytes)
    flags = b"\x81\x80"  # Standard response, no error
    qdcount = request[4:6]  # Number of questions (copied from request)
    ancount = b"\x00\x01"  # 1 answer
    nscount = arcount = b"\x00\x00"  # No additional records

    header = transaction_id + flags + qdcount + ancount + nscount + arcount

    question = request[12 : request.find(b"\x00") + 5]  # Copy question section

    # Answer section (TXT record)
    answer_name = b"\xc0\x0c"  # Pointer to domain name
    answer_type = b"\x00\x10"  # TXT record
    answer_class = b"\x00\x01"  # IN (Internet)
    ttl = struct.pack("!I", 300)  # 300 seconds TTL
    txt_payload = txt_data.encode()
    txt_length = len(txt_payload)
    txt_data_section = bytes([txt_length]) + txt_payload  # TXT format: (length + text)
    data_length = struct.pack("!H", len(txt_data_section))  # Length of TXT data

    answer = answer_name + answer_type + answer_class + ttl + data_length + txt_data_section

    return header + question + answer

def start_dns_server():
    """Start a simple DNS server that handles TXT queries."""
    with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as server_socket:
        server_socket.bind((DNS_SERVER_IP, DNS_SERVER_PORT))
        print(f"DNS Server running on {DNS_SERVER_IP}:{DNS_SERVER_PORT}")

        while True:
            request, client_address = server_socket.recvfrom(512)
            domain, query_type = parse_dns_query(request)

            if query_type == 16:  # TXT Query
                print(f"Received TXT query for: {domain}")
                response = build_dns_response(request, TXT_RESPONSE)
                server_socket.sendto(response, client_address)
            else:
                print(f"Ignoring non-TXT query for: {domain}")

if __name__ == '__main__':
    start_dns_server()
