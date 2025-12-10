import argparse
from mlagents_envs.environment import UnityEnvironment, UnityCommunicatorStoppedException
import traceback

from agent import Agent_AAUTTO

import mlagents_envs
print(mlagents_envs.__version__)

def parse():
    parser = argparse.ArgumentParser(description="CS654 RL Project3")
    parser.add_argument('--env_name', default=None, help='environment name')
    parser.add_argument('--train_dqn', action='store_true', help='whether train DQN')
    parser.add_argument('--train_dqn_again', action='store_true', help='whether train DQN again')
    parser.add_argument('--test_dqn', action='store_true', help='whether test DQN')
    parser.add_argument('--record_video', action='store_true', help='whether to record video during testing')
    try:
        from argument import add_arguments
        parser = add_arguments(parser)
    except:
        pass
    args = parser.parse_args()
    return args

args = parse()

print("Waiting for Unity...")
unity_env = UnityEnvironment(file_name=None)
print("Connected.")

try:
    agent = Agent_AAUTTO(unity_env, args)
    agent.train()
except UnityCommunicatorStoppedException as unity_stopped:
    print(f"Unity stopped.")
    unity_env.close()
except Exception as e:
    print(f"Training exception: {e}")
    unity_env.close()
    traceback.print_exc()

try:
    unity_env.close()
except:
    pass

print(f"Finished")