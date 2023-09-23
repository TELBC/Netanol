# Using Debian as our base image
FROM python:3

# Install tcpreplay and iproute2
RUN apt-get update && \
    apt-get install -y tshark && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Set the working directory inside the container
WORKDIR /

# Copy the replay script into the container
COPY replay.sh /replay.sh
COPY send_payloads.py /send_payloads.py
RUN chmod +x /replay.sh

# TODO: if you are bored, use volumes
# Copy the captures directory
COPY captures /captures/

# Set the default command for the container
ENTRYPOINT ["/replay.sh"]