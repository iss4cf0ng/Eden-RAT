import asyncio
import websockets
import websockets.server

async def handler(sock, path):
    print('OK')

async def start():
    server = await websockets.serve(handler, '0.0.0.0', 80)
    await server.wait_closed()

def main():
    asyncio.run(start())

if __name__ == '__main__':
    main()