import subprocess
import socket
import time
import os

# Equivalent of the bash script
print("Extracting payloads...", flush=True)

time.sleep(2)

try:
    REPLAY_FILE = os.environ['REPLAY_FILE']
    subprocess.run(["tshark", "-r", f"/captures/{REPLAY_FILE}", "-T", "fields", "-e", "udp.payload"], stdout=open('/tmp/payloads.txt', 'w'))
except Exception as e:
    print(f"Failed to extract payloads: {e}", flush=True)
    exit(1)

print("Extracted payloads & Starting python script", flush=True)

# Configuration
target = os.environ['REPLAY_TARGET'].split(':')
UDP_IP = target[0]
UDP_PORT = int(target[1])

print(f"Target: {UDP_IP}:{UDP_PORT}", flush=True)

print("Reading payloads", flush=True)

# Convert hex payloads to binary
try:
    with open('/tmp/payloads.txt', 'r') as f:
        payloads = [bytes.fromhex(line.strip()) for line in f]
except Exception as e:
    print(f"Failed to read payloads: {e}", flush=True)
    exit(1)

total_packets = len(payloads)
print(f"Read {total_packets} payloads", flush=True)

# Send payloads
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.settimeout(5)
idx = 0
sent_packets = 0

while True:
    current_packet_idx = idx % total_packets
    payload = payloads[current_packet_idx]
    packet_size = len(payload)

    try:
        sock.sendto(payload, (UDP_IP, UDP_PORT))
        sent_packets += 1
    except Exception as e:
        print(e, flush=True)
        print("Failed to send packet -> Retrying in 10 seconds", flush=True)
        time.sleep(10)
        continue
    
    print(f"Packet {current_packet_idx + 1}/{total_packets} with {packet_size} bytes, Total Packets: {sent_packets}", flush=True)
    
    idx += 1
    time.sleep(1)

sock.close()
