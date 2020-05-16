# Flash Erasure Words

##### pad-flash-erase-block
( -- )

Pad flash to a erase sector boundary

##### restore-flash
( flash-here -- )

Restore flash to a preexisting state

##### marker
( "name" -- )

Create a MARKER to erase flash/return the flash dictionary to its prior state;
note that the MARKER word that is created is erased when it is executed

##### cornerstone
( "name" -- )

Create a CORNERSTONE to erase flash/return the flash dictionary to its prior
state; note that the CORNERSTONE word that is created is not erased when it is
executed
