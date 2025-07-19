# Hardware Random Number Generator words

Hardware random number generators are initialized at boot time, and can be used as is afterwards. All currently supported platforms support hardware random number generation.

Note that hardware random number generation can be _very_ slow, so it is highly recommended that one use it to seed a pseudorandom number generator such as TinyMT32 rather than directly using it to generate random numbers in sequence.

The following words are in `rng`:

### `rng`

##### `random`
( -- u )

Generate a random 32-bit value using a hardware entropy source.
