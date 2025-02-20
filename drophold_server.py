import asyncio
import websockets
import ssl

async def handler(websocket, path):
    if path == "/payload":
        with open("payload.exe", "rb") as f:
            await websocket.send(f.read())
    else:
        await websocket.close()

async def main():
    ssl_context = ssl.SSLContext(ssl.PROTOCOL_TLS_SERVER)
    ssl_context.load_cert_chain(certfile="cert.pem", keyfile="key.pem")

    async with websockets.serve(handler, "0.0.0.0", 443, ssl=ssl_context):
        await asyncio.Future()

if __name__ == "__main__":
    asyncio.run(main())
