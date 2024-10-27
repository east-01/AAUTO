# SUMO
This documentation assumes that you have:
- Installed [SUMO](https://sumo.dlr.de/docs/Installing/index.html) 
    - Including the GUI and [python packages](https://sumo.dlr.de/docs/Installing/index.html#additional_tools)
- Recommend Linux/MacOS/Windows Subsystem for Linux


## Network
Nodes are junctions or intersections and are measured in meters using the x and y coordinates.
The x coordinates maps to Unity's x coordinate, while y maps to Unity's z coordinate.
Stored in the file `newyork.nod.xml`.
This is manually generated.

Edges are roads, with number of lanes and speed limit in meters per second `m/s`.
Stored in the file `newyork.edg.xml`
This is manually generated.

Network file combines the previous two files via the command:
```bash
netconvert --node-files=newyork.nod.xml  --edge-files=newyork.edg.xml -o newyork.net.xml
```
Stored in the file `newyork.net.xml`

## Trips
Use randomtrips.py to generate trips along the edges where:
- `-n` is the network file
- `-e` is the end time in second
- `-o` is the output file
```bash
$SUMO_HOME/tools/randomTrips.py -n newyork.net.xml -e 3600 -o newyork.trips.xml
```

## Routes
Use `duarouter` to generate routes file, ignoring errors as not all the random trips will be valid:
```bash
duarouter -n newyork.net.xml --route-files=newyork.trips.xml -o newyork.rou.xml --ignore-errors
```

## SUMO Config
`newyork.sumo.cfg` XML config file for configuring the simulation.
This file is manually generated.
Use `sumo-gui` to launch the simulation in SUMO.

## Generate Shell Script
I got tired of re-running all the commands each time that I tested a change to one of the XML files, so I wrote a short script to re-generate the network, trips and routes.
The SUMO config file does not need to be updated, assuming file names do not change.

Assumes running on linux with bash shell, in the sumo/simulations/ directory:
```bash
./generate.sh
```
