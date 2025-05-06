# Interrupt Overloading Words

Interrupt overloading in an optional mechanism to enable multiple handlers to simultaneously share a single interrupt vector. It involves installing an _adapter_ in the interrupt vector which calls the other handlers. Any preexisting interrupt handler is transferred to being the _base handler_ of the adapter, which is always executed last out of all the handlers.

Both handlers and adapters can be uninstalled. Uninstalling a handler just disables that single handler while uninstalling the first adapter installed for a vector disables all handlers except for the base handler for that vector, which is reinstalled directly in the vector if present.

Note that if the user attempts to install more than one adapter on a single vector, only the first adapter is used, and all subsequent adapters are inactive. Inactive adapters have no effect on installing handlers, and uninstalling them is a no-op.

### `int-overload`

The `int-overload` module contains the following words:

##### `x-no-int-adapter`
( -- )

The no interrupt adapter available exception.

##### `int-overload-size`
( -- bytes )

The size of an interrupt handler structure in bytes.

##### `int-adapter-size`
( -- bytes )

The size of an interrupt adapter structure in bytes.

##### `register-int-overload`
( xt vector-index addr -- )

Register an interrupt handler structure *addr* with an interrupt handler execution token *xt* for interrupt vector *vector-index*. If *vector-index* is out of range `interrupt::x-invalid-vector` is raised. If no interrupt adapter is installed for *vector-index* `x-no-int-adapter` is raised.

##### `unregister-int-overload`
( addr -- )

Unregister an interrupt handler structure *addr*. After this point its associated interrupt handler will be removed.

##### `register-int-adapter`
( vector-index addr -- )

Register an interrupt adapter structure *addr* for interrupt vector *vector-index*. If *vector-index* is out of range `interrupt::x-invalid-vector` is raised. If an interrupt adapter is already registered for *vector-index* then the interrupt adapter structure *addr* will be inactive.

##### `unregister-int-adapter`
( addr -- )

Unregister an interrupt adapter structure *addr*. If there are interrupt handlers other than the base handler associated with this structure they are removed. A base handler associated with this structure is restored as the interrupt handler for the interrupt vector associated with this interrupt handler. Note that if this interrupt adapter is inactive this is a no-op.
