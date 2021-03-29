# Compilation-specific words

These words are in `internal-module`.

##### `start-compile-no-push`

Compile the start of a word without the push {lr}

##### `start-compile`

Compile the start of a word

##### `current-link,`

Compile a link field

##### `finalize,`

Finalize the compilation of a word
	
##### `finalize-no-align,`

Finalize the compilation of a word without aligning

##### `end-compile,`

Compile the end of a word

##### `mov-imm,`

Assemble a move immediate instruction

##### `neg,`

Assemble an reverse subtract immediate from zero instruction

##### `blx-reg,`

Compile a blx (register) instruction
	
##### `branch,`

Compile an unconditional branch

##### `0branch,`

Compile a branch on equal to zero

##### `branch-back!`

Compile a back-referenced unconditional branch

##### `0branch-back!`

Compile a back-referenced branch on equal to zero

##### `inline,`

Inline a word
	
##### `call,`

Call a word at an address
	
##### `bl,`

Compile a bl instruction

##### `mov-16-imm,`

Compile a move 16-bit immediate instruction

##### `mov-16-imm!`

Compile a move 16-bit immediate instruction

##### `movt-imm,`

Compile a move top 16-bit immediate instruction

##### `movt-imm!`

Compile a move top 16-bit immediate instruction

##### `literal,`

Assemble a literal

##### `reserve-literal`

Reserve space for a literal

##### `literal!`

Store a literal ( x reg addr -- )
	
##### `b,`

Assemble an unconditional branch

##### `beq,`

Assemble a branch on equal zero instruction

##### `b-back!`

Assemble an unconditional branch

##### `beq-back!`

Assemble a branch on equal zero instruction

##### `b-32,`

Assemble an unconditional branch

##### `beq-32,`

Assemble a branch on equal zero instruction

##### `b-32-back!`

Assemble an unconditional branch

##### `beq-32-back!`

Assemble a branch on equal zero instruction

##### `reserve-branch`

Reserve space for a branch

##### `out-of-range-branch`

Out of range branch exception

##### `not-building`

Not building exception
	
##### `b-16,`

Assemble an unconditional branch

##### `beq-16,`

Assemble a branch on equal zero instruction

##### `b-16-back!`

Assemble an unconditional branch

##### `beq-16-back!`

Assemble a branch on equal zero instruction

##### `cmp-imm,`

Assemble a compare to immediate instruction

##### `lsl-imm,`

Assemble a logical shift left immediate instruction

##### `orr,`

Assemble an or instruction

##### `ldr-imm,`

Assemble an str immediate instruction

##### `str-imm,`

Assemble an str immediate instruction

##### `add-imm,`

Assemble a subtract immediate instruction

##### `sub-imm,`

Assemble a subtract immediate instruction

##### `pull,`

Assemble instructions to pull a value from the stack

##### `push,`

Assemble instructions to push a value onto the stack

##### `adr,`

Word-align an address

##### `word-align,`

Assemble an instruction to generate a PC-relative address

##### `bx,`

Assemble a BX instruction
