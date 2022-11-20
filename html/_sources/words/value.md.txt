# Value and Local Variable Words

Full builds of zeptoforth include support for `value` and `2value` along with local variables. Unlike `variable` and `2variable`, `value` and `2value` variables are initialized, and retain their initialization across reboots. `value`s, `2value`s, and local variables are all set with `to` or added to with `+to`, which are state-smart words that can be used both in interpretation and in compilation mode.

Note that local variables are lexically scoped, and are safe to use within `if` ... `then`, `if` ... `else` ... `then`, `begin` ... `until`, `begin` ... `while` ... `repeat`, `begin` ... `again`, `case` ... `endcase`/`endcasestr`, `of`/`ofstr`/`ofstrcase` ... `endof`,  and `do`/`?do` ... `loop`/`+loop` constructs. Local variables declared between these words are not accessible outside them, and do not retain their values between multiple iterations.

All of these words are in the default `forth` module.

### Values

##### `value`
( x "name" -- )

This compiles a word *name* for a single-cell value which is initialized to *x*, immediately when compiling to RAM and upon boot when compiling to flash. *name* evaluates to its current value when executed. `to` may be used to change the value of *name* at runtime, but this value reverts to the initialization value upon bootup for values compiled to flash.

##### `2value`
( d "name" -- )

This compiles a word *name* for a double-cell value which is initialized to *d*, immediately when compiling to RAM and upon boot when compiling to flash. *name* evaluates to its current value when executed. `to` may be used to change the value of *name* at runtime, but this value reverts to its initialization value upon bootup for values compiled to flash.

### Local Variables

##### `{`
( "xn" ... "x0" [ "--" ... ] "}" -- )

This compiles a set of single-cell local variables within a word that at runtime take their values off the top of the stack, with *x0* (the last local variable declared) taking its value off the very top of the stack. Local variable declarations can extend across multiple lines. `}` declares the end of a local variable declaration. `--` within a local variable declaration declares a comment that extends, across multiple lines, until `}` is reached. After this point each of these local variables evaluate to their set values until the function is completed. *to* may be used to change the values of these local variables after they are declared. Local variables may be used within quotations declared with `[:` and `;]`, including if local variables are used within the containing word, and are properly discarded when `exit` is used within a word. However, they cannot be used within `do` loops or while other words that access the return stack are in use, because local variables are stored on the return stack.

### Setting Values and Local Variables

##### `to`
( x|d "name" -- )

This sets a value or local variable *name* to either a single-cell or double-cell on the stack depending on whether *name* refers to a `value`, `2value`, or local variable. This is state smart, and can be used both in interpretation mode and in compilation mode.

##### `+to`
( x|d "name" -- )

This adds a single-cell or double-cell on the stack to a value or local variable *name* depending on whether *name* refers to a `value`, `2value`, or local variable. This is state smart, and can be used both in interpretation mode and in compilation mode.
