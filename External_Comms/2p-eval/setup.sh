#!/bin/bash

# Update and install basic tools
echo "Updating system and installing basic tools..."
sudo apt-get update
sudo apt-get install -y python3 python3-pip python3-venv git

# Clone your project (optional, if needed)
# git clone https://github.com/your-repo-url.git

# Navigate to your project directory (if cloned or already available)
# cd your-project-directory

# Create a Python virtual environment
# echo "Creating virtual environment..."
# python3 -m venv venv

# Activate the virtual environment
# echo "Activating virtual environment..."
# source venv/bin/activate

# Install Python dependencies from requirements.txt
if test -f "requirements.txt"; then
    echo "Installing dependencies from requirements.txt..."
    pip install -r requirements.txt
else
    echo "requirements.txt not found!"
fi

# Additional custom setup (optional)
# e.g., install other dependencies, download datasets, etc.

echo "Setup completed successfully!"
