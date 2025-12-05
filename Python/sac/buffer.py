import numpy as np

class ReplayBuffer:
    def __init__(self, max_size, state_dim, action_dim):
        self.max_size = max_size
        self.ptr = 0
        self.size = 0

        self.state_buffer = np.zeros((max_size, state_dim))
        self.action_buffer = np.zeros((max_size, action_dim))
        self.reward_buffer = np.zeros((max_size, 1))
        self.next_state_buffer = np.zeros((max_size, state_dim))
        self.done_buffer = np.zeros((max_size, 1))

    def add(self, state, action, reward, next_state, done):
        self.state_buffer[self.ptr] = state
        self.action_buffer[self.ptr] = action
        self.reward_buffer[self.ptr] = reward
        self.next_state_buffer[self.ptr] = next_state
        self.done_buffer[self.ptr] = done

        self.ptr = (self.ptr + 1) % self.max_size
        self.size = min(self.size + 1, self.max_size)

    def sample(self, batch_size):
        max_mem = min(self.size, self.max_size)
        batch_indices = np.random.choice(max_mem, batch_size)
        
        states = self.state_buffer[batch_indices]
        actions = self.action_buffer[batch_indices]
        rewards = self.reward_buffer[batch_indices]
        next_states = self.next_state_buffer[batch_indices]
        dones = self.done_buffer[batch_indices]
        
        return states, actions, rewards, next_states, dones
