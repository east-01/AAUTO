#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import torch.nn as nn
import torch.nn.functional as F


class DQN(nn.Module):
    """Initialize a deep Q-learning network

    Hints:
    -----
        Original paper for DQN
    https://storage.googleapis.com/deepmind-data/assets/papers/DeepMindNature14236Paper.pdf

    This is just a hint. You can build your own structure.
    """

    def __init__(self, in_channels=4, num_actions=4):
        """
        Parameters:
        -----------
        in_channels: number of channel of input.
                i.e The number of most recent frames stacked together, here we use 4 frames, which means each state in Breakout is composed of 4 frames.
        num_actions: number of action-value to output, one-to-one correspondence to action in game.

        You can add additional arguments as you need.
        In the constructor we instantiate modules and assign them as
        member variables.
        """
        super(DQN, self).__init__()
        
        self.conv1 = nn.Conv2d(in_channels, 32, 8, stride=4)    
        self.conv2 = nn.Conv2d(32, 64, 4, stride=2)
        self.conv3 = nn.Conv2d(64, 512, 3, stride=1)
        self.fcflatten = nn.Linear(25088, 512)
        self.fc1 = nn.Linear(512, num_actions)
        
    def forward(self, x):
        """
        In the forward function we accept a Tensor of input data and we must return
        a Tensor of output data. We can use Modules defined in the constructor as
        well as arbitrary operators on Tensors.
        """
        # print(f"shape @ 1: {x.shape}")
        x = F.relu(self.conv1(x))
        # print(f"shape @ 2: {x.shape}")
        x = F.relu(self.conv2(x))
        # print(f"shape @ 3: {x.shape}")
        x = F.relu(self.conv3(x))
        # print(f"shape @ 4: {x.shape}")
        x = x.flatten(start_dim=1)
        # print(f"shape @ 5: {x.shape}")
        x = F.relu(self.fcflatten(x))
        # print(f"shape @ 6: {x.shape}")
        x = self.fc1(x)
        return x
