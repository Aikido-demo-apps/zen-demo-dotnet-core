#!/bin/bash

child_pid=""
should_exit=false

# Function to handle termination signals
cleanup() {
    echo "Received termination signal, shutting down..."
    should_exit=true
    if [ -n "$child_pid" ]; then
        kill -TERM "$child_pid" 2>/dev/null || true
        wait "$child_pid" 2>/dev/null || true
    fi
    exit 0
}

# Function to handle restart signal (SIGHUP)
restart() {
    echo "Received restart signal (SIGHUP), restarting application..."
    if [ -n "$child_pid" ]; then
        kill -TERM "$child_pid" 2>/dev/null || true
        wait "$child_pid" 2>/dev/null || true
    fi
}

# Set up signal handlers
trap cleanup SIGTERM SIGINT
trap restart SIGHUP

# Main loop: restart dotnet process if it exits
while true; do
    echo "Starting dotnet application..."
    dotnet zen-demo-dotnet.dll &
    child_pid=$!
    
    # Wait for the process to exit (capture exit code even if non-zero)
    wait $child_pid
    exit_code=$?
    
    # Check if we should exit
    if [ "$should_exit" = true ]; then
        echo "Shutting down..."
        exit 0
    fi
    
    # Log and restart (regardless of exit code)
    echo "Application exited with code $exit_code, restarting in 2 seconds..."
    sleep 2
    child_pid=""
done

