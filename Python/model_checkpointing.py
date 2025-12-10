import os
import matplotlib.pyplot as plt
from tqdm import tqdm
import time

import os
import time
import matplotlib.pyplot as plt
import torch
from tqdm import tqdm

from utils import secs_to_HHhMMmSSs

BASE_PATH = "./%MODELNAME%/%MODELTAG%.pth"
MODEL_TAG_OPTIONS = ["latest", "best", "trained", "graph"]

def get_pth_path(model_name, tag):
	""" Given a model name and tag combo return the path to the .pth file under the base path. """
	path = BASE_PATH.replace("%MODELNAME%", model_name).replace("%MODELTAG%", str(tag))

	directory = path.replace(os.path.basename(path), "")
	if(not os.path.isdir(directory)):
		os.mkdir(directory)

	return path

def get_model_tag(model_tag):
	""" Verifies model tag and returns option to replace in model file name."""
	if(model_tag not in MODEL_TAG_OPTIONS):
		raise Exception(f"Tag \"{model_tag}\" is not a valid model tag.")
	
	try:
		tag_int = int(model_tag)
		return f"checkpoint_{tag_int}"
	except Exception as e:
		raise Exception(f"Failed to parse model tag \"{model_tag}\" as an integer. If the tag is NOT one of the options out of [{', '.join(MODEL_TAG_OPTIONS)}] you can provide an integer checkpoint step number.")

def print_init_train(agent):
	pre_training = "Starting fresh." if agent.total_train_seconds == 0 else f"Already trained for {secs_to_HHhMMmSSs(agent.total_train_seconds)}"
	print(f"""##### Training model #####
			\r{pre_training}
			\rOn {agent.episode_num}/{agent.args.episodes} ({(agent.episode_num/agent.args.episodes)*100:.0f}%)
			\rHighest avg R={agent.best_episode_avg:.2f}
			\rEpsilon warmup steps: {agent.args.epsilon_warmup_steps}, decay steps: {agent.args.epsilon_decay_steps}""")

def n_steps_complete(agent):
	print_n_step(agent)
	output_training_graph(agent)

def print_n_step(agent):
	avg_steps_in_ep = agent.step_number/agent.episode_num
	elapsed_time = agent.total_train_seconds + (time.time() - agent.start_time)
	recent = list(agent.last_thirty_scores)[-10:]
	tqdm.write(f"""##### Episode {agent.episode_num}/{agent.args.episodes} @ {secs_to_HHhMMmSSs(elapsed_time)} #####
					\r""")
					# \rε={agent.get_epsilon():.4f} rbuff={len(agent.replay_buff)}
					# \rAverages: R={agent.averages[-1]:.2f}, Steps={avg_steps_in_ep:.2f} ({agent.step_number})
					# \rLast 10: [{", ".join([str(int(x)) for x in recent])}]

def output_training_graph(agent):
	""" Matplot lib plot to create the output training curve. """

	fig, ax = plt.subplots(figsize=(8, 4))

	ax.plot(agent.averages, linewidth=2)

	ax.set_title("Reward vs. Episode")
	ax.set_xlabel("Episode")
	ax.set_ylabel("Avg. reward over last 30 episodes")
	ax.grid(True, which="both", linestyle="--", alpha=0.5)

	fig.tight_layout()
	path = get_pth_path(agent.args.model_name, "graph")[:-4]
	path += ".png"
	fig.savefig(path)

	plt.close(fig)

def save_model_data(agent, tag):
	"""
	Save the model data to a .pth file. Filepath generated according to get_pth_path(), using the
		agent.args.model_name and tag as arguments.
	"""
	path = get_pth_path(agent.args.model_name, tag)

	weights_only = tag == "trained" or tag == "best"
	model_data = agent._export_model_data(weights_only)

	torch.save(model_data, path)
	tqdm.write(f"Saved {'model' if weights_only else 'checkpoint'} to \"{path}\"")

def install_model_data(agent, tag):
	""" Load a model name and tag to this agent.
		The tag will be the name of the .pth file under the model name's directory folder. """
	
	path = get_pth_path(agent.args.model_name, tag)
	
	if(not os.path.exists(path)):
		tqdm.write(f"Model data doesn't exist at \"{path}\" starting training from scratch.")
		return agent._load_as_train_default()

	tqdm.write(f"Loading model data from \"{path}\"")
	model_data = torch.load(path, map_location=agent.device, weights_only=False)

	if(model_data is None):
		return agent._load_as_train_default()
	elif("model" in model_data and agent.args.train_dqn):
		return agent._load_as_checkpoint(model_data)
	else:
		return agent._load_as_weights(model_data)
	
def read_model_data(agent, model_name, tag):
	""" Load a model name and tag to this agent.
		The tag will be the name of the .pth file under the model name's directory folder. """
	