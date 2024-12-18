# VOICE Client for Hololens 2

> This repository is part of the [VOICE Operating In Contextual Environments (VOICE)](https://github.com/tsepton/VOICE) project.

## Pre-requisites
App was built using Unity `2022.3.51f`. 
Open the project with Unity, and update the URL to a running [VOICE Backend](https://github.com/tsepton/VOICE-backend): 
1. Open the `Default` scene.
2. Under the `Voice Assistant`>`API` Gameobject, update the `Remote` field with the appropriate URL. 
##### FIXME - this should be asked when app opens 

## Building
First, build the `Default` scene for the Universal Windows Platform using the default parameters.
Then, build a master built and deploy it to a Hololens 2 using Microsoft Visual Studio 2022 or above. 

## Using the Client
State `Hey hololens !` and ask your question. 