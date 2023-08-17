#!/bin/bash

echo "Extracting payloads... "

# Extract UDP payloads from the modified pcap file
tshark -r /captures/"$TRAFFIC_REPLAY_FILE" -T fields -e udp.payload -Y udp > /tmp/payloads.txt

echo "Extracted payloads"

sleep 3

echo "Starting python script"

# Execute the Python script to send payloads
python3 /send_payloads.py