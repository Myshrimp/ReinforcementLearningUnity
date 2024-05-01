import mlagents.trainers.learn
import mlagents
import torch
from mlagents_envs.environment import UnityEnvironment as UE
from mlagents_envs.environment import ActionTuple
import numpy as np
from DQN_Agent import DQN
from collections import deque
import random
epoch=1000
step=0
max_step=50000
lr=0.8
gamma=0.9
epsilon=0.95
update_frequency=100 #每隔5步更新dnn_dot*
buffer_size=1000
mini_batch=64
exp_pool=deque()#经验池
env_path="UnityEnv\AlDroneDQN\AIDrone.exe"
env=UE(file_name=env_path,seed=1,side_channels=[])
env.reset()
behavior_name = list(env.behavior_specs)[0]
spec = env.behavior_specs[behavior_name]
agent=DQN(lr=lr,epsilon=epsilon,gamma=gamma,lr_for_dnn=0.001,update_frequency=update_frequency)
def train():
  learn_round=0
  print('cuda available:',torch.cuda.is_available())
  global epoch
  for episode in range(epoch):
    env.reset()
    learn_round=0
    decision_steps, terminal_steps = env.get_steps(behavior_name)
    tracked_agent = -1  # -1 indicates not yet tracking
    done = False  # For the tracked_agent
    episode_rewards = 0  # For the tracked_agent
    state=decision_steps.obs
    state=torch.tensor(state).squeeze()
    print('episode:',episode)
    agent.epsilon=1.0
    while not done:
      learn_round+=1
      print("learn_round:", learn_round)
      action=agent.choose_action(state)

      # Track the first agent we see if not tracking
      # Note : len(decision_steps) = [number of agents that requested a decision]
      if tracked_agent == -1 and len(decision_steps) >= 1:
        tracked_agent = decision_steps.agent_id[0]
      # Generate an action for all agents
      action_to_unity = ActionTuple()
      action_to_unity.add_discrete(np.array([[action]]))
      # Set the actions
      env.set_actions(behavior_name, action_to_unity)
      # Move the simulation forward
      env.step()
      # Get the new simulation results
      decision_steps, terminal_steps = env.get_steps(behavior_name)
      next_state=decision_steps.obs
      next_state=torch.tensor(next_state).squeeze()
      print("next_state:",next_state)
      reward=0
      if tracked_agent in decision_steps:  # The agent requested a decision
        episode_rewards += decision_steps[tracked_agent].reward
        reward=decision_steps[tracked_agent].reward
        print(decision_steps[tracked_agent].reward)
      if tracked_agent in terminal_steps:  # The agent terminated its episode
        episode_rewards += terminal_steps[tracked_agent].reward
        done = True

      if len(exp_pool) < buffer_size:#############################写入经验池
        exp_pool.append((state, action, reward, next_state, done))
      else:
        exp_pool.popleft()
        exp_pool.append((state, action, reward, next_state, done))
      state=next_state
      if len(exp_pool) > mini_batch * 5:
        train_batch = random.choice(exp_pool)
        agent.learn(state=train_batch[0], reward=train_batch[2], state_=train_batch[3],
                    current_episode=learn_round, action=train_batch[1], done=train_batch[4], trun=train_batch[4])
    print(f"Total rewards for episode {episode} is {episode_rewards}")
  env.close()


train()
