# Closures

Closures in zeptoforth involve binding an execution token with a single-cell or double-cell value at a specified, arbitrary address in RAM; this address will then serve as an execution token itself which, when executed, pushes the bound value onto the stack and then execute the bound execution token. The only requirement for the address specified is that it is halfword-aligned.

### `closure`

The `closure` module contains the following words:

##### `closure-size`
( -- bytes )

The size of a single-cell closure in bytes.

##### `2closure-size`
( -- bytes )

The size of a double-cell closure in bytes.

##### `nclosure-size`
( count -- bytes )

The size of a multi-cell closure containing *count* values in bytes.

##### `bind`
( x addr xt -- )

Bind the execution token *xt* to single-cell value *x* at address *addr* in RAM, which will serve as a new execution token. `closure-size` bytes must be available at *addr*. When *addr* is executed as an execution token, the single-cell value *x* will be pushed onto the stack and then the execution token *xt* will be executed. *addr* can be arbitrarily reused and can be at any address in RAM.

##### `2bind`
( d addr xt -- )

Bind the execution token *xt* to double-cell data *d* at address *addr* in RAM, which will serve as a new execution token. `2closure-size` bytes must be available at *addr*. When *addr* is executed as an execution token, the double-cell value *x* will be pushed onto the stack and then the execution token *xt* will be executed. *addr* can be arbitrarily reused and can be at any address in RAM.

##### `nbind`
( xn ... x0 count addr xt -- )

Bind the execution token *xt* to *count* multiple values *xn* through *x0* at address *addr* in RAM, which will serve as a new execution token. `nclosure-size` (with *count* specified) bytes must be available at *addr*. When *addr* is executed as an execution token, multiple values *xn* through *x0* will be pushed onto the stack and then the execution token *xt* will be executed. *addr* can be arbitrarily reused and can be at any address in RAM.
