from collections import deque, namedtuple
import matplotlib.pyplot as plt
import numpy as np
import random
import random
import time
import torch
import torch.nn.functional as F
import torch.optim as optim
from torch import nn
from tqdm import tqdm
from mlagents_envs.environment import UnityEnvironment, DecisionSteps
from mlagents_envs.base_env import ActionTuple

from model_checkpointing import print_init_train, n_steps_complete, save_model_data, install_model_data

torch.manual_seed(595)
np.random.seed(595)
random.seed(595)

# NOTE: I followed along the pytorch tutorial for DQN https://docs.pytorch.org/tutorials/intermediate/reinforcement_q_learning.html
Transition = namedtuple('Transition',
						('state', 'action', 'next_state', 'reward'))

PRINT_EVERY_N_EPS = 50

class Agent_AAUTTO():
	def __init__(self, env, args):
		"""
		Initialize everything you need here.
		For example: 
			paramters for neural network  
			initialize Q net and target Q net
			parameters for repaly buffer
			parameters for q-learning; decaying epsilon-greedy
			...
		"""

		# Ensure valid arguments
		assert args.batch_size <= args.min_buff_size
		assert args.min_buff_size <= args.max_buff_size

		self.env = env
		self.args = args

		self.device = torch.device("cuda" if torch.cuda.is_available() else "cpu")

		if args.test_dqn:
			install_model_data(agent=self, tag=self.args.model_tag)

		# self.optimizer = torch.optim.Adam(self.dqn.parameters(), lr=self.args.learning_rate)

#region Initializers/Deinitializers
	def init_game_setting(self):
		pass
	
	def init_training(self):
		""" Called at the start of training. """

		install_model_data(self, self.args.model_tag)

		# Other initializers that don't come from checkpoint
		self.last_thirty_sum = sum(self.last_thirty_scores)
		self.start_time = time.time()

		print_init_train(self)
#endregion

#region Agent
	def make_action(self, decision_steps: DecisionSteps, test=True):
		"""
		Return predicted action of your agent
		Input:
			observation: np.array
				stack 4 last preprocessed frames, shape: (84, 84, 4)
		Return:
			action: int
				the predicted action from trained model
		"""

		behavior_name = list(self.env.behavior_specs)[0]
		spec = self.env.behavior_specs[behavior_name]

		# How ChatGPT showed me to unpack it. We'll prolly have to figure out the right way to do
		#   it once we understand our sensors better.		
		n_agents = len(decision_steps)
		for agent_idx in range(n_agents):
			obs_per_agent = [obs[agent_idx] for obs in decision_steps.obs]
			reward = decision_steps.reward[agent_idx]
			agent_id = decision_steps.agent_id[agent_idx]

			for obs in obs_per_agent:
				print("Observed:", obs)

		n_agents = len(decision_steps)
		action_size = spec.action_spec.continuous_size

		acts_cont = np.random.uniform(-1, 1, size=(n_agents, action_size))
		acts_disc = np.array([[0]])
	
		return ActionTuple(continuous=acts_cont, discrete=acts_disc)
#endregion

