
# WIP
![FractiX](https://github.com/NabilNYMansour/Unity-FractiX/assets/56453977/c8f524e6-a74d-4488-9940-7acbc1d8097e)

# Intro

**FractiX** is an open source Cone marching/Ray marching rendering engine written as an post-effect to the Unity rendering pipeline and made for the rendering of fractals.

# Table of Contents
1. [Features](#features)
2. [Install](#install)
3. [Documentation](#documentation)
4. [Demos](#demos)

<a id="features"></a>
## Features
### Current features:
- **FractiX** allows for the efficient rendering of any type of SDFs which permits the rendering of fractals.
<img alt="Fractal" src="https://github.com/NabilNYMansour/Unity-FractiX/assets/56453977/053cee97-13d6-4d89-9933-85e13e028a9a" width="75%">

- The engine also allows the functionality to identify collisions between game-objects and the SDF terrain. You can see this below where the selected ball is turned red due to its collision with the wall, and the other smaller ball is blue since it is not colliding. You can find out more in the [Documentation](#documentation) section.
<img alt="Collisions" src="https://github.com/NabilNYMansour/Unity-FractiX/assets/56453977/c0dd3c89-62e2-45d7-9664-d6bda6bdaafd" width="75%">

- However, the engine handles the rendering of standard polygonal geometry as well as SDF geometry. The polygonal geometry will also cast shadows on the SDF geometry and vice versa. You can see that in the picture below with the Stanford Bunny model. This is done with shadow mapping which you can see the texture for in the red plane.
<img alt="Shadows" src="https://github.com/NabilNYMansour/Unity-FractiX/assets/56453977/6196c8d1-70cb-4c9d-bfa0-cd534f79c5b8" width="75%">

### Potential future features:
1. **Physics**: I aim to have a functional way of interacting with the terrain and have game-objects react in a reasonable manner to the SDF.
2. **Individual object rendering**: right now, the SDF that needs to be defined must include all the ray-marched objects (hence it works best if it is being used as terrain); however, it would be more ideal if there was an ability to make a game object and define for it an *SDF renderer* rather than a *mesh renderer*.
3. **Materials for objects**: right now, all objects (including the polygonal objects) will have the same Blinn-Phong material. It would be better if each object could have its own material.
## Install
- To install and run this engine, you must have [The Unity game engine](https://unity.com/) with **editor version 2022.3.2f1** or more (it could work on previous versions, but that is not something that has been tested).
- Afterwards, clone this repository to your device and add the project through the **Unity Hub**.
![image](https://github.com/NabilNYMansour/Unity-FractiX/assets/56453977/5bd23219-2843-4735-a7a3-f3153944eede)

## Documentation
### Shaders
These are shaders that include the necessary logic for all the calculations of the SDFs. The main file that is of interest here is `hit.hlsl`
![hit.hlsl](https://github.com/NabilNYMansour/Unity-FractiX/assets/56453977/78b5decb-4231-4c61-b202-beddb043e16a)

Inside this shader, the main two functions that you would need to modify are `GetDis` and `GetAlbedo`. 
#### Explanation
- `GetDis`: returns the [Signed distance function result](https://en.wikipedia.org/wiki/Signed_distance_function) to the engine. This is used in multiple places within the engine and can be used in your own custom compute shaders to find the distance to the SDF that you have defined.
- `GetAlbedo`: returns the color or albedo of the SDF. This is where you can define how the color of the fractal or SDF changes according to many factors.
#### Example use
The following is an example of using these functions to define **A flat plane with a chess pattern for the color**: 
```glsl
float GetDis(float3 pos) { // given a position in the world
	return pos.y; // return the y distance of the position.
}
float3 GetAlbedo(float3 pos) { // given a position in the world
	// calculate a chessboard pattern based on the distance.
	float chessboard = frac((floor(pos.x) + floor(pos.y) + floor(pos.z)) * 0.5);
	// if on the white part, use one color to color the terrain, otherwise use a different color.
	float3 col = chessboard > 0 ? float3(0.1, 0.5, 0.6) : float3(1, 0.5, 0.1);
	// return the color.
	return col;
}
```
This results in the following SDF:

<img alt="Chess Terrain" src="https://github.com/NabilNYMansour/Unity-FractiX/assets/56453977/df290088-733f-4a8e-beca-778788ae8bb6" width="75%">

**NOTE: You can look at the `Constructions` section in this shader to see some examples of how these functions are used.**

The rest of the shaders are used for rendering and/or doing the logic for the collisions.

### Camera Setup
In order to use the rendering engine, delete the default camera and import the `Cameras and Light` prefab to your scene
![Prefab](https://github.com/NabilNYMansour/Unity-FractiX/assets/56453977/ee4a2b7b-660f-4b78-a102-6ac84628ef7d)

This prefab will hold all the gameo-bjects and scripts that are necessary for the rendering of the SDFs. However, there are some settings that require some attention with the `Main Camera` game object.

![Main Camera](https://github.com/NabilNYMansour/Unity-FractiX/assets/56453977/13b663fa-f686-470f-916f-2b6abc5a6aba)

![Camera settings](https://github.com/NabilNYMansour/Unity-FractiX/assets/56453977/ce4abe42-94e5-42f1-845e-3889add95279)

Most of them seem intuitive, but here are the ones to keep an eye on:

 1. **Visual Settings**: which can enable and disable some of the rendering features.
 2. **Scene number**: this is the scene value that will be used inside the engine to change between different SDFs in case you want to have multiple SDF scenes in your project. You can see how it is being used inside the `hit.hlsl` shader: 
 ```glsl
 float GetDis(float3 pos) {
	switch (_scene) {
	case 0:
		return apollonianHillsSDF(pos);
	case 1:
		return shadowDemoSDF(pos);
	case 2:
		return foldedReefSDF(pos);
	default:
		return collisionsDemoSDF(pos);
	}
} 
```
 3. **Raymarch Params**: These are the parameters that change the color of the sky fog, ground fog, and sun.

You can experiment and toy with the other parameters, but the current values are the ones that provide good visual quality and performance.
### Collision Components and Scripts
For identifying collisions, there are two main scripts: `SDFCollisionsManager` and `SDFSphereCollider`

![Collidor Scripts](https://github.com/NabilNYMansour/Unity-FractiX/assets/56453977/02dc2ffa-8f33-4123-8784-38264a5b35ce) 

#### Explanation
- `SDFCollisionsManager`: Handles the management for all the colliders in the scene. Must be included in any gameo-bject on the scene in order for the collisions to be calculated. The correct scene number is required for it to calculate the collisions with the correct SDF. 

![Manager](https://github.com/NabilNYMansour/Unity-FractiX/assets/56453977/9650c096-58ad-4586-92c7-8eff189dfb05)

- `SDFSphereCollider`: This is the main collider for a game-object. You will only require to define a radius for it.

**Please take a look at the [Collision Demo](#CollisionDemo) scene in order to see how collisions are done in more details.**

**NOTE**: as of now, only a sphere collider is available. I don't necessarily plan to make other colliders but that is something that can be expanded on.

## Demos
The following demos showcase use cases for this engine and how it is being utilized and what are its rendering capabilities.
### FRACTAL GLIDE
- The game that I made initially in tandem with this engine. A demo for it is available on [Steam](https://store.steampowered.com/app/2565200/Fractal_Glide/).
<img alt="FG" src="https://github.com/NabilNYMansour/Unity-FractiX/assets/56453977/edce00eb-b1f2-43a1-afbc-a8793ab6e188" width="75%">

### Boids simulation
- A complete simulation of boids interacting with ***Apollonian Hills fractal***, the first fractal in **FRACTAL GLIDE**
<img alt="Boids" src="https://github.com/NabilNYMansour/Unity-FractiX/assets/56453977/a93b3f64-2441-4722-a83b-a15779563de7" width="75%">

### Collisions demonstration
- A demonstration of how game-objects can interact with the SDF terrain. 
<img alt="Collider SDF" src="https://github.com/NabilNYMansour/Unity-FractiX/assets/56453977/e64a0681-29c2-4953-aa05-6d211299cd9d" width="75%">

### Moving fractal
- A demo of a moving fractal. In `hit.hlsl` and inside the function `foldedReefSDF` you can see how time is being used as a value for the fractal **(Line 66 in the file)**. This can allow animations to be used inside the SDF.
<img alt="Mover" src="https://github.com/NabilNYMansour/Unity-FractiX/assets/56453977/72007245-2fdb-4dbd-b10d-4a3a6fcfa460" width="75%">

### Polygonal shadows demo
- This demo highlights the use of polygonal geometry alongside the SDF and how shadows are being cast between the two geometries.
<img alt="Shadowers" src="https://github.com/NabilNYMansour/Unity-FractiX/assets/56453977/4154e0cb-a608-4b3e-9e52-8790dd7934d4" width="75%">

## Final notes
This project has been the work of almost 3 years of learning how to program with shaders. I hope that you may find it educational and useful as all the engine code is available here. If you wish to support me in any way, you can simply purchuse [FRACTAL GLIDE](https://store.steampowered.com/app/2565200/Fractal_Glide/) and, hopefully, enjoy it.

You can also contribute into this project if you wish to by making pull requests and what not.

If you need further help, you can try to contact me through the various contact links I have on my [website](https://nabilmansour.com/).

Cheers 👍
