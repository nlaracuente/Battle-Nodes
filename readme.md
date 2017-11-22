# Battle Nodes
---
An online PvP Tank Battles usings web sockets. Though the models, sounds, and effects all come from the Unity/Tanks! tutorial, the script is original and does not follow anything from the tutorial. 

This was entered in the 2017 node knockout and placed 10th. 
- https://www.nodeknockout.com/entries/68-stupid-over-engineered-selectors

## Requirements

- UnitySocketsIO: which is a paid library found on the Unity assest store and allows connections to socket from WebGL

## Getting Started

Clone Repo

````
git clone https://github.com/nlaracuente/battle-nodes/
````

Purchase and download the UnitySocketIO dependency
- https://www.assetstore.unity3d.com/en/#!/content/76508

````

Finally, you will need to build the socket server to listen to the events and because this was a joint effort I did not include that in this code base as I did not work on it. This Unity project contains all of the work I personally did for the hackathon and some work done by another teammate to create the messaging system to communicate between the web sockets and unity.
