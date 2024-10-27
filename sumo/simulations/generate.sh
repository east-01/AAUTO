#!/bin/bash

# Generate network
netconvert --node-files=newyork.nod.xml  --edge-files=newyork.edg.xml -o newyork.net.xml

# Generate random trips
$SUMO_HOME/tools/randomTrips.py -n newyork.net.xml -e 3600 -o newyork.trips.xml

# Generate routes
duarouter -n newyork.net.xml --route-files=newyork.trips.xml -o newyork.rou.xml --ignore-errors
