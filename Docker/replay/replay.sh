#!/bin/sh

echo "Extracting payloads... "

sleep 2

# Extract UDP payloads from the modified pcap file
tshark -r /captures/"$REPLAY_FILE" -T fields -e udp.payload > /tmp/payloads.txt

echo "Extracted payloads & Starting python script"

# Execute the Python script to send payloads
python3 /send_payloads.py