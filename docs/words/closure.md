# Closures

Closures in zeptoforth involve binding an execution token with a single-cell, double-cell, or multi-cell value at a specified, arbitrary address in RAM; this address will then serve as an execution token itself which, when executed, pushes the bound value onto the stack and then execute the bound execution token. The only requirement for the address specified is that it is halfword-aligned.

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

##### `with-closure`
( ? x bound-xt scope-xt -- ? ) scope-xt: ( ? closure-xt -- ? )

Create a closure binding the execution token *bound-xt* to single-cell data *x* in the current task's dictionary, place it on the data stack, and execute *scope-xt*. When the closure is executed, *x* will be pushed onto the data stack and *bound-xt* will be executed. This closure is valid only within the scope of *scope-xt*, and will be de-alloted from the dictionary after it completes executing or if an exception is raised within it, after which it will re-raise said exception.

##### `with-2closure`
( ? d bound-xt scope-xt -- ? ) scope-xt: ( ? closure-xt -- ? )

Create a closure binding the execution token *bound-xt* to double-cell data *d* in the current task's dictionary, place it on the data stack, and execute *scope-xt*. When the closure is executed, *d* will be pushed onto the data stack and *bound-xt* will be executed. This closure is valid only within the scope of *scope-xt*, and will be de-alloted from the dictionary after it completes executing or if an exception is raised within it, after which it will re-raise said exception.

##### `with-nclosure`
( ? xn ... x0 count bound-xt scope-xt -- ? ) scope-xt: ( ? closure-xt -- ? )

Create a closure binding the execution token *bound-xt* to multi-cell data *xn* ... *x0* consisting of *count* cells in the current task's dictionary, place it on the data stack, and execute *scope-xt*. When the closure is executed, *xn* ... *x0* will be pushed onto the data stack and *bound-xt* will be executed. This closure is valid only within the scope of *scope-xt*, and will be de-alloted from the dictionary after it completes executing or if an exception is raised within it, after which it will re-raise said exception.
