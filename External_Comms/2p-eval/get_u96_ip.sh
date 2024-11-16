#!/bin/bash

# Get the IP address of eth0
ip_address=$(ifconfig eth0 | grep 'inet ' | awk '{print $2}')

# Output the IP address
echo "IP address of our u96: $ip_address"
