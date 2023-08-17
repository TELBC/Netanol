print("Started python script", flush=True)

import socket
import time

time.sleep(3)

# Configuration
UDP_IP = "host.docker.internal"
UDP_PORT = 2055

print("Reading payloads", flush=True)
# Convert hex payloads to binary
with open('/tmp/payloads.txt', 'r') as f:
    payloads = [bytes.fromhex(line.strip()) for line in f]
print("Read payloads", flush=True)

# Send payloads
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.settimeout(5)
idx = 0
while True:
    idx += 1
    payload = payloads[idx % len(payloads)]
    sock.sendto(payload, (UDP_IP, UDP_PORT))
    print(f"Sent packet num: {idx}", flush=True)
    time.sleep(1)
    
sock.close()
