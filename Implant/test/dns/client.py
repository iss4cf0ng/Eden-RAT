import socket
import struct

DNS_SERVER_IP = '127.0.0.1'  # Local server for testing
DNS_SERVER_PORT = 5353
QUERY_DOMAIN = "test.com"

def build_dns_query(domain):
    """Construct a DNS query for a TXT record."""
    transaction_id = b"\xaa\xbb"  # Random ID
    flags = b"\x01\x00"  # Standard query
    qdcount = b"\x00\x01"  # 1 question
    ancount = nscount = arcount = b"\x00\x00"  # No additional records

    header = transaction_id + flags + qdcount + ancount + nscount + arcount

    question = b""
    for part in domain.split("."):
        question += bytes([len(part)]) + part.encode()
    question += b"\x00"  # End of domain
    qtype = b"\x00\x10"  # TXT record
    qclass = b"\x00\x01"  # IN (Internet)

    return header + question + qtype + qclass

def parse_dns_response(response):
    """Extract the TXT record from the DNS response."""
    txt_start = response.find(b"\x00\x10\x00\x01") + 10  # Locate TXT record section
    txt_length = response[txt_start]  # First byte = length of TXT data
    txt_data = response[txt_start + 1 : txt_start + 1 + txt_length].decode()
    return txt_data

def send_dns_query():
    """Send a TXT DNS query and receive a response."""
    with socket.socket(socket.AF_INET, socket.SOCK_DGRAM) as client_socket:
        query = build_dns_query(QUERY_DOMAIN)
        client_socket.sendto(query, (DNS_SERVER_IP, DNS_SERVER_PORT))

        response, _ = client_socket.recvfrom(512)
        txt_record = parse_dns_response(response)
        print(f"Received TXT Record: {txt_record}")

if __name__ == '__main__':
    send_dns_query()
