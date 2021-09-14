@ Copyright (c) 2019-2020 Travis Bemann
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

	.include "src/stm32l476/config.s"
	.include "../common/macro.s"
	.include "src/stm32l476/variables.s"
	
	.text

	.include "src/stm32l476/vectors.s"
	
	@@ The first (null) dictionary entry
	.p2align 2
	.word invisible_flag
	.word 0
10:	.byte 0
	.p2align 2
	
	@@ The entry point
handle_reset:
	@@ Initialize r11, relied upon by swdcom
	mov r11, #0
	@@ Initialize the top of stack register
	ldr tos, =0xFEDCBA98
	@@ Initialize the data stack pointer
	ldr r0, =stack_top
	mov dp, r0
	@@ Just in case someone calls this we will restore the return stack
	@@ pointer.
	ldr r0, =rstack_top
	mov sp, r0
	@@ Put a deliberate garbage value in handler
	ldr r0, =0xF0E1C2D3
	ldr r1, =handler
	str r0, [r1]
	@@ Initialize HERE
	ldr r0, =here
	ldr r1, =ram_current
	str r1, [r0]
	@@ Call the rest of the runtime in an exception handler
	push_tos
	ldr tos, =outer_exc_handled
	bl _try
	@@ If the inner loop returns, reboot
	b handle_reset

	@@ The outermost exception handling - if an exception happens here the
	@@ system will reboot
outer_exc_handled:
	bl _init_variables
	bl _init_dict
	bl _uart_init
	bl _use_48mhz
	bl _serial_115200_48mhz
	bl _init_flash_buffers
	bl _do_init
	bl _welcome
	bl _quit

	.ltorg
	
	.include "src/stm32l476/use_48mhz.s"
	.include "src/stm32l476/flashrom.s"
	.include "src/stm32l476/console.s"
	.include "src/stm32l476/handlers.s"
	.include "src/stm32l476/expose.s"
	.include "../common/core.s"
	.include "../common/outer.s"
	.include "../common/cond.s"
	.include "../common/asm.s"
	.include "../common/strings.s"
	.include "../common/double.s"
	.include "../common/exception.s"
	.include "../common/final.s"
