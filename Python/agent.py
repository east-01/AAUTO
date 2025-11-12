from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.base_env import ActionTuple

print("Connecting to unity environment...")

# Launch Unity environment headless
unity_env = UnityEnvironment(file_name=None, no_graphics=True)
unity_env.reset()

print("Connected.")

def train(env: UnityEnvironment):
    behavior_name = list(env.behavior_specs)[0]
    spec = env.behavior_specs[behavior_name]

    # Example: random actions
    import numpy as np
    for i in range(int(1e10)):

        decision_steps, terminal_steps = env.get_steps(behavior_name)
        n_agents = len(decision_steps)
        action_size = spec.action_spec.continuous_size

        acts_cont = np.random.uniform(-1, 1, size=(n_agents, action_size))
        acts_disc = np.array([[0]])

        try:
            action_tuple = ActionTuple(continuous=acts_cont, discrete=acts_disc)
            env.set_actions(behavior_name, action_tuple)
        except Exception as e:
            print(f"Ran into exception: {e}")
            break

        env.step()

    env.close()

try:
    train(unity_env)
except Exception as e:
    print(f"Ran into exception: {e}")
    unity_env.close()

print(f"Finished")