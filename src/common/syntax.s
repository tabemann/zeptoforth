@ Copyright (c) 2023-2024 Travis Bemann
@
@ Permission is hereby granted, free of charge, to any person obtaining a copy
@ of this software and associated documentation files (the "Software"), to deal
@ in the Software without restriction, including without limitation the rights
@ to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
@ copies of the Software, and to permit persons to whom the Software is
@ furnished to do so, subject to the following conditions:
@ 
@ The above copyright notice and this permission notice shall be included in
@ all copies or substantial portions of the Software.
@ 
@ THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
@ IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
@ FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
@ AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
@ LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
@ OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
@ SOFTWARE.

        @ Dump the syntax stack
        define_internal_word "dump-syntax", visible_flag
_dump_syntax:
        ldr r0, =syntax_stack + syntax_stack_size - 1
        ldr r1, =syntax_stack_ptr
        ldr r1, [r1]
1:      cmp r0, r1
        blo 2f
        push_tos
        ldrb tos, [r0]
        subs r0, #1
        b 1b
2:      push_tos
        ldr tos, =syntax_stack + syntax_stack_size
        subs tos, r1
        bx lr
        end_inlined
        
        @ Push a syntax onto the syntax stack
        @ ( syntax -- )
        define_internal_word "push-syntax", visible_flag
_push_syntax:
        push {lr}
        ldr r0, =syntax_stack_ptr
        ldr r1, [r0]
        ldr r2, =syntax_stack
        cmp r1, r2
        bne 1f
        ldr tos, =_syntax_overflow
        bl _raise
1:      subs r1, #1
        strb tos, [r1]
        str r1, [r0]
        pull_tos
        pop {pc}
        end_inlined

        @ Verify a syntax on the syntax stack against one syntax
        @ ( syntax -- )
        define_internal_word "verify-syntax", visible_flag
_verify_syntax:
        push {lr}
        ldr r0, =syntax_stack_ptr
        ldr r1, [r0]
        ldr r2, =syntax_stack + syntax_stack_size
        cmp r1, r2
        bne 1f
        ldr tos, =_syntax_underflow
        bl _raise
1:      ldrb r3, [r1]
        cmp tos, r3
        beq 2f
        ldr tos, =_unexpected_syntax
        bl _raise
2:      pull_tos
        pop {pc}
        end_inlined

        @ Get the topmost syntax
        define_internal_word "get-syntax", visible_flag
_get_syntax: 
        ldr r0, =syntax_stack_ptr
        ldr r1, [r0]
        ldr r2, =syntax_stack + syntax_stack_size
        cmp r1, r2
        bne 1f
        ldr tos, =_syntax_underflow
        bl _raise
1:      push_tos
        ldrb tos, [r1]
        bx lr
        end_inlined
        
        @ Verify a syntax on the syntax stack against two syntaxes
        @ ( syntax1 syntax0 -- )
        define_internal_word "verify-syntax-2", visible_flag
_verify_syntax_2:
        push {lr}
        ldr r0, =syntax_stack_ptr
        ldr r1, [r0]
        ldr r2, =syntax_stack + syntax_stack_size
        cmp r1, r2
        bne 1f
        ldr tos, =_syntax_underflow
        bl _raise
1:      ldrb r3, [r1]
        cmp tos, r3
        bne 2f
        pull_tos
        pull_tos
        pop {pc}
2:      pull_tos
        cmp tos, r3
        beq 3f
        ldr tos, =_unexpected_syntax
        bl _raise
3:      pull_tos
        pop {pc}
        end_inlined

        @ Drop a syntax on the syntax stack
        @ ( -- )
        define_internal_word "drop-syntax", visible_flag
_drop_syntax:
        push {lr}
        ldr r0, =syntax_stack_ptr
        ldr r1, [r0]
        ldr r2, =syntax_stack + syntax_stack_size
        cmp r1, r2
        bne 1f
        ldr tos, =_syntax_underflow
        bl _raise
1:      adds r1, #1
        str r1, [r0]
        pop {pc}
        end_inlined

        @ Unexpected syntax exception
        define_internal_word "x-unexpected-syntax", visible_flag
_unexpected_syntax:
        push {lr}
        string_ln "unexpected syntax"
        bl _type
        pop {pc}
        end_inlined

        @ Syntax underflow
        define_internal_word "x-syntax-underflow", visible_flag
_syntax_underflow:
        push {lr}
        string_ln "syntax underflow"
        bl _type
        pop {pc}
        end_inlined

        @ Syntax overflow
        define_internal_word "x-syntax-overflow", visible_flag
_syntax_overflow:
        push {lr}
        string_ln "syntax overflow"
        bl _type
        pop {pc}
        end_inlined

        .ltorg
        
