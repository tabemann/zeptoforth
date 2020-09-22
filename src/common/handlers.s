@ Copyright (c) 2020 Travis Bemann
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

	@@ Initialize the common handler hooks
	define_internal_word "init-common-handlers", visible_flag
_init_common_handlers:
	movs r0, #0
	ldr r1, =fault_handler_hook
	str r0, [r1]
	ldr r1, =null_handler_hook
	str r0, [r1]
	ldr r1, =systick_handler_hook
	str r0, [r1]
	ldr r1, =svcall_handler_hook
	str r0, [r1]
	ldr r1, =pendsv_handler_hook
	str r0, [r1]
	bx lr
	end_inlined
	
	@@ The fault handler
handle_fault:
	ldr r0, =fault_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr
	end_inlined

	@@ The null handler
handle_null:
	ldr r0, =null_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr
	end_inlined

	@@ The svcall handler
handle_svcall:
	ldr r0, =svcall_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr
	end_inlined

	@@ The pendsv handler
handle_pendsv:
	ldr r0, =pendsv_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr
	end_inlined

	@@ The systick handler
handle_systick:
	ldr r0, =systick_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr
	end_inlined

	.ltorg
	
