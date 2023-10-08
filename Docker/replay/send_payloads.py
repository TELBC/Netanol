print("Started python script", flush=True)

import socket
import time
import os

time.sleep(3)

# Configuration
target = os.environ['REPLAY_TARGET'].split(':')
UDP_IP = target[0]
UDP_PORT = int(target[1])

print(f"Target: {UDP_IP}:{UDP_PORT}", flush=True)

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
    try:
        sock.sendto(payload, (UDP_IP, UDP_PORT))
    except Exception as e:
        print(e, flush=True)
        print("Failed to send packet -> Retrying in 10 seconds", flush=True)
        time.sleep(10)
        continue
        
    print(f"Sent packet num: {idx}", flush=True)
    time.sleep(1)
    
sock.close()
