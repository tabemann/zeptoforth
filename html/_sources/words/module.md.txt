# Module Words

Modules in zeptoforth are built on top of wordlists, and form a user-friendly means of controlling word namespaces instead of manually manipulating wordlists. Modules and wordlists are one and the same, but by convention they are named with `*-module`.

At any given time once `src/forth/common/module.fs` is loaded there is a module stack which controls how module words manipulate the wordlist order. Initially there is one entry on the module stack, corresponding to the `forth-module` wordlist. When new entries are pushed onto the module stack, they save the state of `base` prior to their creation, and restore it once they are popped. Also, module stack entries specify the current wordlist for them, and when module stack entries immediately above them on the stack are popped, their wordlist is restored as the current wordlist.

When modules are defined, they automatically add their wordlist definition as a constant to the containing module. Also, if so specified by using `begin-import-module` or `begin-import-module-once`, the modules may be imported in the containing module. Note that modules may be defined multiple times, each time adding onto the existing definition, unless the module is defined with `begin-module-once` or `begin-import-module-once`, where then it is checked such that the module already exists by the name specified, and if it does exist, `x-module-already-defined` is raised.

Within a given module, the user may import and unimport modules/wordlists, which pushes them on the wordlist order and removes them from that module's portion of the wordlist's order respectively. Note that all the wordlists imported with a module definition are automatically unimported when that module definition is ended. Note that imported and unimported modules must already exist by the name specified, or else `x-module-not-found` is raised.

Note that it is recommended that once `src/common/forth/module.fs` is loaded, the user should not manually use `set-order` or `set-current`, as the module system will not know about this and thus unexpected results may occur.

The following words are defined in `forth-module`:

##### `x-module-already-defined`
( -- )

Module already defined exception.

##### `x-module-not-found`
( -- )

Module not found exception.

##### `^`
( ? "module-name-0" ... "module-name-x" "::" "word-name" -- ? )

Reference word *word-name* in a specified module *module-name-x*, which may be nested within any number of containing modules, where the first module referenced *module-name-0* must be within the current order, and apply it to the current compilation/interpretation state. `::` separates the innermost module *module-name-x*, from *word-name*. This is an immediate word, and the referenced word will be folded or inlined as if it were compiled normally.

##### `begin-module`
( "name" -- )

Begin the definition of module *name* without importing its contents into the containing module or checking whether the module already exists.

##### `begin-module-once`
( "name" -- )

Begin the definition of module *name* without importing its contents into the containing module, but with checking whether the module already exists.

##### `begin-import-module`
( "name" -- )

Begin the definition of module *name* with importing its contents into the containing module, but without checking whether the module already exists.

##### `begin-import-module-once`
( "name" -- )

Begin the definition of module *name* with importing its contents into the containing module and with checking whether the module already exists.

##### `end-module`
( -- )

End the definition of the module at the top of the module stack, removing each wordlist for each module imported within it from the wordlist order.

##### `import`
( "name" -- )

Import a module by the specified name into the current module's wordlist order

##### `unimport`
( "name" -- )

Remove a module by the specified name from the current module's wordlist order; note that it does not remove it from parent modules' wordlist orders, so if it  had been imported within them they are still searchable.
