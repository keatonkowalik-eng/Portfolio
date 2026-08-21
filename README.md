# Portfolio

I’m a software engineer and game developer focused on building interactive software and solving complex technical problems. I primarily work with C# and Unity, with additional experience in Python, Java and C++.

LinkedIn: https://www.linkedin.com/in/keaton-kowalik-5108382a8/details/skills/



## **Shapes: The Tower Defense Game**

Shapes is a 2D top down tower defense shooter where your goal is to get to the highest wave in the game earning money and levels to progress. Strategize your base to build and defend off waves of enemies in an on going battle.


I have built this game entirely from scratch with artwork, music and functionality. In /Projects/Shapes The Tower Defense Game/ I have some example code that I used for this project. Below are some example code with explanations that I worked on:



Entity.cs



Is a abstract parent class that the multiple enemies in my game derive from. This class helps initialize the base functions for each enemy such as locating targets or moving the enemy navigation agent.

    protected virtual void Move()

&#x20;   {

&#x20;       if (target == null || moveTimer.Update()) { return; } // Check Target

&#x20;       if (agent.hasPath \&\& agent.remainingDistance >= (entitySO.behaviour.stopRange / 2)) { return; }



&#x20;       if (Vector3.Distance(target.position, transform.position) > entitySO.behaviour.stopRange)

&#x20;       {

&#x20;           Vector3 position = ((target.position - transform.position).normalized \* entitySO.behaviour.stopRange) + transform.position;

&#x20;           agent.SetDestination(position);

&#x20;       }

&#x20;       else

&#x20;       {

&#x20;           agent.SetDestination(target.position);

&#x20;       }

&#x20;   }

Above is the move function for each enemy. This allows the enemy to simply move when no obstacles are in its way. I first check the targets validity to ensure it is not null while also keeping the code from running every frame with a built in timer class that I created myself. I also only preform the move direction within a specified distance to give the enemies better performance and faster reaction times. BasicEnemy.cs is an example of a child class that I used in my game and EntitySO.cs is the scriptable objects that I use to give the enemies there initial attributes.

IAgent.cs is an Interface that is used in all my agents. This ensures that I use all the same abilities for each agent such as providing a traits controller which controls all the traits in my game.

TraitsController is a normal class that is copied by agents to universally utilize the same functionalities for things like health, healing or leveling. It also localizes these functionality in one place in order to keep everything consistent.


Steam Page: https://store.steampowered.com/app/4923140/Shapes\_The\_Tower\_Defense\_Game/



Here is an itch.io page where I have worked on other games in the past too
Itch.io: https://coffeecode1.itch.io/

