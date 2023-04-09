# Closures

Closures in zeptoforth involve binding an execution token with a single-cell, double-cell, or multi-cell value at a specified, arbitrary address in RAM; this address will then serve as an execution token itself which, when executed, pushes the bound value onto the stack and then execute the bound execution token. The only requirement for the address specified is that it is halfword-aligned.

### `closure`

The `closure` module contains the following words:

##### `closure-size`
( -- bytes )

The size of a single-cell closure in bytes.

##### `dclosure-size`
( -- bytes )

The size of a double-cell closure in bytes.

##### `nclosure-size`
( count -- bytes )

The size of a multi-cell closure containing *count* values in bytes.

##### `ndclosure-size`
( count -- bytes )

The size of a multi-double cell closure containing *count* values in bytes.

##### `refclosure-size`
( -- bytes )

The size of a single-cell reference closure in bytes.

##### `drefclosure-size`
( -- bytes )

The size of a double-cell reference closure in bytes.

##### `nrefclosure-size`
( count -- bytes )

The size of a multi-cell reference closure containing *count* values in bytes.

##### `ndrefclosure-size`
( count -- bytes )

The size of a multi-double-cell reference closure containing *count* values in bytes.

##### `bind`
( x addr xt -- )

Bind the execution token *xt* to single-cell value *x* at address *addr* in RAM, which will serve as a new execution token. `closure-size` bytes must be available at *addr*. When *addr* is executed as an execution token, the single-cell value *x* will be pushed onto the stack and then the execution token *xt* will be executed. *addr* can be arbitrarily reused and can be at any address in RAM.

##### `dbind`
( d addr xt -- )

Bind the execution token *xt* to double-cell data *d* at address *addr* in RAM, which will serve as a new execution token. `dclosure-size` bytes must be available at *addr*. When *addr* is executed as an execution token, the double-cell value *d* will be pushed onto the stack and then the execution token *xt* will be executed. *addr* can be arbitrarily reused and can be at any address in RAM.

##### `nbind`
( xn ... x0 count addr xt -- )

Bind the execution token *xt* to *count* multiple values *xn* through *x0* at address *addr* in RAM, which will serve as a new execution token. `nclosure-size` (with *count* specified) bytes must be available at *addr*. When *addr* is executed as an execution token, multiple values *xn* through *x0* will be pushed onto the stack and then the execution token *xt* will be executed. *addr* can be arbitrarily reused and can be at any address in RAM.

##### `ndbind`
( dn ... d0 count addr xt -- )

Bind the execution token *xt* to *count* multiple double-cell values *dn* through *d0* at address *addr* in RAM, which will serve as a new execution token. `nclosure-size` (with *count* specified) bytes must be available at *addr*. When *addr* is executed as an execution token, multiple values *dn* through *d0* will be pushed onto the stack and then the execution token *xt* will be executed. *addr* can be arbitrarily reused and can be at any address in RAM.

##### `refbind`
( x addr xt -- )

Bind the execution token *xt* to single-cell value *x* at address *addr* in RAM, which will serve as a new execution token. `closure-size` bytes must be available at *addr*. When *addr* is executed as an execution token, an address pointing to RAM containing the single-cell value *x* will be pushed onto the stack and then the execution token *xt* will be executed. *addr* can be arbitrarily reused and can be at any address in RAM.

##### `drefbind`
( d addr xt -- )

Bind the execution token *xt* to double-cell data *d* at address *addr* in RAM, which will serve as a new execution token. `dclosure-size` bytes must be available at *addr*. When *addr* is executed as an execution token, an address pointing to RAM containing the double-cell value *d* will be pushed onto the stack and then the execution token *xt* will be executed. *addr* can be arbitrarily reused and can be at any address in RAM.

##### `nrefbind`
( xn ... x0 count addr xt -- )

Bind the execution token *xt* to *count* multiple values *xn* through *x0* at address *addr* in RAM, which will serve as a new execution token. `nclosure-size` (with *count* specified) bytes must be available at *addr*. When *addr* is executed as an execution token, multiple addresses pointing to RAM containing *xn* through *x0* will be pushed onto the stack and then the execution token *xt* will be executed. *addr* can be arbitrarily reused and can be at any address in RAM.

##### `ndrefbind`
( dn ... d0 count addr xt -- )

Bind the execution token *xt* to *count* multiple double-cell values *dn* through *d0* at address *addr* in RAM, which will serve as a new execution token. `nclosure-size` (with *count* specified) bytes must be available at *addr*. When *addr* is executed as an execution token, multiple addresses pointing to RAM containing *dn* through *d0* will be pushed onto the stack and then the execution token *xt* will be executed. *addr* can be arbitrarily reused and can be at any address in RAM.

