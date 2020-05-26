# Flash Erasure Words

##### `pad-flash-erase-block`
( -- )

Pad flash to a erase sector boundary

##### `restore-flash`
( flash-here -- )

Restore flash to a preexisting state

##### `marker`
( "name" -- )

Create a marker word named *name* to erase flash/return the flash dictionary to its prior state; note that the marker word that is created is erased when it is executed

##### `cornerstone`
( "name" -- )

Create a cornerstone word named *name* to erase flash/return the flash dictionary to its state immediately after `cornerstone` was executed; unlike `marker` the word created does not erase itself when executed and may be executed multiple times