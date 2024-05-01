import torch
from mlagents_envs.environment import UnityEnvironment as UE
from mlagents_envs.environment import ActionTuple
from UnityEnvManager import UEManager
import cv2
env_path="E:\\UnityBuild\AIDroneCameraTest\AIDrone.exe"
env=UEManager(env_path)

state=env.reset()
state,_,_=env.env_step(1)
im=cv2.cvtColor(state,cv2.COLOR_BGR2RGB)
cv2.imshow('image',im)
cv2.waitKey()
env.env.close()