#region Training
	def train(self):

		# if(not torch.cuda.is_available()):
		# 	raise Exception("Not going to train without cuda...")
		
		self.init_training()

		# Manual progress bar to set initial
		progress_bar = tqdm(initial=self.episode_num, total=self.args.episodes, desc="Training")

		for self.episode_num in range(self.episode_num, self.args.episodes):

			self.play_episode()

			if(self.episode_num != 0):
				# tqdm.write(f"New best: {self.best_episode_avg}")
				# save_model_data(agent=self, tag="best")

				if(self.episode_num % PRINT_EVERY_N_EPS == 0):
					n_steps_complete(self)

			progress_bar.update(1)

		progress_bar.close()

		self.env.close()

	def play_episode(self):
		""" Play a single game of breakout, starts with an environment reset and continues until
			  the ball drops. """

		env: UnityEnvironment = self.env
		env.reset()

		if(len(env.behavior_specs) == 0):
			raise Exception(f"No behaviour specs found.")
		
		behavior_name = list(env.behavior_specs)[0]

		if(behavior_name != "CarController?team=0"):
			raise Exception(f"Found unexpected behaviour named: {behavior_name}")

		self.init_game_setting()

		for i in range(int(1e10)):
			decision_steps, terminal_steps = env.get_steps(behavior_name)

			action_tuple = self.make_action(decision_steps, test=False)
			env.set_actions(action_tuple)

			# self.run_grad_desc()

			self.increment_step()
			env.step()

	def run_grad_desc(self):
		pass
	
	def increment_step(self):
		if(self.step_number % self.args.checkpoint_long_steps == 0 and self.step_number != 0):
			save_model_data(agent=self, tag=self.step_number)

		if(self.step_number % self.args.checkpoint_short_steps == 0 and self.step_number != 0):
			save_model_data(agent=self, tag="latest")

		self.step_number += 1
#endregion

#region Training utils
	def get_epsilon(self):
		if(self.step_number < self.args.epsilon_warmup_steps):
			# Warmup phase
			return 1
		elif(self.step_number >= self.args.epsilon_warmup_steps and self.step_number < self.args.epsilon_warmup_steps + self.args.epsilon_decay_steps):
			# Decay phase
			progress = (self.step_number-self.args.epsilon_warmup_steps) / self.args.epsilon_decay_steps
			progress = min(progress, 1.0)
			return self.args.epsilon_start + (self.args.epsilon_end - self.args.epsilon_start) * progress
		else:
			# Post decay sine wave phase
			mine = self.args.epsilon_lowest
			maxe = self.args.epsilon_end
			freq = self.args.epsilon_phase_len / (2*np.pi)
			progress = self.step_number - (self.args.epsilon_warmup_steps + self.args.epsilon_decay_steps)
			return mine + 0.5*(maxe-mine)*(np.sin(progress/freq + np.pi/2) + 1)
		
	def replay_push(self, state, action, next_state, reward):
		""" You can add additional arguments as you need. 
		Push new data to buffer and remove the old one if the buffer is full.
		"""
		self.replay_buff.append(Transition(state, action, next_state, reward))
		
	def replay_buffer(self):
		""" You can add additional arguments as you need.
		Select batch from buffer.
		"""
		if(len(self.replay_buff) < self.args.batch_size):
			raise Exception("Can't sample replay buffer. Replay buffer smaller than batch size.")

		return random.sample(self.replay_buff, self.args.batch_size) # Idea from pytorch tutorial
		
	def replay_batches(self, transitions):
		""" Given a list of batches (collected from replay_buffer()) convert them into pytorch
			  tensors suitable for training on the GPU, along with masks for computing Q values/
			  running gradient descent. """
		# lists to collect per-sample tensors
		state_list = []
		action_list = []
		reward_list = []
		next_state_list = []
		non_final_mask_list = []

		for transition in transitions:
			# State
			state_tensor = self.get_observation_tensor(transition.state)
			state_list.append(state_tensor)
			# Action
			action_list.append(torch.tensor([transition.action], dtype=torch.long).to(device=self.device))
			# Next state (and non-final mask)
			if transition.next_state is None:
				non_final_mask_list.append(False)
			else:
				non_final_mask_list.append(True)

				next_state_tensor = self.get_observation_tensor(transition.next_state)
				next_state_list.append(next_state_tensor)
			# Rewards
			reward_tensor = torch.tensor(transition.reward, dtype=torch.float32).to(device=self.device)
			reward_list.append(reward_tensor)

		# stack into batches
		state_batch  = torch.cat(state_list, dim=0).to(self.device, non_blocking=True)     # (B, C, H, W)
		action_batch = torch.stack(action_list, dim=0).to(self.device, non_blocking=True)    # (B, 1)
		reward_batch = torch.stack(reward_list, dim=0).to(self.device, non_blocking=True)    # (B,)

		# This mask is used for computing the next state values so that we're not computing states that
		#   don't have a next state. This doesn't work with Q learning.
		non_final_mask = torch.tensor(non_final_mask_list, dtype=torch.bool, device=self.device)  # (B,)

		# Create a tensor mask to apply to the target DQN
		if any(non_final_mask_list):
			non_final_next_states = torch.cat(next_state_list, dim=0).to(self.device, non_blocking=True)
		else:
			non_final_next_states = None

		return state_batch, action_batch, reward_batch, non_final_mask, non_final_next_states

	def get_observation_tensor(self, observation):
		""" Convert the numpy array version of the state to a pytorch tensor on the GPU. 
			Used in the DQN. """
		# From env.reset() cell in tutorial
		# https://docs.pytorch.org/docs/stable/generated/torch.permute.html
		# https://docs.pytorch.org/docs/stable/generated/torch.unsqueeze.html
		to_float = torch.from_numpy(observation).float()
		to_zero_one = to_float / 255.0
		# Doing .unsqueeze to add the batch_size dimension: [batch_size, channel, H, W]
		tensor = to_zero_one.permute(2, 0, 1).unsqueeze(0)
		
		return tensor.to(self.device)

	def push_to_last_thirty(self, score):
		""" Keep the last thirty scores, maintaining the averages list for the graph. """

		if(len(self.last_thirty_scores) == self.last_thirty_scores.maxlen):
			oldest = self.last_thirty_scores[0]
			self.last_thirty_scores.append(score)
			self.last_thirty_sum += (score - oldest)
		else:
			self.last_thirty_scores.append(score)
			self.last_thirty_sum += score
		
		average = self.last_thirty_sum/len(self.last_thirty_scores)
		self.averages.append(average)
