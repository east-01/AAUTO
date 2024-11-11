AAUTO - Accelerating Autonomous Vehicles with Unity for
Training and Test Optimization
==============
## Introduction
A CS-601 Group project by Kyle Krick, Ethan Mullen, Tanishq Patil, and Dominic Griffith; we are using Unity to increase training efficiency for autonomous vehicles.<br>
<br>
Our papers: [Kyle Krick](), [Ethan Mullen](https://github.com/east-01/AAUTO/blob/master/Papers/CS_601_Project_Proposal___Ethan_Mullen.pdf), [Tanishq Patil](), [Dominic Griffith]()<br>

## Traffic light/Intersection configuration
Each traffic light will have a TrafficLightController script on it- this script is responsible for managing the state of that single light (RED, YELLOW, GREEN). An intersection can be created by adding a TrafficIntersection script to any GameObject; although placing it on a parent GameObject to all of the TrafficLights is recommended instead for organization purposes. Each group of lights is organized by a TrafficDirection struct which contains: 
- A list of all the traffic lights for that direction
- Amount of time for lights to be green and yellow
- Amount of time for the intersection to clear, i.e. how long all of the lights will be red until the next set turns green

## Training trigger configuration
All training triggers in the environment are based off of the TrainingTrigger prefab, with each type of trigger (i.e. lane deviation, wrong-way deviation) using a prefab variant. It's set up this way so we can easily change the reward values for each type of trigger. For traffic light triggers, the traffic light controllers will control it's corresponding trigger based on the state it's in; we can easily change the reward values for all traffic lights since they're all instances of the TrainingTrafficLight prefab.