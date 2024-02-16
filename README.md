# WIP
![FractiX](https://github.com/NabilNYMansour/Unity-FractiX/assets/56453977/c8f524e6-a74d-4488-9940-7acbc1d8097e)

# Intro

**FractiX** is a Cone marching/Ray marching rendering engine written as an post-effect to the Unity rendering pipeline and made for the rendering of fractals.

# Table of Contents
1. [Features](#features)
2. [Install](#install)
3. [Documentation](#documentation)
4. [Demos](#demos)


## Features
- **FractiX** allows for the efficient rendering of any type of SDFs which permits the rendering of fractals.
<img alt="Fractal" src="https://github.com/NabilNYMansour/Unity-FractiX/assets/56453977/053cee97-13d6-4d89-9933-85e13e028a9a" width="75%">

- The engine also allows the functionality to identify collisions between gameobjects and the SDF terrain. You can see this below where the selected ball is turned red due to its collision with the wall, and the other smaller ball is blue since it is not colliding. You can find out more in the [Documentation](#documentation) section.
<img alt="Collisions" src="https://github.com/NabilNYMansour/Unity-FractiX/assets/56453977/c0dd3c89-62e2-45d7-9664-d6bda6bdaafd" width="75%">

- However, the engine handles the rendering of standard polygonal geometry as well SDF geometry. The polygonal geometry will also cast shadows on the SDF geometry and vice versa. You can see that in the picture below with the Stanford bunny model. This is done with shadow mapping which you can see the texture for in the red plane.
<img alt="Shadows" src="https://github.com/NabilNYMansour/Unity-FractiX/assets/56453977/6196c8d1-70cb-4c9d-bfa0-cd534f79c5b8" width="75%">

### Potential future features:
1. **Physics**: I aim to have a functional way of interacting with the terrain and have gameobjects react in a reasoable manner to the SDF.
2. **Individual object rendering**: right now, the SDF that needs to be defined must include all the raymarched objetcts (hence it works best if it is being used as terrain); however, it would be more idea if there was an ability to make a game object and define for it an *SDF renderer* rather than a *mesh renderer*.
3. **Materials for objects**: right now, all objects (including the polygonal objects) will have the same Blinn-Phong material. It would be better if each object can have its own material.
## Install
- To install and run this engine, you must have [The Unity game engine](https://unity.com/) with **editor version 2022.3.2f1** or more (it could work on previous versions, but that is not something that has been tested).
- Afterwards, clone this repository to your device and add the project through the **Unity Hub**.
![image](https://github.com/NabilNYMansour/Unity-FractiX/assets/56453977/5bd23219-2843-4735-a7a3-f3153944eede)

## Documentation
## Demos
