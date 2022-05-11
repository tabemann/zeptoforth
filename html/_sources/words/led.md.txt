# LED Words

LEDs are initialized at boot time, and can be used as is afterwards.

### `led`

The following words are in the `led` module:

#### Common LED Words

##### `x-led-out-of-range`
( -- )

LED out of range exception.

##### `on`
( -- )

LED on state.

##### `off`
( -- )

LED off state

##### `led!`
( state led -- )

Set *led* to the on/off state *state*

##### `led@`
( led -- state )

Get the on/off state of *led*.

##### `toggle-led`
( led -- )

Toggle the on/off state of *led*.

#### STM32F407 DISCOVERY LED Words

##### `green`
( -- led )

The green LED.

##### `orange`
( -- led )

The orange LED.

##### `red`
( -- led )

The red LED.

##### `blue`
( -- led )

The blue LED.

#### STM32L476 DISCOVERY LED Words

##### `green`
( -- led )

The green LED.

##### `red`
( -- led )

The red LED.

#### Raspberry Pi Pico LED WOrds

##### `green`
( -- led )

The green LED.
