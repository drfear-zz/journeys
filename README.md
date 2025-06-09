# Journeys

Cities:Skylines mod to show full journeys, from beginning to end, across all modes of transport.

The mod will show full planned journeys, from source to final destination, for 
- individual citizens
- all passengers on a public transport
- all citizens who traverse a selected road or rail segment using any form of transport
- all the passengers who use a selected transport line.

You can also show the journeys for all 40,000 or so instantiated citizens in a single view.

The Journeys mod is particularly useful for studying utilization of public transport.

## Mod mechanics
The mod is activated by clicking on the red J box that appears in the top left corner of the screen. (The button can
be moved by right-click-drag.) Once activated, a menu appears.  The menu can be toggled in and out of view by clicking
the J button.  To exit the mod, returning to showing Traffic Route paths instead of Journeys, click the red "exit
from journeys viewer" button at the bottom of the menu.

**Journeys are not updated after the mod is activated, therefore it is highly recommended to pause the game while exploring
the journeys. To update the journeys, you need to exit and re-enter the mod.**

Once the mod is activated you go to into Traffic Routes view (grey map, selectable roads) but with the paths of that
view replaced by Journeys.  You can select an individual citizen, a vehicle, a building or a road segment, to show the
journeys according to the selection.

To select a transport line, use the game icon button to select Transport view and select a line there.  Then go back to
the Traffic Routes view to hide the non-selected lines.  (You can go to any view and the Journeys will stay showing until
you exit the mod.)

Trucks and City Service Vehicles do not show journeys at all, even if selected. (Because inside the game, they have no
associated citizen, therefore no citizen journey.)  Selecting a Car will show the full journey of the driver.


## Colours
When you first enter the mod, journeys are shown in "line colours", meaning that for each section of the journey, the path
is coloured according to the mode of transport.  For example, looking at a single journey, you may see a green path leading
from a residence (green = pedestrian) to a metro, then continuing blue (the colour of the metro line), then green when they
walk from the metro to their destination.  Car journeys are in pink.  Bicycles are pale blue. Planes and intercity trains
show as black (while a local train will show as its line colour).

At the bottom of the white box in the Menu you will see a button "switch to heatmap colours".  Heat - meaning the number of
citizens covering the same journey steps - runs from blue to red.  The heat is reflected both in the colour and the line
width.  The heat associated with each colour is indicated in the Menu, and can be set by the user.  Usually use the buttons
to double or half all the cutoffs at once, but it is possible to simply enter the numbers you want in the text boxes.  The
"min" radio button determines the minimum width of any line.  Usually you will leave this set to 1, but if you are
examining an individual journey, you may like to increase this to make the single journey show more clearly.

The busiest lines in a metropolis will likely have more than 4000 journeys along them.

## The Menu, item by item
At the top of the menu you see the total number of journeys selected, then the number in the subselection after filtering
the main selection according to filters set in the rest of the menu.

### From and To
Selecting "show only from here" reduces the part of each journey shown to be only from the currently selected object. Likewise
for "show only to here" you see journeys from source to selection, only.  Selecting one of the radio button pair deselects
the other.  Selecting an already-selected radio button removes the from/to filter.

### "show only PT stretches and transfers"
This will filter the shown part of journeys to show only travel along public transport, but also including walking from one stop to
another while changing from one transport to another.  The journey from source to the transport is not shown, nor is the journey
from the last used stop to the final destination.  This view is specifically targetted at examining public transport use.

### Residents and Tourists
Another pair of radio buttons, toggle between showing only resident citizen journeys or showing only tourist journeys. Select an
already-selected radio button to switch both off and return to showing both residents and tourists.

### Cycling through lanes and lines
When selecting a road in a large city, there can be a daunting amount of information, because the journeys on all lanes, in all
directions, are all shown.  Use "cycle through lane/lines" to show just one lane at a time (as if you had selected the lane
rather than the whole segment).  If there are two or more lines along a lane (eg two different tram lines along the same tracks)
you will see first one, then the next, before moving on to the next lane.

Be aware, citizens may use more than one lane, and all parts of their journey will show in both lane selections, so
that sometimes there seems to be little difference between successive lane selections.

### Secondary subselection
Another way to restrict showing journeys only if they go via both A and B is to use a "secondary selection".  First click on the
Menu button for this, then make the secondary selection on the map.  This selection will be highlighted in green (whereas the primary
selection is highligheted in red).  You can make a third (or more) secondary selection if you enable the radio button "make a
further secondary selection". There is no theoretical limit.  When there is more than one secondary selection, journeys must go
through all of them to remain visible.

### Extended primary selection
After pressing the "extend the primary selection" button, you can make a further "primary" selection (highlighted in red).  This will
pick out journeys that go through *either* of the primaries.

### Combining primary and secondary selections
You can have more than one of each type.  The rule is: journeys go through *at least one of the red* and *all of the green*.

### Combining primary+secondary selection with From/To
If you have both a red and green selection and press "show only From here" the action will be to restrict to journeys that go *from
red to green* (not the same as if there were only a primary selection).  Likewise "show only To here" will show journeys that go
*from green to red*.

### Blending lines
Sometimes there is more than one line along the same stretch, in which case the lines with less passengers are drawn slightly
narrower than the lines with more (or there is a natural difference in line width due to different heats).  If you click "blend lines
on same lane" you will add the two (or more) lines as if they were all one line.  (Couloured according to the line with the
most passengers.)

### Click through journeys
Successive clicks on this button cycle through each citizen's individual journey in the current (sub)selection.

### Show every known journey
It can be revealing to get a picture of overall public transport use to show all known journeys.  (Do this with restriction to PT stretches
and setting line heat appropriately so you only get red for the very busiest lines.) It can take a second or two to render all
these journeys.



