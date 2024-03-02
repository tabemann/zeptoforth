# Dynamically-Scoped Variable Words

Dynamically-scoped variables enable the definition of variables with a scope within a called execution token or within all execution inside a given thread except for within scopes in which they are redefined. Note that values of dynamically-scoped variables are specific to individual tasks, and do not exist for the main task on bootup or within new tasks on spawning.

### `dynamic`

The `dynamic` module contains the following words:

##### `dyn`
( "name" -- )

This defines a dynamically-scoped single-cell variable with the specified *name*. Note that this does not give it any value. If this is executed during compilation to flash the defined variable will persist across reboots.

##### `2dyn`
( "name" -- )

This defines a dynamically-scoped double-cell variable with the specified *name*. Note that this does not give it any value. If this is executed during compilation to flash the defined variable will persist across reboots.

##### `dyn!`
( xt x|xd variable -- )

This sets a dynamically-scoped variable *variable* to *x* or *xd* depending on whether it is single-cell or double-cell within the scope defined by the execution token *xt* within the current task. This temporarily uses space in the current task's RAM dictionary to store the defined value of *variable*.

##### `dyn-no-scope!`
( x|xd variable -- )

This sets a dynamically-scoped variable *variable* to *xt* or *xd* depending on whether it is single-cell or double-cell within the scope defined by all execution after this point within the current task until overridden by another execution of `dyn!` or `dyn-no-scope!`. Note that this permanently uses space in the current task's RAM dictionary to store the defined value of *variable*; do not use this within implicit compilation.

##### `dyn@`
( variable -- x|xd )

This gets the current value of a dynamically-scoped variable *variable* within the current task as *x* or *xd* depending on whether it is single-cell or double-cell. If it has not been set in the current scope in the current task, `x-dyn-variable-not-set` is raised.

##### `x-dyn-variable-not-set`
( -- )

Dynamic variable not set exception.