#endregion

#region Model saving/loading
	def _export_model_data(self, weights_only=False):
		if(weights_only):
			to_save = self.dqn.state_dict()
		else:
			to_save = {
				"model": self.dqn.state_dict(),
				"optimizer": self.optimizer.state_dict(),
				"epoch": self.episode_num,
				"step": self.step_number,
				"best_metric": self.best_episode_avg,
				"extra": {
					"last_thirty": self.last_thirty_scores,
					"replay_buff": self.replay_buff,
					"averages": self.averages,
					"running_reward": self.running_reward,
					"total_train_seconds": self.total_train_seconds + (time.time() - self.start_time)
				}
			}

		return to_save

	def _load_as_weights(self, data):
		# Strip weights from data in case we're loading a checkpoint for testing.
		if("model" in data):
			data = data["model"]

		self.dqn.load_state_dict(data, strict=False)

		return True

	def _load_as_train_default(self):
		# Correspond to values loaded in load_checkpoint
		# Load these values as defaults since it wasn't loaded above
		self.episode_num = 0
		self.step_number = 0
		self.best_episode_avg = 0

		self.last_thirty_scores = deque(maxlen=30)
		self.replay_buff = deque(maxlen=self.args.max_buff_size)
		self.averages = []
		self.running_reward = 0
		self.total_train_seconds = 0

	def _load_as_checkpoint(self, data):
		self.dqn.load_state_dict(data["model"], strict=False)
		self.optimizer.load_state_dict(data["optimizer"])

		for state in self.optimizer.state.values():
			for k, v in state.items():
				if torch.is_tensor(v):
					state[k] = v.to(self.device, non_blocking=True)

		self.episode_num = data.get("epoch", -1)
		self.step_number  = data.get("step", 0)
		self.best_episode_avg = data.get("best_metric", None)
		
		extra = data["extra"]

		self.last_thirty_scores = extra["last_thirty"]
		self.replay_buff = extra["replay_buff"]
		self.averages = extra["averages"]
		self.running_reward = extra["running_reward"]
		self.total_train_seconds = extra["total_train_seconds"]
		
		if(self.episode_num > self.args.episodes):
			raise Exception(f"Can't load checkpoint. Checkpoint's episode number is {self.episode_num} while the limit set by argument is {self.args.episodes}.")

		return True
#endregion