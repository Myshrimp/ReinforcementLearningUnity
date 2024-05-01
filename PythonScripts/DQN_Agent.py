import gym
import torch
import torch.nn as nn
from torch.nn import functional
import numpy as np
import random
import collections
should_save=False
load_model=False
is_train=True
dnn_dot_path= 'model\\dnn_dot.pt'
dnn_path= 'model\\dnn.pt'
env = gym.make('MountainCar-v0',render_mode='human')
lr=0.8
gamma=0.9
epsilon=1
update_frequency=100 #每隔5步更新dnn_dot
buffer_size=1000
mini_batch=64
exp_pool=collections.deque()
Loss=[]
Rounds=[]
class DNN(nn.Module):
    def __init__(self):
        super(DNN,self).__init__()

        self.fc1=nn.Linear(4,256,bias=True)
        self.fc2=nn.Linear(256,256,bias=True)
        self.fc3=nn.Linear(256,9,bias=True)


    def forward(self,x):
        x=self.fc1(x)
        x=nn.functional.relu(x)
        x=self.fc2(x)
        x=nn.functional.relu(x)
        x=self.fc3(x)
        return x


class DQN():
    def __init__(self,lr,epsilon,gamma,lr_for_dnn,update_frequency):
        self.device = 'cpu'
        self.min_epsilon=0.01
        self.decay=0.98
        self.dnn_theta=DNN().to(self.device)
        self.dnn_theta_dot=DNN().to(self.device) #fixed
        if load_model:
            self.dnn_theta=torch.load(dnn_path)
            self.dnn_theta_dot=torch.load(dnn_dot_path)
        self.lr=lr
        self.epsilon=epsilon
        self.gamma=gamma
        self.optimizer=torch.optim.Adam(self.dnn_theta.parameters(),lr_for_dnn)
        self.loss_fn=nn.MSELoss()
        self.update_frequency=update_frequency

    def choose_action(self,state):
        action=np.random.randint(9)
        rand=np.random.uniform()
        if rand>self.epsilon:
            print('rand-epsilon:',rand-self.epsilon)
            q_values=self.dnn_theta(state)
            action=torch.argmax(q_values).item()
            print('action:',action)
        if self.epsilon>self.min_epsilon:
            self.epsilon*=self.decay
        return action

    def learn(self,state,action,reward,state_,current_episode,done,trun):

        target_Q=self.dnn_theta_dot(state_)
        if trun:
            target=reward
        else:
            target=reward+self.gamma*torch.max(target_Q,dim=0)[0]


        real_q=self.dnn_theta(state)[action]
        if target == 0.0:
            target=real_q
        print('real_q:',real_q)
        print('target:',target)

        loss=self.loss_fn(target,real_q).to(self.device)
        print('loss:',loss)
        #if current_episode%100==0:
            #print("Loss:",loss)
        self.optimizer.zero_grad()
        loss.backward()
        self.optimizer.step()
        if current_episode%update_frequency==0:
            # print("dnn_theta_dot updated")
            #self.dnn_theta_dot.load_state_dict(self.dnn_theta.state_dict())
            for dnn_dot,dnn in zip(self.dnn_theta_dot.parameters(),self.dnn_theta.parameters()):
                dnn_dot.data.copy_(dnn.data)
        if done:
            print('succeeded:', count)
            save()


        return

agent=DQN(lr=lr,epsilon=epsilon,gamma=gamma,lr_for_dnn=0.001,update_frequency=update_frequency)

episode=200
episode_length=1000
if not is_train:
    agent.dnn_theta.eval()
    agent.dnn_theta_dot.eval()
else:
    agent.dnn_theta.train()
    agent.dnn_theta_dot.train()


def save():
    torch.save(agent.dnn_theta, "model\\dnn.pt")
    torch.save(agent.dnn_theta_dot, 'model\\dnn_dot.pt')

count=0
def train():
    global count
    for ep in range(episode):
        print("episode:", ep)
        obs, _ = env.reset()
        obs = torch.tensor(obs, dtype=float).to(agent.device)
        while True:
            count+=1
            env.render()
            action = agent.choose_action(obs)
            observation, reward,done, trun, _ = env.step(action)
            observation = torch.tensor(observation, dtype=float).to(agent.device)
            reward = torch.tensor(reward, dtype=float).to(agent.device)
            if len(exp_pool)<buffer_size:
                exp_pool.append((obs,action,reward,observation,done,trun))
            else:
                exp_pool.popleft()
                exp_pool.append((obs,action,reward,observation,done,trun))
            obs = observation

            learn_round=count
            if len(exp_pool)>mini_batch * 5:
                learn_round += 1
                train_batch = random.choice(exp_pool)
                agent.learn(state=train_batch[0], reward=train_batch[2], state_=train_batch[3],
                            current_episode=learn_round, action=train_batch[1], done=train_batch[4],trun=train_batch[5])

            if done:
                print('succeeded:',count)
                save()
            if done or trun:
                print("Done")
                break
def evaluate():
    for i in range(episode_length):
        obs, _ = env.reset()
        env.render()
        obs = torch.tensor(obs, dtype=float)
        action = agent.choose_action(obs)
        env.step(action)

if __name__=='__main__':
    if not is_train:
        evaluate()
    else:
        train()
