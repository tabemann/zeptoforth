@ Copyright (c) 2019-2023 Travis Bemann
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
	
	.syntax	unified
	.cpu cortex-m4
	.thumb

	.include "config.s"
	.include "../m4_m7/macro.s"
	.include "variables.s"
	
	.text

	.include "vectors.s"
	
	@@ The first (null) dictionary entry
	.p2align 2
	.word invisible_flag
	.word 0
10:	.byte 0
	.p2align 2
	
	@@ The entry point
_handle_reset:
	@@ Initialize r11, relied upon by swdcom
	mov r11, #0
	@@ Initialize the top of stack register
	ldr tos, =0xFEDCBA98
	@@ Get the dictionary base
	ldr r0, =dict_base
	ldr r1, =ram_current
	str r1, [r0]
	@@ Initialize HERE
	ldr r0, =ram_current + ram_here_offset
	ldr r1, =ram_current + user_offset
	str r1, [r0]
	@@ Initialize the data stack pointer
	ldr r1, =stack_top
	movs dp, r1
	@@ Initialize the return stack pointer
	ldr r1, =rstack_top
	mov sp, r1
	@@ Put a garbage value in HANDLER to force a crash if is used
	ldr r0, =ram_current + handler_offset
	ldr r1, =0xF0E1C2D3
	str r1, [r0]
	@@ Initialize the in-RAM vector table
	bl _init_vector_table
	@@ Call the rest of the runtime in an exception handler
	push_tos
	ldr tos, =outer_exc_handled
	bl _try
	@@ If the inner loop returns, reboot
	b _handle_reset

	@@ The outermost exception handling - if an exception happens here the
	@@ system will reboot
outer_exc_handled:
	bl _init_variables
	bl _init_dict
	bl _uart_init
	bl _init_flash_buffers
	bl _do_init
	bl _do_welcome
	bl _quit

	.ltorg
	
	.include "flashrom.s"
	.include "console.s"
	.include "expose.s"
        .include "../common/syntax.s"
	.include "../m4_m7/core.s"
	.include "../m4_m7/divide.s"
	.include "../common/outer.s"
	.include "../common/cond.s"
	.include "../m4_m7/asm.s"
	.include "../common/strings.s"
	.include "../m4_m7/double.s"
	.include "../m4_m7/exception.s"
	.include "../common/final.s"
