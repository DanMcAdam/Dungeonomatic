# Dungeonomatic
A procedural tile-based dungeon generator for Unity Developers 

# Current Status
Status update 1/15/2023: This project is on a temporary hiatus, I'm currently working on a roguelight game that will need a dungeon generator and the current plan is to return to this project when the time comes to start working on level generation. That project can be seen here, and a webGL build is available in the Readme https://github.com/DanMcAdam/Project-Reborn-Rain

I just finished a major refactor on the project, switching the code that runs the generator itself to Unitask to replace the coroutine solution that was required before. Unitask is an async/await plugin that's compatible with WebGL builds (unlike traditional C# async/await). This provides several advantages over coroutines, including performance, but primarily it allowed me to write cleaner code. I also updated the project to Unity 2021.1.3.11f1, which is the current stable branch, in order to allow use of the UI builder. Lastly, I figured out how to make the Unity experimental Navmesh components execute from the dungeon generator rather than having them prebake in the prefabs. This allows me to bypass the NavMesh Links, which were finicky to set up caused some annoying bugs with movement (that could be bypassed with some custom movement scripting). This is a much more user friendly and reliable workflow. 

# ROADMAP
Right now, Dungeonomatic is prettty barebones. It generates a linear dungeon, without a set end, and that's just not very interesting. I've been debating the best way to allow users to interact with the program, and I've decided on a nodemap interface. That's currently in development and making good progress, I expect that to be finished by early November 2022. This release will mark Dungeonomatic becoming a true dungeon generator, with side paths and programmable path patterns. 

The following release will likely contain a UI update in general. This will likely take several iterative updates to smooth out, bug test and get feedback on. The goal of these updates will be to unify the use of the program and make it friendly and easy to understand the workflow. This tool will require users to make their own prefabs in a format that's compatible with the tool. I want to streamline that as much as possible and keep users aware of each requirement. An early version of this can be seen in SetupRoom script in "Assets/Dungeonomatic Core Scripts". It tests each individual requirement of the program (as much as is possible), sets up the requirements itself where possible, and then provides feedback to the user if it's unsuccessful. Right now it's required to place the script on the root gameobject of the prefab, where it will execute and then remove itself. This is hardly intuitive, and I'd like to be able to make the script entirely UI based with tooltips and add some customization for the user (like being able to choose the layer for generation collision detection from a dropdown, for example)

# Dependencies
Currently there are 2 major dependencies in the project. First is the AI Navigation NavMesh Components experimental package. This dependency will be optional in the final release, as I don't want to force users to have to go through the (slightly cumbersome) process of installing the package if they're not planning to use it. It is also experimental, and since I can't guarantee that its implementation/availability won't change in the future, I don't want to rely on it exclusively. Many users will want to use their own pathfinding solutions. 

https://docs.unity3d.com/Packages/com.unity.ai.navigation@1.0/manual/index.html

The second dependancy is Unitask. It's a lightweight plugin that allows for a more reliable Async/Await pattern in Unity without breaking WebGL builds. This will be packaged with the final program. Thankfully it is under the MIT License which allows for redistribution and commercial use, and the team has done a phenomenal job making this useful plugin. 

https://github.com/Cysharp/UniTask
