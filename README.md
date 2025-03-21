# VOICE Client for Hololens 2

> This repository is part of the [VOICE Operating In Contextual Environments (VOICE)](https://github.com/tsepton/VOICE) project.


## Building
> App was tested and built using Unity `2022.3.51f`. 

First, build the `Default` scene for the Universal Windows Platform using the default parameters.
Then, build a master built and deploy it to a Hololens 2 using Microsoft Visual Studio 2022 or above. 

## Using the Client
Enter the VOICE backend url and connect your client. 
State `Hey VOICE!` and ask your question. If you need help with the assistant, try looking at your palm.

## Known bug 
The MRTK3 Dictation service occasionally crashes without any warning.
If the hand menu remains stuck in a loading state for an unusually long time while processing your question, restart the app by closing and reopening it.