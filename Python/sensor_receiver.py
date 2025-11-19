import socket
import numpy as np
import torch

HOST = '0.0.0.0'
PORT = 5005
Height, Width = 1080, 1920

# Setup Server
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.bind((HOST, PORT))
s.listen(1)
connection, address = s.accept()
print(f"Connected: {address}")

# Recieving Function
def receive_tensor(connection, shape,dtype=np.uint8, normalize=False):
    # Read frame length
    length_bytes = connection.recv(4)
    if not length_bytes:
        return None

    length = int.from_bytes(length_bytes, 'little')

    # Read full frame
    data = b''
    while len(data) < length:
        data += connection.recv(length - len(data))

    # Convert to NumPy
    arr = np.frombuffer(data, dtype=dtype).reshape(shape)

    # Convert to PyTorch
    tensor = torch.from_numpy(arr)
    if normalize:
        tensor = tensor.float() / 255.0

    return tensor

# Main Loop
while True:
    # Cameras
    front = receive_tensor(connection, (Height, Width, 3), normalize=True)
    left  = receive_tensor(connection, (Height, Width, 3), normalize=True)
    right = receive_tensor(connection, (Height, Width, 3), normalize=True)

    # Convert HWC -> CHW
    front = front.permute(2,0,1)
    left  = left.permute(2,0,1)
    right = right.permute(2,0,1)

    # Pack into observation dictionary
    obs = {
        "front": front,
        "left": left,
        "right": right,
    }

    print({k: v.shape for k,v in obs.items()})