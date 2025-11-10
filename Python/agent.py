from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.base_env import ActionTuple

# Launch Unity environment headless
env = UnityEnvironment(file_name=None, no_graphics=True)
env.reset()

behavior_name = list(env.behavior_specs)[0]
spec = env.behavior_specs[behavior_name]

# Example: random actions
import numpy as np
for i in range(100):
    decision_steps, terminal_steps = env.get_steps(behavior_name)
    n_agents = len(decision_steps)
    action_size = spec.action_spec.continuous_size
    print(action_size)
    actions = np.random.uniform(-1, 1, size=(n_agents, action_size))
    print(actions)
    action_tuple = ActionTuple(continuous=actions)
    env.set_actions(behavior_name, action_tuple)
    env.step()

env.close()
