import mlagents.trainers.learn
import mlagents
import torch
from mlagents_envs.environment import UnityEnvironment as UE
from mlagents_envs.environment import ActionTuple
import numpy as np
import random
class UEManager():
    def __init__(self,env_path):
        self.tracked_agent=-1
        self.env=UE(file_name=env_path, seed=1, side_channels=[])
        self.env.reset()
        self.behavior_name=list(self.env.behavior_specs)[0]
        print("Behavior name:",self.behavior_name)
    def reset(self):
        decision_steps, terminal_steps = self.env.get_steps(self.behavior_name)
        if self.tracked_agent == -1 and len(decision_steps) >= 1:
            self.tracked_agent = decision_steps.agent_id[0]
        state=decision_steps.obs
        state=state[0][0]
        return state
    def env_step(self,action):
        done=False
        reward = 0
        action_to_unity = ActionTuple()
        action_to_unity.add_discrete(np.array([[action]]))
        self.env.set_actions(self.behavior_name, action_to_unity)
        # Move the simulation forward
        self.env.step()
        decision_steps, terminal_steps = self.env.get_steps(self.behavior_name)
        next_state = decision_steps.obs[0][0]
        if self.tracked_agent in decision_steps:  # The agent requested a decision
            reward = decision_steps[self.tracked_agent].reward
        if self.tracked_agent in terminal_steps:  # The agent terminated its episode
            done = True
        return next_state,reward,done