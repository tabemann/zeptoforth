@ Copyright (c) 2019-2020 Travis Bemann
@
@ This program is free software: you can redistribute it and/or modify
@ it under the terms of the GNU General Public License as published by
@ the Free Software Foundation, either version 3 of the License, or
@ (at your option) any later version.
@
@ This program is distributed in the hope that it will be useful,
@ but WITHOUT ANY WARRANTY; without even the implied warranty of
@ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
@ GNU General Public License for more details.
@
@ You should have received a copy of the GNU General Public License
@ along with this program.  If not, see <http://www.gnu.org/licenses/>.
	
	.syntax	unified
	.cpu cortex-m4
	.thumb

	.include "src/stm32l476/config.s"
	.include "src/common/macro.s"
	.include "src/stm32l476/variables.s"
	
	.text

	.include "src/stm32l476/vectors.s"
	.include "src/stm32l476/use_48mhz.s"
	
	@@ The first (null) dictionary entry
	.p2align 1
	.hword invisible_flag
	.word 0
	.byte 0
	.p2align 1
	@@ The entry point
handle_reset:
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
	bl _quit

	.ltorg
	
	.include "src/stm32l476/flashrom.s"
	.include "src/stm32l476/console.s"
	.include "src/stm32l476/handlers.s"
	.include "src/stm32l476/expose.s"
	.include "src/common/core.s"
	.include "src/common/outer.s"
	.include "src/common/cond.s"
	.include "src/common/asm.s"
	.include "src/common/strings.s"
	.include "src/common/exception.s"
	.include "src/common/final.s"
