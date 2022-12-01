# Module Words

Modules in zeptoforth are built on top of wordlists, and form a user-friendly means of controlling word namespaces instead of manually manipulating wordlists. Modules and wordlists are one and the same, but by convention they are named with `*`.

At any given time once `src/forth/common/module.fs` is loaded there is a module stack which controls how module words manipulate the wordlist order. Initially there is one entry on the module stack, corresponding to the `forth` wordlist. When new entries are pushed onto the module stack, they save the state of `base` prior to their creation, and restore it once they are popped. Also, module stack entries specify the current wordlist for them, and when module stack entries immediately above them on the stack are popped, their wordlist is restored as the current wordlist.

When modules are defined, they automatically add their wordlist definition as a constant to the containing module. Also, if so specified by using `begin-import` or `begin-import-once`, the modules may be imported in the containing module. Note that modules may be defined multiple times, each time adding onto the existing definition, unless the module is defined with `begin-module-once` or `begin-import-once`, where then it is checked such that the module already exists by the name specified, and if it does exist, `x-already-defined` is raised.

Within a given module, the user may import and unimport modules/wordlists, which pushes them on the wordlist order and removes them from that module's portion of the wordlist's order respectively. Note that all the wordlists imported with a module definition are automatically unimported when that module definition is ended.

Words inside modules or inside nested modules may be used without importing the modules in question with *paths* specified with *module*`::`*word* or, mor generally, *module0*`::`...`::`*modulen*`::`*word*. These paths can be used not simply by the outer interpreter but also by any word which looks up another word by name, such as `'`, `[']`, `postpone`, `averts`, and `triggers`.

Note that it is recommended that once `src/common/forth/module.fs` is loaded, the user should not manually use `set-order` or `set-current`, as the module system will not know about this and thus unexpected results may occur.

### `forth`

The following words are defined in `forth`:

##### `x-already-defined`
( -- )

Module already defined exception.

##### `x-not-found`
( -- )

Module not found exception.

##### `begin-module`
( "name" -- )

Begin the definition of module *name*; module *name* must not already exist or `x-already-defined` will be raised.

##### `continue-module`
( "name" -- )

Continue the definition of a preexisting module *name*; if module *name* does not exist `x-not-found` will be raised.

##### `private-module`
( -- )

Begin the definition of a private module, i.e a module not bound to a word.

##### `end-module`
( -- )

End the definition of the module at the top of the module stack, removing each wordlist for each module imported within it from the wordlist order.

##### `end-module>`
( --  module )

End the definition of the module at the top of the module stack, removing each wordlist for each module imported within it from th e wordlist order, and then push the module whose definition had ended onto the data stack.

##### `import`
( module -- )

Import a specified module into the current module's wordlist order; if the module does not exist `x-not-found` is raised.

##### `unimport`
( module -- )

Remove a specified module from the current module's wordlist order; note that it does not remove it from parent modules' wordlist orders, so if it  had been imported within them they are still searchable.

##### `export`
( xt "word-name" -- )

Export *xt* from the module currently being defined as *word-name*.

