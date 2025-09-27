#!/bin/bash

# Set the URL of the page you want to invoke
URL="https://matchtowatch.net/calculator"

# Set the cron schedule
SCHEDULE="0 */12 * * *"  # Run the job every 12 hours on the hour

# Create the cron job file
CRON_FILE="/etc/cron.d/invoke-calculator"
echo "$SCHEDULE root /usr/bin/curl -s $URL > /dev/null 2>&1" | sudo tee $CRON_FILE

echo "Cron job created successfully at $CRON_FILE!"