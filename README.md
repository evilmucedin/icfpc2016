﻿# ICFPC 2016. Team kontur.ru (ID: 89)

## Members
* Alexandr Borozdin — infrastructure, manual solver, solver
* Ivan Dashkevich — convex solver, infrastructure, birthday!!!
* Ivan Domashnikh — solver
* Alexey Dubrovin — infrastructure, solver
* Pavel Egorov — visualizer, manual solver, problems, this readme
* Mikhail Khruschev — constructor solver
* Alexey Kirpichnikov — help everybody
* Alexandr Kokovin — problems
* Andrey Kostousov — convex solver, infrastructure
* Grigoriy Nazarov — help everybody
* Yuriy Okulovskiy — solver
* Dmitriy Titarenko — problems, infrastructure


## Interesting classes in the source code

Solvers:

* lib/ConvexPolygonSolver.cs — for convex problems
* lib/SolutionSpecExt.cs — implementation of the solution folding.
* lib/ProjectionSolver/UltraSolver.cs — for simple not convex problems (try to compose initial square perimeter with rational segments from problem and then restore other points)
* lib/Constructor/ConstructorSolver.cs — for other simple not convex problems (try to construct initial square from polygons of given siluete)
* lib/ManualSolverForm.cs — UI for manual problems solving (you can do several reflections and then apply convex solver)
* lib/VisualizerForm.cs — visualizer of all problems with ability to open ManualSolverForm
* lib/SolutionPacker.cs — optimizer of the solution size.

Problems:

* lib/D4Problem.cs — generator of problems not solvable in 3 dimensions.
* lib/Problems.cs — generator of some other simple problems.

Infrastructure:

* TimeManager — throttler for api calls
* AutoSolver — daemon, download new problems and run solvers.
* problems/ — directory with problems, solutions and responses of solution/submit api call

## Manual solver?!?

Yes, we gaina lot of points via semi automatic solver. Human do several folds to make it convex, and then convex solver do the rest.

<a href="http://www.youtube.com/watch?feature=player_embedded&v=6_2JAzxuTNM
" target="_blank"><img src="http://img.youtube.com/vi/6_2JAzxuTNM/0.jpg" 
alt="IMAGE ALT TEXT HERE" width="240" height="180" border="10" /></a>
