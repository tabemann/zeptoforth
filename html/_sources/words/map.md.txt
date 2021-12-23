# Map Words

Maps in zeptoforth are implemented as fixed-sized hash tables. They have fixed-sized keys and values; it is recommended that these be either addresses or indices into some other data structure, for sake of economy of space. For the sake of alignment, the amount of space taken up by a key and a value together is internally rounded up to the nearest cell.

### `map-module`

The following words are in `map-module`:

##### `map-size`
( count key-size value-size -- map-bytes )

Get the size in bytes of a map with an entry count *count*, a key size *size* in bytes, and a value size *value* in bytes.

##### `init-map`
( hash-xt equals-xt remove-xt count key-size value-size addr -- )

Initialize a map at *addr* with a key size *key-size* in bytes, a value size *value-size* in bytes, an entry count *count*, an entry removal handler *remove-xt* with the signature ( value-addr key-addr -- ) which can be 0, an equals function *equals-xt* with the signature ( key-addr key-addr -- equals? ), and a hash function *hash-xt* with the signature ( key-addr -- hash ).

##### `find-map`
( key-addr map -- value-addr | 0 )

Find a the value of a key at *key-addr* in a map *map* and return the address of its value *value-addr*, or if the key is not found, return 0.

##### `insert-map`
( value-addr key-addr map -- )

Insert a value at *value-addr* at the key at *key-addr* in a map *map*. If the key is already present in the map, call the entry removal handler for the key and value already in the map (if set) and then replace the key and value in the map with those provided. If the key is not already present in the map and there is room for another entry in the map, add the value to the map at the specified key, otherwise raise `x-map-full`.

##### `remove-map`
( key-addr map -- )

Remove a value with a key at *key-addr* from a map *map* if it is present; if this is the case, first call the entry removal handler for the key and value already in the map (if set).

##### `first-map`
( map -- index | -1 )

Get the index *index* of the first entry in a map *map* or return -1 if the map is empty.

##### `next-map`
( index map -- index' | -1 )

Get the next index *index'* of an entry in a map *map* after the entry at the index *index*; if there is no following entry, return -1.

##### `at-map`
( index map -- value-addr key-addr )

Get the value and key at *value-addr* and *key-addr* of the entry in map *map* at index *index*; if *index* is out of range, raise `x-map-index-out-of-range`, and if *index* does not correspond to a valid entry, raise `x-map-index-no-entry`.

##### `clear-map`
( map -- )

Clear map *map* of all entries, calling the entry removal handler for each cleared entry if set.

##### `copy-map`
( src-map dest-map -- )

Copy each entry in the source map *src-map* into the destination map *dest-map*, overwriting existing entries with identical keys (and calling the entry removal handler for them, if set for the destination map). If insufficient room is available to copy further entries into the destination map, `x-map-full` is raised. If the key size, value size, hash function, or equals function differ between the two maps, `x-dest-map-not-match` is raised.

##### `map-entry-count`
( map -- entry-count )

Get the total entry count of a map *map*.

##### `map-used-entry-count`
( map -- used-entry-count )

Get the used entry count of a map *map*.

##### `map-key-size`
( map -- key-size )

Get the key size in bytes of a map *map*.

##### `map-value-size`
( map -- value-size )

Get the value size in bytes of a map *map*.

##### `map-hash-xt`
( map -- hash-xt )

Get the hash function of a map *map*.

##### `map-equals-xt`
( map -- equals-xt )

Get the equals function of a map *map*.

##### `map-remove-xt` )
( map -- remove-xt )

Get the entry removal handler of a map *map*.

##### `x-map-full`
( -- )

Map is full exception.

##### `x-map-index-out-of-range`
( -- )

Map index is out of range exception.

##### `x-map-index-no-entry`
( -- )

Map index has no entry exception.

##### `x-dest-map-not-match`
( -- )

Destination map does not match source map with regard to key size, value size, hash function, or equals function exception.

## Counted String Maps

Counted string maps are a subset of maps that have keys that are implemented as counted strings allocated (aside from an address) outside of the map data structure.

### `cstr-map-module`

The following words are in `cstr-map-module`:

##### `cstr-map-size`
( count value-size -- map-bytes )

Get the size in bytes of a counted string map with an entry count *count* and a value size *value* in bytes.

##### `init-cstr-map`
( hash-xt equals-xt remove-xt count key-size value-size addr -- )

Initialize a counted string map at *addr* with a value size *value-size* in bytes, an entry count *count*, and an entry removal handler *remove-xt* with the signature ( value-addr key-addr -- ) which can be 0.

##### `find-cstr-map`
( key-cstr cstr-map -- value-addr | 0 )

Find a the value of a counted string key *key-cstr* in a counted string map *cstr-map* and return the address of its value *value-addr*, or if the key is not found, return 0.

##### `insert-cstr-map`
( value-addr key-cstr cstr-map -- )

Insert a value at *value-addr* at the counted string key *key-cstr* in a counted string map *cstr-map*. If the key is already present in the map, call the entry removal handler for the key and value already in the map (if set) and then replace the key and value in the map with those provided. If the key is not already present in the map and there is room for another entry in the map, add the value to the map at the specified key, otherwise raise `x-map-full`.

##### `remove-cstr-map`
( key-cstr cstr-map -- )

Remove a value with a counted string key *key-cstr* from a counted string map *cstr-map* if it is present; if this is the case, first call the entry removal handler for the key and value already in the map (if set).

##### `at-cstr-map`
( index cstr-map -- value-addr key-cstr )

Get the value at *value-addr* and counted string key *key-cstr* of the entry in a counted string map *cstr-map* at index *index*; if *index* is out of range, raise `x-map-index-out-of-range`, and if *index* does not correspond to a valid entry, raise `x-map-index-no-entry`.

## Integer Maps

Integer maps are a subset of maps that have keys that are cell-sized integers.

### `int-map-module`

The following words are in `int-map-module`:

##### `int-map-size`
( count value-size -- map-bytes )

Get the size in bytes of a integer map with an entry count *count* and a value size *value* in bytes.

##### `init-int-map`
( hash-xt equals-xt remove-xt count key-size value-size addr -- )

Initialize a integer map at *addr* with a value size *value-size* in bytes, an entry count *count*, and an entry removal handler *remove-xt* with the signature ( value-addr key-addr -- ) which can be 0.

##### `find-int-map`
( key-int int-map -- value-addr | 0 )

Find a the value of a integer key *key-int* in a map *int-map* and return the address of its value *value-addr*, or if the key is not found, return 0.

##### `insert-int-map`
( value-addr key-int int-map -- )

Insert a value at *value-addr* at the integer key *key-int* in an integer map *int-map*. If the key is already present in the map, call the entry removal handler for the key and value already in the map (if set) and then replace the key and value in the map with those provided. If the key is not already present in the map and there is room for another entry in the map, add the value to the map at the specified key, otherwise raise `x-map-full`.

##### `remove-int-map`
( key-int int-map -- )

Remove a value with a integer key *key-int* from an integer map *int-map* if it is present; if this is the case, first call the entry removal handler for the key and value already in the map (if set).

##### `at-int-map`
( index int-map -- value-addr key-int )

Get the value at *value-addr* and integer key *key-int* of the entry in an integer map *int-map* at index *index*; if *index* is out of range, raise `x-map-index-out-of-range`, and if *index* does not correspond to a valid entry, raise `x-map-index-no-entry`.
