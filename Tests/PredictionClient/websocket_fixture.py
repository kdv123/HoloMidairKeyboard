"""Adversarial transport fixture; run with the TextSlinger server Python environment."""
import argparse
import asyncio
import json

from websockets.asyncio.server import serve
from websockets.exceptions import ConnectionClosed


def reply(request, word="test"):
    return {
        "version": 1,
        "request_id": request["request_id"],
        "session_id": request["session_id"],
        "revision": request["revision"],
        "status": "ok",
        "data": {"words": {"predictions": [{
            "word": word,
            "replacement": {
                "start": 0, "end": len(request["left_context"].rsplit(" ", 1)[-1]), "text": word,
            },
            "probability": {"log_retained_joint_mass": -0.5},
        }]}},
    }


async def handle(socket):
    mode = socket.request.path.lstrip("/")
    if mode == "close-before-ready":
        await socket.close()
        return
    await socket.send(json.dumps({"version": 1, "type": "ready"}))
    try:
        raw = await socket.recv()
        assert isinstance(raw, str), "Client must send text frames"
        request = json.loads(raw)
        assert request["operation"] == "words"
        assert not request.get("input_events")
        assert request["input_channel"]["complete_prior"] == 0
        if mode == "fragment":
            response = reply(request, "héllo😀")
            response["padding"] = "λ" * 6000
            encoded = json.dumps(response, ensure_ascii=False)
            await socket.send([encoded[:20], encoded[20:80], encoded[80:]])
        elif mode == "reverse":
            second = json.loads(await socket.recv())
            await socket.send(json.dumps(reply(second)))
            await socket.send(json.dumps(reply(request)))
        elif mode == "close":
            await socket.close()
        elif mode == "binary":
            await socket.send(b"binary response")
        elif mode == "malformed":
            await socket.send("{not json")
        elif mode == "silent":
            pass
        else:
            raise AssertionError("Unknown fixture mode: " + mode)
        await socket.wait_closed()
    except ConnectionClosed:
        pass


async def main(port):
    async with serve(handle, "127.0.0.1", port, subprotocols=["textslinger.v1"]):
        print(f"Fixture ready on ws://127.0.0.1:{port}", flush=True)
        await asyncio.Future()


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--port", type=int, default=18766)
    args = parser.parse_args()
    try:
        asyncio.run(main(args.port))
    except KeyboardInterrupt:
        pass