##### `with-closure`
( ? x bound-xt scope-xt -- ? ) scope-xt: ( ? closure-xt -- ? )

Create a closure binding the execution token *bound-xt* to single-cell data *x* in the current task's dictionary, place it on the data stack, and execute *scope-xt*. When the closure is executed, *x* will be pushed onto the data stack and *bound-xt* will be executed. This closure is valid only within the scope of *scope-xt*, and will be de-alloted from the dictionary after it completes executing or if an exception is raised within it, after which it will re-raise said exception.

##### `with-dclosure`
( ? d bound-xt scope-xt -- ? ) scope-xt: ( ? closure-xt -- ? )

Create a closure binding the execution token *bound-xt* to double-cell data *d* in the current task's dictionary, place it on the data stack, and execute *scope-xt*. When the closure is executed, *d* will be pushed onto the data stack and *bound-xt* will be executed. This closure is valid only within the scope of *scope-xt*, and will be de-alloted from the dictionary after it completes executing or if an exception is raised within it, after which it will re-raise said exception.

##### `with-nclosure`
( ? xn ... x0 count bound-xt scope-xt -- ? ) scope-xt: ( ? closure-xt -- ? )

Create a closure binding the execution token *bound-xt* to multi-cell data *xn* ... *x0* consisting of *count* cells in the current task's dictionary, place it on the data stack, and execute *scope-xt*. When the closure is executed, *xn* ... *x0* will be pushed onto the data stack and *bound-xt* will be executed. This closure is valid only within the scope of *scope-xt*, and will be de-alloted from the dictionary after it completes executing or if an exception is raised within it, after which it will re-raise said exception.

##### `with-ndclosure`
( ? dn ... d0 count bound-xt scope-xt -- ? ) scope-xt: ( ? closure-xt -- ? )

Create a closure binding the execution token *bound-xt* to multi-double-cell data *dn* ... *d0* consisting of *count* cells in the current task's dictionary, place it on the data stack, and execute *scope-xt*. When the closure is executed, *dn* ... *d0* will be pushed onto the data stack and *bound-xt* will be executed. This closure is valid only within the scope of *scope-xt*, and will be de-alloted from the dictionary after it completes executing or if an exception is raised within it, after which it will re-raise said exception.

##### `with-refclosure`
( ? x bound-xt scope-xt -- ? ) scope-xt: ( ? closure-xt -- ? )

Create a closure binding the execution token *bound-xt* to single-cell data *x* in the current task's dictionary, place it on the data stack, and execute *scope-xt*. When the closure is executed, an address pointing to RAM containing *x* will be pushed onto the data stack and *bound-xt* will be executed. This closure is valid only within the scope of *scope-xt*, and will be de-alloted from the dictionary after it completes executing or if an exception is raised within it, after which it will re-raise said exception.

##### `with-drefclosure`
( ? d bound-xt scope-xt -- ? ) scope-xt: ( ? closure-xt -- ? )

Create a closure binding the execution token *bound-xt* to double-cell data *d* in the current task's dictionary, place it on the data stack, and execute *scope-xt*. When the closure is executed, an address pointing to RAM containing *d* will be pushed onto the data stack and *bound-xt* will be executed. This closure is valid only within the scope of *scope-xt*, and will be de-alloted from the dictionary after it completes executing or if an exception is raised within it, after which it will re-raise said exception.

##### `with-nrefclosure`
( ? xn ... x0 count bound-xt scope-xt -- ? ) scope-xt: ( ? closure-xt -- ? )

Create a closure binding the execution token *bound-xt* to multi-cell data *xn* ... *x0* consisting of *count* cells in the current task's dictionary, place it on the data stack, and execute *scope-xt*. When the closure is executed, addresses pointing to RAM containing *xn* ... *x0* will be pushed onto the data stack and *bound-xt* will be executed. This closure is valid only within the scope of *scope-xt*, and will be de-alloted from the dictionary after it completes executing or if an exception is raised within it, after which it will re-raise said exception.

##### `with-ndrefclosure`
( ? dn ... d0 count bound-xt scope-xt -- ? ) scope-xt: ( ? closure-xt -- ? )

Create a closure binding the execution token *bound-xt* to multi-double-cell data *dn* ... *d0* consisting of *count* cells in the current task's dictionary, place it on the data stack, and execute *scope-xt*. When the closure is executed, addresses pointing to RAM containing *dn* ... *d0* will be pushed onto the data stack and *bound-xt* will be executed. This closure is valid only within the scope of *scope-xt*, and will be de-alloted from the dictionary after it completes executing or if an exception is raised within it, after which it will re-raise said exception.
