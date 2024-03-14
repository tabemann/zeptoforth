# Profiler Words

An optional profiler may be loaded from `extra/common/profile.fs`. Once loaded, any `:` or `:noname` words defined afterwards may be profiled by executing `profile::init-profile` with a provided profiler map size, which initializes a map for storing profiler entries, and then dumped to the console by executing `profile::dump-profile`.

Note that the profile map size is in entries, and each entry is eight bytes in size (take care to not exhaust all the RAM available); also note that this size must be a power of two. If more distinct words compiled after `extra/common/profile.fs` is loaded are executed than the profile map size, an infinite loop will arise. (This is a consequence of avoiding putting a check in the profiler for exhausting the profile map space, in order to optimize its performance.)

### `profile`

The `profile` module contains the following words:

##### `init-profile`
( entries -- )

Initialize the profiler with a profile map with *entries* entries, where *entries* must be a power of two and each entry takes up eight bytes of RAM.

##### `dump-profile`
( -- )

Dump the xt, name, and number of times executed for each `:` or `:noname` word compiled after `extra/common/profile.fs` is loaded and executed after `profile::init-profile` is invoked.

##### `clear-profile`
( -- )

Reset the profiler execution counts to zero.

##### `x-profile-map-entries-not-power-of-two`
( -- )

Profiler map entry count specified is not a power of two exception.

##### `x-profile-already-inited`
( -- )

Profiler already initialized exception.

### `forth`

The `forth` module contains redefinitions of `:` and `:noname` to enable the profiler to function. Note that this does not change prior words defined with these words.
