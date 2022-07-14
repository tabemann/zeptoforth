# Object Orientation

zeptoforth includes an object-orientation layer implemented by the `oo` module. Its object model involves each class having a single superclass (with the ultimatet superclass, `object`, being its own superclass), and methods and members being independent words which are tied to the class for which they are originally declared which may be applied to objects of that class or of any subclass of that class.

Binding is late by default, but if one explicitly specifies a class to use early binding is also available, and is necessary in some use cases such as `new` or `destroy` method implementations calling `new` or `destroy` respectively in their super classes.

Outside of early binding, classes provide no sort of namespace mechanism; for that one must rely on modules or naming conventions.

Declaring the methods and members associated with a class and all its subclasses is separate from providing implementations for classes' methods, which may be inherited by subclasses. Methods and members of classes are declared between `begin-class` and `end-class` with `method` and `member` respectively, while method implementations are declared between `begin-implement` and `end-implement` with `define`. Note that no code may call any methods on a class until after `end-implement` is called matching the call to `begin-implement` for that class, but calls to methods on that class can be compiled before that.

For a simple example of the definition of a class and a subclass of it, take the following example:

    oo import
    
    object begin-class my-class
      cell member member-0
      method member-0@
      method display-msg
    end-class
    
    my-class begin-implement
      :noname
        dup [ object ] -> new
        1 swap member-0 !
      ; define new
      :noname
        ." Destroying my-class "
        [ object ] -> destroy
      ; define destroy
      :noname member-0 @ ; define member-0@
      :noname drop ." foo" ; define display-msg
    end-implement
    
    my-class begin-class my-subclass
      cell member member-1
      method member-1@
    end-class

    my-subclass begin-implement
      :noname
        dup [ my-class ] -> new
        -1 swap member-1 !
      ; define new
      :noname
        ." Destroying my-subclass "
        [ my-class ] -> destroy
      ; define destroy
      :noname drop ." bar" ; define display-msg
      :noname member-1 @ ; define member-1@
    end-implement

Here we define two classes, `my-class` and `my-subclass`, which inherit from `object` and `my-class` respectively. `new` defined for `my-class`, which overrides `new` in `object`, first calls `new` on `object`, then initializes its member `my-member-0` to 1. `destroy` defined for `my-class`, which overrides `destroy` in `object`, first displays a message then calls `destroy` on `object`. `member-0@` declared on and defined for `my-class` returns the value of `my-member-0`. `display-msg` declared on and defined for `my-class` prints `foo`. `new` defined for `my-subclass`, which overrides `new` in `my-class`, first calls `new` on `my-class`, then initializes its member `my-member-1` to -1. `destroy` defined for `my-subclass`, which overrides `destroy` in `my-class`, first displays a message then calls `destroy` on `my-class`. `member-0@` is inherited by `my-subclass` from `my-class`. `display-msg` defined for `my-subclass` prints `bar`, overriding `display-msg` inherited from `my-class`. `member-1@` declared on and defined for `my-subclass` returns the value of `my-member-1`.

To practically use this object system, one has to consider how to create objects. The object system does not concern itself with memory management; it is up to the user to manage memory as they see fit. A object of a given class is a block of memory equal in size to `class-size` applied to the class in general. It may live in a task's dictionary, in a heap, in an array, or like.

Actually initializing an object is not normally done with calling `new` directly but rather through calling `init-object` with the object's class and the starting address at which the object will live, because `init-object` does preliminary initialization before `new` can be called (which it calls itself).

Note that it is good manners for `new` of a given class to call the `new` of its superclass before it does anything else with a new object, or else the object may not function as intended.

Destroying an object is done through calling `destroy` on an object, which will carry out cleanup for an object. Note that actually freeing space for the object is up to the user. The normal convention is that each `destroy` of a given class calls the `destroy` of its superclass after it has cleaned up for itself; however, this should be broken with if a class handles freeing memory allocated for itself, e.g. in a heap, where this should be done after all superclasses' `destroy` methods have been called.

Early binding is carried out through `->`, an immediate state-sensitive word which takes the class to bind a method in off the top of the stack (including while compiling, so one will likely want to specify that class between `[` and `]`) and then either compiles that word (if compiling) or executes that word (if interpreting). The method definition used will be that defined closest in the chain of class definitions leading from that class to `object`. This can be used to call methods in superclasses explicitly, even if they are defined definitely in a given object's class.

### `oo`

The following words are defined in this module:

##### `object`
( -- class )

The ultimate superclass of all classes.

##### `new`
( object -- )

The class constructor method.

##### `destroy`
( object -- )

The class destructor method.

##### `begin-class`
( superclass "name" -- class member-offset method-list )

Begin the declaration of class *name* inheriting from *superclass* as *class*, with an initial member offset *member-offset* and method list *method-list*.

##### `end-class`
( class member-offset method-list -- )

Finish the declaration of *class* with a final member offset *member-offset* and method list *method-list*, alloting the method table for *class*.

##### `begin-implement`
( class -- class )

Begin the definition of methods of *class*.

##### `end-implement`
( class -- )

End the defintion of methods of *class*, populating the method table for *class* with methods defined in the superclass if they are not defined for *class*.

##### `member`
( member-offset method-list member-size "name" -- member-offset method-list )

Declare a member *name* of size *member-size* of the class currently being declared. Note that no automatic alignment is done, so the user must take care to properly align their members.

##### `method`
( method-list "name" -- method-list )

Declare a method *name* of the class currently being declared.

##### `define`
( class xt "name" -- class )

Define method *name* as being bound to *xt* for *class*.

##### `class-size`
( class -- bytes )

Get the size *bytes* of an object of *class*.

##### `object-class`
( object -- class )

Get the class of *object*.

##### `init-object`
( class addr -- )

Initialize the memory at *addr* to be an object of *class* and then call *new* for that object.
