# Use a slimmer base image
FROM python:3-slim

# Install tshark and clean-up in a single step to reduce layer size
RUN apt-get update && \
    apt-get install -y --no-install-recommends tshark && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Set the working directory
WORKDIR /

# Copy the Python script into the container
COPY send_payloads.py /send_payloads.py

# If captures directory is not too large and changes infrequently
COPY captures /captures/

# Set the default command for the container to your Python script
ENTRYPOINT ["python", "/send_payloads.py"]
